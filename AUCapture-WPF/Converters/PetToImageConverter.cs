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

            return new BitmapImage(new Uri($"https://automuteus.nyc3.cdn.digitaloceanspaces.com/Pets/{petID}.png"));

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
