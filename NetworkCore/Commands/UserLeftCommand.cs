using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCore.Commands
{
	public class UserLeftCommand : Command
	{
		private User m_user;
		public User User { get { return m_user; } }
		internal UserLeftCommand(User pUser)
			: base(CommandType.UserLeft)
		{
			m_user = pUser;
		}

		internal UserLeftCommand(byte[] pData)
			: base(CommandType.UserLeft)
		{
			m_type = (CommandType)pData[0];

            int offset = 1;
            m_user = User.FromByteArray(pData, ref offset);
		}

		internal override byte[] ToByteArray()
		{
			return CommandSerializer.PrefixCommand(m_type, m_user.ToByteArray());
		}
	}
}
