using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListApp {
	interface IMetadata : IRecoverable {
		bool Verify(object val);
	}
	[Serializable]
	class NumberMetadata : IMetadata {
		//properties
		internal int MinValue { get; set; }
		internal int MaxValue { get; set; }
		//constructors
		internal NumberMetadata() {
			//unbounded by default
			MinValue = int.MinValue;
			MaxValue = int.MaxValue;
		}
		internal NumberMetadata(Dictionary<string, string> decoded) {
			MinValue = int.Parse(decoded[nameof(MinValue)]);
			MaxValue = int.Parse(decoded[nameof(MaxValue)]);
		}
		internal NumberMetadata(int min, int max) {
			MinValue = min;
			MaxValue = max;
		}
		public bool Verify(object val) {
			int i = (int)val;
			return i >= MinValue && i <= MaxValue;
		}
		public string ToRecoverable() {
			Dictionary<string, string> rec = new Dictionary<string, string>();
			rec.Add(nameof(MinValue), MinValue.ToString());
			rec.Add(nameof(MaxValue), MaxValue.ToString());
			return Utils.EncodeMultiple(rec);
		}
	}
	[Serializable]
	class DecimalMetadata : IMetadata {
		//properties
		internal int MaxDecimals { get; set; }
		internal float MinValue { get; set; }
		internal float MaxValue { get; set; }
		//constructors
		internal DecimalMetadata() {
			//unbounded by default
			MaxDecimals = int.MaxValue;
			MinValue = float.MinValue;
			MaxValue = float.MaxValue;
		}
		internal DecimalMetadata(Dictionary<string, string> decoded) {
			MaxDecimals =	int.Parse(decoded[nameof(MaxDecimals)]);
			MinValue =		float.Parse(decoded[nameof(MinValue)]);
			MaxValue =		float.Parse(decoded[nameof(MaxValue)]);
		}
		internal DecimalMetadata(int maxDec, float min, float max) {
			//unbounded by default
			MaxDecimals = maxDec;
			MinValue = min;
			MaxValue = max;
		}
		public bool Verify(object val) {
			float f = (float)val;
			return f >= MinValue && f <= MaxValue && CountDecimals(f.ToString()) <= MaxDecimals;
		}
		private int CountDecimals(string s) {
			int iPeriod = s.IndexOf('.');
			return iPeriod == -1 ? 0 : s.Substring(iPeriod).Length - 1;
		}
		public string ToRecoverable() {
			Dictionary<string, string> rec = new Dictionary<string, string>();
			rec.Add(nameof(MaxDecimals), MaxDecimals.ToString());
			rec.Add(nameof(MinValue), MinValue.ToString());
			rec.Add(nameof(MaxValue), MaxValue.ToString());
			return Utils.EncodeMultiple(rec);
		}
	}
	[Serializable]
	class ImageMetadata : IMetadata {
		//properties
		internal double MaxHeight { get; set; }
		//constructors
		internal ImageMetadata() {
			MaxHeight = C.DEFAULT_IMAGE_DISPLAY_HEIGHT;
		}
		internal ImageMetadata(Dictionary<string, string> decoded) {
			MaxHeight = double.Parse(decoded[nameof(MaxHeight)]);
		}
		internal ImageMetadata(double maxHeight) {
			MaxHeight = maxHeight;
		}
		public bool Verify(object val) {
			return true;
		}

		public string ToRecoverable() {
			Dictionary<string, string> rec = new Dictionary<string, string>();
			rec.Add(nameof(MaxHeight), MaxHeight.ToString());
			return Utils.EncodeMultiple(rec);
		}
	}
	[Serializable]
	class EnumMetadata : IMetadata {
		//properties
		internal string[] Entries { get; set; }
		//constructors
		internal EnumMetadata(params string[] entries) {
			Entries = entries;
		}
		internal EnumMetadata(Dictionary<string, string> decoded) {
			Entries = Utils.DecodeSequence(decoded[nameof(Entries)]).ToArray();
		}
		public bool Verify(object val) {
			return (int)val < Entries.Length;
		}
		public string ToRecoverable() {
			Dictionary<string, string> rec = new Dictionary<string, string>();
			rec.Add(nameof(Entries), Utils.EncodeSequence(Entries));
			return Utils.EncodeMultiple(rec);
		}
	}
}
