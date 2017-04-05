using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LOUV.Torp.ICore;
using LOUV.Torp.CommLib;
using LOUV.Torp.BaseType;
using LOUV.Torp.CommLib.TCP;
using LOUV.Torp.CommLib.UDP;
namespace LOUV.Torp.LiveService
{
    public class NetLiveService_Torp : IMonNetCore
    {
        private readonly static object SyncObject = new object();
        private static IMonNetCore _netInstance;
        private ITCPClientService _tcpShellService;
        private ITCPClientService _tcpDataService;

        private TcpClient _cmdtcpClient;
        private TcpClient _datatcpClient;

        //udp网络
        private UdpClient _udpListClient;
        private UdpClient _udpBroadClient;
        private IUDPService _udpTraceService;
        private UDPBaseComm _udpBroadCaster;
        private CommNet _commConf;
        private Observer<CustomEventArgs> _DataObserver;
        public string Error { get; set; }
        public bool IsInitialize { get; set; }
        public int SendBytes { get; set; }
        public UdpClient UDPBroadCaster { get; set; }

        public static IMonNetCore GetInstance(CommNet conf, Observer<CustomEventArgs> observer)
        {
            lock (SyncObject)
            {
                if (conf!=null)
                    return _netInstance ?? (_netInstance = new NetLiveService_Torp(conf, observer));
                else
                {
                    return null;
                }
            }
        }

        protected NetLiveService_Torp(CommNet conf, Observer<CustomEventArgs> observer)
        {
            _commConf = conf;
            _DataObserver = observer;
        }

        public IUDPService UDPDataService
        {
            get
            {
                return _udpTraceService ?? (_udpTraceService = (new TorpDataServiceFactory()).CreateService());
            }
        }
        /// <summary>
        /// tcp数据交换初始化
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public bool StartTCPService()
        {
            IsTCPWorking = false;
            // 同步方法，会阻塞进程，调用init用task
            TCPShellService.ConnectSync();
            TCPDataService.ConnectSync();
            TCPShellService.Register(NetDataObserver);
            TCPDataService.Register(NetDataObserver);
            if (TCPShellService.Connected && TCPShellService.Start() && TCPDataService.Connected &&
                TCPDataService.Start())
            {
                IsTCPWorking = true;
                return IsTCPWorking;
            }
                
            return IsTCPWorking;
        }
        public void StopTCpService()
        {
            TCPShellService.UnRegister(NetDataObserver);
            TCPShellService.Stop();

            TCPDataService.UnRegister(NetDataObserver);
           TCPDataService.Stop();
            IsTCPWorking = false;
        }
        public bool StartUDPService()
        {
            IsUDPWorking = false;
            if (!UDPDataService.Start()) return IsUDPWorking;
            UDPDataService.Register(NetDataObserver);
            //if (!UDPDataService.Start()) return false;
            //UDPDataService.Register(NetDataObserver);
            IsUDPWorking = true;
            return IsUDPWorking;
        }

        public void StopUDPService()
        {
            UDPDataService.Stop();
            UDPDataService.UnRegister(NetDataObserver);
            //UDPDataService.Stop();
            //UDPDataService.UnRegister(NetDataObserver);
            IsUDPWorking = false;
        }

        public ITCPClientService TCPDataService
        {
            get
            {
                return _tcpDataService ?? (_tcpDataService = (new TCPDataServiceFactory()).CreateService());
            }
        }

        public ITCPClientService TCPShellService
        {
            get
            {
                return _tcpShellService ?? (_tcpShellService = (new TCPShellServiceFactory()).CreateService());
            }
        }

        

        public Observer<CustomEventArgs> NetDataObserver
        {
            get { return _DataObserver; }
            set { _DataObserver = value; }
        }

     
        public void Initialize()
        {
            if (IsInitialize)
            {
                throw new Exception("服务已初始化");
            }
            /*
            _cmdtcpClient = new TcpClient { SendTimeout = 1000 };
            _datatcpClient = new TcpClient { SendTimeout = 1000 };
            if (!TCPShellService.Init(_cmdtcpClient, IPAddress.Parse(IPAddress.Any), _commConf.CmdPort) ||
                (!TCPDataService.Init(_datatcpClient, IPAddress.Parse(_commConf.IP), _commConf.DataPort)))
                throw new Exception("网络初始化失败,请检查网络连接状态并重启程序");
         */
            _udpListClient = new UdpClient(_commConf.RecvPort);
            if (!UDPDataService.Init(_udpListClient)) throw new Exception("UDP网络绑定失败");
           
            _udpBroadClient = new UdpClient(_commConf.BroadPort+100);//绑定一个比广播端口大100的端口
            _udpBroadClient.EnableBroadcast = true;
            IsInitialize = true;
        }

        public void Stop()
        {
            if (IsInitialize)
            {

                //StopTCpService();
                StopUDPService();


            }

            IsInitialize = false;
        }

        public void Start()
        {
            
            if (_commConf == null || _DataObserver == null)
                throw new Exception("网络通信无法设置");
            //if (!StartTCPService()) throw new Exception("网络服务无法启动");
            if (!StartUDPService()) throw new Exception("启动广播网络失败");
 
        }


        public Task<bool> SendConsoleCMD(string cmd)
        {
                var shellcmd = new ACNTCPShellCommand(_cmdtcpClient, cmd);
                return Command.SendTCPAsync(shellcmd);
        }

        public Task<bool> SendCMD(byte[] buf)
        {
            var ret = SendConsoleCMD("gd -n");
            if (ret.Result)
            {
                TaskEx.Delay(100);
                var cmd = new ACNTCPDataCommand(_datatcpClient, buf);
                return Command.SendTCPAsync(cmd);
            }
            return ret;


        }

        public async Task<bool> DownloadFile(Stream file, DownLoadFileType type)
        {
            SendBytes = 0;
            string argu = "";
            switch (type)
            {
                case DownLoadFileType.BootLoader:
                    argu = " -b";
                    break;
                case DownLoadFileType.FPGA:
                    argu = " -f";
                    break;
                case DownLoadFileType.FixFirm:
                    argu = " -u";
                    break;
                case DownLoadFileType.FloatM2:
                    argu = " -t -m2";
                    break;
                case DownLoadFileType.FloatM4:
                    argu = " -t - m4";
                    break;
                case DownLoadFileType.RltUpdate:
                    argu = " -l";
                    break;
                case DownLoadFileType.Wave:
                    argu = "";
                    break;
                default:
                    argu = "";
                    break;

            }
            var shellcmd = new ACNTCPShellCommand(_cmdtcpClient, "gd" + argu);
            var ret = Command.SendTCPAsync(shellcmd);
            if(await ret)
            {
                var datacmd = new ACNTCPStreamCommand(_datatcpClient, file, reportprogress);
                TCPDataService.Register(datacmd);
                return await Command.SendTCPAsync(datacmd).ContinueWith(x =>
                {
                    TCPDataService.UnRegister(datacmd);
                    return x.Result;
                });
            }
            return false;
        }


        private void reportprogress(int i)
        {
            SendBytes = i;
        }




        public int BroadCast(byte[] buf)
        {
            return UDPBroadCaster.Send(buf, buf.Length, IPAddress.Broadcast.ToString(), _commConf.BroadPort);
        }

        

       

        public bool IsUDPWorking { get; set; }


        public bool IsTCPWorking { get; set; }
        public bool IsWorking
        {
            get
            {
                return IsUDPWorking;
            }
            set
            {
                throw new NotImplementedException();
            }

        }
    }
}
