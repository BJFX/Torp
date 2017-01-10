using System.Dynamic;
using System.IO;
using LOUV.Torp.CommLib;
//using LOUV.Torp.DAL;
using TinyMetroWpfLibrary.EventAggregation;
using LOUV.Torp.BaseType;
using System.Threading;
using System.Threading.Tasks;
namespace LOUV.Torp.ICore
{
    public enum DownLoadFileType
    {
        Wave,
        RltUpdate,
        FixFirm,
        FloatM2,
        FloatM4,
        FPGA,
        BootLoader,

    }
    public interface ICore
    {
        void Initialize();
        void Stop();
        void Start();
        bool IsWorking { get; set; }
        bool IsInitialize { get; set; }
        string Error { get; set; }
    }


    public interface IMonNetCore : ICore
    {
        //TCP客户端接收数据服务
        ITCPClientService TCPShellService { get; }

    //TCP客户端shell服务
        ITCPClientService TCPDataService { get; }

        Task<bool> SendConsoleCMD(string cmd);

        Task<bool> SendCMD(byte[] buf);
        Task<bool> DownloadFile(Stream file, DownLoadFileType type);
        Task<bool> BroadCast(byte[] buf);
        Task<bool> Listening();
        int SendBytes { get; }
        Observer<CustomEventArgs> NetDataObserver { get; }
        bool StartUDPService();
        void StopUDPService();

        void StopTCpService();

        bool StartTCPService();

        bool IsUDPWorking { get; set; }

        bool IsTCPWorking { get; set; }
    }
    public interface ICommCore:ICore
    {
        ISerialService SerialService { get; }
        Task<bool> SendConsoleCMD(string cmd);
        //串口数据接收服务
        ISerialService BPSerialService { get; }
        ISerialService ADCPSerialService { get; }

        Task<bool> SendConsoleCMD(byte[] cmd,int Bpid);
        Task<bool> SendLoaderCMD(string cmd);
        Task<bool> SendCMD(byte[] buf);

        Task<bool> SendFile(Stream file);
        void Sendbreak();
        Task<bool> Sendcs();
        /// <summary>
        /// 数据观察类，主要负责数据的解析和保存
        /// </summary>
        Observer<CustomEventArgs> CommDataObserver { get; }

    }
    public interface IFileCore : ICore
    {

        //文件服务
        //TBD
        /// <summary>
        /// 数据观察类，主要负责数据的解析和保存
        /// </summary>
        CommLib.Observer<CustomEventArgs> FileDataObserver { get; }

    }
}