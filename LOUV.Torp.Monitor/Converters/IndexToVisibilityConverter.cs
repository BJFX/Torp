using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace LOUV.Torp.Monitor.Converters
{
    public class IndexToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var index = (int)value;
            string para = parameter as string;
            switch (index)
            {
                case 0:
                    {
                        if(para== "3D段启动")
                        {
                            return Visibility.Visible;
                        }
                        if (para == "定距定时")
                        {
                            return Visibility.Collapsed;
                        }
                    }
                    break;
                case 3:
                    {
                        if (para == "3D段启动")
                        {
                            return Visibility.Collapsed;
                        }
                        if (para == "定距定时")
                        {
                            return Visibility.Visible;
                        }
                    }
                    break;
                default:
                    return Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
