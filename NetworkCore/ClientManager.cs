using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AltarNet;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Drawing;

using System.IO;
using NetworkCore.Commands;

namespace NetworkCore
{
	public class ClientManager
	{
		#region Event definitions

		/// <summary>
		/// Occurs when the connection to the Rcon server is lost
		/// </summary>
		public event EventHandler<TcpEventArgs> OnDisconnected;

		/// <summary>
		/// Occurs when the client receive a command from the server
		/// </summary>
		public event EventHandler<CommandReceivedArgs> OnCommandReceived;

		/// <summary>
		/// Occurs when the server send a wrong command data to the client
		/// </summary>
		public event EventHandler<TcpErrorEventArgs> OnCommandError;

		#endregion

		#region Attributes

		private TcpClientHandler m_client;
		private IPAddress m_ip;
		private int m_port;

		#endregion

		#region Properties

		/// <summary>
		/// Get or set the Ip address of the Rcon server
		/// </summary>
		public IPAddress Ip
		{
			get { return m_ip; }
			set
			{
				if (IsConnected)
					throw new InvalidOperationException("Can't change server Ip address while connected to a server");
				m_ip = value;
			}
		}

		/// <summary>
		/// Get or set the port of the Rcon server
		/// </summary>
		public int Port
		{
			get { return m_port; }
			set
			{
				if (IsConnected)
					throw new InvalidOperationException("Can't change server port while connected to a server");
				m_port = value;
			}
		}

		/// <summary>
		/// Determine whether the client is connected to a server or not
		/// </summary>
		public bool IsConnected
		{
			get;
			private set;
		}

		/// <summary>
		/// Get the last connection error detail
		/// </summary>
		public Exception LastConnectionError
		{
			get { return m_client.LastConnectError; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Create a client with the default server port (8888), but without server Ip address
		/// </summary>
		public ClientManager()
		{
			m_port = 8888;
		}

		/// <summary>
		/// Create a client with a specified server Ip address and server port
		/// </summary>
		/// <param name="pIp"></param>
		/// <param name="pPort"></param>
		public ClientManager(IPAddress pIp, int pPort)
		{
			m_ip = pIp;
			m_port = pPort;
		}

		#endregion

		#region Connect / Disconnect

		/// <summary>
		/// Connect to a server. If already connected to one, it will be disconnected to connect to the new one
		/// </summary>
		/// <returns>True if able to connect, or false if it didn't succeed</returns>
		public bool Connect()
		{
			if (m_ip == null)
				throw new InvalidOperationException("Can not connect to a server if no Ip address is specified");
			if (IsConnected)
			{
				throw new InvalidOperationException("Le client est déjà connecté");
			}
			m_client = new TcpClientHandler(m_ip, m_port);
			if (m_client.Connect())
			{
				IsConnected = true;
				m_client.Disconnected += m_client_Disconnected;
				m_client.ReceivedFull += m_client_ReceivedFull;
				return true;
			}

			IsConnected = false;
			m_client = null;
			return false;
		}

		public async Task<bool> ConnectAsync()
		{
			if (m_ip == null)
				throw new InvalidOperationException("Can not connect to a server if no Ip address is specified");
			if (IsConnected)
			{
				throw new InvalidOperationException("Le client est déjà connecté");
			}
			m_client = new TcpClientHandler(m_ip, m_port);

			bool isSuccessful = await m_client.ConnectAsync();

			if (isSuccessful)
			{
				IsConnected = true;
				m_client.Disconnected += m_client_Disconnected;
				m_client.ReceivedFull += m_client_ReceivedFull;
				return true;
			}

			IsConnected = false;
			m_client = null;
			return false;
		}

		/// <summary>
		/// Disconnect from current server
		/// </summary>
		public void Disconnect()
		{
			if (!IsConnected)
				return;

			IsConnected = false;
			m_client.Disconnect();
			m_client = null;
		}

		#endregion

		#region Events

		#region Disconnected from server

		private void m_client_Disconnected(object sender, TcpEventArgs e)
		{
			IsConnected = false;
			if (OnDisconnected != null)
			{
				OnDisconnected(this, e);
			}
			m_client = null;
		}

		#endregion

		#region CommandReceived handler

		private void m_client_ReceivedFull(object sender, TcpReceivedEventArgs e)
		{
			Command command = null;
			try
			{
				command = CommandSerializer.DeserializeCommand(e.Data);
			}
			catch (InvalidCommandException ex)
			{
				if (OnCommandError != null)
					OnCommandError(this, new TcpErrorEventArgs(e.Client, ex));

				return;
			}

			if (OnCommandReceived != null)
			{
				OnCommandReceived(this, new CommandReceivedArgs(command, (User)e.Client.Tag));
			}

			SendCommand(command);

		}

		#endregion

		#endregion

		#region Commands

		/// <summary>
		/// Send a command to the server
		/// </summary>
		/// <param name="pCommand">Command to send</param>
		public void SendCommand(Command pCommand)
		{
			if (!IsConnected)
				throw new InvalidOperationException("Cannot send commands while not connected to a server");

			m_client.Send(pCommand.ToByteArray());
		}

		#endregion

		#region Utility

		public bool FindServer(int port)
		{
			foreach (NetworkInterface netwIntrf in NetworkInterface.GetAllNetworkInterfaces())
			{
				//if the current interface doesn't have an IP, skip it
				if (!(netwIntrf.GetIPProperties().GatewayAddresses.Count > 0))
				{
					continue;
				}

				//get current IP Address(es)
				foreach (UnicastIPAddressInformation uniIpInfo in netwIntrf.GetIPProperties().UnicastAddresses)
				{
					if (uniIpInfo.Address.IsIPv6LinkLocal || uniIpInfo.IPv4Mask == IPAddress.Any)
						continue;

					//get the subnet mask and the IP address as bytes
					byte[] subnetMask = uniIpInfo.IPv4Mask.GetAddressBytes();
					byte[] ipAddr = uniIpInfo.Address.GetAddressBytes();

					// we reverse the byte-array if we are dealing with littl endian.
					if (BitConverter.IsLittleEndian)
					{
						Array.Reverse(subnetMask);
						Array.Reverse(ipAddr);
					}

					uint maskAsInt = BitConverter.ToUInt32(subnetMask, 0);
					uint ipAsInt = BitConverter.ToUInt32(ipAddr, 0);

					//we negate the subnet to determine the maximum number of host possible in this subnet
					uint validHostsEndingMax = ~BitConverter.ToUInt32(subnetMask, 0);

					uint validHostsStart = BitConverter.ToUInt32(ipAddr, 0) & BitConverter.ToUInt32(subnetMask, 0);

					//we increment the startIp to the number of maximum valid hosts in this subnet and for each we check the intended port (refactoring needed)
					for (uint i = 1; i <= validHostsEndingMax; i++)
					{
						uint host = validHostsStart + i;
						byte[] hostBytes = BitConverter.GetBytes(host);
						if (BitConverter.IsLittleEndian)
						{
							Array.Reverse(hostBytes);
						}

						//this is the candidate IP address in "readable format" 
						String ipCandidate = Convert.ToString(hostBytes[0]) + "." + Convert.ToString(hostBytes[1]) + "." + Convert.ToString(hostBytes[2]) + "." + Convert.ToString(hostBytes[3]);

						this.Ip = new IPAddress(hostBytes);

						m_client = new TcpClientHandler(m_ip, m_port);

						IAsyncResult result = m_client.ConnectAsync();
						bool success = result.AsyncWaitHandle.WaitOne(20, true);

						if (success && m_client.InfoHandler.Client.Connected)
						{
							m_client.Disconnected += m_client_Disconnected;
							m_client.ReceivedFull += m_client_ReceivedFull;
							IsConnected = true;
							return true;
						}
						else
						{
							m_client.Disconnect();
							m_client = null;
						}
					}
				}
			}
			Console.WriteLine("No server found");
			return false;
		}

		#endregion

	}
}
