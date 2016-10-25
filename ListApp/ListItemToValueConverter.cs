using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ListApp {
	class ListItemToValueConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			MItem mi = value as MItem;
			ConverterData data = parameter as ConverterData;
			return mi[data.Name].Value;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ListItemToEnumConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			MItem mi = value as MItem;
			ConverterData data = parameter as ConverterData;
			return mi[data.Name].ToVisibleValue(data.Template.Metadata);
			//EnumField ef = mi[fti.Name] as EnumField;
			//return ef.GetSelectedValue(fti.Metadata as EnumMetadata);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ListItemToNumberConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			MItem mi = value as MItem;
			ConverterData data = parameter as ConverterData;
			return string.Format("{0:n0}", (int)mi[data.Name].Value);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ListItemToDecimalConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			MItem mi = value as MItem;
			ConverterData data = parameter as ConverterData;
			return ((float)mi[data.Name].Value).ToString();
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ListItemToImageConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			MItem mi = value as MItem;
			ConverterData data = parameter as ConverterData;
			//ImageField imgF = mi[fti.Name] as ImageField;
			return mi[data.Name].ToVisibleValue(data.Template.Metadata);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ConverterData {
		internal string Name { get; set; }
		internal FieldTemplateItem Template { get; set; }
	}
}
