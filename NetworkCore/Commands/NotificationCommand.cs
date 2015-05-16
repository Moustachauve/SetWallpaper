using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCore.Commands
{
	public class NotificationCommand : Command
	{
		private string m_text;
		public string Text
		{
			get { return m_text; }
		}

		public NotificationCommand(string pText)
			: base(CommandType.Notification)
		{
			m_text = pText;
		}
		public NotificationCommand(byte[] pData)
			: base(CommandType.Notification)
		{
			Encoding.UTF8.GetString(pData, 1, pData.Length - 1);
		}

		internal override byte[] ToByteArray()
		{
			byte[] text = System.Text.Encoding.UTF8.GetBytes(m_text);
			return CommandSerializer.PrefixCommand(m_type, text);
		}
	}
}
