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

namespace LOUV.Torp.Monitor.ViewModel
{
    public class HomePageViewModel : ViewModelBase, IHandleMessage<ShowAboutSlide>, 
        IHandleMessage<RefreshBuoyInfoEvent>,
        IHandleMessage<SwitchMapModeEvent>
    {
        private DispatcherTimer Dt;
        private List<Point3D> Path = new List<Point3D>();//target 3D track
        private void CalTargetLocateCallBack(object sender, EventArgs e)
        {
            //UnitCore.Instance.TargetObj.Longitude += 0.1f;
            //UnitCore.Instance.TargetObj.Latitude += 0.1f;
            //RefreshTarget();
            //return;
            UnitCore.Instance.BuoyLock.WaitOne();
            PointLatLng center = PointLatLng.Zero;
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
        public override void Initialize()
        {
            ObjTarget = new Target();
            Buoy1 = new Buoy(1);
            Buoy2 = new Buoy(2);
            Buoy3 = new Buoy(3);
            Buoy4 = new Buoy(4);
            Dt = new DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.DataBind, CalTargetLocateCallBack, Dispatcher.CurrentDispatcher);
            Buoy1 = null;
            Buoy2 = null;
            Buoy3 = null;
            Buoy4 = null;
            /*UnitCore.Instance.TargetObj = new Target()
            {
                Status = "已定位",
                UTCTime = DateTime.UtcNow,
                Longitude = 116.3187f,
                Latitude = 39.98543f,
                Depth = 18.2334454f,
            };*/
            /*var lpoint1 = new Locate2D(DateTime.UtcNow, 116.3187, 39.98544, 24.4949);
            MonProtocol.TriangleLocate.Buoys.Add(1, lpoint1);
            var lpoint2 = new Locate2D(DateTime.UtcNow, 116.318932, 39.98544, 20.81258);
            MonProtocol.TriangleLocate.Buoys.Add(2, lpoint2);
            var lpoint3 = new Locate2D(DateTime.UtcNow, 116.3187, 39.9856224, 38.2326355);
            MonProtocol.TriangleLocate.Buoys.Add(3, lpoint3);*/
        
        }

        public override void InitializePage(object extraData)
        {
            AboutVisibility = false;
            MapMode = 0;
            if(Dt.IsEnabled==false)
                Dt.Start();
        }

        private void RefreshTarget()
        {
            ObjectMarker targetMarker = null;
            GMapMarker target = null;
            //refresh 2D target
            if(MapMode==0)
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
                ObjTarget = null;
                ObjTarget = UnitCore.Instance.TargetObj;
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
        }
        
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

        
    }
}
