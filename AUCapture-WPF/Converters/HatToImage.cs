using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AUCapture_WPF.Converters
{
    public class HatToImage : IMultiValueConverter
    {
        public Dictionary<string, int> Hats = new()
        {
            {"0", 0},
            {"1", 1},
            {"2", 1},
            {"3", 1},
            {"4", 0},
            {"5", 1},
            {"6", 0},
            {"7", 1},
            {"8", 1},
            {"9", 1},
            {"10", 1},
            {"11", 1},
            {"12", 1},
            {"13", 1},
            {"14", 1},
            {"15", 0},
            {"16", 1},
            {"17", 1},
            {"18", 1},
            {"19", 1},
            {"20", 1},
            {"21", 1},
            {"22", 1},
            {"23", 1},
            {"24", 1},
            {"25", 1},
            {"26", 1},
            {"27", 1},
            {"28", 1},
            {"29", 0},
            {"30", 1},
            {"31", 1},
            {"32", 1},
            {"33", 1},
            {"34", 1},
            {"35", 1},
            {"36", 1},
            {"37", 1},
            {"38", 1},
            {"39", 0},
            {"40", 1},
            {"41", 1},
            {"42", 0},
            {"43", 1},
            {"44", 1},
            {"45", 1},
            {"46", 1},
            {"47", 1},
            {"48", 1},
            {"49", 1},
            {"50", 1},
            {"51", 1},
            {"52", 1},
            {"53", 1},
            {"54", 1},
            {"55", 1},
            {"56", 1},
            {"57", 1},
            {"58", 1},
            {"59", 1},
            {"60", 1},
            {"61", 1},
            {"62", 1},
            {"63", 1},
            {"64", 1},
            {"65", 1},
            {"66", 1},
            {"67", 1},
            {"68", 1},
            {"69", 1},
            {"70", 1},
            {"71", 1},
            {"72", 1},
            {"73", 1},
            {"74", 1},
            {"75", 0},
            {"76", 1},
            {"77", 1},
            {"78", 1},
            {"79", 1},
            {"80", 1},
            {"81", 1},
            {"82", 1},
            {"83", 1},
            {"84", 1},
            {"85", 0},
            {"86", 1},
            {"87", 1},
            {"88", 1},
            {"89", 1},
            {"90", 1},
            {"91", 1},
            {"92", 1},
            {"93", 1},
            {"94", 1}
        };
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var hatID = values[0] as uint? ?? 0;
            var alive = values[1] as bool? ?? false;
            if (!alive)
            {
                return "";
            }

            
            if (hatID == 0)
            {
                return "";
            }
            var finalName = hatID + "-" + Hats[(hatID%95).ToString()];
            return $"https://cdn.automute.us/Hats/{finalName}.png";


        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class HatToZ : IValueConverter
    {
        public Dictionary<string, int> Hats = new()
        {
            {"0", 0},
            {"1", 1},
            {"2", 1},
            {"3", 1},
            {"4", 0},
            {"5", 1},
            {"6", 0},
            {"7", 1},
            {"8", 1},
            {"9", 1},
            {"10", 1},
            {"11", 1},
            {"12", 1},
            {"13", 1},
            {"14", 1},
            {"15", 0},
            {"16", 1},
            {"17", 1},
            {"18", 1},
            {"19", 1},
            {"20", 1},
            {"21", 1},
            {"22", 1},
            {"23", 1},
            {"24", 1},
            {"25", 1},
            {"26", 1},
            {"27", 1},
            {"28", 1},
            {"29", 0},
            {"30", 1},
            {"31", 1},
            {"32", 1},
            {"33", 1},
            {"34", 1},
            {"35", 1},
            {"36", 1},
            {"37", 1},
            {"38", 1},
            {"39", 0},
            {"40", 1},
            {"41", 1},
            {"42", 0},
            {"43", 1},
            {"44", 1},
            {"45", 1},
            {"46", 1},
            {"47", 1},
            {"48", 1},
            {"49", 1},
            {"50", 1},
            {"51", 1},
            {"52", 1},
            {"53", 1},
            {"54", 1},
            {"55", 1},
            {"56", 1},
            {"57", 1},
            {"58", 1},
            {"59", 1},
            {"60", 1},
            {"61", 1},
            {"62", 1},
            {"63", 1},
            {"64", 1},
            {"65", 1},
            {"66", 1},
            {"67", 1},
            {"68", 1},
            {"69", 1},
            {"70", 1},
            {"71", 1},
            {"72", 1},
            {"73", 1},
            {"74", 1},
            {"75", 0},
            {"76", 1},
            {"77", 1},
            {"78", 1},
            {"79", 1},
            {"80", 1},
            {"81", 1},
            {"82", 1},
            {"83", 1},
            {"84", 1},
            {"85", 0},
            {"86", 1},
            {"87", 1},
            {"88", 1},
            {"89", 1},
            {"90", 1},
            {"91", 1},
            {"92", 1},
            {"93", 1},
            {"94", 1}
        };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hatID = value as uint? ?? 0;
            if (hatID == 0)
            {
                return 0;
            }

            if (Hats[(hatID%94).ToString()] == 1)
            {
                return 1;
            }

            return -1;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
