using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ListApp {
	[Serializable]
	class Location : ISerializable{
		//members
		private int x, y;
		//constructors
		internal Location() : this(0, 0) {	}
		internal Location(int x, int y) {
			this.x = x;
			this.y = y;
		}
		public Location(SerializationInfo info, StreamingContext context) {
			x = (int) info.GetValue("x", typeof(int));
			y = (int)info.GetValue("y", typeof(int));
		}
		//properties
		internal int X { get { return x; } }
		internal int Y { get { return y; } }
		//methods
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("x", x);
			info.AddValue("y", y);
		}
	}
	static class Extensions {
		internal static BitmapImage ConvertToBitmapImage(this Bitmap b) {
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
