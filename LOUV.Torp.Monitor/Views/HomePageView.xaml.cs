﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LOUV.Torp.Monitor.Core;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET.WindowsPresentation;
using GMap.NET;
using LOUV.Torp.Monitor.Events;

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
            UnitCore.Instance.MainMapCfg = null;
            try
            {
                UnitCore.Instance.MainMapCfg = UnitCore.Instance.MonConfigueService.LoadMapCfg();
            }
            catch(Exception ex)
            {
                var errmsg = new ErrorEvent(ex,LogType.Both);
                errmsg.Message = "地图配置错误，请修改Basic.conf后重启程序";
                UnitCore.Instance.EventAggregator.PublishMessage(errmsg);
            }
            // config map

            MainMap.Position = new PointLatLng(UnitCore.Instance.MainMapCfg.CenterLat, 
                UnitCore.Instance.MainMapCfg.CenterLng);
            MainMap.MapName = UnitCore.Instance.MainMapCfg.Title;
            ///TBD
            // map events
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
            MainMap.MouseMove += new System.Windows.Input.MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MainMap_MouseLeftButtonDown);
            MainMap.Loaded += new RoutedEventHandler(MainMap_Loaded);
            MainMap.MouseEnter += new MouseEventHandler(MainMap_MouseEnter);
        }

        private void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        private void MainMap_Loaded(object sender, RoutedEventArgs e)
        {
            MainMap.ZoomAndCenterMarkers(null);
        }

        private void MainMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MainMap_OnMapTypeChanged(MapType type)
        {
            throw new NotImplementedException();
        }

        private void MainMap_OnTileLoadStart()
        {
            throw new NotImplementedException();
        }

        private void MainMap_OnTileLoadComplete(long ElapsedMilliseconds)
        {
            throw new NotImplementedException();
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            Splasher.CloseSplash();
        }
    }
}
