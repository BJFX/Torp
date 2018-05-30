using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading.Tasks;
using LOUV.Torp.Monitor.Events;
using TinyMetroWpfLibrary.ViewModel;
using TinyMetroWpfLibrary.EventAggregation;
using LOUV.Torp.Monitor.Core;
using LOUV.Torp.BaseType;
using LOUV.Torp.Monitor.Controls.MapCustom;
using GMap.NET.WindowsPresentation;
using GMap.NET;
using System.Windows;
using LOUV.Torp.Utility;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System.IO;
using LOUV.Torp.CommLib;
using LOUV.Torp.MonProtocol;

namespace LOUV.Torp.Monitor.ViewModel
{
    public class HomePageViewModel : ViewModelBase, IHandleMessage<ShowAboutSlide>, 
        IHandleMessage<RefreshBuoyInfoEvent>,
        IHandleMessage<SwitchMapModeEvent>,
        IHandleMessage<ChangeValidIntervalEvent>,
        IHandleMessage<ReplayModeEvent>
    {
        private DispatcherTimer Dt;
        private DispatcherTimer replayTimer;
        private List<Point3D> Path = new List<Point3D>();//target 3D track
        PointLatLng center = PointLatLng.Zero;
        PointLatLng center2 = PointLatLng.Zero;
        private int ReplayFileIndex = 0;
        private void CalTargetLocateCallBack(object sender,EventArgs e)
        {

            UnitCore.Instance.BuoyLock.WaitOne();
            var valid1 = UnitCore.Instance.Locate1.Valid(ref center);
            var valid2 = UnitCore.Instance.Locate2.Valid(ref center2);
            UnitCore.Instance.BuoyLock.ReleaseMutex();
            if(valid1)
                Locate(0);
            if (valid2)
                Locate(1);

        }

        private string Locate(int index)
        {
            string log = "";
            TriangleLocate locate = null;
            Locate3D targetpos = new Locate3D(DateTime.UtcNow);
            if (index == 0)
                locate = UnitCore.Instance.Locate1;
            if (index == 1)
                locate = UnitCore.Instance.Locate2;
            var itor = locate.Buoys.GetEnumerator();
            float presure = 0.0f;
            while (itor.MoveNext())
            {
                log += index.ToString()+" - "+"(" + itor.Current.Key.ToString() + ":" + "Lat" + itor.Current.Value.Lat.ToString() + "  Long" + itor.Current.Value.Lng.ToString() + " Distance" + itor.Current.Value.Range.ToString() + ")";

            }
            for (int i = 0; i < 4; i++)
            {
                if (index == 0)
                {
                    if (((Buoy)UnitCore.Instance.Buoy[i]).teleRange1.Crc == 0)
                    {
                        float p;
                        var ret = float.TryParse(((Buoy)UnitCore.Instance.Buoy[i]).teleRange1.Presure, out p);
                        if (ret)
                            presure = p;
                    }
                }
                if (index == 1)
                {
                    if (((Buoy)UnitCore.Instance.Buoy[i]).teleRange2.Crc == 0)
                    {
                        float p;
                        var ret = float.TryParse(((Buoy)UnitCore.Instance.Buoy[i]).teleRange2.Presure, out p);
                        if (ret)
                            presure = p;
                    }
                }
            }

            if (MonProtocol.TriangleLocate.UseMatrix)
            {
                Dxy1 = 0;
                Dxy2 = 0;
                if (locate.CalTargetByMatrix(out targetpos))
                {
                    targetpos.centerLat = center.Lat;
                    targetpos.centerLng = center.Lng;
                    if (index == 0)
                    {
                        UnitCore.Instance.TargetObj1 = new Target("AUV1")
                        {
                            ID = (byte)UnitCore.Instance.MonConfigueService.GetSetup().AUVID1,
                            Status = "已定位",
                            UTCTime = targetpos.Time,
                            Longitude = (float)Util.LongOffset(targetpos.centerLng, targetpos.centerLat, targetpos.X),
                            Latitude = (float)Util.LatOffset(targetpos.centerLat, targetpos.Y),
                            Depth = presure,// (float)(targetpos.Z + UnitCore.Instance.MonConfigueService.GetSetup().SonarDepth),
                        };
                        log += "定位结果:" + "long:" + UnitCore.Instance.TargetObj1.Longitude + "  lat:" + UnitCore.Instance.TargetObj1.Latitude;
                        RefreshTarget1();
                    }
                    if (index == 1)
                    {
                        UnitCore.Instance.TargetObj2 = new Target("AUV2")
                        {
                            ID = (byte)UnitCore.Instance.MonConfigueService.GetSetup().AUVID2,
                            Status = "已定位",
                            UTCTime = targetpos.Time,
                            Longitude = (float)Util.LongOffset(targetpos.centerLng, targetpos.centerLat, targetpos.X),
                            Latitude = (float)Util.LatOffset(targetpos.centerLat, targetpos.Y),
                            Depth = presure,// (float)(targetpos.Z + UnitCore.Instance.MonConfigueService.GetSetup().SonarDepth),
                        };
                        log += "定位结果:" + "long:" + UnitCore.Instance.TargetObj2.Longitude + "  lat:" + UnitCore.Instance.TargetObj2.Latitude;
                        RefreshTarget2();
                    }
                    
                }
                else
                {
                    Dxy1 = 0;
                    Dxy2 = 0;
                    log += "定位结果:未成功定位";
                }

            }
            else
            {
                var sonardepth = (float)UnitCore.Instance.MonConfigueService.GetSetup().SonarDepth;
                MonProtocol.TriangleLocate.SonarDepth = sonardepth / 1000;
                var result = locate.CalTargetByApproach();
                if (index == 0)
                {
                    UnitCore.Instance.TargetObj1 = new Target("AUV1")
                    {
                        ID = (byte)UnitCore.Instance.MonConfigueService.GetSetup().AUVID1,
                        Status = "已定位",
                        UTCTime = DateTime.UtcNow,
                        Longitude = (float)result[1],
                        Latitude = (float)result[0],
                        Depth = presure,//(float)result[2],
                    };
                    Dxy1 = (float)result[3];
                    log += "定位结果:" + "long:" + UnitCore.Instance.TargetObj1.Longitude + "  lat:" + UnitCore.Instance.TargetObj1.Latitude + " D=" + Dxy1.ToString("F06");
                    RefreshTarget1();
                }
                if (index == 1)
                {
                    UnitCore.Instance.TargetObj2 = new Target("AUV2")
                    {
                        ID = (byte)UnitCore.Instance.MonConfigueService.GetSetup().AUVID2,
                        Status = "已定位",
                        UTCTime = DateTime.UtcNow,
                        Longitude = (float)result[1],
                        Latitude = (float)result[0],
                        Depth = presure,//(float)result[2],
                    };
                    Dxy2 = (float)result[3];
                    log += "定位结果:" + "long:" + UnitCore.Instance.TargetObj2.Longitude + "  lat:" + UnitCore.Instance.TargetObj2.Latitude + " D=" + Dxy2.ToString("F06");
                    RefreshTarget2();
                }
            }
            UnitCore.Instance.MonTraceService.Save("Position", log);
            return log;
        }

        private void Refresh3DView()
        {
            double x, y,z;
            x = 0;
            y = 0;
            z = 0;
            center.Lat = (Buoy1.gps.Latitude + Buoy2.gps.Latitude +
                Buoy3.gps.Latitude + Buoy4.gps.Latitude) / 4;
            center.Lng = (Buoy1.gps.Longitude + Buoy2.gps.Longitude +
                Buoy3.gps.Longitude + Buoy4.gps.Longitude) / 4;
            UpdateLatLong(center);
            if (Buoy1 != null)
            {
                Util.GetReltXY(new PointLatLng(Buoy1.gps.Latitude, Buoy1.gps.Longitude), center, out x, out y);
                Buoy1Center = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");
            }
            if (Buoy2 != null)
            {
                Util.GetReltXY(new PointLatLng(Buoy2.gps.Latitude, Buoy2.gps.Longitude), center, out x, out y);
                Buoy2Center = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");
            }
            if (Buoy3 != null)
            {
                Util.GetReltXY(new PointLatLng(Buoy3.gps.Latitude, Buoy3.gps.Longitude), center, out x, out y);
                Buoy3Center = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");
            }

            if (Buoy4 != null)
            {
                Util.GetReltXY(new PointLatLng(Buoy4.gps.Latitude, Buoy4.gps.Longitude), center, out x, out y);
                Buoy4Center = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");
            }
            if (ObjTarget1 != null)
            {
                Util.GetReltXY(new PointLatLng(ObjTarget1.Latitude, ObjTarget1.Longitude), center, out x, out y);
                z = -ObjTarget1.Depth;
                ObjectCenter = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");
                if (AutoTrack)
                {
                    //UnitCore.Instance.PosView3D.CameraController.ChangeDirection(new Vector3D(0, 0, -2000), new Vector3D(0, 1, 0), 1000);
                    UnitCore.Instance.PosView3D.Camera.Position = new Point3D(x, y, z + 4000);
                }
                //CamPos = x.ToString("F02") + "," + y.ToString("F02") + "," + (z-2000).ToString("F02");
                UpdataTrack(x, y, z);
                OffsetX = x;
                OffsetY = y;
                OffsetZ = z;
            }
        }
        public override void Initialize()
        {
            //ObjTarget = new Target();
            ObjTarget1 = UnitCore.Instance.TargetObj1;
            ObjTarget2 = UnitCore.Instance.TargetObj2;
            StartReplayCMD = RegisterCommand(ExecuteStartReplayCMD, CanExecuteStartReplayCMD, true);
            ResumeReplayCMD = RegisterCommand(ExecuteResumeReplayCMD, CanExecuteResumeReplayCMD, true);
            PauseReplayCMD = RegisterCommand(ExecutePauseReplayCMD, CanExecutePauseReplayCMD, true);
            ExitReplayCMD = RegisterCommand(ExecuteExitReplayCMD, CanExecuteExitReplayCMD, true);
            ReplayState = 0;//0:normal,1:replaying,2:pause
            //Dt = new DispatcherTimer(TimeSpan.FromSeconds(UnitCore.Instance.MonConfigueService.GetSetup().ValidInterval), DispatcherPriority.DataBind, CalTargetLocateCallBack, Dispatcher.CurrentDispatcher);
            Buoy1 = (Buoy)UnitCore.Instance.Buoy[0];
            Buoy2 = (Buoy)UnitCore.Instance.Buoy[1];
            Buoy3 = (Buoy)UnitCore.Instance.Buoy[2];
            Buoy4 = (Buoy)UnitCore.Instance.Buoy[3];
            CamPos = "0,0,4000";
            TrackVisible = false;
            ReplaySpeed = 1;
            /*
            UnitCore.Instance.TargetObj1.Longitude = 116.39999f;
            UnitCore.Instance.TargetObj1.Latitude = 39.59355f;
            UnitCore.Instance.TargetObj2.Longitude = 116.49999f;
            UnitCore.Instance.TargetObj2.Latitude = 39.69355f;*/
            ///
            //Refresh3DView();
        }
        public override void InitializePage(object extraData)
        {
            AboutVisibility = false;
            MapMode = 0;
            
        }
        private void UpdateLatLong(PointLatLng center)
        {
            longbottom0 = Util.LongOffset(center.Lng, center.Lat, 500);
            longbottom1 = Util.LongOffset(center.Lng, center.Lat, 1000);
            longbottom2 = Util.LongOffset(center.Lng, center.Lat, 1500);
            longbottom3 = Util.LongOffset(center.Lng, center.Lat, 2000);

            longTop0 = Util.LongOffset(center.Lng, center.Lat, -500);
            longTop1 = Util.LongOffset(center.Lng, center.Lat, -1000);
            longTop2 = Util.LongOffset(center.Lng, center.Lat, -1500);
            longTop3 = Util.LongOffset(center.Lng, center.Lat, -2000);

            latleft0 = Util.LatOffset(center.Lat, -500);
            latleft1 = Util.LatOffset(center.Lat, -1000);
            latleft2 = Util.LatOffset(center.Lat, -1500);
            latleft3 = Util.LatOffset(center.Lat, -2000);

            latTop0 = Util.LatOffset(center.Lat, 500);
            latTop1 = Util.LatOffset(center.Lat, 1000);
            latTop2 = Util.LatOffset(center.Lat, 1500);
            latTop3 = Util.LatOffset(center.Lat, 2000);

        }
        private void RefreshTarget1()
        {
            ObjectMarker targetMarker = null;
            GMapMarker target = null;
            ObjTarget1 = UnitCore.Instance.TargetObj1;
            //refresh 2D target
            if (MapMode == 0)
            {
                if (UnitCore.Instance.mainMap != null)
                {
                    var itor = UnitCore.Instance.mainMap.Markers.GetEnumerator();
                    while (itor.MoveNext())
                    {
                        var marker = itor.Current;

                        if ((int)marker.Tag == 901)//900+1,2,3,4...
                        {
                            if (marker.Shape is ObjectMarker obj)
                            {
                                targetMarker = obj;
                                target = marker;
                            }
                            break;
                        }
                        
                    }
                }
                if (target == null || targetMarker == null)
                    return;

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    targetMarker.Refresh(UnitCore.Instance.TargetObj1);
                    var point = new PointLatLng(UnitCore.Instance.TargetObj1.Latitude,
                        UnitCore.Instance.TargetObj1.Longitude);
                    point.Offset(UnitCore.Instance.MainMapCfg.MapOffset.Lat,
                        UnitCore.Instance.MainMapCfg.MapOffset.Lng);
                    target.Position = point;
                    //remove legacy route
                    var isExist = UnitCore.Instance.mainMap.Markers.Contains(UnitCore.Instance.TargetRoute1);
                    UnitCore.Instance.BuoyLock.WaitOne();
                    UnitCore.Instance.mainMap.Markers.Remove(UnitCore.Instance.TargetRoute1);
                    UnitCore.Instance.BuoyLock.ReleaseMutex();
                    UnitCore.Instance.routePoint1.Remove(PointLatLng.Zero);

                    UnitCore.Instance.routePoint1.Add(point);
                    //if (UnitCore.Instance.routePoint.Count > 300)
                    //    UnitCore.Instance.routePoint.RemoveAt(0);
                    if (UnitCore.Instance.routePoint1.Count == 1)
                        return;
                    UnitCore.Instance.TargetRoute1 = new GMapMarker(UnitCore.Instance.routePoint1[0]);
                    {

                        UnitCore.Instance.TargetRoute1.Tag = 101;
                        UnitCore.Instance.TargetRoute1.routebrush = Brushes.Red;

                        UnitCore.Instance.TargetRoute1.Route.AddRange(UnitCore.Instance.routePoint1);

                        UnitCore.Instance.TargetRoute1.RegenerateRouteShape(UnitCore.Instance.mainMap);
                        UnitCore.Instance.TargetRoute1.ZIndex = -1;
                    }
                    if (isExist)
                    {
                        UnitCore.Instance.BuoyLock.WaitOne();
                        UnitCore.Instance.mainMap.Markers.Add(UnitCore.Instance.TargetRoute1);
                        UnitCore.Instance.BuoyLock.ReleaseMutex();
                    }

                    if (UnitCore.Instance.AutoTrace)
                        UnitCore.Instance.mainMap.Position = point;
                }));
            }
            else//3D
            {
                Refresh3DView();
                if (ObjInfoVisible)
                {
                    TargetInfo = UnitCore.Instance.TargetObj1.Name + "   " + UnitCore.Instance.TargetObj1.Time + "(UTC)\r" +
                    "经度:" + UnitCore.Instance.TargetObj1.Longitude.ToString("F06") + "\r|纬度:" +
                    UnitCore.Instance.TargetObj1.Latitude.ToString("F06") + "\r" +
                    "测算深度:" + UnitCore.Instance.TargetObj1.Depth.ToString("F02") +
                    "测距状态:" + UnitCore.Instance.TargetObj1.Status;
                }
                else
                {
                    TargetInfo = "";
                }
            }
        }
        private void RefreshTarget2()
        {
            ObjectMarker targetMarker = null;
            GMapMarker target = null;
            ObjTarget2 = UnitCore.Instance.TargetObj2;
            //refresh 2D target
            if (MapMode==0)
            {
                if (UnitCore.Instance.mainMap != null)
                {
                    var itor = UnitCore.Instance.mainMap.Markers.GetEnumerator();
                    while (itor.MoveNext())
                    {
                        var marker = itor.Current;

                        if ((int)marker.Tag == 902)//900+1,2,3,4...
                        {
                            if (marker.Shape is ObjectMarker obj)
                            {
                                targetMarker = obj;
                                target = marker;
                            }
                            break;
                        }
                    }
                }
                if (target == null || targetMarker == null)
                    return;
                
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    targetMarker.Refresh(UnitCore.Instance.TargetObj2);
                    var point = new PointLatLng(UnitCore.Instance.TargetObj2.Latitude,
                        UnitCore.Instance.TargetObj2.Longitude);
                    point.Offset(UnitCore.Instance.MainMapCfg.MapOffset.Lat,
                        UnitCore.Instance.MainMapCfg.MapOffset.Lng);
                    target.Position = point;
                    //remove legacy route
                    var isExist = UnitCore.Instance.mainMap.Markers.Contains(UnitCore.Instance.TargetRoute2);
                    UnitCore.Instance.BuoyLock.WaitOne();
                    UnitCore.Instance.mainMap.Markers.Remove(UnitCore.Instance.TargetRoute2);
                    UnitCore.Instance.BuoyLock.ReleaseMutex();
                    UnitCore.Instance.routePoint2.Remove(PointLatLng.Zero);

                    UnitCore.Instance.routePoint2.Add(point);
                    //if (UnitCore.Instance.routePoint.Count > 300)
                    //    UnitCore.Instance.routePoint.RemoveAt(0);
                    if (UnitCore.Instance.routePoint2.Count == 1)
                        return;
                    UnitCore.Instance.TargetRoute2 = new GMapMarker(UnitCore.Instance.routePoint2[0]);
                    {

                        UnitCore.Instance.TargetRoute2.Tag = 102;
                        UnitCore.Instance.TargetRoute2.routebrush = Brushes.Yellow;

                        UnitCore.Instance.TargetRoute2.Route.AddRange(UnitCore.Instance.routePoint2);

                        UnitCore.Instance.TargetRoute2.RegenerateRouteShape(UnitCore.Instance.mainMap);
                        UnitCore.Instance.TargetRoute2.ZIndex = -1;
                    }
                    if(isExist)
                    {
                        UnitCore.Instance.BuoyLock.WaitOne();
                        UnitCore.Instance.mainMap.Markers.Add(UnitCore.Instance.TargetRoute2);
                        UnitCore.Instance.BuoyLock.ReleaseMutex();
                    }
                        
                    if (UnitCore.Instance.AutoTrace)
                        UnitCore.Instance.mainMap.Position = point;
                }));
            }
            
        }
        private void RefreshBuoy(int index,Buoy buoy)
        {
            //update auv info
            ObjTarget1.Head = buoy.teleRange1.Head;
            ObjTarget1.Pitch = buoy.teleRange1.Pitch;
            ObjTarget1.Roll = buoy.teleRange1.Roll;
            ObjTarget2.Head = buoy.teleRange2.Head;
            ObjTarget2.Pitch = buoy.teleRange2.Pitch;
            ObjTarget2.Roll = buoy.teleRange2.Roll;
            Head1 = ObjTarget1.Head;
            Pitch1 = ObjTarget1.Pitch;
            Roll1 = ObjTarget1.Roll;
            Head2 = ObjTarget2.Head;
            Pitch2 = ObjTarget2.Pitch;
            Roll2 = ObjTarget2.Roll;
            switch (index)
            {
                case 0:
                    Buoy1 = null;
                    Buoy1 = buoy;
                    break;
                case 1:
                    Buoy2 = null;
                    Buoy2 = buoy;
                    break;
                case 2:
                    Buoy3 = null;
                    Buoy3 = buoy;
                    break;
                case 3:
                    Buoy4 = null;
                    Buoy4 = buoy;
                    break;
                default:
                    break;
            }
            Refresh3DView();
            RefreshBuoyInfo(BuoyInfoVisible);
        }
        private void RefreshBuoyInfo(bool enable)
        {
            if(enable)
            {
                Buoy1Info= Buoy1.Name + "   " + Buoy1.Time + "(UTC)\r"+
                    "经度:" + Buoy1.gps.Longitude.ToString("F06") + "\r纬度:" + 
                    Buoy1.gps.Latitude.ToString("F06")+"\r"+
                    "测距1:" + Buoy1.Range1.ToString("F02")+"\r" +
                    "测距2:" + Buoy1.Range2.ToString("F02");

                Buoy2Info = Buoy2.Name + "   " + Buoy2.Time + "(UTC)\r" +
                    "经度:" + Buoy2.gps.Longitude.ToString("F06") + "\r纬度:" +
                    Buoy2.gps.Latitude.ToString("F06") + "\r" +
                    "测距1:" + Buoy1.Range1.ToString("F02") + "\r" +
                    "测距2:" + Buoy1.Range2.ToString("F02");

                Buoy3Info = Buoy3.Name + "   " + Buoy3.Time + "(UTC)\r" +
                    "经度:" + Buoy3.gps.Longitude.ToString("F06") + "\r纬度:" +
                    Buoy3.gps.Latitude.ToString("F06") + "\r" +
                    "测距1:" + Buoy1.Range1.ToString("F02") + "\r" +
                    "测距2:" + Buoy1.Range2.ToString("F02");

                Buoy4Info = Buoy4.Name + "   " + Buoy4.Time + "(UTC)\r" +
                    "经度:" + Buoy4.gps.Longitude.ToString("F06") + "\r纬度:" +
                    Buoy4.gps.Latitude.ToString("F06") + "\r" +
                    "测距1:" + Buoy1.Range1.ToString("F02") + "\r" +
                    "测距2:" + Buoy1.Range2.ToString("F02");
            }
            else
            {
                Buoy1Info = "";
                Buoy2Info = "";
                Buoy3Info = "";
                Buoy4Info = "";
            }
        }
        #region Binding

        #region latitude
        public double latleft0
        {
            get { return GetPropertyValue(() => latleft0); }
            set { SetPropertyValue(() => latleft0, value); }
        }
        public double latleft1
        {
            get { return GetPropertyValue(() => latleft1); }
            set { SetPropertyValue(() => latleft1, value); }
        }
        public double latleft2
        {
            get { return GetPropertyValue(() => latleft2); }
            set { SetPropertyValue(() => latleft2, value); }
        }
        public double latleft3
        {
            get { return GetPropertyValue(() => latleft3); }
            set { SetPropertyValue(() => latleft3, value); }
        }
        public double latTop0
        {
            get { return GetPropertyValue(() => latTop0); }
            set { SetPropertyValue(() => latTop0, value); }
        }
        public double latTop1
        {
            get { return GetPropertyValue(() => latTop1); }
            set { SetPropertyValue(() => latTop1, value); }
        }
        public double latTop2
        {
            get { return GetPropertyValue(() => latTop2); }
            set { SetPropertyValue(() => latTop2, value); }
        }
        public double latTop3
        {
            get { return GetPropertyValue(() => latTop3); }
            set { SetPropertyValue(() => latTop3, value); }
        }
        #endregion
        
        #region Longitude
        public double longbottom0
        {
            get { return GetPropertyValue(() => longbottom0); }
            set { SetPropertyValue(() => longbottom0, value); }
        }
        public double longbottom1
        {
            get { return GetPropertyValue(() => longbottom1); }
            set { SetPropertyValue(() => longbottom1, value); }
        }
        public double longbottom2
        {
            get { return GetPropertyValue(() => longbottom2); }
            set { SetPropertyValue(() => longbottom2, value); }
        }
        public double longbottom3
        {
            get { return GetPropertyValue(() => longbottom3); }
            set { SetPropertyValue(() => longbottom3, value); }
        }
        public double longTop0
        {
            get { return GetPropertyValue(() => longTop0); }
            set { SetPropertyValue(() => longTop0, value); }
        }
        public double longTop1
        {
            get { return GetPropertyValue(() => longTop1); }
            set { SetPropertyValue(() => longTop1, value); }
        }
        public double longTop2
        {
            get { return GetPropertyValue(() => longTop2); }
            set { SetPropertyValue(() => longTop2, value); }
        }
        public double longTop3
        {
            get { return GetPropertyValue(() => longTop3); }
            set { SetPropertyValue(() => longTop3, value); }
        }
        #endregion

        public int Head1
        {
            get { return GetPropertyValue(() => Head1); }
            set { SetPropertyValue(() => Head1, value); }
        }
        public int Pitch1
        {
            get { return GetPropertyValue(() => Pitch1); }
            set { SetPropertyValue(() => Pitch1, value); }
        }
        public int Roll1
        {
            get { return GetPropertyValue(() => Roll1); }
            set { SetPropertyValue(() => Roll1, value); }
        }
        public int Head2
        {
            get { return GetPropertyValue(() => Head2); }
            set { SetPropertyValue(() => Head2, value); }
        }
        public int Pitch2
        {
            get { return GetPropertyValue(() => Pitch2); }
            set { SetPropertyValue(() => Pitch2, value); }
        }
        public int Roll2
        {
            get { return GetPropertyValue(() => Roll2); }
            set { SetPropertyValue(() => Roll2, value); }
        }
        public float Dxy1
        {
            get { return GetPropertyValue(() => Dxy1); }
            set { SetPropertyValue(() => Dxy1, value); }
        }
        public float Dxy2
        {
            get { return GetPropertyValue(() => Dxy2); }
            set { SetPropertyValue(() => Dxy2, value); }
        }
        public bool BuoyInfoVisible
        {
            get { return GetPropertyValue(() => BuoyInfoVisible); }
            set
            {

                RefreshBuoyInfo(value);
                SetPropertyValue(() => BuoyInfoVisible, value);
            }
        }
        public bool ObjInfoVisible
        {
            get { return GetPropertyValue(() => ObjInfoVisible); }
            set
            {
                if (value == false)
                    TargetInfo = "";
                else
                {
                    TargetInfo = ObjTarget1.Name + "   " + ObjTarget1.Time + "(UTC)\r" +
                    "经度:" + ObjTarget1.Longitude.ToString("F06") + "\r|纬度:" +
                    ObjTarget1.Latitude.ToString("F06") + "\r" +
                    "测算深度:" + ObjTarget1.Depth.ToString("F02")+
                    "测距状态:" + ObjTarget1.Status;
                }
                SetPropertyValue(() => ObjInfoVisible, value);
            }
        }

        public string Buoy1Info
        {
            get { return GetPropertyValue(() => Buoy1Info); }
            set { SetPropertyValue(() => Buoy1Info, value); }
        }
        public string Buoy2Info
        {
            get { return GetPropertyValue(() => Buoy2Info); }
            set { SetPropertyValue(() => Buoy2Info, value); }
        }
        public string Buoy3Info
        {
            get { return GetPropertyValue(() => Buoy3Info); }
            set { SetPropertyValue(() => Buoy3Info, value); }
        }
        public string Buoy4Info
        {
            get { return GetPropertyValue(() => Buoy4Info); }
            set { SetPropertyValue(() => Buoy4Info, value); }
        }
        public string TargetInfo
        {
            get { return GetPropertyValue(() => TargetInfo); }
            set { SetPropertyValue(() => TargetInfo, value); }
        }
        public bool TrackVisible
        {
            get { return GetPropertyValue(() => TrackVisible); }
            set
            {

                if (value == false)
                    TrackModel = null;
                else
                {
                    // create the WPF3D model
                    var m = new Model3DGroup();
                    var gm = new MeshBuilder();
                    gm.AddTube(Path, 18, 10, false);
                    m.Children.Add(new GeometryModel3D(gm.ToMesh(), Materials.Red));
                    TrackModel = m;
                }
                SetPropertyValue(() => TrackVisible, value);

            }
        }
        private void RmoveTrack()
        {
            //UnitCore.Instance.routePoint.RemoveAll((s) => { return s != null; });
            UnitCore.Instance.mainMap.Markers.Remove(UnitCore.Instance.TargetRoute1);
            UnitCore.Instance.mainMap.Markers.Remove(UnitCore.Instance.TargetRoute2);
            TrackVisible = false;
            Path.RemoveAll((s) => { return s != null; });

        }
        public bool AutoTrack
        {
            get { return GetPropertyValue(() => AutoTrack); }
            set
            {
                if(value == false)
                {
                    CamPos = "0,0,4000";
                }
                else
                {
                    CamPos = ObjectCenter;
                }
                SetPropertyValue(() => AutoTrack, value);
            }
        }
        private void UpdataTrack(double x, double y, double z)
        {
            Path.Add(new Point3D(x, y, z));
            if (TrackVisible)
            {
                // create the WPF3D model
                var m = new Model3DGroup();
                var gm = new MeshBuilder();
                gm.AddTube(Path, 18, 10, false);
                m.Children.Add(new GeometryModel3D(gm.ToMesh(), Materials.Red));
                TrackModel = m;
            }
        }
        public double OffsetX
        {
            get { return GetPropertyValue(() => OffsetX); }
            set { SetPropertyValue(() => OffsetX, value); }
        }
        public double OffsetY
        {
            get { return GetPropertyValue(() => OffsetY); }
            set { SetPropertyValue(() => OffsetY, value); }
        }
        public double OffsetZ
        {
            get { return GetPropertyValue(() => OffsetZ); }
            set { SetPropertyValue(() => OffsetZ, value); }
        }
        public string CamPos
        {
            get { return GetPropertyValue(() => CamPos); }
            set { SetPropertyValue(() => CamPos, value); }
        }
        public bool AboutVisibility
        {
            get { return GetPropertyValue(() => AboutVisibility); }
            set { SetPropertyValue(() => AboutVisibility, value); }
        }
        public int MapMode//0:2D,1:3D
        {
            get { return GetPropertyValue(() => MapMode); }
            set { SetPropertyValue(() => MapMode, value); }
        }

        public Target ObjTarget1
        {
            get { return GetPropertyValue(() => ObjTarget1); }
            set { SetPropertyValue(() => ObjTarget1, value); }
        }
        public Target ObjTarget2
        {
            get { return GetPropertyValue(() => ObjTarget2); }
            set { SetPropertyValue(() => ObjTarget2, value); }
        }
        public Buoy Buoy1
        {
            get { return GetPropertyValue(() => Buoy1); }
            set { SetPropertyValue(() => Buoy1, value); }
        }
        public Buoy Buoy2
        {
            get { return GetPropertyValue(() => Buoy2); }
            set { SetPropertyValue(() => Buoy2, value); }
        }
        public Buoy Buoy3
        {
            get { return GetPropertyValue(() => Buoy3); }
            set { SetPropertyValue(() => Buoy3, value); }
        }
        public Buoy Buoy4
        {
            get { return GetPropertyValue(() => Buoy4); }
            set { SetPropertyValue(() => Buoy4, value); }
        }

        public Model3D TrackModel
        {
            get { return GetPropertyValue(() => TrackModel); }
            set { SetPropertyValue(() => TrackModel, value); }
        }
        public string Buoy1Center
        {
            get { return GetPropertyValue(() => Buoy1Center); }
            set { SetPropertyValue(() => Buoy1Center, value); }
        }
        public string Buoy2Center
        {
            get { return GetPropertyValue(() => Buoy2Center); }
            set { SetPropertyValue(() => Buoy2Center, value); }
        }
        public string Buoy3Center
        {
            get { return GetPropertyValue(() => Buoy3Center); }
            set { SetPropertyValue(() => Buoy3Center, value); }
        }
        public string Buoy4Center
        {
            get { return GetPropertyValue(() => Buoy4Center); }
            set { SetPropertyValue(() => Buoy4Center, value); }

        }
        public string ObjectCenter
        {
            get { return GetPropertyValue(() => ObjectCenter); }
            set { SetPropertyValue(() => ObjectCenter, value); }
        }
        public bool RelplayMode
        {
            get { return GetPropertyValue(() => RelplayMode); }
            set { SetPropertyValue(() => RelplayMode, value); }
        }
        public uint ReplayState//0:normal,1:replaying,2:pause
        {
            get { return GetPropertyValue(() => ReplayState); }
            set
            {
                SetPropertyValue(() => ReplayState, value);
               
            }
        }
        public int ReplaySpeed
        {
            get { return GetPropertyValue(() => ReplaySpeed); }
            set
            {
                if (replayTimer != null)
                {
                    replayTimer.Stop();
                    replayTimer.Interval =TimeSpan.FromMilliseconds( 200 / value);
                    replayTimer.Start();
                }
                SetPropertyValue(() => ReplaySpeed, value);

            }
        }

        #endregion

        #region Event Handle


        public void Handle(ShowAboutSlide message)
        {
            AboutVisibility = message.showslide;
        }

        public void Handle(RefreshBuoyInfoEvent message)
        {
            UnitCore.Instance.BuoyLock.WaitOne();
            if (UnitCore.Instance.Buoy.ContainsKey(message._index))
            {
                RefreshBuoy(message._index, (Buoy)UnitCore.Instance.Buoy[message._index]);
            }
            UnitCore.Instance.BuoyLock.ReleaseMutex();
            CalTargetLocateCallBack(null,null);
        }

        public void Handle(SwitchMapModeEvent message)
        {
            if (MapMode == 0)
            {
                MapMode = 1;
            }
            else
            {
                MapMode = 0;
            }
        }

        public void Handle(ChangeValidIntervalEvent message)
        {
            Dt.Stop();
            Dt.Interval = TimeSpan.FromSeconds(message.ValidInterval);
            Dt.Start();
            
        }

        public void Handle(ReplayModeEvent message)
        {
            RelplayMode = UnitCore.Instance.IsReplay;
        }
        #endregion

        #region Binding method
        public ICommand StartReplayCMD
        {
            get { return GetPropertyValue(() => StartReplayCMD); }
            set { SetPropertyValue(() => StartReplayCMD, value); }
        }
        private void CanExecuteStartReplayCMD(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }
        private async void ExecuteStartReplayCMD(object sender, ExecutedRoutedEventArgs eventArgs)
        {

                if (ReplayState != 0)
                {
                    var md = new MetroDialogSettings();
                    md.AffirmativeButtonText = "确定";
                    md.NegativeButtonText = "取消";
                    var ret = await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame,"重新开始回放？", "正在回放数据，确定要重新选择回放文件吗？",
                        MessageDialogStyle.AffirmativeAndNegative, md);
                    if (ret != MessageDialogResult.Affirmative)
                    {
                        return;
                    }
                }
                //start replay
                CleanScreen();

                ReplayFileIndex = 0;
                if (replayTimer != null)
                    replayTimer.Stop();
            OpenFileDialog OpenFileDlg = new OpenFileDialog();
            if (OpenFileDlg.ShowDialog() == true)
            {
                var ReplayFileName = OpenFileDlg.FileName;
                if (UnitCore.Instance.Replaylist == null)
                    UnitCore.Instance.Replaylist = new List<byte[]>();
                var tsk = await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowProgressAsync(MainFrameViewModel.pMainFrame, "请稍候", "正在处理回放数据...", false);
                tsk.SetIndeterminate();
                SplitDataFile(ReplayFileName);
                await TaskEx.Delay(1000);
                await tsk.CloseAsync();
                ReplayState = 1;
                if (replayTimer == null)
                    replayTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Input,
                ResultReplaying, Dispatcher.CurrentDispatcher);
                replayTimer.Start();
            }

        }

        private void SplitDataFile(string replayFileName)
        {
            FileStream file;

            using (file = new FileStream(replayFileName, FileMode.Open))
            {
                UnitCore.Instance.Replaylist.Clear();
                var bytes = new byte[1034];//add 2 byte ip address
                while (file.Read(bytes, 0, 1034)>0)
                {
                    var buf = new byte[1034];
                    Buffer.BlockCopy(bytes, 0, buf, 0, 1034);
                    switch (BitConverter.ToInt16(bytes, 2))
                    {
                        case 0x0128:
                        case 0x0129:
                        case 0x012A:
                            UnitCore.Instance.Replaylist.Add(buf);
                            break;
                        case 0x012B:
                            //udp ans
                            break;

                    }
                    
                }

            }
        }

        private void CleanScreen()
        {
            TrackModel = null;
            RmoveTrack();
            var gpsinfo = new GpsInfo()
            {
                UTCTime = DateTime.UtcNow,
                Latitude = 29.592966F,
                Longitude = 118.983188F,
            };

            var by = new Buoy()
            {
                Id = 1,
                gps = gpsinfo,
                IP = "192.168.2.101",
            };
            Buoy1 = by;
            by.Id = 2;
            Buoy2 = by;
            by.Id = 3;
            Buoy3 = by;
            by.Id = 4;
            Buoy4 = by;
            //UnitCore.Instance.TargetObj.Latitude = 29.592966F;
            //UnitCore.Instance.TargetObj.Longitude = 118.983188F;
            //UnitCore.Instance.TargetObj.Status = "未定位";
            RefreshBuoy(0, Buoy1);
            RefreshBuoy(1, Buoy2);
            RefreshBuoy(2, Buoy3);
            RefreshBuoy(3, Buoy4);
            
            RefreshBuoyInfo(BuoyInfoVisible);
        }

        public ICommand ResumeReplayCMD
        {
            get { return GetPropertyValue(() => ResumeReplayCMD); }
            set { SetPropertyValue(() => ResumeReplayCMD, value); }
        }
        private void CanExecuteResumeReplayCMD(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private void ExecuteResumeReplayCMD(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if (ReplayState != 1)
            {

                //check the filelist
                if (UnitCore.Instance.Replaylist != null && UnitCore.Instance.Replaylist.Count > 0)
                {
                    if (replayTimer == null)
                        replayTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Input,
                    ResultReplaying, Dispatcher.CurrentDispatcher);
                    replayTimer.Start();
                    ReplayState = 1;

                }
                else
                {
                    return;
                }
            }

        }

        private void ResultReplaying(object sender, EventArgs e)
        {
            var bytes = UnitCore.Instance.Replaylist[ReplayFileIndex];//must be 1034 bytes
            CallMode mode = CallMode.GPS;
            var ip = BitConverter.ToInt16(bytes, 0);
            switch (BitConverter.ToInt16(bytes, 2))
            {
                case 0x0128:
                    mode = CallMode.Range;
                    break;
                case 0x0129:
                    mode = CallMode.TeleRange;
                    break;
                case 0x012A:
                    mode = CallMode.GPS;
                    break;
                case 0x012B:
                    return;

            }
            var buf = new byte[1032];
            Buffer.BlockCopy(bytes, 2, buf, 0, 1032);
            var arg = new CustomEventArgs(0, null, buf, buf.Length, true, null, mode, "192.168.2."+ip.ToString());
            UnitCore.Instance.Observer.Handle(this, arg);
            ReplayFileIndex++;
            if (ReplayFileIndex == UnitCore.Instance.Replaylist.Count)
            {
                ReplayFileIndex = 0;
                replayTimer.Stop();
                ReplayState = 0;
            }

        }

        public ICommand ExitReplayCMD
        {
            get { return GetPropertyValue(() => ExitReplayCMD); }
            set { SetPropertyValue(() => ExitReplayCMD, value); }
        }
        private void CanExecuteExitReplayCMD(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private async void ExecuteExitReplayCMD(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            var md = new MetroDialogSettings();
            md.AffirmativeButtonText = "退出";
            md.NegativeButtonText = "取消";
            var ret = await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame,
                "退出回放模式",
                "确认退出回放模式？", MessageDialogStyle.AffirmativeAndNegative, md);
            if (ret == MessageDialogResult.Affirmative)
            {
                replayTimer.Stop();
                ReplayState = 0;
                UnitCore.Instance.IsReplay = false;
                RelplayMode = UnitCore.Instance.IsReplay;
                CleanScreen();

            }

        }
        public ICommand PauseReplayCMD
        {
            get { return GetPropertyValue(() => PauseReplayCMD); }
            set { SetPropertyValue(() => PauseReplayCMD, value); }
        }
        private void CanExecutePauseReplayCMD(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private void ExecutePauseReplayCMD(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if (ReplayState == 1)
            {
                replayTimer.Stop();

                ReplayState = 2;
                
                return;
            }
            if (ReplayState == 2)
            {
                replayTimer.Start();

                ReplayState = 1;
                
            }

        }
        #endregion

    }
}
