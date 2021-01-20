using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AUCapture_WPF.Converters
{
    class PetToImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var petID = values[0] as uint? ?? 0;
            var alive = values[1] as bool? ?? false;
            if (!alive)
            {
                return new BitmapImage();
            }

            try
            {
                return new BitmapImage(new Uri($"pack://application:,,,/Resources/Pets/{petID}.png"));
            }
            catch (Exception e)
            {
                try
                {
                    return new BitmapImage();
                }
                catch (Exception er)
                {
                    return new BitmapImage();
                }
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
