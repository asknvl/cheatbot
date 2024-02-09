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
    public class GroupStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var status = (GroupStatus)value;
            switch (status)
            {
                case GroupStatus.part:
                    return Brushes.Red;
                case GroupStatus.full:
                    return Brushes.Green;                
                default:
                    return Brushes.Black;
            }  
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
