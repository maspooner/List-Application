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
			FieldTemplateItem iti = parameter as FieldTemplateItem;
			return mi[iti.Name].Value;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ListItemToEnumConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			MItem mi = value as MItem;
			FieldTemplateItem iti = parameter as FieldTemplateItem;
			EnumField ef = mi[iti.Name] as EnumField;
			return ef.GetSelectedValue(iti.Metadata as EnumMetadata);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ListItemToNumberConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			MItem mi = value as MItem;
			FieldTemplateItem iti = parameter as FieldTemplateItem;
			return string.Format("{0:n0}", (int)mi[iti.Name].Value);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ListItemToDecimalConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			MItem mi = value as MItem;
			FieldTemplateItem iti = parameter as FieldTemplateItem;
			return ((float)mi[iti.Name].Value).ToString();
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	class ListItemToImageConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			MItem mi = value as MItem;
			FieldTemplateItem iti = parameter as FieldTemplateItem;
			ImageField imgF = mi[iti.Name] as ImageField;
			return imgF.GetBitmapImage();
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
