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
			FieldTemplateItem fti = parameter as FieldTemplateItem;
			return mi[fti.Name].GetVisibleValue(fti.Metadata);
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
			FieldTemplateItem fti = parameter as FieldTemplateItem;
			//ImageField imgF = mi[fti.Name] as ImageField;
			return mi[fti.Name].GetVisibleValue(fti.Metadata);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
