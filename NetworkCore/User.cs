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
            if (BitConverter.IsLittleEndian)
                Array.Reverse(pData, offset, 4);

            int userLenght = BitConverter.ToInt32(pData, 1);

            offset += 4;

            byte[] ipData = new byte[userLenght];
            Buffer.BlockCopy(pData, offset, ipData, 0, userLenght);

            IPAddress ip = new IPAddress(ipData);

            offset += userLenght;

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

            byte[] intBytes = BitConverter.GetBytes(result.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
			
			return CommandSerializer.MergeByteArrays(intBytes, result);
		}

		#endregion
    }
}
