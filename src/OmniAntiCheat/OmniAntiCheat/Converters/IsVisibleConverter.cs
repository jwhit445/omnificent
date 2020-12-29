using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OmniAntiCheat.Converters {
	public class IsVisibleConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			bool val = (bool)value;
			return val ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
