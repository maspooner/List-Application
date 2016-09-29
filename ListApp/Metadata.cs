using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListApp {
	interface IMetadata {
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
		internal NumberMetadata(int min, int max) {
			MinValue = min;
			MaxValue = max;
		}
		public bool Verify(object val) {
			int i = (int)val;
			return i >= MinValue && i <= MaxValue;
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
	}
	[Serializable]
	class ImageMetadata : IMetadata {
		//properties
		internal double MaxHeight { get; set; }
		//constructors
		internal ImageMetadata() {
			MaxHeight = C.DEFAULT_IMAGE_DISPLAY_HEIGHT;
		}
		internal ImageMetadata(double maxHeight) {
			MaxHeight = maxHeight;
		}
		public bool Verify(object val) {
			return true;
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
		public bool Verify(object val) {
			return (int)val < Entries.Length;
		}
	}
}
