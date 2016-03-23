using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;


namespace ListApp {
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
		//methods
		public abstract int CompareTo(ListItemField other);
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
		internal BasicField(string fieldName, string value) : base(fieldName){
			this.value = value;
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
	}
	[Serializable]
	class DateField : ListItemField {
		//members
		private DateTime value;
		//constructors
		internal DateField(string fieldName, DateTime value) : base(fieldName) {
			this.value = value;
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
	}
	[Serializable]
	class ImageField : ListItemField {
		//members
		private Image value;
		//constructors
		internal ImageField(string fieldName, string fileName) : base(fieldName) {
			value = Image.FromFile(fileName); //TODO change file size when caching
		}
		public ImageField(SerializationInfo info, StreamingContext context) : base(info, context) {
			value = info.GetValue("value", typeof(Image)) as Image;
		}
		//methods
		public override int CompareTo(ListItemField other) {
			return 0;
		}
		public override object GetValue() {
			return value;
		}
	}
}
