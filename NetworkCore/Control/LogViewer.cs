using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkCore.Control
{
	public enum MessageType
	{
		Normal,
		Notification,
		Error,
		ServerAction,
		UserAction
	}

	public class LogViewer : WebBrowser
	{
		private string m_stylesheet;
		private string m_dateFormat;

		/// <summary>
		/// Get or set the path to the stylesheet relative to the executable
		/// </summary>
		public string StyleSheet
		{
			get { return m_stylesheet; }
			set { m_stylesheet = value; }
		}

		public string DateFormat
		{
			get { return m_dateFormat; }
			set { m_dateFormat = value; }
		}

		public LogViewer()
			: base()
		{
			this.Navigate("about:blank");
			this.AllowNavigation = false;
			this.WebBrowserShortcutsEnabled = false;
            this.AllowWebBrowserDrop = false;

			m_stylesheet = "/Style/log.css";
			m_dateFormat = "yyyy/MM/dd | HH:mm:ss";

			Clear();
		}

		public void Clear()
		{
			Document.OpenNew(false);
			WriteHtml("<!DOCTYPE html>");
			WriteHtml("<html lang=\"en\">");
			WriteHtml("<head>");
			WriteHtml("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
			WriteHtml("<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\">");
			WriteHtml("<title>" + DateTime.Now.ToString() + "</title>");
			//WriteHtml("<link rel=\"stylesheet\" type=\"text/css\" href=\"Style/log.css\">");
			WriteHtml("<link rel=\"StyleSheet\" HREF=\"file:///" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace("\\", "/") + m_stylesheet + "\" />");
			WriteHtml("</head>");
			WriteHtml("<body>");
            while (Document.Body == null)
            {
                Application.DoEvents();
            }
		}

		/// <summary>
		/// Write a message in the viewer without the date
		/// </summary>
		/// <remarks>This method should only be used to add information to another message on another line.</remarks>
		/// <param name="pMessage">The message to insert in the viewer</param>
		public void Write(string pMessage)
		{
			WriteHtml(pMessage);
		}

		/// <summary>
		/// Write a message in the viewer as a normal message
		/// </summary>
		/// <param name="pMessage">The message to insert in the viewer</param>
		public void WriteLine(string pMessage)
		{
			WriteLine(pMessage, MessageType.Normal);
		}

		/// <summary>
		/// Write a message in the viewer with the correct prefix
		/// </summary>
		/// <param name="pMessage">The message</param>
		/// <param name="pType">Type of the message</param>
		public void WriteLine(string pMessage, MessageType pType)
		{
			string html = GetHtmlDate();
			html += GetPrefix(pType);
			html += pMessage;

			WriteHtml(html);

		}

		/// <summary>
		/// Write an image in the viewer
		/// </summary>
		/// <param name="pImage">Image to write</param>
		public void WriteLine(Image pImage)
		{
			WriteLine(pImage, "", MessageType.Normal);
		}

		/// <summary>
		/// Write an image to the viewer with a normal caption
		/// </summary>
		/// <param name="pImage">Image to write</param>
		/// <param name="pMessage">Caption to write before the image</param>
		public void WriteLine(Image pImage, string pMessage)
		{ 
			WriteLine(pImage, pMessage, MessageType.Normal);
		}

		/// <summary>
		/// Write an image to the viewer with a caption and the correct prefix
		/// </summary>
		/// <param name="pImage">Image to write</param>
		/// <param name="pMessage">Caption to write before the image</param>
		/// <param name="pType">Type of the message</param>
		public void WriteLine(Image pImage, string pMessage, MessageType pType)
		{
			string html = GetHtmlDate();
			html += GetPrefix(pType);
			html += pMessage;
			html += "<img src=\"" + ImageToBase64(pImage) + "\" height=\"100px\" />";

			WriteHtml(html);
		}

		private void WriteHtml(string pHtml)
		{
			Document.Write("<div>" + pHtml  + Environment.NewLine + "</div>");
            if (Document.Body != null)
                Document.Window.ScrollTo(0, Document.Body.ScrollRectangle.Height);
		}

		private string GetPrefix(MessageType pType)
		{
			switch (pType)
			{
				case MessageType.Notification:
					return "<span class='prefix notif'>[NOTIF]</span> ";

				case MessageType.Error:
					return "<span class='prefix error'>[ERROR]</span> ";

				case MessageType.ServerAction:
					return "<span class='prefix server'>[SERVER]</span> ";

				case MessageType.UserAction:
					return "<span class='prefix guest'>[USER]</span> ";

				default:
					return "";
			}
		}

		/// <summary>
		/// Get the current date in a formated html block
		/// </summary>
		/// <returns>String containing the html markup and the current date</returns>
		private string GetHtmlDate()
		{
			string date = DateTime.Now.ToString(m_dateFormat);

			return "<span class='date'>[" + date + "]</span> ";
		}

		private string ImageToBase64(Image pImage)
		{
			string format = new ImageFormatConverter().ConvertToString(pImage.RawFormat);
			ImageConverter imgCon = new ImageConverter();
			byte[] imageArr = (byte[])imgCon.ConvertTo(pImage, typeof(byte[]));

			return "data:image/" + format + ";base64," + Convert.ToBase64String(imageArr);
		}

	}
}
