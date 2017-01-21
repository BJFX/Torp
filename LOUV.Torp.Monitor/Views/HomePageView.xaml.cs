using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET.WindowsPresentation;

namespace LOUV.Torp.Monitor.Views
{
    /// <summary>
    /// HomePageView.xaml 的交互逻辑
    /// </summary>
    public partial class HomePageView : Page
    {
        public HomePageView()
        {
            InitializeComponent();
        }

        private void czuZoomUp_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int)MainMap.Zoom) + 1;
            czuZoomDown.IsEnabled = true;
            if(MainMap.Zoom==17)
                czuZoomUp.IsEnabled = false;
        }

        private void czuZoomDown_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int)(MainMap.Zoom + 0.99)) - 1;
            czuZoomUp.IsEnabled = true;
            if (MainMap.Zoom == 1)
                czuZoomDown.IsEnabled = false;
        }

        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void HomePageView_OnKeyUp(object sender, KeyEventArgs e)
        {
            int offset = 22;

            if (MainMap.IsFocused)
            {
                if (e.Key == Key.Left)
                {
                    MainMap.Offset(-offset, 0);
                }
                else if (e.Key == Key.Right)
                {
                    MainMap.Offset(offset, 0);
                }
                else if (e.Key == Key.Up)
                {
                    MainMap.Offset(0, -offset);
                }
                else if (e.Key == Key.Down)
                {
                    MainMap.Offset(0, offset);
                }
                else if (e.Key == Key.Add)
                {
                    czuZoomUp_Click(null, null);
                }
                else if (e.Key == Key.Subtract)
                {
                    czuZoomDown_Click(null, null);
                }
            }
        }

        private void HomePageView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageUp)
            {
                MainMap.Bearing--;
            }
            else if (e.Key == Key.PageDown)
            {
                MainMap.Bearing++;
            }
        }

        private void MainMap_GotFocus(object sender, RoutedEventArgs e)
        {
            ZoomSlide.Opacity = 0.9;
        }

        private void MainMap_LostFocus(object sender, RoutedEventArgs e)
        {
            ZoomSlide.Opacity = 0.2;
        }
    }
}
