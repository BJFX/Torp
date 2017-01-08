using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using LOUV.Torp.JsonUtils;
using TinyMetroWpfLibrary.Utility;

namespace LOUV.Torp.CommLib.Serial
{
    public abstract class SerialSerialServiceBase :ISerialService
    {
        #region 属性
        protected SerialPort _serialPort;
       // public CCheckBP check;

        public event EventHandler<CustomEventArgs> DoParse;
        private List<byte> _recvQueue = new List<byte>();
        public SerialServiceMode SerialServiceMode { get; set; }
        #endregion

        #region 方法
        public bool Init(SerialPort serialPort)
        {
            try
            {
                SerialServiceMode = SerialServiceMode.HexMode;
                _recvQueue.Clear();
                _serialPort = serialPort;
                if (SerialPort.GetPortNames().All(t => t != _serialPort.PortName.ToUpper()))
                {
                   return false;
                }
                if (!_serialPort.IsOpen) _serialPort.Open();
                return _serialPort.IsOpen;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
            
        }

        public virtual void Register(Observer<CustomEventArgs> observer)
        {
            DoParse -= observer.Handle;
            DoParse+=observer.Handle;
        }

        public virtual void ChangeMode(SerialServiceMode mode)
        {
            SerialServiceMode = mode;
        }

        public virtual void UnRegister(Observer<CustomEventArgs> observer)
        {
            DoParse -= observer.Handle;
        }

        public bool Stop()
        {
            _serialPort.DataReceived -= _SerialPort_DataReceived;
            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
            }
            catch (Exception)
            {
                
                return false;
            }
            return true;
        }

        public virtual bool Start()
        {
            _serialPort.DataReceived -= _SerialPort_DataReceived;
            _serialPort.DataReceived += _SerialPort_DataReceived;
            return _serialPort.IsOpen;
        }

        public virtual SerialPort ReturnSerialPort()
        {
            return _serialPort;
        }
        private void _SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var nCount = _serialPort.BytesToRead;
            /*if (SerialServiceMode==SerialServiceMode.HexMode && nCount < 16)
            {
                Thread.Sleep(50);
                return;
            }*/
            for (int i = nCount - 1; i >= 0; i--)
            {
                _recvQueue.Add((byte)_serialPort.ReadByte());                
            }

            CheckQueue(ref _recvQueue);
            
        }

        protected abstract void CheckQueue(ref List<byte> lstBytes);

        public void OnParsed(CustomEventArgs eventArgs)
        {
            if (DoParse != null)
            {
                DoParse(this, eventArgs);
            }
        }
        #endregion
    }
    //使用ACN协议的串口服务类，Start()中传入解析后的数据类
    public class ACNSerialSerialService: SerialSerialServiceBase
    {
        
        
        protected override void CheckQueue(ref List<byte> queue)
        {
            var bytes = new byte[queue.Count];
            queue.CopyTo(bytes);
            var strcmd = Encoding.ASCII.GetString(bytes);
            switch (SerialServiceMode)
            {
                case SerialServiceMode.HexMode:
                    while (strcmd.Contains("EB90")&&strcmd.Contains("END"))
                    {
                        Debug.WriteLine(strcmd);
                        var eb90index = strcmd.IndexOf("EB90", StringComparison.Ordinal);
                        var endindex = strcmd.IndexOf("END", StringComparison.Ordinal)+3;
                        if ( eb90index < endindex  )
                        {
                            var payload = strcmd.Substring(eb90index, endindex - eb90index);
                            strcmd = strcmd.Remove(eb90index, endindex - eb90index);
                            var buf = new byte[endindex - eb90index];
                            Array.Copy(bytes, eb90index, buf, 0, endindex - eb90index);
                            //删除移走的字符
                            queue.RemoveRange(0, endindex);
                            //ParseOnHexMode(buf,payload);
                            ///tbd
                        }
                        else
                        {
                            strcmd = strcmd.Remove(0, eb90index);
                            //删除无法识别的字符
                            queue.RemoveRange(0, eb90index);
                        }
                    }
                    
                    break;
                case SerialServiceMode.LoaderMode:
                    ParseOnLoaderMode(strcmd);
                    queue.Clear();
                    break;
                default:
                    ParseOnLoaderMode(strcmd);
                    queue.Clear();
                    break;
            }
        
        }

        private void ParseOnLoaderMode(string hexString)
        {
            var e = new CustomEventArgs(0, hexString, null, 0, true, null, CallMode.LoaderMode, _serialPort); 
            OnParsed(e);
        }

       
   
        }
    public class ACNBPSerialSerialService: SerialSerialServiceBase
    {
        private CCheckBP check = new CCheckBP();

        protected override void CheckQueue(ref List<byte> queue)
        {
            var bytes = new byte[queue.Count];
            queue.CopyTo(bytes);
            

            //对串口接收的数据进行解析
            byte[] CmdType = new byte[7];
            byte[] ch = new byte[4096];
            check.WriteData(bytes, (uint)queue.Count);//写入循环缓冲区，取完整帧和校验
            while ( check.IsFull())//取完整帧
            {
                if (!check.IsCorrect())
                {
                    break;
                    //continue;//校验不正确退出
                }
                uint lenth = 0;
                check.GetFullData(ch, ref lenth);//得到完整帧以及帧的长度
                Buffer.BlockCopy(ch, 7, CmdType, 0, 6);//找出命令类型
                if (string.Compare(Encoding.UTF8.GetString(CmdType), "result") == 0)
                {
                    byte[] DataBuffer = new byte[lenth + 2];
                    UInt16 uid = 0x2002; //与网络包格式一致
                    Buffer.BlockCopy(BitConverter.GetBytes(uid), 0, DataBuffer, 0, 2);
                    Buffer.BlockCopy(ch, 0, DataBuffer, 2, (int)lenth);
                    var e = new CustomEventArgs(0, null, DataBuffer, DataBuffer.Length, true, null, CallMode.Sail, _serialPort);
                    OnParsed(e);
                }                
                break;
            }
            queue.Clear();

        }           
     }
    //使用ACCP协议的串口服务类，Start()中传入解析后的数据类
    public class ACNADCPSerialSerialService : SerialSerialServiceBase
    {
        private CheckADCP check = new CheckADCP();
        
        protected override void CheckQueue(ref List<byte> queue)
        {
            var bytes = new byte[queue.Count];
            queue.CopyTo(bytes);
            byte[] ch = new byte[4096];
            //对串口接收的数据进行解析
            if (queue.Count!=0)
            {
                check.WriteData(bytes, (uint)queue.Count);//写入循环缓冲区，取完整帧和校验
                while (check.IsFull())//取完整帧
                {
                    uint lenth = 0;
                    check.GetFullData(ch, ref lenth);//得到完整帧以及帧的长度
                    byte[] DataBuffer = new byte[lenth + 2];
                    UInt16 uid = 0x2006; //与网络包格式一致
                    Buffer.BlockCopy(BitConverter.GetBytes(uid), 0, DataBuffer, 0, 2);
                    Buffer.BlockCopy(ch, 0, DataBuffer, 2, (int)lenth);
                    var e = new CustomEventArgs(0, null, DataBuffer, DataBuffer.Length, true, null, CallMode.Sail, _serialPort);
                    OnParsed(e);                    
                    break;
                }
                queue.Clear();

            }
        }
            
    }





    
}
