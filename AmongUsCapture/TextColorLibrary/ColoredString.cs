using System.Drawing;

namespace AmongUsCapture.TextColorLibrary
{
    public class ColoredString
    {
        public ColoredString(string text, Color textColor)
        {
            this.text = text;
            this.textColor = textColor;
        }

        public override string ToString()
        {
            return $"Text: {text} Color: {textColor.ToString()}";
        }

        public string text { get; set; }
        public Color textColor { get; set; }
    }
}