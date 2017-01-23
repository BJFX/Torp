using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TinyMetroWpfLibrary.Utility;
using LOUV.Torp.BaseType;

namespace LOUV.Torp.MonitorConf
{
    public class MonConf
    {
        private static readonly object SyncObject = new object();
        private static MonConf _movConf;

        //配置文件
        private string xmldoc = "BasicConf.xml"; //const
        public Exception ex { get; set; }

        public static MonConf GetInstance()
        {
            lock (SyncObject)
            {
                return _movConf ?? (_movConf = new MonConf());
            }
        }

        public string MyExecPath;

        protected MonConf()
        {
            ex = null;
            MyExecPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
            xmldoc = MyExecPath + "\\" + xmldoc;

        }

        protected string GetValue(string[] str)
        {
            return XmlHelper.GetConfigValue(xmldoc, str);
        }

        protected bool SetValue(string[] str, string value)
        {
            return XmlHelper.SetConfigValue(xmldoc, str, value);
        }

        protected string GetGPSComm()
        {
            string[] str = { "GPS", "ComPort" };
            return GetValue(str);
        }

        protected int GetGPSDataRate()
        {
            string[] str = { "GPS", "DataRate" };
            return int.Parse(GetValue(str));
        }

        protected int GetNetDataPort()
        {
            string[] str = { "Net", "DataPort" };
            return int.Parse(GetValue(str));
        }
        protected int GetNetCmdPort()
        {
            string[] str = { "Net", "CmdPort" };
            return int.Parse(GetValue(str));
        }
        protected int GetNetBroadPort()
        {
            string[] str = { "Net", "BroadPort" };
            return int.Parse(GetValue(str));
        }
        protected int GetNetRecvPort()
        {
            string[] str = { "Net", "RecvPort" };
            return int.Parse(GetValue(str));
        }

        protected string GetNetIP()
        {
            string[] str = { "Net", "IP" };
            return GetValue(str);
        }
        public bool SetNetIP(string newip)
        {
            string[] str = { "Net", "IP" };
            return SetValue(str, newip);
        }
        public CommGPS GetGPS()
        {
            var gpscomm = new CommGPS();
            try
            {
                gpscomm.Comm = GetGPSComm();
                gpscomm.DataRate = GetGPSDataRate();
                return gpscomm;
            }
            catch (Exception e)
            {
                ex = e;
                return null;
            }

        }

        public CommNet GetNet()
        {
            var netcomm = new CommNet();
            try
            {
                netcomm.IP = GetNetIP();
                netcomm.CmdPort = GetNetCmdPort();
                netcomm.DataPort = GetNetDataPort();
                netcomm.BroadPort = GetNetBroadPort();
                netcomm.RecvPort = GetNetRecvPort();
                return netcomm;
            }
            catch (Exception e)
            {
                ex = e;
                return null;
            }
        }
        public string GetBuoyModel()
        {
            string[] str = { "Asset", "Buoy" };
            return GetValue(str);
        }
        /// <summary>
        /// model的相对路径
        /// </summary>
        /// <returns></returns>
        public string GetObjModel()
        {
            string[] str = { "Asset", "OBJ" };
            return GetValue(str);
        }

        public string GetVelProfileName()
        {
            string[] str = { "Profile", "Name" };
            return GetValue(str);
        }
        public bool SetVelProfileName(string filename)
        {
            string[] str = { "Profile", "Name" };
            return SetValue(str, filename);
        }

        public MapCfg LoadMapCfg()
        {
            var mapcfg = new MapCfg();
            mapcfg.Title = GetMapName();
            mapcfg.CenterLat = GetCenLat();
            mapcfg.CenterLng = GetCenLng();
            mapcfg.MapType = GetMapType();
            mapcfg.MapOffset = GetMapOffset();
            return mapcfg;
            
        }

        public Offset GetMapOffset()
        {
            var point = new Offset();
            point.Lat = GetMapOffsetLat();
            point.Lng = GetMapOffsetLong();
            return point;
        }
        public bool SetMapOffset(Offset point)
        {
            return SetMapOffsetLat(point.Lat) && SetMapOffsetLong(point.Lng);
        }
        private double GetMapOffsetLong()
        {
            string[] str = { "Map", "Offset", "Long" };
            return double.Parse(GetValue(str));
        }
        private double GetMapOffsetLat()
        {
            string[] str = { "Map", "Offset", "Lat" };
            return double.Parse(GetValue(str));
        }
        private bool SetMapOffsetLong(double lng)
        {
            string[] str = { "Map", "Offset", "Long" };
            return SetValue(str,lng.ToString());
        }
        private bool SetMapOffsetLat(double lat)
        {
            string[] str = { "Map", "Offset", "Lat" };
            return SetValue(str, lat.ToString());
        }
        public string GetMapType()
        {
            string[] str = { "Map", "MapType" };
            return GetValue(str);
        }
        public bool SetMapType(string type)
        {
            string[] str = { "Map", "MapType" };
            return SetValue(str, type);
        }

        public double GetCenLng()
        {
            string[] str = { "Map", "Center", "Long" };
            return double.Parse(GetValue(str));
        }
        public bool SetCenLng(double lng)
        {
            string[] str = { "Map", "Center","Long"};
            return SetValue(str, lng.ToString());
        }
        public double GetCenLat()
        {
            string[] str = { "Map", "Center", "Lat" };
            return double.Parse(GetValue(str));
        }

        public bool SetCenLat(double lat)
        {
            string[] str = { "Map", "Center", "Lat" };
            return SetValue(str, lat.ToString());
        }
        public string GetMapName()
        {
            string[] str = { "Map", "Name" };
            return GetValue(str);
        }
        public bool SetMapName(string title)
        {
            string[] str = { "Map", "Name" };
            return SetValue(str, title);
        }

    }
}
