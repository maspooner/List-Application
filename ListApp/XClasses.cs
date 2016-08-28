using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ListApp {
	[Serializable]
	class XImage : ISerializable {
		//members
		[NonSerialized]
		private Bitmap img;
		private bool isWeb;
		private string filePath;
		private bool isLoaded;
		//constructors
		internal XImage(string filePath, bool isWeb) {
			this.isWeb = isWeb;
			this.filePath = filePath;
			this.isLoaded = LoadImage();
		}
		public XImage(SerializationInfo info, StreamingContext context) {
			byte[] imageBytes = (byte[])info.GetValue("imgBytes", typeof(byte[]));
			if (imageBytes != null) {
				using (MemoryStream ms = new MemoryStream(imageBytes)) {
					this.img = new Bitmap(ms);
				}
			}
			this.isLoaded = imageBytes != null;
			this.isWeb = info.GetBoolean("isWeb");
			this.filePath = info.GetString("filePath");
		}
		//properties
		internal bool IsLoaded { get { return isLoaded; } }
		internal Bitmap Img { get { return img; } }
		//methods
		internal void ClearImage() {
			if(img != null) {
				img.Dispose();
			}
			img = null;
			isLoaded = false;
		}
		internal void ReloadImage() {
			if (!isLoaded) {
				isLoaded = LoadImage();
			}
		}
		private bool LoadImage() {
			if (isWeb) {
				try {
					using (WebClient wc = new WebClient()) {
						using (Stream s = wc.OpenRead(filePath)) {
							this.img = new Bitmap(s).ShrinkForCache();
						}
					}
					return true;
				}
				catch(Exception e) {
					//TODO inform user
					Console.WriteLine("Not a valid image url: " + e);
				}
				return false;
			}
			else {
				try {
					this.img = new Bitmap(Bitmap.FromFile(filePath)).ShrinkForCache();
					return true;
				}
				catch(Exception e) {
					//TODO inform user
					Console.WriteLine("Not a valid image path: " + e.Message);
				}
				return false;
			}
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("isWeb", isWeb);
			info.AddValue("filePath", filePath);
			info.AddValue("imgBytes", img == null ? null : img.ToBytes());
		}
		public override string ToString() {
			return "XImage " + (isLoaded ? "" : "not ") + "loaded from " + (isWeb ? "Website" : "File") + ": " + filePath;
		}
	}
	[Serializable]
	class XDate : IComparable<XDate> {
		//members
		private int? year;
		private int? month;
		private int? day;
		private bool unknown;
		private int id;
		//constructors
		internal XDate(DateTime date) {
			year = date.Year;
			month = date.Month;
			day = date.Day;
			unknown = false;
			id = -1;
		}
		internal XDate(int? year, int? month, int? day, int id) {
			this.year = year;
			this.month = month;
			this.day = day;
			unknown = true;
			this.id = id;
		}
		//properties

		//methods
		private int CompareValue(int? val1, int? val2, int val2Id) {
			if (val1 != null && val2 != null) {
				return val1.Value.CompareTo(val2.Value);
			}
			if (val1 != null) {
				return 1;
			}
			if(val2 != null) {
				return -1;
			}
			return id.CompareTo(val2Id);
		}
		public int CompareTo(XDate other) {
			int yearComp = CompareValue(year, other.year, other.id);
			if(yearComp == 0) {
				int monthComp = CompareValue(month, other.month, other.id);
				if(monthComp == 0) {
					return CompareValue(day, other.day, other.id);
				}
				return monthComp;
			}
			return yearComp;
		}
		public override string ToString() {
			if (month == null && day == null && year == null) return "";
			string s = month == null ? "" : CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[month.Value - 1];
			if(s.Length > 0 && day != null) {
				s += " ";
			}
			s += day == null ? "" : day.Value.ToString() + ",";
			if (s.Length > 0 && year != null) {
				s += " ";
			}
			s += year == null ? "" : year.Value.ToString();
			if (unknown) {
				s += " (" + id + ")";
			}
			return s;
		}
	}
}
