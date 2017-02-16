using GMap.NET.WindowsPresentation;
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
using LOUV.Torp.BaseType;
using LOUV.Torp.Monitor.Views;
using System.Windows.Controls.Primitives;

namespace LOUV.Torp.Monitor.Controls.MapCustom
{
    /// <summary>
    /// BuoyMarker.xaml 的交互逻辑
    /// </summary>
    public partial class BuoyMarker
    {
        Popup Popup;
        BuoyTip Tip;
        GMapMarker Marker;
        HomePageView MainWindow;
        public BuoyMarker(HomePageView window, GMapMarker marker, Buoy buoy)
        {
            this.InitializeComponent();

            this.MainWindow = window;
            this.Marker = marker;

            Popup = new Popup();
            Tip = new BuoyTip();

            this.Loaded += new RoutedEventHandler(BuoyMarker_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(BuoyMarker_SizeChanged);
            this.MouseEnter += new MouseEventHandler(MarkerControl_MouseEnter);
            this.MouseLeave += new MouseEventHandler(MarkerControl_MouseLeave);
            this.MouseMove += new MouseEventHandler(BuoyMarker_MouseMove);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(BuoyMarker_MouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(BuoyMarker_MouseLeftButtonDown);

            Popup.Placement = PlacementMode.Mouse;
            {
                Tip.SetBuoy(buoy);
            }
            Popup.Child = Tip;
        }

        private void BuoyMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BuoyMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void BuoyMarker_MouseMove(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BuoyMarker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BuoyMarker_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
