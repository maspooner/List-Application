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
using System.Xml;
using MColor = System.Windows.Media.Color;

namespace ListApp {
	static class C {
		internal const int FIELD_GRID_WIDTH = 10; //TODO adjustable
		internal const int SHOWN_AUTOSAVE_TEXT_TIME = 5000; //TODO adjust
		internal const int MAX_CACHED_WIDTH = 500; //TODO adjustable
		internal const int MAX_CACHED_HEIGHT = 500; //TODO adjustable
		internal const int COLOR_SEED = 5003534; //TODO adjust 5003534
	}
	[Serializable]
	class Location{
		//members
		private int x, y;
		//constructors
		internal Location() : this(0, 0) {	}
		internal Location(int x, int y) {
			this.x = x;
			this.y = y;
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
	}
	static class Extensions {
		internal static XmlNode FindChild(this XmlNode parent, string tagName) {
			foreach (XmlNode n in parent.ChildNodes) {
				if (n.Name.Equals(tagName))
					return n;
			}
			return null;
		}
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
		internal static Bitmap ShrinkForCache(this Bitmap b) {
			double newWidth = b.Width;
			double newHeight = b.Height;
			double ratio = (double)b.Width / b.Height;
			if(newWidth > C.MAX_CACHED_WIDTH) {
				newHeight = C.MAX_CACHED_WIDTH / ratio;
				newWidth = C.MAX_CACHED_WIDTH;
			}
			if(newHeight > C.MAX_CACHED_HEIGHT) {
				newHeight = C.MAX_CACHED_HEIGHT;
				newWidth = ratio * C.MAX_CACHED_HEIGHT;
			}
			Bitmap scaled = new Bitmap((int)newWidth, (int)newHeight);
			using(Graphics g = Graphics.FromImage(scaled)) {
				g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
				g.DrawImage(b, 0, 0, scaled.Width, scaled.Height);
			}
			return scaled;
		}
		internal static byte[] ToBytes(this Bitmap b) {
			ImageConverter ic = new ImageConverter();
			return ic.ConvertTo(b, typeof(byte[])) as byte[];
		}
	}
	class Utils {
		internal static void SetupContentGrid(Grid g, List<ItemTemplateItem> template) {
			for (int i = 0; i < C.FIELD_GRID_WIDTH; i++) {
				ColumnDefinition cd = new ColumnDefinition();
				g.ColumnDefinitions.Add(cd);
			}
			int maxHeight = 0;
			foreach (ItemTemplateItem iti in template) {
				if (iti.Y + iti.Height > maxHeight) {
					maxHeight = iti.Y + iti.Height;
				}
			}
			maxHeight += 7;
			for (int i = 0; i < maxHeight; i++) {
				RowDefinition rd = new RowDefinition();
				g.RowDefinitions.Add(rd);
			}
		}
		internal static bool ShouldUseWhite(MColor c) {
			return c.R * 299 + c.G * 587 + c.B * 114 <= 127500;
        }
		internal static MColor RandomColor(Random rand) {
            return HSBtoColor(rand.Next(0, 360), 0.45, 0.85);
        }
		private static MColor HSBtoColor(double h, double s, double br) {
			double r = 0, g = 0, bl = 0;
			// the color wheel consists of 6 sectors. Figure out which sector you're in.
			double sectorPos = h / 60.0;
			int sectorNumber = (int)(Math.Floor(sectorPos));
			// get the fractional part of the sector
			double fractionalSector = sectorPos - sectorNumber;
			// calculate values for the three axes of the color.
			double p = br * (1.0 - s);
			double q = br * (1.0 - (s * fractionalSector));
			double t = br * (1.0 - (s * (1 - fractionalSector)));
			// assign the fractional colors to r, g, and b based on the sector
			// the angle is in.
			switch (sectorNumber) {
				case 0:
					r = br; g = t; bl = p;
					break;
				case 1:
					r = q; g = br; bl = p;
					break;
				case 2:
					r = p; g = br; bl = t;
					break;
				case 3:
					r = p; g = q; bl = br;
					break;
				case 4:
					r = t; g = p; bl = br;
					break;
				case 5:
					r = br; g = p; bl = q;
					break;
			}
			r *= 256;
			g *= 256;
			bl *= 256;
			Console.WriteLine("r: " + r);
			Console.WriteLine("g: " + g);
			Console.WriteLine("b: " + bl);
			return MColor.FromRgb((byte)r, (byte)g, (byte)bl);
		}
	}
}
