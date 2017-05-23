using System;
using System.Net.Sockets;
using System.Text;
using DevExpress.Xpf.Editors.ExpressionEditor;
using LOUV.Torp.CommLib;
using LOUV.Torp.Monitor.Events;
using LOUV.Torp.Monitor.Helpers;
using DevExpress.Xpf.Core;
using Microsoft.Win32;
using System.Threading;
using LOUV.Torp.BaseType;
using LOUV.Torp.Monitor.Controls.MapCustom;
using System.Windows;
using GMap.NET;
using LOUV.Torp.Monitor.ViewModel;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LOUV.Torp.Monitor.Core
{
    public class MonitorDataObserver:Observer<CustomEventArgs>
    {
  
        public void Handle(object sender, CustomEventArgs e)
        {
            if (e.ParseOK)
            {
              //
                try
                {
                    var buf = new byte[e.DataBuffer.Length + 2];
                    
                    var ip = e.CallSrc as string;
                    int id = 0;
                    id = Int16.Parse(ip.Substring(ip.Length - 1));
                    if (ip != null&&!UnitCore.Instance.IsReplay)
                    {
                        
                        //save
                        Buffer.BlockCopy( e.DataBuffer, 0, buf, 2, e.DataBuffer.Length);
                        Buffer.BlockCopy(BitConverter.GetBytes(id), 0, buf, 0, 2);
                        UnitCore.Instance.MonTraceService.Save("ALL", buf);
                        switch (id)
                        {
                            case 1:
                                UnitCore.Instance.MonTraceService.Save("Buoy1", e.DataBuffer);
                                break;
                            case 2:
                                UnitCore.Instance.MonTraceService.Save("Buoy2", e.DataBuffer);
                                break;
                            case 3:
                                UnitCore.Instance.MonTraceService.Save("Buoy3", e.DataBuffer);
                                break;
                            case 4:
                                UnitCore.Instance.MonTraceService.Save("Buoy4", e.DataBuffer);
                                break;
                            default:
                                break;

                        }
                    }

                    Buoy buoy = null;

                    if (UnitCore.Instance.Buoy.ContainsKey(id - 1))
                    {
                        buoy = ((Buoy)UnitCore.Instance.Buoy[id - 1]);
                    }
                    if (buoy == null)
                        return;
                    //类型标志
                    if (e.Mode == CallMode.UDPAns)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent("浮标命令发送成功", LogType.OnlyInfoandClose));
                        }));
                        return;
                    }
                    else if (e.Mode == CallMode.GPS) //gps
                    {
                        var gpsbuf = new byte[1030];
                        Buffer.BlockCopy(e.DataBuffer, 2, gpsbuf, 0, 1030);
                        var info = MonProtocol.MonProtocol.ParseGps(gpsbuf);
                        if(info!=null)
                            buoy.gps = info;

                    }
                    else if (e.Mode == CallMode.Range)
                    {
                        var litebuf = new byte[14];
                        Buffer.BlockCopy(e.DataBuffer, 2, litebuf, 0, 14);
                        var range = MonProtocol.MonProtocol.ParsePulseRange(litebuf);
                        
                        var gpsbuf = new byte[1030];
                        Buffer.BlockCopy(e.DataBuffer, 16, gpsbuf, 0, 1032 - 16);
                        var info = MonProtocol.MonProtocol.ParseGps(gpsbuf);
                        buoy.liteRange = range;
                        if (info != null)
                            buoy.gps = info;
                        buoy.Range = MonProtocol.MonProtocol.CalDistanceByLite(range);
                    }
                    else if (e.Mode == CallMode.TeleRange)
                    {
                        var litebuf = new byte[14];
                        Buffer.BlockCopy(e.DataBuffer, 2, litebuf, 0, 14);
                        var range = MonProtocol.MonProtocol.ParsePulseRange(litebuf);
                        
                        var length = BitConverter.ToUInt16(e.DataBuffer, 31);
                        var combuf = new byte[17 + length];
                        Buffer.BlockCopy(e.DataBuffer, 16, combuf, 0, 17 + length);
                        var telerange = MonProtocol.MonProtocol.ParseTeleRange(combuf, length);
                        
                        var gpsbuf = new byte[1030];
                        Buffer.BlockCopy(e.DataBuffer, 33 + length, gpsbuf, 0, 1032 - 33 - length);
                        var info = MonProtocol.MonProtocol.ParseGps(gpsbuf);
                        buoy.teleRange = telerange;
                        buoy.liteRange = range;
                        if (info != null)
                            buoy.gps = info;
                        buoy.Range = MonProtocol.MonProtocol.CalDistanceByTele(range,telerange);
                    }
                    if (buoy.gps == null)
                        return;
                    if(buoy.Range>0.5)
                    {
                        var lpoint = new Locate2D(buoy.gps.UTCTime, buoy.gps.Longitude, buoy.gps.Latitude, buoy.Range);
                        //remove possible duplicate data
                        MonProtocol.TriangleLocate.Buoys.Remove(id);
                        MonProtocol.TriangleLocate.Buoys.Add(id, lpoint);
                    }
                    
                    var point = new PointLatLng(buoy.gps.Latitude, buoy.gps.Longitude);
                    point.Offset(UnitCore.Instance.MainMapCfg.MapOffset.Lat,
                        UnitCore.Instance.MainMapCfg.MapOffset.Lng);
                    UnitCore.Instance.EventAggregator.PublishMessage(new RefreshBuoyInfoEvent(id - 1));
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (UnitCore.Instance.mainMap != null)
                        {
                            UnitCore.Instance.BuoyLock.WaitOne();
                            var itor = UnitCore.Instance.mainMap.Markers.GetEnumerator();
                            while (itor.MoveNext())
                            {
                                var marker = itor.Current;
                                if ((int)marker.Tag == id)
                                {
                                    
                                    if (marker.Shape is BuoyMarker buoymarker)
                                    {


                                        buoymarker.Refresh(buoy);
                                        marker.Position = point;

                                    }



                                }

                            }
                            UnitCore.Instance.BuoyLock.ReleaseMutex();
                        }
                    }));

                }
                catch (Exception ex)
                {
                    if(UnitCore.Instance.BuoyLock.WaitOne(100)==true)
                        UnitCore.Instance.BuoyLock.ReleaseMutex();
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));
                    }));
                }


            }
            else
            {
                if (e.Mode == CallMode.ErrMode)
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        UnitCore.Instance.NetCore.StopUDPService();
                       // UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(e.Ex, LogType.Both));
                    }));
                    
                }
            }
        }


    }
}
