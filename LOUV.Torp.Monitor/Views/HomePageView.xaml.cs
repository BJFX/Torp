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
using System.Threading.Tasks;

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
        //private MapRoute trackRoute;
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
            UnitCore.Instance.mainMap = MainMap;
            //add buoy marker to 2D/3D map
            AddBuoyToMap();
            // map events
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
            MainMap.MouseMove += new System.Windows.Input.MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MainMap_MouseLeftButtonDown);
            MainMap.Loaded += new RoutedEventHandler(MainMap_Loaded);
            MainMap.MouseEnter += new MouseEventHandler(MainMap_MouseEnter);
            MainMap.MouseLeave += MainMap_MouseLeave;
            MainMap.GotFocus += MainMap_GotFocus;
            MainMap.LostFocus += MainMap_LostFocus;
        }

        private void AddBuoyToMap()
        {
            UnitCore.Instance.BuoyLock.WaitOne();
            //2D
            var it = UnitCore.Instance.Buoy.Values.GetEnumerator();
            while(it.MoveNext())
            {
                var buoy = (Buoy)it.Current;
                var marker = new GMapMarker(new PointLatLng(buoy.gps.Latitude,buoy.gps.Longitude));
                {
                    marker.Shape = new BuoyMarker(this, marker, buoy);
                    marker.Offset = new Point(-15, -15);
                    marker.ZIndex = int.MaxValue;
                    marker.Tag = buoy.Id;
                    MainMap.Markers.Add(marker);
                }
                marker.Position.Offset(GpsTomapOffset);
            }
            var targetmarker = new GMapMarker(new PointLatLng(0, 0));//default(0,0)
            {
                targetmarker.Shape = new ObjectMarker(this, targetmarker, UnitCore.Instance.TargetObj);
                targetmarker.Offset = new Point(-15, -15);
                targetmarker.ZIndex = int.MaxValue;
                targetmarker.Tag = 901;
                MainMap.Markers.Add(targetmarker);
            }
            //targetmarker.Position.Offset(GpsTomapOffset);
            UnitCore.Instance.BuoyLock.ReleaseMutex();
            //3D TBD
        }

        void MainMap_MouseLeave(object sender, MouseEventArgs e)
        {
            ZoomSlide.Opacity = 0.3;
        }

        private void RefreshMap(MapCfg cfg)
        {
            GpsTomapOffset = new PointLatLng(cfg.MapOffset.Lat,cfg.MapOffset.Lng);
            mapToGpsOffset = new PointLatLng(-GpsTomapOffset.Lat,GpsTomapOffset.Lng);
            MainMap.Position = new PointLatLng(cfg.CenterLat,cfg.CenterLng);
            MainMap.Position.Offset(GpsTomapOffset.Lat,GpsTomapOffset.Lng);
            MainMap.MapName = cfg.Title;
            Mapnamebox.Text = cfg.Title;
            CenterLngBox.Text = cfg.CenterLng.ToString();
            CenterLatBox.Text = cfg.CenterLat.ToString();
            OffsetLngBox.Text = cfg.MapOffset.Lng.ToString();
            OffsetLatBox.Text = cfg.MapOffset.Lat.ToString();
            MainMap.Manager.Mode = (AccessMode)Enum.Parse(typeof(AccessMode), cfg.AccessMode);
            MainMap.MapType = (MapType)Enum.Parse(typeof(MapType), cfg.MapType);
            MapTypeBox.ItemsSource = Enum.GetNames(typeof(MapType));
            MapTypeBox.SelectedItem = cfg.MapType;
            CacheMode.ItemsSource = Enum.GetNames(typeof(AccessMode));
            CacheMode.SelectedItem = cfg.AccessMode;
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
            ZoomSlide.Opacity = 1;
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Task<bool> ret = null;
            ret = UnitCore.Instance.LoadAssets();
            await ret;
            UnitCore.Instance.ThreeDEnable = ret.Result;
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

        private void ShowBuoyInfo_IsCheckedChanged(object sender, EventArgs e)
        {

                var itor = UnitCore.Instance.mainMap.Markers.GetEnumerator();
                while (itor.MoveNext())
                {
                    var marker = itor.Current;

                    if ((int)marker.Tag < 900)
                    {
                        if (marker.Shape is BuoyMarker buoy)
                        {
                            buoy.ShowTip(ShowBuoyInfo.IsChecked==true);
                        }
                        break;
                    }
                }

        }

        private void CancelCfg_Click(object sender, RoutedEventArgs e)
        {
            var cfg = UnitCore.Instance.MainMapCfg;
            Mapnamebox.Text = cfg.Title;
            CenterLngBox.Text = cfg.CenterLng.ToString();
            CenterLatBox.Text = cfg.CenterLat.ToString();
            OffsetLngBox.Text = cfg.MapOffset.Lng.ToString();
            OffsetLatBox.Text = cfg.MapOffset.Lat.ToString();
            MapTypeBox.ItemsSource = Enum.GetNames(typeof(MapType));
            MapTypeBox.SelectedItem = cfg.MapType;
            CacheMode.ItemsSource = Enum.GetNames(typeof(AccessMode));
            CacheMode.SelectedItem = cfg.AccessMode;
            UnitCore.Instance.EventAggregator.PublishMessage(new ShowAboutSlide(false));
        }

        private void SaveMapCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (double.Parse(CenterLngBox.Text) > 180 || double.Parse(CenterLngBox.Text) < -180)
                {
                    UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent("经度格式不正确", LogType.OnlyInfo));
                    return;
                }
                if (double.Parse(CenterLatBox.Text) > 180 || double.Parse(CenterLatBox.Text) < -180)
                {
                    UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent("纬度格式不正确", LogType.OnlyInfo));
                    return;
                }
                MonConf.GetInstance().SetCenLat(double.Parse(CenterLatBox.Text));
                MonConf.GetInstance().SetCenLng(double.Parse(CenterLngBox.Text));
                MonConf.GetInstance().SetAccessMode(CacheMode.SelectedItem.ToString());
                MonConf.GetInstance().SetMapName(Mapnamebox.Text);
                MonConf.GetInstance().SetMapType(MapTypeBox.SelectedItem.ToString());
                var point = new Offset();
                point.Lng = double.Parse(OffsetLngBox.Text);
                point.Lat = double.Parse(OffsetLatBox.Text);
                MonConf.GetInstance().SetMapOffset(point);
                UnitCore.Instance.MainMapCfg = UnitCore.Instance.MonConfigueService.LoadMapCfg();
                RefreshMap(UnitCore.Instance.MainMapCfg);
                UnitCore.Instance.EventAggregator.PublishMessage(new ShowAboutSlide(false));
            }
            catch(Exception ex)
            {
                UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent(ex.Message, LogType.OnlyInfo));
            }
        }
    }

}
