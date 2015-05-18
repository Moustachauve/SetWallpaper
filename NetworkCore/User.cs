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

            byte[] IDData = new byte[16];
            Buffer.BlockCopy(pData, offset, IDData, 0, 16);
            Guid id = new Guid(IDData);

            byte[] ipData = new byte[userLength - 16];
            Buffer.BlockCopy(pData, offset + 16, ipData, 0, userLength - 16);
            IPAddress ip = new IPAddress(ipData);

            offset += userLength;

            User user = new User(ip);
            user.ID = id;

            return user;
        }

        #endregion

        #region Attributes

        private Guid m_id;
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

        public Guid ID
        {
            get { return m_id; }
            private set { m_id = value; }
        }

        #endregion

        #region Constructor

        public User(IPAddress pIp)
        {
            m_ip = pIp;
            m_id = Guid.NewGuid();
        }

        #endregion

        #region Serialize

        internal byte[] ToByteArray()
        {
            byte[] ip = m_ip.GetAddressBytes();
            byte[] guid = m_id.ToByteArray();

            byte[] result = CommandSerializer.MergeByteArrays(guid, ip);

            byte[] intArr = CommandSerializer.Serialize(result.Length);

            return CommandSerializer.MergeByteArrays(intArr, result);
        }

        #endregion
    }
}
