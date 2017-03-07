using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Controls;

//using LOUV.Torp.CommLib.Properties;
using LOUV.Torp.CommLib.UDP;
using LOUV.Torp.LiveService;
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
using LOUV.Torp.Monitor.Controls.MapCustom;

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
        private IMonNetCore _iNetCore;
        //串口服务接口，如果有
        //private ICommCore _iCommCore;
        //文件服务接口
        private IFileCore _iFileCore;
        private MonTraceService _MonTraceService;
        //基础配置信息
        private MonConf _monConf;//系统设置类
        private MonitorDataObserver _observer;
        private bool _serviceStarted;
        private CommNet _MonConfInfo;
        public SettleSoundFile SoundFile = null;
        public string Error { get; private set; }

        public Mutex ACMMutex { get; set; }//全局解析锁

        public Model3D BuoyModel { get; set; }//buoy 模型
        public Model3D ObjModel { get; set; }//目标模型
        public Mutex BuoyLock { get; set; }//全局buoy列表操作锁
        public Hashtable Buoy = new Hashtable();
        //public Hashtable InfoBoard = new Hashtable();
        public Target TargetObj = new Target();
        public MapCfg MainMapCfg { get; set; }//map配置
        public Setup SetupCfg { get; set; }//计算配置
        public Map mainMap = null;//指向MainMap
        InitialData initpara = new InitialData();
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
        private async Task<bool> LoadAssets()
        {
            if(_monConf==null)
            {
                EventAggregator.PublishMessage(new LogEvent("配置没有初始化", LogType.Both));
                return false;
            }
            var buoypath = _monConf.GetBuoyModel();
            if (buoypath == null)
                throw new Exception("未找到3D组件！");
            buoypath = _monConf.MyExecPath + "\\" + buoypath;//found
            BuoyModel = await LoadAsync(buoypath, false);
            if (BuoyModel == null)
                throw new Exception("加载浮标组件失败！");
            var objpath = _monConf.GetObjModel();
            if (objpath == null)
                throw new Exception("未找到3D组件！");
            objpath = _monConf.MyExecPath + "\\" + objpath;//found
            ObjModel = await LoadAsync(objpath, false);
            if (ObjModel == null)
                throw new Exception("加载模型组件失败！");
            return true;
        }
        public bool LoadConfiguration()
        {
            bool ret = true;
            try
            {
                _monConf = MonConf.GetInstance();
                _MonConfInfo = _monConf.GetNet();
                if(_MonConfInfo==null)
                {
                    throw _monConf.ex;
                }
                SetupCfg = _monConf.GetSetup();
                if (SetupCfg == null)
                {
                    throw _monConf.ex;
                }
                _serviceStarted = true;//if failed never get here

                return _serviceStarted;
    
            }
            catch (Exception ex)
            {
                ret = false;
                EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));
            }
            return ret;
        }
        private void ReadInitPara()
        {
            string gridname = MonConf.GetInstance().MyExecPath+ "\\" + "default.ini";
            Stream stream = null;
            try
            {
                stream = new FileStream(gridname, FileMode.Open, FileAccess.Read, FileShare.Read);
                IFormatter formatter = new BinaryFormatter();
                initpara = (InitialData)formatter.Deserialize(stream);
                stream.Close();
                Buoy = initpara.buoy;
                //clean the range info,only need gps location
                foreach(var buoy in Buoy.Values)
                {
                    var b = (Buoy)buoy;
                    b.liteRange = new LiteRange();
                    b.teleRange = new TeleRange();
                }
                //InfoBoard = initpara.info;

            }
            catch (Exception MyEx)
            {
                if (stream!=null)
                    stream.Close();
                var gpsinfo = new GpsInfo()
                {
                    UTCTime = DateTime.UtcNow,
                    Latitude = 29.592966F,
                    Longitude = 118.983188F,
                };

                var by1 = new Buoy()
                {
                    Id = 1,
                    gps= gpsinfo,
                };
                Buoy.Add(0,by1);
                var by2 = new Buoy()
                {
                    Id = 2,
                    gps= gpsinfo,
                };
                Buoy.Add(1,by2);
                var by3 = new Buoy()
                {
                    Id = 3,
                    gps= gpsinfo,
                };
                Buoy.Add(2,by3);
                var by4 = new Buoy()
                {
                    Id = 4,
                    gps= gpsinfo,
                };
                Buoy.Add(3,by4);
                //InfoBoard = new Hashtable();
                SaveInitPara();
                var errmsg = new ErrorEvent(MyEx, LogType.Both)
                {
                    Message = "浮标信息读取失败，使用默认参数"
                };
                UnitCore.Instance.EventAggregator.PublishMessage(errmsg);
            }
        }

        private void SaveInitPara()
        {
            string gridname = MonConf.GetInstance().MyExecPath + "\\" + "default.ini";
            Stream stream = new FileStream(gridname, FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                initpara.buoy = Buoy;
                //initpara.info = InfoBoard;

                formatter.Serialize(stream, initpara);
                stream.Close();
            }
            catch (Exception MyEx)
            {
                stream.Close();
                var errmsg = new ErrorEvent(MyEx, LogType.Both)
                {
                    Message = "保存浮标信息失败：" + MyEx.Message,
                };
                UnitCore.Instance.EventAggregator.PublishMessage(errmsg);
            }
        }
        public IEventAggregator EventAggregator
        {
            get { return _eventAggregator ?? (_eventAggregator = UnitKernal.Instance.EventAggregator); }
        }



        public IMonNetCore NetCore
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
                ReadInitPara();
                if(NetCore.IsInitialize)
                    NetCore.Stop();
                NetCore.Initialize();
                if(!NetCore.StartUDPService())//只启动udp服务，tcp服务单独启动
                   
                if(!MonTraceService.CreateService()) throw new Exception("数据服务启动失败");
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
            SaveInitPara();
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
        public MonConf MonConfigueService
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

        private async Task<Model3DGroup> LoadAsync(string model3DPath, bool freeze)
        {
            return await Task.Factory.StartNew(() =>
            {
                var mi = new ModelImporter();

                // Alt 1. - freeze the model 
                return mi.Load(model3DPath, null, true);

            });
        }
    }
}
