using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using AmongUsCapture;

namespace AUCapture_WPF.Converters
{
    class PlayerToImage : IMultiValueConverter  
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() == 2)
            {
                var color = values[0] as PlayerColor? ?? PlayerColor.Red;
                var alive = values[1] as bool? ?? false;
                return new BitmapImage(new Uri($"pack://application:,,,/Resources/Players/au{color.ToString().ToLower()}{(alive ? "" : "dead")}.png"));
            } 
            
            return new BitmapImage(new Uri($"pack://application:,,,/Resources/Players/aured.png"));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { };
        }
    }
}
