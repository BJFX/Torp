using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LOUV.Torp.BaseType;
using LOUV.Torp.Monitor.Controls.MapCustom;
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
using System.Windows.Threading;
using System.IO;
using LOUV.Torp.MonitorConf;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LOUV.Torp.Monitor.Helpers;

namespace LOUV.Torp.Monitor.Views
{
    /// <summary>
    /// HomePageView.xaml 的交互逻辑
    /// </summary>
    public partial class HomePageView : Page
    {
        private PointLatLng mapToGpsOffset;//地图内部坐标转换为GPS坐标
        private PointLatLng GpsTomapOffset;//GPS坐标转换为地图内部坐标
        private PointLatLng currentClick;//当前点击的位置
        private bool maploaded = false;
        //private bool MouseInMap = false;
        private MapRoute trackRoute;
        public HomePageView()
        {
            InitializeComponent();
            try
            {
                UnitCore.Instance.MainMapCfg = UnitCore.Instance.MonConfigueService.LoadMapCfg();
                
            }
            catch (Exception ex)
            {
                var errmsg = new ErrorEvent(ex, LogType.Both)
                {
                    Message = "地图配置错误，请修改BasicConf.xml后重启程序"
                };
                UnitCore.Instance.EventAggregator.PublishMessage(errmsg);
                return;
            }
            // config map
            RefreshMap(UnitCore.Instance.MainMapCfg);
            // map events
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
            MainMap.MouseMove += new System.Windows.Input.MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MainMap_MouseLeftButtonDown);
            MainMap.Loaded += new RoutedEventHandler(MainMap_Loaded);
            MainMap.MouseEnter += new MouseEventHandler(MainMap_MouseEnter);
            MainMap.GotFocus+=MainMap_GotFocus;
            MainMap.LostFocus+=MainMap_LostFocus;
            var gps = new GpsInfo();
            gps.UTCTime = DateTime.UtcNow;
            var test = new Buoy()
            {
                id = 4,
                gps = gps,
            };
            currentMarker = new GMapMarker(MainMap.Position);
            {
                currentMarker.Shape = new BuoyMarker(this, currentMarker,test);
                currentMarker.Offset = new System.Windows.Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                MainMap.Markers.Add(currentMarker);
            }
            
        }

        

        

        private void RefreshMap(MapCfg cfg)
        {
            GpsTomapOffset = new PointLatLng(cfg.MapOffset.Lat,cfg.MapOffset.Lng);
            mapToGpsOffset = new PointLatLng(-GpsTomapOffset.Lat,GpsTomapOffset.Lng);
            MainMap.Position = new PointLatLng(cfg.CenterLat,cfg.CenterLng);
            MainMap.Position.Offset(GpsTomapOffset.Lat,GpsTomapOffset.Lng);
            MainMap.MapName = cfg.Title;
            MainMap.MapType = (MapType)Enum.Parse(typeof(MapType), cfg.MapType);
        }

        #region map event
        private void MainMap_MouseMove(object sender, MouseEventArgs e)
        {

                System.Windows.Point p = e.GetPosition(MainMap);
                var pt = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
                pt.Offset(mapToGpsOffset);
                MainMap.MousePosition = pt;
                MainMap.InvalidateVisual();
           
        }
        private void BackToCenter(object sender, RoutedEventArgs e)
        {
            Goto(UnitCore.Instance.MainMapCfg.CenterLat, UnitCore.Instance.MainMapCfg.CenterLng);
            MainMap.ZoomAndCenterMarkers(null);
        }
        private void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        private void MainMap_Loaded(object sender, RoutedEventArgs e)
        {
            if (!maploaded)
            {
                maploaded=MainMap.ZoomAndCenterMarkers(null);
            }
        }

        private void MainMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
             System.Windows.Point p = e.GetPosition(MainMap);
             currentClick = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
             FindClickMarker();
        }

        private void FindClickMarker()
        {

        }


        private void MainMap_OnMapTypeChanged(MapType type)
        {
            sliderZoom.Minimum = MainMap.MinZoom;
            sliderZoom.Maximum = MainMap.MaxZoom;
        }

        private void MainMap_OnTileLoadStart()
        {
            System.Windows.Forms.MethodInvoker m = delegate()
            {
                MapLoadBar.Visibility = Visibility.Visible;
            };

            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch
            {
            }
        }

        private void MainMap_OnTileLoadComplete(long ElapsedMilliseconds)
        {
            
            System.Windows.Forms.MethodInvoker m = delegate()
            {
                MapLoadBar.Visibility = Visibility.Hidden;
            };

            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch
            {
            }
        }

        private void czuZoomUp_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int) MainMap.Zoom) + 1;
            czuZoomDown.IsEnabled = true;
            if (MainMap.Zoom == sliderZoom.Maximum)
                czuZoomUp.IsEnabled = false;
        }

        private void czuZoomDown_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int) (MainMap.Zoom + 0.99)) - 1;
            czuZoomUp.IsEnabled = true;
            if (MainMap.Zoom == sliderZoom.Minimum)
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
            ZoomSlide.Opacity = 1;
        }

        private void MainMap_LostFocus(object sender, RoutedEventArgs e)
        {
            ZoomSlide.Opacity = 0.3;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            Splasher.CloseSplash();
        }
        #endregion

        #region method
        //将当前map保存成图片
        private void SaveCurrentMapView(string filename)
        {
            try
            {
                ImageSource img = MainMap.ToImageSource();
                PngBitmapEncoder en = new PngBitmapEncoder();
                en.Frames.Add(BitmapFrame.Create(img as BitmapSource));

                // Save document


                using (System.IO.Stream st = System.IO.File.OpenWrite(filename))
                {
                    en.Save(st);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Goto(double lat, double lng)
        {
            var center = new PointLatLng(lat, lng);
            center.Offset(GpsTomapOffset.Lat, GpsTomapOffset.Lng);
            MainMap.Position = center;
        }
        
        #endregion

        public GMapMarker currentMarker { get; set; }


        
    }

}
