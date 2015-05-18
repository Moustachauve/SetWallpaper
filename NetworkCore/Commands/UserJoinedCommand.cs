﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCore.Commands
{
	public class UserJoinedCommand : Command
	{
		private User m_user;
		public User User { get { return m_user; } }
		internal UserJoinedCommand(User pUser)
			: base(CommandType.UserJoined)
		{
			m_user = pUser;
		}

		internal UserJoinedCommand(byte[] pData)
			: base(CommandType.UserJoined)
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
