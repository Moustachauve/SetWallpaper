using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCore.Commands
{
	public class SetWallpaperCommand : Command
	{
		private Image m_image;
		public Image Image { get { return m_image; } }

		private Wallpaper.Style m_style;
		public Wallpaper.Style Style { get { return m_style; } }

		public SetWallpaperCommand(Image pImage, Wallpaper.Style pStyle)
			: base(CommandType.SetWallpaper)
		{
			m_image = pImage;
			m_style = pStyle;
		}

		public SetWallpaperCommand(byte[] pData)
			: base(CommandType.SetWallpaper)
		{
			m_style = (Wallpaper.Style)pData[1];

			byte[] imageArr = new byte[pData.Length - 2];
			Buffer.BlockCopy(pData, 2, imageArr, 0, pData.Length - 2);
			m_image = (Bitmap)((new ImageConverter()).ConvertFrom(pData));

		}


		internal override byte[] ToByteArray()
		{
			ImageConverter imgCon = new ImageConverter();
			byte[] imageArr = (byte[])imgCon.ConvertTo(m_image, typeof(byte[]));

			byte[] data = new byte[2];
			data[0] = (byte)m_type;
			data[1] = (byte)m_style;

			return CommandSerializer.MergeByteArrays(data, imageArr);
		}
	}
}
