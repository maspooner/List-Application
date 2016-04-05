using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace ListApp {
	enum ItemType { BASIC, DATE, IMAGE, ENUM }
	[Serializable]
	abstract class ListItemField : IComparable<ListItemField>, ISerializable {
		//members
		private string fieldName;
		//constructors
		internal ListItemField(string fieldName) {
			this.fieldName = fieldName;
		}
		public ListItemField(SerializationInfo info, StreamingContext context) {
			fieldName = info.GetValue("fieldName", typeof(string)) as string;
		}
		//properties
		internal string Name { get { return fieldName; } }
		//methods
		public abstract int CompareTo(ListItemField other);
		public abstract void SetValue(object obj);
		public abstract object GetValue();
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("fieldName", fieldName);
			info.AddValue("value", GetValue());
		}
	}
	[Serializable]
	class BasicField : ListItemField {
		//members
		private string value;
		//constructors
		internal BasicField(string fieldName) : base(fieldName){
			value = null;
		}
		public BasicField(SerializationInfo info, StreamingContext context) : base(info, context) {
			value = info.GetValue("value", typeof(string)) as string;
		}
		//methods
		public override int CompareTo(ListItemField other) {
			return other is BasicField ? this.value.CompareTo((other as BasicField).value) : -1;
		}
		public override object GetValue() {
			return value;
		}
		public override void SetValue(object obj) {
			value = obj as string;
		}
	}
	[Serializable]
	class DateField : ListItemField {
		//members
		private DateTime value;
		//constructors
		internal DateField(string fieldName) : base(fieldName) {
			value = DateTime.MinValue;
		}
		public DateField(SerializationInfo info, StreamingContext context) : base(info, context) {
			value = (DateTime) info.GetValue("value", typeof(DateTime));
		}
		//methods
		public override int CompareTo(ListItemField other) {
			return other is DateField ? this.value.CompareTo((other as DateField).value) : -1;
		}
		public override object GetValue() {
			return value;
		}
		public override void SetValue(object obj) {
			value = (DateTime)obj;
		}
	}
	[Serializable]
	class ImageField : ListItemField {
		//members
		[NonSerialized]
		private BitmapImage value;
		//constructors
		internal ImageField(string fieldName) : base(fieldName) {
			value = null;
			//TODO change file size when caching
		}
		public ImageField(SerializationInfo info, StreamingContext context) : base(info, context) {
			byte[] imageBytes = (byte[])info.GetValue("value", typeof(byte[]));
			if (imageBytes != null) {
				using (MemoryStream ms = new MemoryStream(imageBytes)) {
					value = new Bitmap(ms).ConvertToWPFImage();
				}
			}
		}
		//methods
		internal BitmapImage GetBitmap() {
			return value;
		}
		public override int CompareTo(ListItemField other) {
			return 0;
		}
		public override object GetValue() {
			byte[] imageBytes = null;
			if(value != null) {
				using(MemoryStream ms = new MemoryStream()) {
					PngBitmapEncoder pngbe = new PngBitmapEncoder();
					BitmapFrame frame = BitmapFrame.Create(value);
					pngbe.Frames.Add(frame);
					pngbe.Save(ms);
					imageBytes = ms.ToArray();
				}
			}
			return imageBytes;
		}
		public override void SetValue(object obj) {
			value = new Bitmap(obj as Bitmap, new Size(300, 100)).ConvertToWPFImage(); //TODO adjustable
		}
	}
	[Serializable]
	class EnumField : ListItemField {
		//members
		private int value;
		//constructors
		internal EnumField(string fieldName) : base(fieldName) {
			value = 0;
		}
		public EnumField(SerializationInfo info, StreamingContext context) : base(info, context) {
			value = (int)info.GetValue("value", typeof(int));
		}
		//methods
		public string GetSelectedValue(object metadata) {
			return (metadata as string[])[value];
		}
		public override int CompareTo(ListItemField other) {
			return other is EnumField ? value - (other as EnumField).value : -1;
		}
		public override object GetValue() {
			return value;
		}
		public override void SetValue(object obj) {
			value = (int)obj;
		}
	}
}
