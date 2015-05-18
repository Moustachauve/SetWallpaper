using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using NetworkCore.Commands;

namespace NetworkCore
{
	public class User
    {

        #region Static

        internal static User FromByteArray(byte[] pData, ref int offset)
        {
            int userLength = CommandSerializer.DeserializeInt(pData, ref offset);

            byte[] ipData = new byte[userLength];
            Buffer.BlockCopy(pData, offset, ipData, 0, userLength);

            IPAddress ip = new IPAddress(ipData);

            offset += userLength;

            return new User(ip);
        }

        #endregion

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

		#endregion

		#region Serialize

		internal byte[] ToByteArray()
		{
            byte[] result = m_ip.GetAddressBytes();
            byte[] intArr = CommandSerializer.Serialize(result.Length);

            return CommandSerializer.MergeByteArrays(intArr, result);
		}

		#endregion
    }
}
