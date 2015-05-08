using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AltarNet;
using System.Drawing;
using System.IO;

namespace NetworkCore
{
	#region NotificationReceived

	/// <summary>
	/// Represent information relative to a notification
	/// </summary>
	public class NotificationReceivedArgs : EventArgs
	{
		private string m_notification;
		private string m_type;

		public string Message
		{
			get { return m_notification; }
		}

		public string Type
		{
			get { return m_type; }
		}

        internal NotificationReceivedArgs(string pMessage, string type)
		{
			m_notification = pMessage;
			m_type = type;
		}
	}

	#endregion

    #region WallpaperReceived

    /// <summary>
    /// Represent information relative to a wallpaper sent by a client
    /// </summary>
    public class WallpaperReceivedArgs : EventArgs
    {
        private Image m_image;
		private Wallpaper.Style m_style;

        public Image Image
        {
            get { return m_image; }
        }

		public Wallpaper.Style Style
		{
			get { return m_style; }
		}

		internal WallpaperReceivedArgs(Image pImage, Wallpaper.Style pStyle)
        {
            m_image = pImage;
			m_style = pStyle;
        }

        internal WallpaperReceivedArgs(byte[] pImage, Wallpaper.Style pStyle)
        {
            using (MemoryStream mStream = new MemoryStream(pImage))
            {
                m_image = Image.FromStream(mStream);
            }
			m_style = pStyle;
        }
    }


    #endregion
}
