using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AUCapture_WPF.Converters
{
    class PetToImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var petID = values[0] as uint? ?? 0;
            var alive = values[1] as bool? ?? false;
            if (petID == 0)
            {
                return "";
            }
            if (!alive)
            {
                return "";
            }

            return $"https://cdn.automute.us/Pets/{petID}.png";

        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
