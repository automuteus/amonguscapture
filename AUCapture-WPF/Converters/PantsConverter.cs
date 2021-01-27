using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using AmongUsCapture;

namespace AUCapture_WPF.Converters
{
    class PantsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() == 2)
            {
                var pantID = values[0] as uint? ?? 0;
                var alive = values[1] as bool? ?? false;
                if (pantID == 0)
                {
                    return "";
                }
                return !alive
                    ? ""
                    : $"https://cdn.automute.us/Pants/{pantID}.png";
            } 
            
            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
