using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AmongUsCapture;
using MahApps.Metro.Controls;

namespace AUCapture_WPF.Converters
{
    public class PlayerColorToBrush : IValueConverter {
        public static Dictionary<PlayerColor, Brush> BrushMapping = new() {
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
            { PlayerColor.Watermelon,     new SolidColorBrush(Color.FromRgb(168, 50, 62))},
            { PlayerColor.Chocolate,     new SolidColorBrush(Color.FromRgb(60, 48, 44))},
            { PlayerColor.SkyBlue,     new SolidColorBrush(Color.FromRgb(61, 129, 255))},
            { PlayerColor.Beige,     new SolidColorBrush(Color.FromRgb(240, 211, 165))},
            { PlayerColor.HotPink,     new SolidColorBrush(Color.FromRgb(236, 61, 255))},
            { PlayerColor.Turquoise,     new SolidColorBrush(Color.FromRgb(61, 255, 181))},
            { PlayerColor.Lilac,     new SolidColorBrush(Color.FromRgb(186, 161, 255))},
            { PlayerColor.Unknown, null},
            { PlayerColor.Rainbow, null}

        };

        public static SolidColorBrush RainColorBrush;
        public static SolidColorBrush RainColorBrushShaded;

        static PlayerColorToBrush() {
            var rainbowColor = new SolidColorBrush(Colors.Red);
            var rainbowColorShaded = new SolidColorBrush(Colors.Red);
            var normalAnimation = GenerateAnimation(false);
            var ShadedAnimation = GenerateAnimation(true);
            rainbowColor.BeginAnimation(SolidColorBrush.ColorProperty, normalAnimation);
            rainbowColorShaded.BeginAnimation(SolidColorBrush.ColorProperty, ShadedAnimation);
            RainColorBrush = rainbowColor;
            RainColorBrushShaded = rainbowColorShaded;
            BrushMapping[PlayerColor.Rainbow] = rainbowColor;
            var unknownBrush =  new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Resources/missingtex.png")));
            unknownBrush.TileMode = TileMode.Tile;
            unknownBrush.ViewportUnits = BrushMappingMode.Absolute;
            unknownBrush.Viewport = new Rect(0, 0, 25, 25);
            BrushMapping[PlayerColor.Unknown] = unknownBrush;
            

        }

        public static ColorAnimationUsingKeyFrames GenerateAnimation(bool shaded) {
            ColorAnimationUsingKeyFrames myAnimation = new ColorAnimationUsingKeyFrames();
            myAnimation.KeyFrames = new ColorKeyFrameCollection();
            myAnimation.KeyFrames.Add(new RainbowColorKeyFrame(Colors.Red, KeyTime.Uniform, shaded));
            myAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(3000));
            myAnimation.RepeatBehavior = RepeatBehavior.Forever;
            return myAnimation;
        }

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

        private static Dictionary<PlayerColor, SolidColorBrush> BrushMapping = new() {
            { PlayerColor.Red,         new SolidColorBrush(Color.FromRgb(197, 17, 17))},
            { PlayerColor.Blue,        new SolidColorBrush(Color.FromRgb(19, 46, 209))},
            { PlayerColor.Green,       new SolidColorBrush(Color.FromRgb(17, 127, 45))},
            { PlayerColor.Pink,        new SolidColorBrush(Color.FromRgb(237, 84, 186))},
            { PlayerColor.Orange,      new SolidColorBrush(Color.FromRgb(239, 125, 13))},
            { PlayerColor.Yellow,      new SolidColorBrush(Color.FromRgb(245, 245, 87))},
            { PlayerColor.Black,       new SolidColorBrush(Color.FromRgb(63, 71, 78))},
            { PlayerColor.White,       new SolidColorBrush(Color.FromRgb(214, 224, 240))},
            { PlayerColor.Purple,      new SolidColorBrush(Color.FromRgb(107, 47, 187))},
            { PlayerColor.Brown,       new SolidColorBrush(Color.FromRgb(113, 73, 30))},
            { PlayerColor.Cyan,        new SolidColorBrush(Color.FromRgb(56, 254, 220))},
            { PlayerColor.Lime,        new SolidColorBrush(Color.FromRgb(80, 239, 57))},
            { PlayerColor.Watermelon,  new SolidColorBrush(Color.FromRgb(168, 50, 62))},
            { PlayerColor.Chocolate,   new SolidColorBrush(Color.FromRgb(60, 48, 44))},
            { PlayerColor.SkyBlue,     new SolidColorBrush(Color.FromRgb(61, 129, 255))},
            { PlayerColor.Beige,       new SolidColorBrush(Color.FromRgb(240, 211, 165))},
            { PlayerColor.HotPink,     new SolidColorBrush(Color.FromRgb(236, 61, 255))},
            { PlayerColor.Turquoise,   new SolidColorBrush(Color.FromRgb(61, 255, 181))},
            { PlayerColor.Lilac,       new SolidColorBrush(Color.FromRgb(186, 161, 255))},

        };

        static PlayerColorToBrushShaded() {
            if (PlayerColorToBrush.BrushMapping[PlayerColor.Unknown] is null || PlayerColorToBrush.BrushMapping[PlayerColor.Rainbow] is null) {
                var temp1 = new PlayerColorToBrush();
            }

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = value as PlayerColor? ?? PlayerColor.Red;
            if (color == PlayerColor.Rainbow) {
                return PlayerColorToBrush.RainColorBrushShaded;
            }
            else if (color == PlayerColor.Unknown) {
                return PlayerColorToBrush.BrushMapping[PlayerColor.Unknown];
            }
            var mainColor = BrushMapping[color];
            var shaded = shadeColor(mainColor.Color, -20f);
            return new SolidColorBrush(shaded);
        }
        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class RainbowColorKeyFrame : ColorKeyFrame
    {
        #region Constructors

        /// <summary>
        /// Creates a new LinearColorKeyFrame.
        /// </summary>
        public RainbowColorKeyFrame(bool shaded) {
            Shaded = shaded;
        }

        /// <summary>
        /// Creates a new LinearColorKeyFrame.
        /// </summary>
        public RainbowColorKeyFrame(Color value)
            : base(value)
        {
        }

        public bool Shaded = false;
        /// <summary>
        /// Creates a new LinearColorKeyFrame.
        /// </summary>
        public RainbowColorKeyFrame(Color value, KeyTime keyTime, bool shaded)
            : base(value, keyTime) {
            Shaded = shaded;
        }

        #endregion

        #region Freezable

        /// <summary>
        /// Implementation of <see cref="System.Windows.Freezable.CreateInstanceCore">Freezable.CreateInstanceCore</see>.
        /// </summary>
        /// <returns>The new Freezable.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new RainbowColorKeyFrame(Shaded);
        }

        #endregion

        #region ColorKeyFrame

        public static Color Rainbow(double progress)
        {
            double div = (Math.Abs(progress % 1) * 6);
            byte ascending = (byte) ((div % 1) * 255);
            byte descending = (byte) (255 - @ascending);

            switch ((int) div)
            {
                case 0:
                    return Color.FromArgb(255, 255, @ascending, 0);
                case 1:
                    return Color.FromArgb(255, @descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, @ascending);
                case 3:
                    return Color.FromArgb(255, 0, @descending, 255);
                case 4:
                    return Color.FromArgb(255, @ascending, 0, 255);
                default: // case 5:
                    return Color.FromArgb(255, 255, 0, @descending);
            }
        }

        /// <summary>
        /// Implemented to linearly interpolate between the baseValue and the
        /// Value of this KeyFrame using the keyFrameProgress.
        /// </summary>
        protected override Color InterpolateValueCore(Color baseValue, double keyFrameProgress) {
            var color = Rainbow(keyFrameProgress);
            if (Shaded)
            {
                color = PlayerColorToBrushShaded.shadeColor(color, -20f);
            }

            return color;
        }

        #endregion
    }
}
