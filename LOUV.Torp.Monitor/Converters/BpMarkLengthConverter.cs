using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace LOUV.Torp.Monitor.Converters
{
    public class BpMarkLengthConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var distance = (float)value;
            if (distance != float.NaN)
            {
                if (distance > 100)
                    return 150;
                return 50+Math.Sqrt(10000-Math.Pow(distance-100,2));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
