using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ListApp {
	class NumberTextBox : TextBox {
		//members
		private int maxValue;
		private int minValue;
		//constructors
		internal NumberTextBox() : this(int.MinValue, int.MaxValue) { }
		internal NumberTextBox(int minValue, int maxValue) {
			this.minValue = minValue;
			this.maxValue = maxValue;
			LostFocus += NumberTextBox_LostFocus;
		}
		//methods
		internal bool IsValid() {
			int val;
			if (int.TryParse(Text, out val)) {
				return val >= minValue && val <= maxValue;
			}
			return false;
		}
		internal int ParseValue() {
			return int.Parse(Text);
		}
		private void NumberTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
			if (!IsValid()) {
				Text = "";
			}
		}
	}
	class DecimalTextBox : TextBox {
		//members
		private int maxDecimals;
		private float minValue;
		private float maxValue;
		//constructors
		internal DecimalTextBox() : this(int.MaxValue) { }
		internal DecimalTextBox(int maxDecimals) : this(maxDecimals, float.MinValue, float.MaxValue) { }
		internal DecimalTextBox(int maxDecimals, float minValue, float maxValue) {
			this.maxDecimals = maxDecimals;
			this.minValue = minValue;
			this.maxValue = maxValue;
			LostFocus += DecimalTextBox_LostFocus;
		}
		//methods
		internal bool IsValid() {
			if (Text.Contains('.')) {
				int decimalPlaces = Text.Substring(Text.IndexOf('.')).Length - 1;
                if (decimalPlaces > maxDecimals) {
					Text = Text.Substring(0, Text.IndexOf('.') + 1 + maxDecimals);
					//TODO test
				}
			}
			float val;
			if(float.TryParse(Text, out val)) {
				return val >= minValue && val <= maxValue;
			}
			return false;
		}
		internal float ParseValue() {
			return float.Parse(Text);
		}
		private void DecimalTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
			if (!IsValid()) {
				Text = "";
			}
		}
	}
}
