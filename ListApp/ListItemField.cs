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
			value = info.GetString("value");
		}
		//methods
		public override int CompareTo(ListItemField other) {
			return other is BasicField ? this.value.CompareTo((other as BasicField).value) : -1;
		}
		public override object GetValue() {
			return value;
		}
		public override void SetValue(object obj) {
			value = obj.ToString();
		}
	}
	[Serializable]
	class DateField : ListItemField {
		//members
		private XDate value;
		//constructors
		internal DateField(string fieldName) : base(fieldName) {
			value = null;
		}
		public DateField(SerializationInfo info, StreamingContext context) : base(info, context) {
			value = info.GetValue("value", typeof(XDate)) as XDate;
		}
		//methods
		public override int CompareTo(ListItemField other) {
			return other is DateField ? this.value.CompareTo((other as DateField).value) : -1;
		}
		public override object GetValue() {
			return value;
		}
		public override void SetValue(object obj) {
			if(obj is DateTime) {
				value = new XDate((DateTime)obj);
			}
			else {
				value = obj as XDate;
			}
		}
	}
	[Serializable]
	class ImageField : ListItemField {
		//members
		private XImage value;
		[NonSerialized]
		private BitmapImage bImg;
		//constructors
		internal ImageField(string fieldName) : base(fieldName) {
			value = null;
			bImg = null;
		}
		public ImageField(SerializationInfo info, StreamingContext context) : base(info, context) {
			value = info.GetValue("value", typeof(XImage)) as XImage;
			bImg = CreateBitmapImage();
		}
		//methods
		internal BitmapImage GetBitmapImage() {
			return bImg;
		}
		public override int CompareTo(ListItemField other) {
			return 0;
		}
		public override object GetValue() {
			return value;
		}
		public override void SetValue(object obj) {
			value = obj as XImage;
			bImg = CreateBitmapImage();
		}
		private BitmapImage CreateBitmapImage() {
			if (value != null && value.IsLoaded) {
				return value.Img.ConvertToBitmapImage();
			}
			return null;
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
			value = info.GetInt32("value");
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
