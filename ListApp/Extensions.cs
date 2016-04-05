using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ListApp {
	static class Extensions {
		internal static BitmapImage ConvertToWPFImage(this Bitmap b) {
			MemoryStream ms = new MemoryStream();
			b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
			ms.Position = 0;
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.StreamSource = ms;
			bi.EndInit();
			return bi;
		}
	}
}
