using asknvl;
using Avalonia.Data.Converters;
using Avalonia.Media;
using cheatbot.ViewModels.subscribes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Views.converters
{
    public class GroupStatusBackColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var status = (GroupStatus)value;
            switch (status)
            {
                case GroupStatus.part:
                    return Brushes.Transparent;
                case GroupStatus.full:
                    return Brushes.LightGreen;
                case GroupStatus.ignore:
                    return Brushes.Transparent;
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
