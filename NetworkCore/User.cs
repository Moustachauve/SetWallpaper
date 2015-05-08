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

		#endregion
	}
}
