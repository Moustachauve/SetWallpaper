using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace NetworkCore
{
	public class User
	{
		#region Attributes

		private IPAddress m_ip;

		#endregion

		#region Properties

		/// <summary>
		/// Get the client Ip address
		/// </summary>
		public IPAddress Ip
		{
			get { return m_ip; }
		}


		#endregion

		#region Constructor

		public User(IPAddress pIp)
		{
			m_ip = pIp;
		}
		internal User(byte[] pData, int offset)
		{
			byte[] ipData = new byte[pData.Length - offset];
			Buffer.BlockCopy(pData, offset, ipData, 0, ipData.Length);

			m_ip = new IPAddress(ipData);

		}

		#endregion

		#region Serialize

		internal byte[] ToByteArray()
		{
			byte[] result = m_ip.GetAddressBytes();
			return result;
		}

		#endregion
	}
}
