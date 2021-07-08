using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AmongUsCapture;

namespace AUCapture_WPF.Converters
{
    public class PlayerColorToBrush : IValueConverter
    {
        private static readonly Dictionary<PlayerColor, SolidColorBrush> BrushMapping = new() {
            { PlayerColor.Red,     new SolidColorBrush(Color.FromRgb(197, 17, 17))},
            { PlayerColor.Blue,    new SolidColorBrush(Color.FromRgb(19, 46, 209))},
            { PlayerColor.Green,   new SolidColorBrush(Color.FromRgb(17, 127, 45))},
            { PlayerColor.Pink,    new SolidColorBrush(Color.FromRgb(237, 84, 186))},
            { PlayerColor.Orange,  new SolidColorBrush(Color.FromRgb(239, 125, 13))},
            { PlayerColor.Yellow,  new SolidColorBrush(Color.FromRgb(245, 245, 87))},
            { PlayerColor.Black,   new SolidColorBrush(Color.FromRgb(63, 71, 78))},
            { PlayerColor.White,   new SolidColorBrush(Color.FromRgb(214, 224, 240))},
            { PlayerColor.Purple,  new SolidColorBrush(Color.FromRgb(107, 47, 187))},
            { PlayerColor.Brown,   new SolidColorBrush(Color.FromRgb(113, 73, 30))},
            { PlayerColor.Cyan,    new SolidColorBrush(Color.FromRgb(56, 254, 220))},
            { PlayerColor.Lime,    new SolidColorBrush(Color.FromRgb(80, 239, 57))},
            { PlayerColor.Maroon,  new SolidColorBrush(Color.FromRgb(95, 29, 46))},
            { PlayerColor.Rose,    new SolidColorBrush(Color.FromRgb(236, 192, 211))},
            { PlayerColor.Banana,  new SolidColorBrush(Color.FromRgb(240, 231, 168))},
            { PlayerColor.Gray,    new SolidColorBrush(Color.FromRgb(117, 133, 147))},
            { PlayerColor.Tan,     new SolidColorBrush(Color.FromRgb(145, 136, 119))},
            { PlayerColor.Coral,   new SolidColorBrush(Color.FromRgb(215, 100, 100))}

        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
             var color = value as PlayerColor? ?? PlayerColor.Red;
             return BrushMapping[color];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PlayerColorToBrushShaded : IValueConverter
    {
        public static Color shadeColor(Color inColor, float percent) {
            
            float R = (inColor.R * (100 + percent)) / 100;
            float G = (inColor.G * (100 + percent)) / 100;
            float B = (inColor.B * (100 + percent)) / 100;
            R = R < 255 ? R : 255;
            G = G < 255 ? G : 255;
            B = B < 255 ? B : 255;
            return Color.FromArgb(255, (byte) R, (byte) G, (byte) B);
        }

        private static readonly Dictionary<PlayerColor, SolidColorBrush> BrushMapping = new() {
            { PlayerColor.Red,     new SolidColorBrush(Color.FromRgb(197, 17, 17))},
            { PlayerColor.Blue,    new SolidColorBrush(Color.FromRgb(19, 46, 209))},
            { PlayerColor.Green,   new SolidColorBrush(Color.FromRgb(17, 127, 45))},
            { PlayerColor.Pink,    new SolidColorBrush(Color.FromRgb(237, 84, 186))},
            { PlayerColor.Orange,  new SolidColorBrush(Color.FromRgb(239, 125, 13))},
            { PlayerColor.Yellow,  new SolidColorBrush(Color.FromRgb(245, 245, 87))},
            { PlayerColor.Black,   new SolidColorBrush(Color.FromRgb(63, 71, 78))},
            { PlayerColor.White,   new SolidColorBrush(Color.FromRgb(214, 224, 240))},
            { PlayerColor.Purple,  new SolidColorBrush(Color.FromRgb(107, 47, 187))},
            { PlayerColor.Brown,   new SolidColorBrush(Color.FromRgb(113, 73, 30))},
            { PlayerColor.Cyan,    new SolidColorBrush(Color.FromRgb(56, 254, 220))},
            { PlayerColor.Lime,    new SolidColorBrush(Color.FromRgb(80, 239, 57))},
            { PlayerColor.Maroon,  new SolidColorBrush(Color.FromRgb(95, 29, 46))},
            { PlayerColor.Rose,    new SolidColorBrush(Color.FromRgb(236, 192, 211))},
            { PlayerColor.Banana,  new SolidColorBrush(Color.FromRgb(240, 231, 168))},
            { PlayerColor.Gray,    new SolidColorBrush(Color.FromRgb(117, 133, 147))},
            { PlayerColor.Tan,     new SolidColorBrush(Color.FromRgb(145, 136, 119))},
            { PlayerColor.Coral,   new SolidColorBrush(Color.FromRgb(215, 100, 100))}

        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = value as PlayerColor? ?? PlayerColor.Red;
            var mainColor = BrushMapping[color];
            var shaded = shadeColor(mainColor.Color, -20f);
            return new SolidColorBrush(shaded);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
