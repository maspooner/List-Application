using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListApp {
	[Serializable]
	class NumberMetadata {
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
	}
	[Serializable]
	class DecimalMetadata {
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
	}
	[Serializable]
	class ImageMetadata {
		//properties
		internal double MaxHeight { get; set; }
		//constructors
		internal ImageMetadata() {
			MaxHeight = 50.0; //TODO constantize
		}
		internal ImageMetadata(double maxHeight) {
			MaxHeight = maxHeight;
		}
	}
	[Serializable]
	class EnumMetadata {
		//properties
		internal string[] Entries { get; set; }
		//constructors
		internal EnumMetadata(params string[] entries) {
			Entries = entries;
		}
	}
}
