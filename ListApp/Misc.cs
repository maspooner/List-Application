using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
		internal int X
		{
			get { return x; }
			set { x = value; }
		}
		internal int Y
		{
			get { return y; }
			set { y = value; }
		}
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
		internal static void ClearGrid(this Grid g) {
			g.Children.Clear();
			g.RowDefinitions.Clear();
			g.ColumnDefinitions.Clear();
		}
		internal static FrameworkElement FindAt(this Grid g, int x, int y) {
			foreach (FrameworkElement fe in g.Children) {
				if(Grid.GetColumn(fe) == x && Grid.GetRow(fe) == y) {
					return fe;
				}
			}
			return null;
		}
	}
	class Utils {
		internal static void SetupContentGrid(Grid g, List<ItemTemplateItem> template) {
			for (int i = 0; i < MList.FIELD_GRID_WIDTH; i++) {
				ColumnDefinition cd = new ColumnDefinition();
				g.ColumnDefinitions.Add(cd);
			}
			int maxHeight = 0;
			foreach (ItemTemplateItem iti in template) {
				if (iti.Y > maxHeight) {
					maxHeight = iti.Y;
				}
			}
			maxHeight += 5;
			for (int i = 0; i < maxHeight; i++) {
				RowDefinition rd = new RowDefinition();
				g.RowDefinitions.Add(rd);
			}
		}
	}
}
