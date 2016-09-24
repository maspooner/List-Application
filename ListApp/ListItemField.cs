using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace ListApp {
	enum ItemType { BASIC, DATE, IMAGE, ENUM, NUMBER, DECIMAL }
	[Serializable]
	abstract class ListItemField : IComparable<ListItemField>, ISerializable {
		//members
		private string fieldName;
		private object value;
		//constructors
		internal ListItemField(string fieldName) {
			this.fieldName = fieldName;
			value = StartingValue();
		}
		public ListItemField(SerializationInfo info, StreamingContext context) {
			fieldName = info.GetValue("fieldName", typeof(string)) as string;
			value = DeserializeValue(info);
		}
		//properties
		internal string Name { get { return fieldName; } }
		internal virtual object Value { get { return value; } set { this.value = value; } }
		//methods
		public abstract int CompareTo(ListItemField other);
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("fieldName", fieldName);
			info.AddValue("value", value);
		}
		internal abstract object DeserializeValue(SerializationInfo info);
		internal virtual object StartingValue() {
			return null;
		}
	}
	[Serializable]
	class BasicField : ListItemField {
		//constructors
		internal BasicField(string fieldName) : base(fieldName) { }
		public BasicField(SerializationInfo info, StreamingContext context) : base(info, context) { }
		//methods
		internal override object DeserializeValue(SerializationInfo info) {
			return info.GetString("value");
		}
		public override int CompareTo(ListItemField other) {
			return other is BasicField ? (this.Value as string).CompareTo(other.Value as string) : -1;
		}
	}
	[Serializable]
	class NumberField : ListItemField {
		//constructors
		internal NumberField(string fieldName) : base(fieldName) { }
		public NumberField(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public override int CompareTo(ListItemField other) {
			return other is NumberField ? ((int)this.Value).CompareTo((int)other.Value) : -1;
		}
		internal override object DeserializeValue(SerializationInfo info) {
			return info.GetInt32("value");
		}
	}
	[Serializable]
	class DecimalField : ListItemField {
		//constructors
		internal DecimalField(string fieldName) : base(fieldName) { }
		public DecimalField(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public override int CompareTo(ListItemField other) {
			return other is DecimalField ? ((float)this.Value).CompareTo((float)other.Value) : -1;
		}
		internal override object DeserializeValue(SerializationInfo info) {
			return info.GetSingle("value");
		}
	}
	[Serializable]
	class DateField : ListItemField {
		//constructors
		internal DateField(string fieldName) : base(fieldName) { }
		public DateField(SerializationInfo info, StreamingContext context) : base(info, context) { }
		//properties
		internal override object Value {
			get { return base.Value; }
			set {
				if (value is DateTime) {
					base.Value = new XDate((DateTime)value);
				}
				else {
					base.Value = value;
				}
			}
		}
		//methods
		public override int CompareTo(ListItemField other) {
			return other is DateField ? (this.Value as XDate).CompareTo(other.Value as XDate) : -1;
		}
		internal override object DeserializeValue(SerializationInfo info) {
			return info.GetValue("value", typeof(XDate)) as XDate;
		}
	}
	[Serializable]
	class ImageField : ListItemField {
		//members
		[NonSerialized]
		private BitmapImage bImg;
		//constructors
		internal ImageField(string fieldName) : base(fieldName) {
			bImg = null;
		}
		public ImageField(SerializationInfo info, StreamingContext context) : base(info, context) {
			bImg = CreateBitmapImage();
		}
		//properties
		internal override object Value {
			get { return base.Value; }
			set {
				base.Value = value;
				bImg = CreateBitmapImage();
			}
		}
		//methods
		internal BitmapImage GetBitmapImage() {
			return bImg;
		}
		public override int CompareTo(ListItemField other) {
			return 0;
		}
		internal override object DeserializeValue(SerializationInfo info) {
			return info.GetValue("value", typeof(XImage)) as XImage;
		}
		private BitmapImage CreateBitmapImage() {
			XImage xImg = Value as XImage; 
			if (xImg != null && xImg.IsLoaded) {
				BitmapImage bi = xImg.Img.ConvertToBitmapImage();
				bi.Freeze();
                return bi;
			}
			return null;
		}
	}
	[Serializable]
	class EnumField : ListItemField {
		//constructors
		internal EnumField(string fieldName) : base(fieldName) { }
		public EnumField(SerializationInfo info, StreamingContext context) : base(info, context) { }
		//methods
		internal override object StartingValue() {
			return 0;
		}
		internal override object DeserializeValue(SerializationInfo info) {
			return info.GetInt32("value");
		}
		public string GetSelectedValue(object metadata) {
			return (metadata as string[])[(int)this.Value];
		}
		public override int CompareTo(ListItemField other) {
			return other is EnumField ? ((int)this.Value).CompareTo((int)other.Value) : -1;
		}
	}
}
