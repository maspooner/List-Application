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
	/// <summary>
	/// Contains a list of constants for the program.
	/// </summary>
	static class C {
		internal const int FIELD_GRID_WIDTH = 10; //TODO adjustable
		internal const int SHOWN_AUTOSAVE_TEXT_TIME = 5000; //TODO adjust
		internal const int MAX_CACHED_WIDTH = 500; //TODO adjustable
		internal const int MAX_CACHED_HEIGHT = 500; //TODO adjustable
		internal const int COLOR_SEED = 5003534; //TODO adjust 5003534
		internal const double DEFAULT_IMAGE_DISPLAY_HEIGHT = 50.0;
	}
	/// <summary>
	/// Models a rectangle in space with specified dimensions
	/// </summary>
	[Serializable]
	class Space {
		//properties
		internal int X { get; set; }
		internal int Y { get; set; }
		internal int Width { get; set; }
		internal int Height { get; set; }
		//constructors
		/// <summary>
		/// Constructs a <seealso cref="Space"/> at a given x and y
		/// position with a size of 1 by 1
		/// </summary>
		/// <param name="x">the x location</param>
		/// <param name="y">the y location</param>
		internal Space(int x, int y) : this(x, y, 1, 1) { }
		/// <summary>
		/// A copy constructor that constructs a <seealso cref="Space"/>
		/// with the same properties as another <seealso cref="Space"/>
		/// </summary>
		/// <param name="space">the space to copy values from</param>
		internal Space(Space space) : this(space.X, space.Y, space.Width, space.Height) { }
		/// <summary>
		/// Constructs a <seealso cref="Space"/> at a given x and y
		/// with a given width and height
		/// </summary>
		/// <param name="x">the x location</param>
		/// <param name="y">the y location</param>
		/// <param name="width">the width of the space</param>
		/// <param name="height">the height of the space</param>
		internal Space(int x, int y, int width, int height) {
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
		//methods
		/// <summary>
		/// Does this <seealso cref="Space"/> contain a point (p, q)?
		/// </summary>
		/// <param name="p">the x coordinate to test</param>
		/// <param name="q">the y coordinate to test</param>
		internal bool Contains(int p, int q) {
			//must have both p and q be inside the dimensions of the rectangle
			return (X <= p && p < X + Width) && (Y <= q && q < Y + Height);
		}
		/// <summary>
		/// Does this <seealso cref="Space"/> overlap with another space?
		/// </summary>
		/// <param name="thatSpace">another space to test against</param>
		internal bool Intersects(Space thatSpace) {
			//for every coordinate in the other space
			for(int p = thatSpace.X; p < thatSpace.X + thatSpace.Width; p++) {
				for(int q = thatSpace.Y; q < thatSpace.Y + thatSpace.Height; q++) {
					//if this space contains the point of the other space
					if(Contains(p, q)) {
						//they intersect
						return true;
					}
				}
			}
			//no intersections
			return false;
		}
	}
	/// <summary>
	/// Contains extension methods for classes
	/// </summary>
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
		internal static void SetupContentGrid(Grid g, IEnumerable<Space> spaces) {
			for (int i = 0; i < C.FIELD_GRID_WIDTH; i++) {
				ColumnDefinition cd = new ColumnDefinition();
				g.ColumnDefinitions.Add(cd);
			}
			int maxHeight = 0;
			foreach (Space sp in spaces) {
				if (sp.Y + sp.Height > maxHeight) {
					maxHeight = sp.Y + sp.Height;
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
