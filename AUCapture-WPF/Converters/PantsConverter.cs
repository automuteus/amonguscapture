using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using AmongUsCapture;

namespace AUCapture_WPF.Converters
{
    class PantsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pantID = value as uint? ?? 0;
            return new BitmapImage(new Uri($"pack://application:,,,/Resources/Pants/{pantID}.png"));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(true.ToString()) ?? false;
        }
    }
}
