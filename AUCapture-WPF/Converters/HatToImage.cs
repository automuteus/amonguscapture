using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AUCapture_WPF.Converters
{
    public class HatToImage : IMultiValueConverter
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
                try
                {
                    return new BitmapImage(new Uri($"pack://application:,,,/Resources/hats/{hatID}-0.png"));
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
    public class HatToZ : IValueConverter
    {
        public static string[] GetResourceNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new System.Resources.ResourceReader(stream))
                {
                    return reader.Cast<DictionaryEntry>().Select(entry => 
                        (string)entry.Key).ToArray();
                }
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hatID = value as uint? ?? 0;

            if (GetResourceNames().Any(x => x == $"resources/hats/{hatID}-0.png"))
            {
                return -1;
            }
            else
            {
                return 1;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
