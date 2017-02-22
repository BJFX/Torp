using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET.WindowsPresentation;
using LOUV.Torp.BaseType;
using LOUV.Torp.Monitor.Views;

namespace LOUV.Torp.Monitor.Controls.MapCustom
{
    /// <summary>
    /// ObjectMarker.xaml 的交互逻辑
    /// </summary>
    public partial class ObjectMarker
    {
        Popup Popup;
        BuoyTip Tip;
        GMapMarker Marker;
        private Target _target;
        HomePageView MainWindow;
        readonly ScaleTransform scale = new ScaleTransform(1, 1);
        public ObjectMarker(HomePageView window, GMapMarker marker, Target target)
        {
            this.InitializeComponent();

            this.MainWindow = window;
            this.Marker = marker;

            Popup = new Popup();
            Tip = new BuoyTip();
            RenderTransform = scale;
            this.Loaded += new RoutedEventHandler(ObjectMarker_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(ObjectMarker_SizeChanged);
            this.MouseEnter += new MouseEventHandler(ObjectMarker_MouseEnter);
            this.MouseLeave += new MouseEventHandler(ObjectMarker_MouseLeave);

            this.MouseLeftButtonUp += new MouseButtonEventHandler(ObjectMarker_MouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ObjectMarker_MouseLeftButtonDown);
            Popup.PlacementTarget = icon;
            Popup.HorizontalOffset = 30;
            Popup.VerticalOffset = -120;
            Popup.Placement = PlacementMode.Relative;
            {
                Tip.SetTarget(target);
            }
            Popup.Child = Tip;
            CanPopUp = true;
            _target = target;
        }

        public void Refresh(Target target)
        {
            _target = target;
            Tip.SetTarget(_target);
        }
        public bool CanPopUp{get;set;}
        private void ObjectMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        private void ObjectMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }
        public DropShadowEffect ShadowEffect;
        private void ObjectMarker_MouseLeave(object sender, MouseEventArgs e)
        {
            Marker.ZIndex -= 10000;
            Cursor = Cursors.Arrow;
            this.Effect = null;

            scale.ScaleY = 1;
            scale.ScaleX = 1;
            if (CanPopUp)
                Popup.IsOpen = false;
        }

        private void ObjectMarker_MouseEnter(object sender, MouseEventArgs e)
        {
            Marker.ZIndex += 10000;
            Cursor = Cursors.Hand;
            this.Effect = ShadowEffect;

            scale.ScaleY = 1.2;
            scale.ScaleX = 1.2;
            if (CanPopUp)
                Popup.IsOpen = true;
        }

        private void ObjectMarker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height);
        }

        private void ObjectMarker_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }
    }
}
