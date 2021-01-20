using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace AUCapture_WPF.Converters
{
    class BrushInverter : IValueConverter
    {
        public static Color Invert (Color color)
        {
            return Color.FromRgb((byte) (255 - color.R), (byte) (255 - color.G), (byte) (255 - color.B));
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                var brushy = (SolidColorBrush) value;
                var inverted = Invert(brushy.Color);
                return new SolidColorBrush(inverted);
            }

            return new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
