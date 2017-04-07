using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LOUV.Torp.CommLib.UDP
{
    public abstract class  UDPBaseService:IUDPService
    {
        protected UdpClient _udpClient;
        private Thread UdpReceiver;
        protected bool flag = false;
        public static event EventHandler<CustomEventArgs> DoParse;
        private List<byte> _recvQueue = new List<byte>();
        public bool Init(UdpClient udpClient)
        {
            try
            {
                _recvQueue.Clear();
                _udpClient = udpClient;
                return (_udpClient.Client != null);
            }
            catch (Exception exception)
            {

                return false;
            }
        }

        

        public bool Start()
        {
            //打开udp监听端口
            UdpReceiver  = new Thread(ListensenUDP);
            UdpReceiver.Start();
            return UdpReceiver.IsAlive;
        }

        public void Stop()
        {
            if (_udpClient != null)
            {
                StopReceive();
                //Thread.Sleep(1000);
                //_udpClient.Close();
            }
        }
        void StopReceive()
        {
            byte[] datagram = new byte[] { 0x00 };
            flag = false;
            _udpClient.Send(datagram, datagram.Length, new IPEndPoint(IPAddress.Loopback, ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port));
        }
        public UdpClient ReturnUdpClient()
        {
            return _udpClient;
        }
        public void Register(Observer<CustomEventArgs> observer)
        {
            DoParse -= observer.Handle;
            DoParse += observer.Handle;
        }
        public void UnRegister(Observer<CustomEventArgs> observer)
        {
            DoParse -= observer.Handle;
        }

        public void OnParsed(CustomEventArgs eventArgs)
        {
            if (DoParse != null)
            {
                DoParse(this, eventArgs);
            }
        }
        protected abstract void ListensenUDP();
    }
    //使用ACN协议的UDP DEBUG服务类，Start()中传入解析后的数据类
    public class ACNDebugUDPService:UDPBaseService
    {
        protected override void ListensenUDP()
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            flag = true;
            string returnData = string.Empty;
            string error = string.Empty;
            while (flag)
            {
                try
                {
                    var receiveBytes = _udpClient.Receive(ref remoteIpEndPoint);
                    returnData = Encoding.Default.GetString(receiveBytes);
                }
                catch (SocketException exception)
                {
                    if(exception.ErrorCode==0x2714)
                        break;
                    error = exception.Message;
                    flag = false;
                }
                finally
                {
                    var e = new CustomEventArgs(0, returnData, null, 0, flag, error, CallMode.NoneMode, _udpClient);
                    OnParsed(e);
                }
            }
        }
    }
    public class ACNDataUDPService:UDPBaseService
    {
        protected override void ListensenUDP()
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            flag =true;
            var buffer = new byte[4096*2];
            string error = string.Empty;
            var numberOfBytesRead = 0;
            var mode = CallMode.DataMode;
            while (flag)
            {
                try
                {

                    Array.Clear(buffer, 0, 4096*2);
                    var receiveBytes = _udpClient.Receive(ref remoteIpEndPoint);
                    if (BitConverter.ToUInt16(receiveBytes, 0) != 0xEE01)
                        continue;
                    if (receiveBytes.Length < 4)
                        continue;
                    var PacketLength = BitConverter.ToUInt16(receiveBytes, 2);

                    Array.Copy(receiveBytes, 4, buffer, numberOfBytesRead, receiveBytes.Length - 4);
                    numberOfBytesRead = receiveBytes.Length - 4;
                    // Incoming message may be larger than the buffer size.
                    while (numberOfBytesRead < PacketLength)
                    {
                        receiveBytes = _udpClient.Receive(ref remoteIpEndPoint);
                        Array.Copy(receiveBytes, 0, buffer, numberOfBytesRead, receiveBytes.Length);
                        numberOfBytesRead += receiveBytes.Length;
                    }

                }
                catch (SocketException exception)
                {
                    if (exception.ErrorCode != 0x2714) //程序关闭
                    {
                        error = exception.Message;
                        mode = CallMode.ErrMode;
                        flag = false;
                    }
                    else
                    {
                        flag = false;
                        return;
                    }

                }
                finally
                {
                    if (numberOfBytesRead > 4)
                    {
                        var e = new CustomEventArgs(0, string.Empty, buffer, numberOfBytesRead, flag, error, mode,
                            _udpClient);
                        numberOfBytesRead = 0;
                        OnParsed(e);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 4500接收外部广播数据服务1：避碰，ADCP，航控，
    /// </summary>
    public class ACMUWAService : UDPBaseService
    {
        protected override void ListensenUDP()
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            flag = true;
            byte[] buffer = null;
            string error = string.Empty;
            var mode = CallMode.Sail;
            while (flag)
            {
                try
                {
                    buffer = _udpClient.Receive(ref remoteIpEndPoint);

                }
                catch (SocketException exception)
                {
                    if (exception.ErrorCode != 0x2714) //程序关闭
                    {
                        error = exception.Message;
                        mode = CallMode.ErrMode;
                        flag = false;
                    }
                    else
                    {
                        flag = false;
                        return;
                    }

                }
                finally
                {
                    if (buffer == null)
                    {
                        var e = new CustomEventArgs(0, string.Empty, null, 0, flag, error, mode,
                            _udpClient);
                        OnParsed(e);
                    }
                    else
                    {
                        var e = new CustomEventArgs(0, string.Empty, buffer, buffer.Length, flag, error, mode,
                            _udpClient);
                        OnParsed(e);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 4500接收外部广播数据服务2：USBL
    /// </summary>
    public class ACMUSBLService : UDPBaseService
    {
        protected override void ListensenUDP()
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            flag = true;
            byte[] buffer = null;
            string error = string.Empty;
            var mode = CallMode.USBL;
            while (flag)
            {
                try
                {
                    buffer = _udpClient.Receive(ref remoteIpEndPoint);
                }
                catch (SocketException exception)
                {
                    if (exception.ErrorCode != 0x2714) //程序关闭
                    {
                        error = exception.Message;
                        mode = CallMode.ErrMode;
                        flag = false;
                    }
                    else
                    {
                        flag = false;
                        return;
                    }

                }
                finally
                {
                    if (buffer == null)
                    {
                        var e = new CustomEventArgs(0, string.Empty, null, 0, flag, error, mode,
                            _udpClient);
                        OnParsed(e);
                    }
                    else
                    {
                        var e = new CustomEventArgs(0, string.Empty, buffer, buffer.Length, flag, error, mode,
                            _udpClient);
                        OnParsed(e);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 4500接收外部广播数据服务3：GPS
    /// </summary>
    public class ACMGPSService : UDPBaseService
    {
        protected override void ListensenUDP()
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            flag = true;
            byte[] buffer = null;
            string error = string.Empty;
            var mode = CallMode.GPS;
            while (flag)
            {
                try
                {
                    buffer = _udpClient.Receive(ref remoteIpEndPoint);
                }
                catch (SocketException exception)
                {
                    if (exception.ErrorCode != 0x2714) //程序关闭
                    {
                        error = exception.Message;
                        mode = CallMode.ErrMode;
                        flag = false;
                    }
                    else
                    {
                        flag = false;
                        return;
                    }

                }
                finally
                {
                    if (buffer == null)
                    {
                        var e = new CustomEventArgs(0, string.Empty, null, 0, flag, error, mode,
                            _udpClient);
                        OnParsed(e);
                    }
                    else
                    {
                        var e = new CustomEventArgs(0, string.Empty, buffer, buffer.Length, flag, error, mode,
                            _udpClient);
                        OnParsed(e);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 接收外部广播数据服务：GPS/TeleRange/Range
    /// </summary>
    public class TorpDataService : UDPBaseService
    {
        private List<byte> _recvQueue = new List<byte>();
        protected override void ListensenUDP()
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            flag = true;
            _recvQueue.Clear();
            byte[] buffer = null;
            string error = string.Empty;
            CallMode mode = CallMode.GPS;
            while (flag)
            {
                try
                {
                    buffer = _udpClient.Receive(ref remoteIpEndPoint);
                    _recvQueue.AddRange(buffer);

                    CheckQueue(ref _recvQueue, remoteIpEndPoint);
                }
                catch (SocketException exception)
                {
                    if (exception.ErrorCode != 0x2714) //程序关闭
                    {
                        error = exception.Message;
                        mode = CallMode.ErrMode;
                        flag = false;
                    }
                    else
                    {
                        flag = false;
                        return;
                    }

                }
                catch(Exception ex)
                {
                    if(ex!=null)
                    {
                        flag = false;
                        return;
                    }
                }
            }
        }

        private void CheckQueue(ref List<byte> queue,IPEndPoint RemoteIpEndPoint)
        {
            var bytes = new byte[1032];
            CallMode mode= CallMode.GPS;
            while (queue.Count >= 1032)//够一次数据
            {
                
                if (queue[1] == 0x01 && (queue[0] == 0x28) || (queue[0] == 0x29) || (queue[0] == 0x2A)|| (queue[0] == 0x2B))//find head
                {
                    queue.CopyTo(0,bytes,0,1032);
                    queue.RemoveRange(0,1032);
                }
                else
                {
                    queue.RemoveAt(0);
                    continue;
                }
                

                switch (BitConverter.ToInt16(bytes, 0))
                {
                    case 0x0128:
                        mode = CallMode.Range;
                        break;
                    case 0x0129:
                        mode = CallMode.TeleRange;
                        break;
                    case 0x012A:
                        mode = CallMode.GPS;
                        break;
                    case 0x012B:
                        mode = CallMode.UDPAns;
                        break;
                        
                }
                var e = new CustomEventArgs(0, null, bytes, bytes.Length, true, null, mode, RemoteIpEndPoint.ToString().Split(':')[0]);
                OnParsed(e);
            }
        }
    }
}
