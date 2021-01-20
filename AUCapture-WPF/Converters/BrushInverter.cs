using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using AmongUsCapture;

namespace AUCapture_WPF.Converters
{
    class GameStateToVisability: IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameState statey)
            {
                if (statey == GameState.MENU)
                {
                    return System.Windows.Visibility.Hidden;
                }
                else
                {
                    return System.Windows.Visibility.Visible;
                }
            }
            else
            {
                return System.Windows.Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
