using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
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

namespace LOUV.Torp.Monitor.ViewModel
{
    public class HomePageViewModel : ViewModelBase, IHandleMessage<ShowAboutSlide>, 
        IHandleMessage<RefreshBuoyInfoEvent>,
        IHandleMessage<SwitchMapModeEvent>,
        IHandleMessage<ChangeValidIntervalEvent>
    {
        private DispatcherTimer Dt;
        private List<Point3D> Path = new List<Point3D>();//target 3D track
        PointLatLng center = PointLatLng.Zero;
        private void CalTargetLocateCallBack(object sender, EventArgs e)
        {
            //test case
            /*
            UnitCore.Instance.TargetObj.Latitude += 0.001f;
            UnitCore.Instance.TargetObj.Longitude += 0.001f;
            RefreshTarget();
            return;*/
            //
            UnitCore.Instance.BuoyLock.WaitOne();
            var valid = MonProtocol.TriangleLocate.Valid(UnitCore.Instance.MonConfigueService.GetSetup().TimeOut, ref center);
            UnitCore.Instance.BuoyLock.ReleaseMutex();
            if (valid == false)
                return;
            Locate3D targetpos;
            var itor = MonProtocol.TriangleLocate.Buoys.GetEnumerator();
            string log = "";
            while (itor.MoveNext())
            {
                log += "("+itor.Current.Key.ToString() + ":" +"Lat"+ itor.Current.Value.Lat.ToString()+"  Long"+ itor.Current.Value.Lng.ToString()+")";
            }
            if (MonProtocol.TriangleLocate.CalTargetLocation(out targetpos))
            {
                targetpos.centerLat = center.Lat;
                targetpos.centerLng = center.Lng;
                UnitCore.Instance.TargetObj = new Target()
                {
                    Status = "已定位",
                    UTCTime = targetpos.Time,
                    Longitude = (float)Util.LongOffset(targetpos.centerLng, targetpos.centerLat, targetpos.X),
                    Latitude = (float)Util.LatOffset(targetpos.centerLat,targetpos.Y),
                    Depth = (float)targetpos.Z,
                };
                log +="定位结果:" +  "long:" + UnitCore.Instance.TargetObj.Longitude + "  lat:" + UnitCore.Instance.TargetObj.Latitude;
                RefreshTarget();
            }
            else
            {
                log += "定位结果:未成功定位";
            }
            UnitCore.Instance.MonTraceService.Save("Position", log);
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
            if (Buoy1!=null)
                Util.GetReltXY(new PointLatLng(Buoy1.gps.Latitude,Buoy1.gps.Longitude),center,out x,out y);
            Buoy1Center = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");
            if (Buoy2 != null)
                Util.GetReltXY(new PointLatLng(Buoy2.gps.Latitude, Buoy2.gps.Longitude), center, out x, out y);
            Buoy2Center = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");
            if (Buoy3 != null)
                Util.GetReltXY(new PointLatLng(Buoy3.gps.Latitude, Buoy3.gps.Longitude), center, out x, out y);
            Buoy3Center = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");

            if (Buoy4 != null)
                Util.GetReltXY(new PointLatLng(Buoy4.gps.Latitude, Buoy4.gps.Longitude), center, out x, out y);
            Buoy4Center = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");
            if(ObjTarget!=null)
                Util.GetReltXY(new PointLatLng(ObjTarget.Latitude, ObjTarget.Longitude), center, out x, out y);
            z=-ObjTarget.Depth;
            ObjectCenter = x.ToString("F02") + "," + y.ToString("F02") + "," + z.ToString("F02");
            if (AutoTrack)
            {
                //UnitCore.Instance.PosView3D.CameraController.ChangeDirection(new Vector3D(0, 0, -2000), new Vector3D(0, 1, 0), 1000);
                UnitCore.Instance.PosView3D.Camera.Position = new Point3D(x, y, z+4000);
            }
            //CamPos = x.ToString("F02") + "," + y.ToString("F02") + "," + (z-2000).ToString("F02");
            UpdataTrack(x, y, z);

        }
        public override void Initialize()
        {
            //ObjTarget = new Target();
            ObjTarget = null;
            Dt = new DispatcherTimer(TimeSpan.FromSeconds(UnitCore.Instance.MonConfigueService.GetSetup().ValidInterval), DispatcherPriority.DataBind, CalTargetLocateCallBack, Dispatcher.CurrentDispatcher);
            Buoy1 = null;
            Buoy2 = null;
            Buoy3 = null;
            Buoy4 = null;
            CamPos = "0,0,4000";
            TrackVisible = false;
            ///test case
            /*Buoy1 = new Buoy(1);
            Buoy1.gps.Latitude = 39.58544f;
            Buoy1.gps.Longitude = 116.3887f;
            Buoy1.Range = 2400.4949f;
            Buoy2 = new Buoy(2);
            Buoy2.gps.Latitude = 39.58544f;
            Buoy2.gps.Longitude = 116.418932f;
            Buoy2.Range = 2400.81258f;
            Buoy3 = new Buoy(3);
            Buoy3.gps.Latitude = 39.6056224f;
            Buoy3.gps.Longitude = 116.418932f;
            Buoy3.Range = 2400.81258f;
            Buoy4 = new Buoy(4);
            Buoy4.gps.Latitude = 39.6056224f;
            Buoy4.gps.Longitude = 116.3887f;
            Buoy4.Range = 2400.4949f;
            var lpoint1 = new Locate2D(DateTime.UtcNow, Buoy1.gps.Longitude, Buoy1.gps.Latitude, Buoy1.Range);
            MonProtocol.TriangleLocate.Buoys.Add(1, lpoint1);
            var lpoint2 = new Locate2D(DateTime.UtcNow, Buoy2.gps.Longitude, Buoy2.gps.Latitude, Buoy2.Range);
            MonProtocol.TriangleLocate.Buoys.Add(2, lpoint2);
            var lpoint3 = new Locate2D(DateTime.UtcNow, Buoy3.gps.Longitude, Buoy3.gps.Latitude, Buoy3.Range);
            MonProtocol.TriangleLocate.Buoys.Add(3, lpoint3);
            var lpoint4 = new Locate2D(DateTime.UtcNow, Buoy4.gps.Longitude, Buoy4.gps.Latitude, Buoy4.Range);
            MonProtocol.TriangleLocate.Buoys.Add(4, lpoint4);
            UnitCore.Instance.TargetObj.Longitude = 116.39999f;
            UnitCore.Instance.TargetObj.Latitude = 39.59355f;*/

            ///
            //Refresh3DView();
        }
        public override void InitializePage(object extraData)
        {
            AboutVisibility = false;
            MapMode = 0;
            if(Dt.IsEnabled==false)
                Dt.Start();
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
        private void RefreshTarget()
        {
            ObjectMarker targetMarker = null;
            GMapMarker target = null;
            ObjTarget = null;
            ObjTarget = UnitCore.Instance.TargetObj;
            //refresh 2D target
            if (MapMode==0)
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
                    targetMarker.Refresh(UnitCore.Instance.TargetObj);
                    var point = new PointLatLng(UnitCore.Instance.TargetObj.Latitude, 
                        UnitCore.Instance.TargetObj.Longitude);
                    point.Offset(UnitCore.Instance.MainMapCfg.MapOffset.Lat,
                        UnitCore.Instance.MainMapCfg.MapOffset.Lng);
                    target.Position = point;
                    //remove legacy route
                    var isExist = UnitCore.Instance.mainMap.Markers.Contains(UnitCore.Instance.TargetRoute);
                    UnitCore.Instance.mainMap.Markers.Remove(UnitCore.Instance.TargetRoute);
                    UnitCore.Instance.routePoint.Remove(PointLatLng.Zero);
                    UnitCore.Instance.routePoint.Add(point);
                    if (UnitCore.Instance.routePoint.Count > 300)
                        UnitCore.Instance.routePoint.RemoveAt(0);
                    UnitCore.Instance.TargetRoute = new GMapMarker(UnitCore.Instance.routePoint[0]);
                    {
                        UnitCore.Instance.TargetRoute.Tag = 101;
                        UnitCore.Instance.TargetRoute.Route.AddRange(UnitCore.Instance.routePoint);
                        UnitCore.Instance.TargetRoute.RegenerateRouteShape(UnitCore.Instance.mainMap);

                        UnitCore.Instance.TargetRoute.ZIndex = -1;
                    }
                    if(isExist)
                        UnitCore.Instance.mainMap.Markers.Add(UnitCore.Instance.TargetRoute);

                    if (UnitCore.Instance.AutoTrace)
                        UnitCore.Instance.mainMap.Position = point;
                }));
            }
            else//3D
            {
                Refresh3DView();
                if(ObjInfoVisible)
                {
                    TargetInfo = ObjTarget.Name + "   " + ObjTarget.Time + "(UTC)\r" +
                    "经度:" + ObjTarget.Longitude.ToString("F06") + "\r|纬度:" +
                    ObjTarget.Latitude.ToString("F06") + "\r" +
                    "测算深度:" + ObjTarget.Depth.ToString("F02") +
                    "测距状态:" + ObjTarget.Status;
                }
                else
                {
                    TargetInfo = "";
                }
            }
        }
        private void RefreshBuoy(int index,Buoy buoy)
        {
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
                    "测距:" + Buoy1.Range.ToString("F02");

                Buoy2Info = Buoy2.Name + "   " + Buoy2.Time + "(UTC)\r" +
                    "经度:" + Buoy2.gps.Longitude.ToString("F06") + "\r纬度:" +
                    Buoy2.gps.Latitude.ToString("F06") + "\r" +
                    "测距:" + Buoy2.Range.ToString("F02");

                Buoy3Info = Buoy3.Name + "   " + Buoy3.Time + "(UTC)\r" +
                    "经度:" + Buoy3.gps.Longitude.ToString("F06") + "\r纬度:" +
                    Buoy3.gps.Latitude.ToString("F06") + "\r" +
                    "测距:" + Buoy3.Range.ToString("F02");

                Buoy4Info = Buoy4.Name + "   " + Buoy4.Time + "(UTC)\r" +
                    "经度:" + Buoy4.gps.Longitude.ToString("F06") + "\r纬度:" +
                    Buoy4.gps.Latitude.ToString("F06") + "\r" +
                    "测距:" + Buoy4.Range.ToString("F02");
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
                    TargetInfo = ObjTarget.Name + "   " + ObjTarget.Time + "(UTC)\r" +
                    "经度:" + ObjTarget.Longitude.ToString("F06") + "\r|纬度:" +
                    ObjTarget.Latitude.ToString("F06") + "\r" +
                    "测算深度:" + ObjTarget.Depth.ToString("F02")+
                    "测距状态:" + ObjTarget.Status;
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
                    m.Children.Add(new GeometryModel3D(gm.ToMesh(), Materials.Gold));
                    TrackModel = m;
                }
                SetPropertyValue(() => TrackVisible, value);

            }
        }
        private void RmoveTrack()
        {
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
                m.Children.Add(new GeometryModel3D(gm.ToMesh(), Materials.Gold));
                TrackModel = m;
            }
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

        public Target ObjTarget
        {
            get { return GetPropertyValue(() => ObjTarget); }
            set { SetPropertyValue(() => ObjTarget, value); }
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

        public Model3D Buoy1Model
        {
            get { return GetPropertyValue(() => Buoy1Model); }
            set { SetPropertyValue(() => Buoy1Model, value); }
        }
        public Model3D Buoy2Model
        {
            get { return GetPropertyValue(() => Buoy2Model); }
            set { SetPropertyValue(() => Buoy2Model, value); }
        }
        public Model3D Buoy3Model
        {
            get { return GetPropertyValue(() => Buoy3Model); }
            set { SetPropertyValue(() => Buoy3Model, value); }
        }
        public Model3D Buoy4Model
        {
            get { return GetPropertyValue(() => Buoy4Model); }
            set { SetPropertyValue(() => Buoy4Model, value); }
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
        #endregion
    }
}
