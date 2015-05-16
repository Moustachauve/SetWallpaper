using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCore.Command
{

	public enum CommandType : byte
	{
		Notification = 0,
		SetWallpaper,
		UserJoined,
		UserLeft,
	}

	public abstract class Command
	{
		protected CommandType m_type;
		public CommandType Type
		{
			get { return m_type; }
		}


		protected Command(CommandType type)
		{
			m_type = type;
		}


		internal abstract byte[] ToByteArray();
	}
}
