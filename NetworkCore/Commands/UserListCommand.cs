using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCore.Commands
{
    public class UserListCommand : Command
    {

        private List<User> m_userList;
        public IList<User> UserList { get { return m_userList.AsReadOnly(); } }

        internal UserListCommand(List<User> users)
            : base(CommandType.UserList)
        {
            m_userList = new List<User>(users);
        }

        internal UserListCommand(byte[] pData)
            : base(CommandType.UserList)
        {
            int offset = 1;

            int listCount = CommandSerializer.DeserializeInt(pData, ref offset);
            m_userList = new List<User>(listCount);

            for (int i = 0; i < listCount; i++)
            {
                m_userList.Add(User.FromByteArray(pData, ref offset));
            }
        }

        internal override byte[] ToByteArray()
        {
            byte[] data = CommandSerializer.Serialize(m_userList.Count);

            foreach (User user in UserList)
            {
                data = CommandSerializer.MergeByteArrays(data, user.ToByteArray());
            }

            return CommandSerializer.PrefixCommand(m_type, data);
        }
    }
}
