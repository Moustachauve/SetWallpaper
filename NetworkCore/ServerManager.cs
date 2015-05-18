using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using AltarNet;
using System.Drawing;
using System.IO;
using NetworkCore.Commands;

namespace NetworkCore
{
	public class ServerManager
	{

		#region Event definitions

		/// <summary>
		/// Occurs when a client connect to the server
		/// </summary>
		public event EventHandler<TcpEventArgs> OnClientConnected;

		/// <summary>
		/// Occurs when a client disconnect from the server
		/// </summary>
		public event EventHandler<TcpEventArgs> OnClientDisonnected;

		/// <summary>
		/// Occurs when a client send a command to the server
		/// </summary>
        public event EventHandler<CommandReceivedArgs> OnCommandReceived;

		/// <summary>
		/// Occurs when a client send a wrong command data to the server
		/// </summary>
		public event EventHandler<TcpErrorEventArgs> OnCommandError;

		#endregion

		#region Attributes

		private TcpServerHandler m_server;
		private IPAddress m_ip;
		private int m_port;

		#endregion

		#region Properties

		/// <summary>
		/// Get or set the Ip address the Rcon will listen to
		/// </summary>
		public IPAddress Ip
		{
			get { return m_ip; }
			set
			{
				if (IsRunning)
					throw new InvalidOperationException("Can't change server listen Ip while server is running");
				m_ip = value;
			}
		}

		/// <summary>
		/// Get or set the port the Rcon will listen to
		/// </summary>
		public int Port
		{
			get { return m_port; }
			set
			{
				if (IsRunning)
					throw new InvalidOperationException("Can't change server listen port while server is running");
				m_port = value;
			}
		}

		/// <summary>
		/// Determine whether the Rcon server is running or not
		/// </summary>
		public bool IsRunning
		{
			get { return m_server != null; }
		}

		/// <summary>
		/// Get a list of all the client that are connected to the Rcon server
		/// </summary>
		public ICollection<TcpClientInfo> ConnectedClients
		{
			get
			{
				if (!IsRunning)
					throw new InvalidOperationException("Can't get list of client while the server is not running");

				return m_server.Clients.Values;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Create a new Rcon server manager that listen to all Ip address with the default port (8888)
		/// </summary>
		public ServerManager()
		{
			m_ip = IPAddress.Any;
			m_port = 8888;
		}

		/// <summary>
		/// Create a new Rcon server manager with a specific Ip address and port
		/// </summary>
		/// <param name="pIp">Ip address to listen</param>
		/// <param name="pPort">Port to listen</param>
		public ServerManager(IPAddress pIp, int pPort)
		{
			m_ip = pIp;
			m_port = pPort;
		}

		#endregion

		#region Start / Stop

		/// <summary>
		/// Start the Rcon server and begin to listen for connection
		/// </summary>
		public void Start()
		{
			if (IsRunning)
				return;

			m_server = new TcpServerHandler(m_ip, m_port);

			m_server.Connected += m_server_Connected;
			m_server.Disconnected += m_server_Disconnected;
			m_server.ReceivedFull += m_server_ReceivedFull;
			m_server.Start();
		}

		/// <summary>
		/// Close all connection to the Rcon server and stop it
		/// </summary>
		public void Stop()
		{
			if (!IsRunning)
				return;
			SendCommand(new NotificationCommand("Server is shutting down..."));

			m_server.Connected -= m_server_Connected;
			m_server.Disconnected -= m_server_Disconnected;
			m_server.ReceivedFull -= m_server_ReceivedFull;

			m_server.DisconnectAll();
			m_server.Stop();
			m_server = null;
		}

		#endregion

		#region Event listeners

		#region OnClientConnect
		private void m_server_Connected(object sender, TcpEventArgs e)
		{
			IPAddress clientIp = ((IPEndPoint)e.Client.Client.Client.RemoteEndPoint).Address;

			User user = new User(clientIp);

			e.Client.Tag = user;

			if (OnClientConnected != null)
			{
				OnClientConnected(this, e);
			}

			SendCommand(new UserJoinedCommand(user));

            List<User> users = new List<User>();

            foreach(TcpClientInfo client in ConnectedClients) {
                users.Add((User)client.Tag);
            }

            SendCommand(new UserListCommand(users), e.Client);
		}

		#endregion

		#region OnCliendDisconnect

		private void m_server_Disconnected(object sender, TcpEventArgs e)
		{
			if (OnClientDisonnected != null)
			{
				OnClientDisonnected(this, e);
			}
			SendCommand(new UserLeftCommand((User)e.Client.Tag));
		}

		#endregion

		#region OnCommandReceived

		private void m_server_ReceivedFull(object sender, TcpReceivedEventArgs e)
		{
			Command command = null;
			try
			{
				command = CommandSerializer.DeserializeCommand(e.Data);
			}
			catch (EncoderFallbackException ex)
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

		#region Server commands

		/// <summary>
		/// Send a command to all connected client
		/// </summary>
		/// <param name="pCommand">Command to send</param>
		public void SendCommand(Command pCommand)
		{
			if (!IsRunning)
				throw new InvalidOperationException("Cannot send commands while server is not running");

			m_server.SendAll(pCommand.ToByteArray());
		}

		public void SendCommand(Command pCommand, TcpClientInfo pClient)
		{
			if (!IsRunning)
				throw new InvalidOperationException("Cannot send commands while server is not running");

			m_server.Send(pClient, pCommand.ToByteArray());
		}

		#endregion

	}
}
