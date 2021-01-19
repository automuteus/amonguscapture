using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AUCapture_WPF.Converters
{
    public class HatToImageForeGround : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var hatID = values[0] as uint? ?? 0;
            var alive = values[1] as bool? ?? false;
            if (!alive)
            {
                return new BitmapImage();
            }

            try
            {
                return new BitmapImage(new Uri($"pack://application:,,,/Resources/hats/{hatID}-1.png"));
            }
            catch (Exception e)
            {
                return new BitmapImage();
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class HatToImageBackground : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var hatID = values[0] as uint? ?? 0;
            var alive = values[1] as bool? ?? false;
            if (!alive)
            {
                return new BitmapImage();
            }

            try
            {
                return new BitmapImage(new Uri($"pack://application:,,,/Resources/hats/{hatID}-0.png"));
            }
            catch (Exception e)
            {
                return new BitmapImage();
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
