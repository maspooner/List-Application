using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

/// <summary>
/// DOCUMENTATION
/// Update: 9/26/16
/// </summary>
namespace ListApp {
	/// <summary>
	/// Represents a <seealso cref="TextBox"/> that checks for valid number formatting,
	/// and places restrictions on the numbers entered
	/// </summary>
	class NumberTextBox : TextBox {
		//members
		private NumberMetadata meta;
		//constructors
		/// <summary>
		/// Constructs a <seealso cref="NumberTextBox"/> with the given boundries
		/// </summary>
		/// <param name="meta">metadata about min and max values accepted by this box</param>
		internal NumberTextBox(NumberMetadata meta) {
			this.meta = meta;
			//listen for focus lost to check validity
			LostFocus += NumberTextBox_LostFocus;
		}
		//methods
		/// <summary>
		/// Checks if the text is a number and falls in the min and max value range
		/// </summary>
		internal bool IsValid() {
			int val;
			if (int.TryParse(Text, out val)) {
				//val is an int
				//in range?
				return val >= meta.MinValue && val <= meta.MaxValue;
			}
			//not an int
			return false;
		}
		/// <summary>
		/// Gets the text as an int
		/// </summary>
		internal int ParseValue() {
			return int.Parse(Text);
		}
		/// <summary>
		/// On focus lost, validates the text, calling <seealso cref="IsValid"/>
		/// </summary>
		private void NumberTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
			//don't accept text that isn't valid
			if (!IsValid()) {
				Text = "";
			}
		}
	}



	/// <summary>
	/// Represents a <seealso cref="TextBox"/> that checks for valid decimal formatting,
	/// checking for valid ranges and decimal place numbers
	/// </summary>
	class DecimalTextBox : TextBox {
		//members
		private DecimalMetadata meta;
		//constructors
		/// <summary>
		/// Constructs a <seealso cref="DecimalTextBox"/> with the given boundries 
		/// and decimal place allowances
		/// </summary>
		/// <param name="meta">metadata about the min, max, and decimal place number</param>
		internal DecimalTextBox(DecimalMetadata meta) {
			this.meta = meta;
			//listen for focus lost to check validity
			LostFocus += DecimalTextBox_LostFocus;
		}
		//methods
		/// <summary>
		/// Checks if the text is a number and falls in the min and max value range, and has
		/// no more than the max decimal places
		/// </summary>
		internal bool IsValid() {
			//if value contains a decimal
			if (Text.Contains('.')) {
				//count the number of decimal places after the '.'
				int decimalPlaces = Text.Substring(Text.IndexOf('.')).Length - 1;
				//if there are more than there should be
                if (decimalPlaces > meta.MaxDecimals) {
					//remove the extra places
					Text = Text.Substring(0, Text.IndexOf('.') + 1 + meta.MaxDecimals);
				}
			}
			float val;
			if(float.TryParse(Text, out val)) {
				//float
				//make sure it's in range
				return val >= meta.MinValue && val <= meta.MaxValue;
			}
			//not a float
			return false;
		}
		/// <summary>
		/// Gets the text as a float
		/// </summary>
		internal float ParseValue() {
			return float.Parse(Text);
		}
		/// <summary>
		/// On focus lost, validates the text, calling <seealso cref="IsValid"/>
		/// </summary>
		private void DecimalTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
			//don't accept text that isn't valid
			if (!IsValid()) {
				Text = "";
			}
		}
	}



	/// <summary>
	/// Models an <seealso cref="Image"/> that keeps an <seealso cref="XImage"/> 
	/// backup of the displayed <seealso cref="BitmapImage"/>
	/// </summary>
	class BackupImage : System.Windows.Controls.Image {
		//properties
		private XImage backup;
		private ImageMetadata meta;
		//constructor
		/// <summary>
		/// Constructs a new <seealso cref="BackupImage"/> with the given max height data
		/// </summary>
		/// <param name="meta">metadata about the max image height</param>
		internal BackupImage(ImageMetadata meta) {
			this.meta = meta;
			backup = null;
			Height = meta.MaxHeight;
		}
		/// <summary>
		/// Sets the <seealso cref="XImage"/> to display while keeping it stored also
		/// </summary>
		/// <param name="xi">the image to display and store</param>
		internal void SetSourceAndBackup(XImage xi) {
			backup = xi;
			//convert XImage -> BitmapImage to show
			Source = xi == null ? null : xi.Img.ConvertToBitmapImage();
		}
		/// <summary>
		/// Gets the original <seealso cref="XImage"/> from which the 
		/// displayed <seealso cref="BitmapImage"/> is from
		/// </summary>
		internal XImage GetBackup() { return backup; }
	}
}
