using System.Net.PeerToPeer.Collaboration;
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
using System.Windows.Media.Effects;

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
        private Buoy _buoy;
        HomePageView MainWindow;
        readonly ScaleTransform scale = new ScaleTransform(1, 1);
        public BuoyMarker(HomePageView window, GMapMarker marker, Buoy buoy)
        {
            this.InitializeComponent();

            this.MainWindow = window;
            this.Marker = marker;

            Popup = new Popup();
            Tip = new BuoyTip();
            RenderTransform = scale;
            this.Loaded += new RoutedEventHandler(BuoyMarker_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(BuoyMarker_SizeChanged);
            this.MouseEnter += new MouseEventHandler(MarkerControl_MouseEnter);
            this.MouseLeave += new MouseEventHandler(MarkerControl_MouseLeave);
            //this.MouseMove += new MouseEventHandler(BuoyMarker_MouseMove);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(BuoyMarker_MouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(BuoyMarker_MouseLeftButtonDown);
            Popup.PlacementTarget = icon;
            Popup.HorizontalOffset = 30;
            Popup.VerticalOffset = -120;
            Popup.Placement = PlacementMode.Relative;
            {
                Tip.SetBuoy(buoy);
            }
            Popup.Child = Tip;
            CanPopUp = true;
            _buoy = buoy;
        }

        public void Refresh(Buoy buoy)
        {
            _buoy = buoy;
            Tip.SetBuoy(_buoy);
        }
        public bool CanPopUp{get;set;}
        private void BuoyMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        private void BuoyMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }
        public DropShadowEffect ShadowEffect;
        private void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Marker.ZIndex -= 10000;
            Cursor = Cursors.Arrow;
            this.Effect = null;

            scale.ScaleY = 1;
            scale.ScaleX = 1;
            if (CanPopUp)
                Popup.IsOpen = false;
        }

        private void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Marker.ZIndex += 10000;
            Cursor = Cursors.Hand;
            this.Effect = ShadowEffect;

            scale.ScaleY = 1.2;
            scale.ScaleX = 1.2;
            if (CanPopUp)
                Popup.IsOpen = true;
        }

        private void BuoyMarker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height);
        }

        private void BuoyMarker_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }
    }
}
