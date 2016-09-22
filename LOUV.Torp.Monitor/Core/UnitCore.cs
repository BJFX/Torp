﻿using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;
using LOUV.Torp.ACMP;
using LOUV.Torp.CommLib.Properties;
using LOUV.Torp.CommLib.UDP;
using LOUV.Torp.LiveService;
using LOUV.Torp.TraceFileService;
using TinyMetroWpfLibrary.EventAggregation;
using LOUV.Torp.CommLib;
using LOUV.Torp.Mov4500Conf;
using LOUV.Torp.Mov4500TraceService;
using LOUV.Torp.ICore;
using LOUV.Torp.BaseType;
using LOUV.Torp.Monitor.Events;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;
using LOUV.Torp.WaveBox;
using System.Net.NetworkInformation;
namespace LOUV.Torp.Monitor.Core
{
    /// <summary>
    /// 核心业务类，包括通信服务，数据解析，服务状态及一些其他的系统变量
    /// </summary>
    public class UnitCore
    {
        private readonly static object SyncObject = new object();
        //静态接口，用于在程序域中任意位置操作UnitCore中的成员
        private static UnitCore _instance;

        public AutoResetEvent PostMsgEvent_Tick;//同步事件,是否允许定时检测器录音

        //事件绑定接口，用于事件广播
        private IEventAggregator _eventAggregator;
        //网络服务接口
        private IMovNetCore _iNetCore;
        //串口服务接口，如果有
        private ICommCore _iCommCore;
        //文件服务接口
        private IFileCore _iFileCore;
        private MovTraceService _movTraceService;
        //基础配置信息
        private MovConf _mov4500Conf;//系统设置类
        private MovConfInfo _movConfInfo;//除通信以外其他设置类
        private CommConfInfo _commConf;//通信设置
        private Observer<CustomEventArgs> _observer; 
        private bool _serviceStarted = false;
        //通信机版本信息
        public string Version = "";
        public string Error { get; private set; }
        public MonitorMode WorkMode{get; set;}
        public Mutex ACMMutex { get; set; }//全局解析锁
        public byte[] Single = null;
        public byte[] RecvOrOK = null;
        public byte[] AskOrOK = null;
        public byte[] AgreeOrReqRise = null;
        public byte[] RiseOrUrgent = null;
        public byte[] Disg = null;
        public byte[] RelBuoy = null;
        public WaveControl Wave = null;
        public delegate void UpdateLiveViewHandle(ModuleType type, string msg, Image img);
        public UpdateLiveViewHandle LiveHandle;

        public delegate void AddFHLiveViewHandle(string msg);
        public AddFHLiveViewHandle AddFHHandle;

        public delegate void AddImgLiveViewHandle(Image img);
        public AddImgLiveViewHandle AddImgHandle;

        public AutoResetEvent PostMsgEvent_BPpara = new AutoResetEvent(false);//同步事件,是否允许发送配置参数消息
        public AutoResetEvent PostMsgEvent_BP = new AutoResetEvent(true);//同步事件,是否允许发送信号处理消息
        public AutoResetEvent PostMsgEvent_BPsend = new AutoResetEvent(false);//同步事件,是否允许向串口写数据

        //MFSK文字还可输入字符数
        private int _MFSK_LeftSize = 20;

        public MovTraceService MovTraceService
        {
            get { return _movTraceService ?? (_movTraceService = new MovTraceService(WorkMode)); }
        }


        public static UnitCore GetInstance()
        {
            lock (SyncObject)
            {

                return _instance ?? (_instance = new UnitCore());
            }
        }

        protected UnitCore()
        {
            
            ACMMutex = new Mutex();
            PostMsgEvent_Tick=new AutoResetEvent(true);//定时器

        }

        public bool LoadConfiguration()
        {
            bool ret = true;
            try
            {
                if (_mov4500Conf==null)
                {
                    _mov4500Conf = MovConf.GetInstance();
                    _mov4500Conf.SetGMode((MonitorGMode)Enum.Parse(typeof(MonitorGMode), "1"));//首次打开需要将增益模式设置为自动模式
                }
                else
                {
					_mov4500Conf = MovConf.GetInstance();
                }
                _commConf = _mov4500Conf.GetCommConfInfo();
                _movConfInfo = _mov4500Conf.GetMovConfInfo();
                WorkMode = (MonitorMode)Enum.Parse(typeof(MonitorMode),_movConfInfo.Mode.ToString());
                NetLiveService_ACM.RenewNetLiveService_ACM(_commConf, _movConfInfo);
            }
            catch (Exception ex)
            {
                ret = false;
                EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));
            }
            return ret;
        }


        public IEventAggregator EventAggregator
        {
            get { return _eventAggregator ?? (_eventAggregator = UnitKernal.Instance.EventAggregator); }
        }



        public IMovNetCore NetCore
        {
            get { return _iNetCore ?? (_iNetCore = NetLiveService_ACM.GetInstance(_commConf, _movConfInfo, Observer)); }
        }
        public ICommCore CommCore
        {
            get { return _iCommCore ?? (_iCommCore = CommService_BPADCP.GetInstance(_commConf, Observer)); }
        }

        private bool LoadMorse()
        {
            string soundpath = MovConf.GetInstance().MyExecPath + "\\" + "morse";
            if (Directory.Exists(soundpath))
            {
                try
                {
                    Single = File.ReadAllBytes(soundpath + "\\" + "1.dat");
                    RecvOrOK = File.ReadAllBytes(soundpath + "\\" + "3.dat");
                    AskOrOK = File.ReadAllBytes(soundpath + "\\" + "2.dat");
                    AgreeOrReqRise = File.ReadAllBytes(soundpath + "\\" + "22.dat");
                    RiseOrUrgent = File.ReadAllBytes(soundpath + "\\" + "5.dat");
                    Disg = File.ReadAllBytes(soundpath + "\\" + "33.dat");
                    RelBuoy = File.ReadAllBytes(soundpath + "\\" + "222.dat");
                    return true;
                }
                catch (Exception)
                {
                    //do nothing
                }
                return false;
            }
            return false;
        }
        public bool Start()
        {
            try
            {
                NetworkChange.NetworkAvailabilityChanged += new
            NetworkAvailabilityChangedEventHandler(AvailabilityChangedCallback);
                if(!LoadConfiguration()) throw new Exception("无法读取基本配置");
                ACM4500Protocol.Init(_mov4500Conf.GetOASID(), (MonitorMode)1);
                if (!LoadMorse()) throw new Exception("无法读取Morse数据");
                if(NetCore.IsInitialize)
                    NetCore.Stop();
                NetCore.Initialize();
                NetCore.Start();//只启动udp服务，tcp服务单独启动
                if (WorkMode == MonitorMode.SUBMARINE)
                {
                    if (CommCore.IsInitialize)
                        CommCore.Stop();
                    CommCore.Initialize();
                    CommCore.Start();
                }                
                if(!MovTraceService.CreateService()) throw new Exception("数据保存服务启动失败");
                _serviceStarted = true;
                Error = NetCore.Error;
                return _serviceStarted;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));
                return false;
            }
            
            
        }

        private void AvailabilityChangedCallback(object sender, NetworkAvailabilityEventArgs e)
        {
            NetworkAvailabilityEventArgs myEg = (NetworkAvailabilityEventArgs)e;
            if (!myEg.IsAvailable)
            {
                if (NetCore.IsTCPWorking)
                {
                    NetCore.StopTCpService();
                }

                App.Current.Dispatcher.Invoke(new Action(()=>
                {
                    EventAggregator.PublishMessage(new LogEvent("网络出错，请检查网络！", LogType.Both));
                }));

            }
        }

        public void Stop()
        {
            if(NetCore==null)
                return;
            if (NetCore.IsTCPWorking)
                NetCore.StopTCpService();
            if (NetCore.IsUDPWorking)
                NetCore.StopUDPService();
            /*if (CommCore.IsWorking)
                CommCore.Stop();*/
            if (MovTraceService!=null)
                MovTraceService.TearDownService();
            _serviceStarted = false;
            
        }
        public bool IsWorking
        {
            get { return _serviceStarted; }
        }

        public int MFSK_LeftSize
        {
            get { return _MFSK_LeftSize; }
            set { _MFSK_LeftSize = value; }
        }
        

        public MovConf MovConfigueService
        {
            get { return _mov4500Conf; }
        }

        public Observer<CustomEventArgs> Observer
        {
            get { return _observer ?? (_observer = new Mov4500DataObserver()); }

        }

        public IFileCore FileCore
        {
            get { return _iFileCore; }
            set { _iFileCore = value; }
        }

        public static UnitCore Instance
        {
            get { return GetInstance(); }
        }
        
        
    }
}
