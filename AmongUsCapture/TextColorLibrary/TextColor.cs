using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TextColorLibrary
{
    public static class TextColor
    {
        private static char colorIndicator = '§'; //Represent color codes as §FFFFFF (6 Letters, Hex decode)

        private static string FromColor(Color textColor)
        {
            return FromRGB(textColor.R, textColor.G, textColor.B);
        }

        public static string ToTextColor(this Color tColor)
        {
            return FromColor(tColor);
        }

        private static Color HexToColor(string Hex)
        {
            var newHex = Hex.IndexOf('#') == 0 ? Hex.Substring(1) : Hex;
            var int32 = Convert.ToInt32(newHex, 16);
            return Color.FromArgb(
                (byte) (int32 >> 16 & (int) byte.MaxValue),
                (byte) (int32 >> 8 & (int) byte.MaxValue),
                (byte) (int32 & (int) byte.MaxValue));
        }


        public static string FromRGB(int red, int green, int blue)
        {
            red = Math.Clamp(red, 0, 255);
            green = Math.Clamp(green, 0, 255);
            blue = Math.Clamp(blue, 0, 255);
            return colorIndicator + red.ToString("X2") + green.ToString("X2") + blue.ToString("X2");
        }

        public static string StripColor(string colorString)
        {
            var parts = toParts(colorString);

            return parts.Aggregate("", (current, part) => current + part.text);
        }

        public static List<ColoredString> toParts(string coloredString)
        {
            var outputList = new List<ColoredString>();
            var indexes = new List<int>();
            var counter = 0;
            if (!coloredString.StartsWith(colorIndicator))
            {
                coloredString = FromColor(Color.White) + coloredString; //Start out white
            }

            foreach (var character in coloredString)
            {
                if (character == colorIndicator)
                {
                    indexes.Add(counter);
                }

                counter++;
            }

            indexes.Add(coloredString.Length);
            for (var i = 0; i < indexes.Count - 1; i++)
            {
                var colorCode = coloredString.Substring(indexes[i] + 1, 6);
                var text = coloredString.Substring(indexes[i] + 1 + 6, indexes[i + 1] - (indexes[i] + 7));
                outputList.Add(new ColoredString(text, HexToColor(colorCode)));
            }

            return outputList;
        }
    }
}