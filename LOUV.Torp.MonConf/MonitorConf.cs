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
            catch (Exception)
            {
                return null;
            }

        }

        public CommNet GetNet()
        {
            var netcomm = new CommNet();
            try
            {
                netcomm.CmdPort = GetNetCmdPort();
                netcomm.DataPort = GetNetDataPort();
                netcomm.BroadPort = GetNetBroadPort();
                netcomm.RecvPort = GetNetRecvPort();
                return netcomm;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
