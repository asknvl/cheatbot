using asknvl;
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
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var status = (DropStatus)value;
            switch (status)
            {
                case DropStatus.stopped:
                    return Brushes.Red;
                case DropStatus.verification:
                    return Brushes.LightGreen;
                case DropStatus.active:
                    return Brushes.Green;
                case DropStatus.revoked:
                    return Brushes.Yellow;
                case DropStatus.banned:
                    return Brushes.Black;
                case DropStatus.removed:
                    return Brushes.LightGray;
                case DropStatus.subscription:
                    return Brushes.Orange;
                default:
                    return Brushes.Transparent;
            }  
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
