using System;
using System.Collections.Generic;
using System.Linq;
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

namespace LOUV.Torp.Monitor.Views
{
    /// <summary>
    /// HomePageView.xaml 的交互逻辑
    /// </summary>
    public partial class HomePageView : Page
    {
        private Offset mapToGpsOffset ;//地图内部坐标转换为GPS坐标
        private Offset GpsTomapOffset;//GPS坐标转换为地图内部坐标
        private PointLatLng currentClick;//当前点击的位置
        List<BuoyMarker> buoyMarkers = new List<BuoyMarker>();
        List<InfoBoard> buoyMarkers = new List<InfoBoard>();
        List<ObjectMarker> buoyMarkers = new List<ObjectMarker>();
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
                var errmsg = new ErrorEvent(ex, LogType.Both);
                errmsg.Message = "地图配置错误，请修改BasicConf.xml后重启程序";
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
            
        }

        private void RefreshMap(MapCfg cfg)
        {
            GpsTomapOffset = cfg.MapOffset;
            mapToGpsOffset = -GpsTomapOffset;
            MainMap.Position = new PointLatLng(cfg.CenterLat,cfg.CenterLng);
            MainMap.Position.Offset(GpsTomapOffset);
            MainMap.MapName = cfg.Title;
            MainMap = Enum.Parse(typeof (MapType), cfg.MapType);
        }

        #region map event
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
             System.Windows.Point p = e.GetPosition(MainMap);
             currentClick = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
             FindClickMarker();
        }

        private void FindClickMarker()
        {

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
            if (MainMap.Zoom == 17)
                czuZoomUp.IsEnabled = false;
        }

        private void czuZoomDown_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int) (MainMap.Zoom + 0.99)) - 1;
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
            center.Offset();
            MainMap.Position = currentMarker.Position;
        }
        #endregion
    }

}
