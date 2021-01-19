using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AmongUsCapture;

namespace AUCapture_WPF.Converters
{
    public class PlayerColorToBrush : IValueConverter
    {
        private static readonly SolidColorBrush Red = new SolidColorBrush(Color.FromRgb(197, 17, 17));
        public static SolidColorBrush Blue { get; } = new SolidColorBrush(Color.FromRgb(19, 46, 209));
        private static SolidColorBrush Green = new SolidColorBrush(Color.FromRgb(17, 127, 45));
        private static SolidColorBrush Pink = new SolidColorBrush(Color.FromRgb(237, 84, 186));
        private static SolidColorBrush Orange = new SolidColorBrush(Color.FromRgb(239, 125, 13));
        private static SolidColorBrush Yellow = new SolidColorBrush(Color.FromRgb(245, 245, 87));
        private static SolidColorBrush Black = new SolidColorBrush(Color.FromRgb(63, 71, 78));
        private static SolidColorBrush White = new SolidColorBrush(Color.FromRgb(214, 224, 240));
        private static SolidColorBrush Purple = new SolidColorBrush(Color.FromRgb(107, 47, 187));
        private static SolidColorBrush Brown = new SolidColorBrush(Color.FromRgb(113, 73, 30));
        private static SolidColorBrush Cyan = new SolidColorBrush(Color.FromRgb(56, 254, 220));
        private static SolidColorBrush Lime = new SolidColorBrush(Color.FromRgb(80, 239, 57));
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (PlayerColor) value switch
            {
                PlayerColor.Red => Red,
                PlayerColor.Blue => Blue,
                PlayerColor.Green => Green,
                PlayerColor.Pink => Pink,
                PlayerColor.Orange => Orange,
                PlayerColor.Yellow => Yellow,
                PlayerColor.Black => Black,
                PlayerColor.White => White,
                PlayerColor.Purple => Purple,
                PlayerColor.Brown => Brown,
                PlayerColor.Cyan => Cyan,
                PlayerColor.Lime => Lime,
                _ => new SolidColorBrush(Color.FromRgb(0, 0, 0))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = (SolidColorBrush) value;
            if (brush.Color == Red.Color)
            {
                return PlayerColor.Red;
            }
            else if (brush.Color == Blue.Color)
            {
                return PlayerColor.Blue;
            }
            else if (brush.Color == Green.Color)
            {
                return PlayerColor.Green;
            }
            else if (brush.Color == Pink.Color)
            {
                return PlayerColor.Pink;
            }
            else if (brush.Color == Orange.Color)
            {
                return PlayerColor.Orange;
            }
            else if (brush.Color == Yellow.Color)
            {
                return PlayerColor.Yellow;
            }
            else if (brush.Color == Black.Color)
            {
                return PlayerColor.Black;
            }
            else if (brush.Color == White.Color)
            {
                return PlayerColor.White;
            }
            else if (brush.Color == Purple.Color)
            {
                return PlayerColor.Purple;
            }
            else if (brush.Color == Brown.Color)
            {
                return PlayerColor.Brown;
            }
            else if (brush.Color == Cyan.Color)
            {
                return PlayerColor.Cyan;
            }
            else if (brush.Color == Lime.Color)
            {
                return PlayerColor.Lime;
            }

            return PlayerColor.Red;
        }
    }
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public BoolToVisibilityConverter()
        {
            // set defaults
            FalseValue = Visibility.Hidden;
            TrueValue = Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
