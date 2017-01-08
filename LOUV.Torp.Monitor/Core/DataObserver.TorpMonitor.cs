using System;
using System.Net.Sockets;
using System.Text;

using LOUV.Torp.CommLib;
using LOUV.Torp.Monitor.Events;
using LOUV.Torp.Monitor.Helpers;
using DevExpress.Xpf.Core;
using Microsoft.Win32;
using System.Threading;
using LOUV.Torp.Device;
namespace LOUV.Torp.Monitor.Core
{
    public class MonitorDataObserver:Observer<CustomEventArgs>
    {
  
        public void Handle(object sender, CustomEventArgs e)
        {
            string datatype = "";
            if (e.ParseOK)
            {
              //
                try
                {
                    int id = 0;
                    byte[] buffer = null;
                    
                    if (e.Mode != CallMode.NoneMode)
                    {
                        id = BitConverter.ToUInt16(e.DataBuffer, 0);
                        if (e.Mode == CallMode.Sail)
                        {
                            buffer = new byte[e.DataBufferLength - 2];
                            Buffer.BlockCopy(e.DataBuffer, 2, buffer, 0, e.DataBufferLength - 2);
                        }
                        else
                        {
                            buffer = new byte[e.DataBufferLength - 4];
                            Buffer.BlockCopy(e.DataBuffer, 4, buffer, 0, e.DataBufferLength - 4);
                        }
                        
                    }
                    else//shell
                    {
                        string shell = e.Outstring;
                       
                    }
                    //类型标志
                    if (e.Mode == CallMode.Sail) //水下航控或ADCP或BP
                    {
                       
                            

                    }
                    else if (e.Mode == CallMode.USBL)
                    {
                        datatype = "USBL";
                    }
                    else if (e.Mode == CallMode.GPS)
                    {
                        datatype = "GPS";
                    }
                    else if (e.Mode == CallMode.DataMode) //payload or ssb
                    {
                        
                    }
                    //保持数据
                    
                    //开始处理
                    if (e.Mode == CallMode.DataMode)
                    {
                        

                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            try
                            {
                                UnitCore.Instance.ACMMutex.WaitOne();
                                
                                UnitCore.Instance.ACMMutex.ReleaseMutex();

                            }
                            catch (Exception ex)
                            {
                                    if (UnitCore.Instance.ACMMutex.WaitOne(100) == true)//如果能获取Mutex或已经获取Mutex就释放它
                                    {
                                        UnitCore.Instance.ACMMutex.ReleaseMutex();
                                    }
                                    UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));

                            }
                            

                        }));


                        
                    }
                    else if (e.Mode == CallMode.Sail)
                    {
                       
                    }
                    
                }
                catch (Exception ex)
                {             
                  
                        UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));

                }


            }
            else
            {
                if (e.Mode == CallMode.ErrMode)
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        UnitCore.Instance.NetCore.StopTCpService();
                       // UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(e.Ex, LogType.Both));
                    }));
                    
                }
            }
        }


    }
}
