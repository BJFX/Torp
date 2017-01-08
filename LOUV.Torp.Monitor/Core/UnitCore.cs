using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;

//using LOUV.Torp.CommLib.Properties;
using LOUV.Torp.CommLib.UDP;
using LOUV.Torp.LiveService;
using LOUV.Torp.TraceService;
using TinyMetroWpfLibrary.EventAggregation;
using LOUV.Torp.CommLib;
using LOUV.Torp.MonitorConf;
using LOUV.Torp.MonTrace;
using LOUV.Torp.ICore;
using LOUV.Torp.BaseType;
using LOUV.Torp.Monitor.Events;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;
//using LOUV.Torp.WaveBox;
using System.Net.NetworkInformation;
using LOUV.Torp.MonitorConf;
using LOUV.Torp.LiveService;
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
        private INetCore _iNetCore;
        //串口服务接口，如果有
        private ICommCore _iCommCore;
        //文件服务接口
        private IFileCore _iFileCore;
        private MonTraceService _MonTraceService;
        //基础配置信息
        private MonConf _monConf;//系统设置类
        private MonitorDataObserver _observer;
        private bool _serviceStarted;
        private CommNet _MonConfInfo;

        public string Error { get; private set; }

        public Mutex ACMMutex { get; set; }//全局解析锁


        public MonTraceService MonTraceService
        {
            get { return _MonTraceService ?? (_MonTraceService = new MonTraceService()); }
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
                if (_monConf==null)
                {
                    _monConf = MonConf.GetInstance();
                    
                }
                else
                {
                    _monConf = MonConf.GetInstance();
                }
                _MonConfInfo = _monConf.GetNet();
 
    
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



        public INetCore NetCore
        {
            get { return _iNetCore ?? (_iNetCore = NetLiveService_Torp.GetInstance(_MonConfInfo, Observer)); }
        }
      
        
        
        public bool Start()
        {
            try
            {
                NetworkChange.NetworkAvailabilityChanged += new
            NetworkAvailabilityChangedEventHandler(AvailabilityChangedCallback);
                if(!LoadConfiguration()) throw new Exception("无法读取基本配置");
                //
                
                if(NetCore.IsInitialize)
                    NetCore.Stop();
                NetCore.Initialize();
                NetCore.Start();//只启动udp服务，tcp服务单独启动
                   
                if(!MonTraceService.CreateService()) throw new Exception("数据保存服务启动失败");
                _serviceStarted = true;
                Error = NetCore.Error;
                return ServiceStarted;
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
                    EventAggregator.PublishMessage(new LogEvent("网络连接出错，请检查网络再连接节点！", LogType.Both));
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
            if (MonTraceService!=null)
                MonTraceService.TearDownService();
            _serviceStarted = false;
            
        }
        public MonConf MovConfigueService
        {
            get { return _monConf; }
        }

        public Observer<CustomEventArgs> Observer
        {
            get { return _observer ?? (_observer = new MonitorDataObserver()); }

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

        public bool ServiceStarted
        {
            get { return _serviceStarted; }
        }
    }
}
