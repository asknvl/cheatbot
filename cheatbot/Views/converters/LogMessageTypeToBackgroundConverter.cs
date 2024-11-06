using asknvl.logger;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Views.converters
{
    public class LogMessageTypeToBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                LogMessageType.inf => new SolidColorBrush(0x00000000),
                LogMessageType.inf_urgent => new SolidColorBrush(0xFF00FF00),
                LogMessageType.err => new SolidColorBrush(0xFFFF0000),
                LogMessageType.dbg => new SolidColorBrush(0xFFAAAAAA),

                _ => new SolidColorBrush(0xFF000000)
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
