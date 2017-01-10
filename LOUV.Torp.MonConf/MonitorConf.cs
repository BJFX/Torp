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

        protected int GetGPSPort()
        {
            string[] str = { "GPS", "GPSPort" };
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
                gpscomm.GPSPort = GetGPSPort();
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
        public string GetModelPath(string name)
        {
            string[] str = { "Model", name };
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
    }
}
