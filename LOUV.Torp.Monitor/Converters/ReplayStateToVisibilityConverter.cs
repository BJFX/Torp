using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace LOUV.Torp.Monitor.Converters
{
    public class ReplayStateToVisibilityConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (uint)value;
            string para = parameter as string;
            if (state == 0)
            {
                switch (para)
                {
                    case "继续回放":
                        return Visibility.Collapsed;
                        break;
                    case "暂停回放":
                        return Visibility.Collapsed;
                        break;
                    case "退出回放":
                        return Visibility.Collapsed;
                        break;
                    default:
                        return Visibility.Visible;
                }
            }
            else if (state == 1) //replaying
            {
                switch (para)
                {
                    case "继续回放":
                        return Visibility.Collapsed;
                        break;
                    case "暂停回放":
                        return Visibility.Visible;
                        break;
                    case "退出回放":
                        return Visibility.Visible;
                        break;
                    default:
                        return Visibility.Collapsed;
                }
            }
            else
            {
                switch (para)
                {
                    case "继续回放":
                        return Visibility.Visible;
                        break;
                    case "暂停回放":
                        return Visibility.Collapsed;
                        break;
                    case "退出回放":
                        return Visibility.Visible;
                        break;
                    default:
                        return Visibility.Collapsed;
                }
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
