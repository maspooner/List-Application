using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

/// <summary>
/// DOCUMENTATION
/// Update: 10/1/16
/// </summary>
namespace ListApp {
	/// <summary>
	/// Models the type of data an <seealso cref="MField"/>
	/// can hold
	/// </summary>
	enum FieldType {
		BASIC, DATE, IMAGE, ENUM, NUMBER, DECIMAL
	}


	/// <summary>
	/// Models a type of field that can be added to an <seealso cref="MItem"/>
	/// that has an <seealso cref="IComparable{MField}"/> value an a specified <seealso cref="FieldType"/>
	/// </summary>
	[Serializable]
	class MField : IComparable<MField>, ISerializable, IRecoverable {
		//members
		internal virtual IComparable Value { get; set; }
		internal FieldType FieldType { get; private set; }
		//constructors
		/// <summary>
		/// Constructs an <seealso cref="MField"/> with a 
		/// specified <seealso cref="FieldType"/>. The value
		/// is initialized to a starting value
		/// </summary>
		/// <param name="fieldType">the type of field this field is</param>
		internal MField(FieldType fieldType) {
			FieldType = fieldType;
			Value = StartingValue(); //starting value depends on field type
		}
		internal MField(FieldType fieldType, Dictionary<string, string> decoded) {
			//Parse the field type by its string name
			FieldType = fieldType;
			Value = ParseValue(decoded[nameof(Value)]);
		}
		/// <summary>
		/// Special constructor for building an <seealso cref="MField"/>
		/// by deserializing a stream of object data
		/// </summary>
		public MField(SerializationInfo info, StreamingContext context) {
			FieldType = (FieldType)info.GetValue("fieldType", typeof(FieldType));
			//deserialize the value into the right type
			Value = (IComparable)info.GetValue("value", GetValueType());
		}
		//methods
		/// <summary>
		/// Compares the value of this field to another
		/// </summary>
		/// <param name="other">the other field to compare to</param>
		/// <returns>and int representing the relative position of this field</returns>
		public int CompareTo(MField other) {
			return this.Value.CompareTo(other.Value);
		}
		/// <summary>
		/// Prepares the field for serializaiton by adding its value and
		/// <seealso cref="FieldType"/> to a specified <seealso cref="SerializationInfo"/>
		/// </summary>
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("value", Value);
			info.AddValue("fieldType", FieldType);
		}
		/// <summary>
		/// Finds a correct starting value for the field type of this <seealso cref="MField"/>
		/// </summary>
		/// <returns>the starting value of this field</returns>
		internal virtual IComparable StartingValue() {
			switch (FieldType) {
				case FieldType.BASIC:	return null;
				case FieldType.DATE:	return null;
				case FieldType.DECIMAL: return 0f;
				case FieldType.ENUM:	return 0;
				case FieldType.IMAGE:	return null;
				case FieldType.NUMBER:	return 0;
				default:				throw new NotImplementedException();
			}
		}
		/// <summary>
		/// Returns type information as to what <seealso cref="Type"/>
		/// this field should hold based on its <seealso cref="FieldType"/>
		/// </summary>
		/// <returns>the type of this kind of field</returns>
		private Type GetValueType() {
			switch (FieldType) {
				case FieldType.BASIC:	return typeof(string);
				case FieldType.DATE:	return typeof(XDate);
				case FieldType.DECIMAL: return typeof(float);
				case FieldType.ENUM:	return typeof(int);
				case FieldType.IMAGE:	return typeof(XImage);
				case FieldType.NUMBER:	return typeof(int);
				default:				throw new NotImplementedException();
			}
		}
		private IComparable ParseValue(string val) {
			switch (FieldType) {
				case FieldType.BASIC:	return val;
				case FieldType.DATE:	return new XDate(Utils.DecodeMultiple(val));
				case FieldType.DECIMAL: return float.Parse(val);
				case FieldType.ENUM:	return int.Parse(val);
				case FieldType.IMAGE:	return new XImage(Utils.DecodeMultiple(val));
				case FieldType.NUMBER:	return int.Parse(val);
				default:				throw new NotImplementedException();
			}
		}
		/// <summary>
		/// Gets the representation of this field's value in a visible
		/// form, which for regular <seealso cref="MField"/>s means
		/// just it's value
		/// </summary>
		/// <param name="metadata">metadata for the rendering of the 
		/// visible form</param>
		/// <returns>the visible form of the value</returns>
		internal virtual object ToVisibleValue(IMetadata metadata) {
			return Value;
		}
		/// <summary>
		/// Gets the representation of this field's value in a writable
		/// to file form, which for regular <seealso cref="MField"/>s means
		/// just it's value
		/// </summary>
		/// <param name="metadata">metadata for converting to the 
		/// writable form</param>
		/// <returns>the writable form of this field</returns>
		internal virtual string ToReadable(IMetadata metadata) {
			return GetRecoverableValue();
		}
		public string ToRecoverable() {
			Dictionary<string, string> rec = new Dictionary<string, string>();
			rec.Add(nameof(FieldType), ((int)FieldType).ToString());
			rec.Add(nameof(Value), GetRecoverableValue());
			return Utils.EncodeMultiple(rec);
		}
		protected virtual string GetRecoverableValue() {
			return Value == null ? null : (Value is IRecoverable) ? 
				(Value as IRecoverable).ToRecoverable() : Value.ToString();
		}
	}




	[Serializable]
	class ImageField : MField {
		//members
		[NonSerialized]
		private BitmapImage bImg; //holds the visible representation of an XImage
		/// <summary>
		/// Gets the <seealso cref="XImage"/> value,
		/// but when set, sets the <seealso cref="XImage"/> and 
		/// creates a <seealso cref="BitmapImage"/> to use for rendering the UI
		/// </summary>
		internal override IComparable Value {
			get { return base.Value; }
			set {
				base.Value = value;
				//create a bitmap image representation of the XImage
				bImg = CreateBitmapImage();
			}
		}
		//constructors
		/// <summary>
		/// Constructs an <seealso cref="ImageField"/> with the IMAGE <seealso cref="FieldType"/>
		/// with the visible <seealso cref="BitmapImage"/> set to null
		/// </summary>
		internal ImageField() : base(FieldType.IMAGE) {
			bImg = null;
		}
		internal ImageField(Dictionary<string, string> decoded) : base(FieldType.IMAGE, decoded) {
			bImg = CreateBitmapImage();
		}
		public ImageField(SerializationInfo info, StreamingContext context) : base(info, context) {
			bImg = CreateBitmapImage();
		}
		//methods
		/// <summary>
		/// The visible value for an <seealso cref="XImage"/>
		/// is a <seealso cref="BitmapImage"/>
		/// </summary>
		internal override object ToVisibleValue(IMetadata metadata) {
			return bImg;
		}
		/// <summary>
		/// Creates a <seealso cref="BitmapImage"/> representation
		/// of this field's <seealso cref="XImage"/> value
		/// </summary>
		private BitmapImage CreateBitmapImage() {
			XImage xImg = Value as XImage;
			//if there is an XImage to show and it's loaded
			if (xImg != null && xImg.IsLoaded) {
				//convert to a bitmap image
				BitmapImage bi = xImg.Img.ConvertToBitmapImage();
				//make it unmodifyable
				bi.Freeze();
                return bi;
			}
			//no valid image to show
			return null;
		}
		internal override string ToReadable(IMetadata metadata) {
			return Value == null ? "" : (Value as XImage).ToReadable();
		}
		protected override string GetRecoverableValue() {
			return Value == null ? null : (Value as XImage).ToRecoverable();
		}
	}




	/// <summary>
	/// Models an <seealso cref="MField"/> that contains information
	/// about an enumeration
	/// </summary>
	[Serializable]
	class EnumField : MField {
		//constructors
		/// <summary>
		/// Constructs an <seealso cref="EnumField"/> with the <seealso cref="FieldType"/>
		/// of ENUM
		/// </summary>
		internal EnumField() : base(FieldType.ENUM) { }
		internal EnumField(Dictionary<string, string> decoded) : base(FieldType.ENUM, decoded) { }
		/// <summary>
		/// Constructs an <seealso cref="EnumField"/> from a serialization context
		/// </summary>
		public EnumField(SerializationInfo info, StreamingContext context) : base(info, context) { }
		//methods
		/// <summary>
		/// Gets this value (int) represented as a string, using the
		/// <seealso cref="EnumMetadata"/> provided
		/// </summary>
		/// <param name="metadata">the enum metadata containing the list of entries</param>
		/// <returns>a string representation of the value</returns>
		internal override object ToVisibleValue(IMetadata metadata) {
			return (metadata as EnumMetadata).Entries[(int)Value];
		}
		/// <summary>
		/// Gets this value (int) represented as a string, using the
		/// <seealso cref="EnumMetadata"/> provided
		/// </summary>
		/// <param name="metadata">the enum metadata containing the list of entries</param>
		internal override string ToReadable(IMetadata metadata) {
			return ToVisibleValue(metadata).ToString();
		}
	}



	/// <summary>
	/// Models an <seealso cref="MField"/> that contains information
	/// about an enumeration
	/// </summary>
	[Serializable]
	class DateField : MField {
		//constructors
		internal DateField() : base(FieldType.DATE) { }
		internal DateField(Dictionary<string, string> decoded) : base(FieldType.DATE, decoded) { }
		public DateField(SerializationInfo info, StreamingContext context) : base(info, context) { }
		//methods
		internal override object ToVisibleValue(IMetadata metadata) {
			return ToReadable(metadata);
		}
		protected override string GetRecoverableValue() {
			return Value == null ? null : (Value as XDate).ToRecoverable();
		}
		internal override string ToReadable(IMetadata metadata) {
			return Value == null ? "" : (Value as XDate).ToReadable();
		}
	}
}
