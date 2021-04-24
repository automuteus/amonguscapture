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
            { PlayerColor.Lime,     new SolidColorBrush(Color.FromRgb(80, 239, 57))},
            { PlayerColor.Skincolor, new SolidColorBrush(Color.FromRgb(239, 191, 192)) },
            { PlayerColor.Bordeaux, new SolidColorBrush(Color.FromRgb(109, 7, 26)) },
            { PlayerColor.Olive, new SolidColorBrush(Color.FromRgb(154, 140, 61)) },
            { PlayerColor.Turqoise, new SolidColorBrush(Color.FromRgb(22, 132, 176)) },
            { PlayerColor.Mint, new SolidColorBrush(Color.FromRgb(111, 192, 156)) },
            { PlayerColor.Lavender, new SolidColorBrush(Color.FromRgb(173, 126, 201)) },
            { PlayerColor.Nougat, new SolidColorBrush(Color.FromRgb(160, 101, 56)) },
            { PlayerColor.Peach, new SolidColorBrush(Color.FromRgb(255, 164, 119)) },
            { PlayerColor.NeonGreen, new SolidColorBrush(Color.FromRgb(51, 255, 119)) },
            { PlayerColor.HotPink, new SolidColorBrush(Color.FromRgb(255, 51, 102)) },
            { PlayerColor.Gray, new SolidColorBrush(Color.FromRgb(147, 147, 147)) },
            { PlayerColor.Petrol, new SolidColorBrush(Color.FromRgb(0, 99, 105)) }

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
            { PlayerColor.Lime,     new SolidColorBrush(Color.FromRgb(80, 239, 57))},
            { PlayerColor.Skincolor, new SolidColorBrush(Color.FromRgb(182, 119, 114)) },
            { PlayerColor.Bordeaux, new SolidColorBrush(Color.FromRgb(54, 2, 11)) },
            { PlayerColor.Olive, new SolidColorBrush(Color.FromRgb(104, 95, 40)) },
            { PlayerColor.Turqoise, new SolidColorBrush(Color.FromRgb(15, 89, 117)) },
            { PlayerColor.Mint, new SolidColorBrush(Color.FromRgb(65, 148, 111)) },
            { PlayerColor.Lavender, new SolidColorBrush(Color.FromRgb(131, 58, 203)) },
            { PlayerColor.Nougat, new SolidColorBrush(Color.FromRgb(115, 15, 78)) },
            { PlayerColor.Peach, new SolidColorBrush(Color.FromRgb(238, 128, 100)) },
            { PlayerColor.NeonGreen, new SolidColorBrush(Color.FromRgb(0, 234, 77)) },
            { PlayerColor.HotPink, new SolidColorBrush(Color.FromRgb(232, 0, 58)) },
            { PlayerColor.Gray, new SolidColorBrush(Color.FromRgb(120, 120, 120)) },
            { PlayerColor.Petrol, new SolidColorBrush(Color.FromRgb(0, 61, 54)) }

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
