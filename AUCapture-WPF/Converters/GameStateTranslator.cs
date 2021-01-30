using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AmongUsCapture;
using AUCapture_WPF.Properties;
using Gu.Localization;

namespace AUCapture_WPF.Converters
{
    class GameStateTranslator : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() == 2) {
                if (values[0] is null)
                {
                    return string.Empty;
                }
                string EnumString;
                try
                {
                    EnumString = Enum.GetName(values[0].GetType(), values[0]);
                    return TranslationFor("GameState."+EnumString.ToUpper()).Translated;
                }
                catch
                {
                    return string.Empty;
                }
                
            } 
            
            return "";
        }
        public static ITranslation TranslationFor(string key, ErrorHandling errorHandling = ErrorHandling.ReturnErrorInfoPreserveNeutral)
        {
            return Gu.Localization.Translation.GetOrCreate(Resources.ResourceManager, key, errorHandling);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
