using System;
using System.Net.Sockets;
using System.Text;
using LOUV.Torp.MonP;
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
                IProtocol pro = null;
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
                        if (shell.Contains("ver\r\n"))
                        {
                            UnitCore.Instance.Version = shell.Substring(shell.LastIndexOf("ver\r\n") + 5);
                           
                        }
                        if (shell.Contains("浮点处理器"))
                        {
                            UnitCore.Instance.Version += shell;
                            if (shell.Contains("/>"))
                                UnitCore.Instance.Version  = UnitCore.Instance.Version.Replace("/>","");
                        }
                        if(shell.Contains("非法的ldr文件"))
                        {
                            UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent("非法的ldr文件，请重新烧写正确的ldr！", LogType.OnlyInfo));
                        }
                    }
                    //类型标志
                    if (e.Mode == CallMode.Sail) //水下航控或ADCP或BP
                    {
                        switch (id)
                        {
                            case (int) ExchageType.SUBPOST:
                                pro = new Subposition();
                                datatype = "SAILTOACOUSTIC";

                                break;
                            case (int) ExchageType.BP:
                                pro = new Bpdata();
                                datatype = "BP";

                                break;
                            case (int) ExchageType.CTD:
                                pro = new Ctddata();
                                datatype = "SAILTOACOUSTIC";

                                break;
                            case (int) ExchageType.LIFESUPPLY:
                                pro = new Lifesupply();
                                datatype = "SAILTOACOUSTIC";

                                break;
                            case (int) ExchageType.ENERGY:
                                pro = new Energysys();
                                datatype = "SAILTOACOUSTIC";

                                break;
                            case (int) ExchageType.ALERT:
                                pro = new Alertdata();
                                datatype = "SAILTOACOUSTIC";

                                break;

                            case (int) ExchageType.ADCP:
                                pro = new Adcpdata();
                                datatype = "ADCP";

                                break;
                            default:
                                throw new Exception("未知的UDP数据类型");
                                break;
                        }     

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
                        switch (id)
                        {
                            case (int) ModuleType.SSBNULL:
                            case (int) ModuleType.SSB:
                                datatype = "RECVVOICE";

                                break;
                            case (int) ModuleType.MFSK:
                                datatype = "RECVFSK";

                                break;
                            case (int) ModuleType.MPSK:
                                datatype = "RECVPSK";

                                break;
                            case (int) ModuleType.FH:
                                datatype = "FH";
                                break;
                            case (int) ModuleType.Req:
                                LogHelper.WriteLog("收到DSP请求");
                                if (UnitCore.Instance.WorkMode == MonitorMode.SUBMARINE)
                                {
                                    if (ACM4500Protocol.UwvdataPool.HasImg)
                                    {
                                        var data = ACM4500Protocol.PackData(ModuleType.MPSK);
                                        LogHelper.WriteLog("发送MPSK");
                                        UnitCore.Instance.NetCore.Send((int) ModuleType.MPSK, data);
                                        UnitCore.Instance.MonTraceService.Save("XMTPSK", data);
                                        return;
                                    }
                                }
                                var fskdata = ACM4500Protocol.PackData(ModuleType.MFSK);
                                LogHelper.WriteLog("发送MFSK");
                                UnitCore.Instance.NetCore.Send((int) ModuleType.MFSK, fskdata);
                                UnitCore.Instance.MonTraceService.Save("XMTFSK", fskdata);
                                UnitCore.Instance.EventAggregator.PublishMessage(new DSPReqEvent());//发送完MFSK通知界面                                
                                return;
                            case (int)ModuleType.FeedBack:
                                LogHelper.WriteLog("收到DSP反馈的增益");
                                UnitCore.Instance.EventAggregator.PublishMessage(new DspFeedbackComEvent(
                                    ModuleType.FeedBack, buffer));
                                return;
                            default:
                                return;
                        }
                    }
                    //保持数据
                    if (id == (int)ModuleType.FH)
                        UnitCore.Instance.MonTraceService.Save(datatype, Encoding.Default.GetString(buffer)); //FH
                    else
                    {
                        if (id == (int) ModuleType.SSBNULL)
                        {
                            int length = BitConverter.ToUInt16(buffer,0);
                            buffer = new byte[length*2];
                            Array.Clear(buffer,0,length*2);
                        }
                        UnitCore.Instance.MonTraceService.Save(datatype, buffer); //保存上面除FH全部数据类型
                    }
                    //开始处理
                    if (e.Mode == CallMode.DataMode)
                    {
                        if (id == (int)ModuleType.SSBNULL)
                            return;
                        if (id == (int) ModuleType.SSB)
                        {/*
                            if (UnitCore.Instance.Wave != null)
                            {
                                UnitCore.Instance.Wave.Dispatcher.Invoke(new Action(() =>
                                {
                                    UnitCore.Instance.Wave.Display(buffer);
                                }));
                            }*/
                            return;
                        }
                        if (id == (int) ModuleType.FH)
                        {
                            LogHelper.WriteLog("收到跳频数据:" + Encoding.Default.GetString(buffer).Replace("\0",""));
                            if (ACM4500Protocol.ParseFH(buffer))
                            {
                                UnitCore.Instance.EventAggregator.PublishMessage(new MovDataEvent(
                                    ModuleType.FH, ACM4500Protocol.Results));
                            }
                            return;
                        }

                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            try
                            {
                                UnitCore.Instance.ACMMutex.WaitOne();
                                var ret = ACM4500Protocol.DecodeACNData(buffer, (ModuleType)Enum.Parse(typeof(ModuleType), id.ToString()));
                                //var ret = buffer;
                                if (ret != null)
                                {

                                    switch (id)
                                    {
                                        case (int)ModuleType.MFSK:
                                            LogHelper.WriteLog("收到MFSK数据");
                                            UnitCore.Instance.MonTraceService.Save("FSKSRC", ret);
                                            if (ACM4500Protocol.ParseFSK(ret))
                                            {
                                                UnitCore.Instance.EventAggregator.PublishMessage(new MovDataEvent(
                                                    ModuleType.MFSK, ACM4500Protocol.Results));
                                            }
                                            break;
                                        case (int)ModuleType.MPSK:
                                            LogHelper.WriteLog("收到MPSK数据");
                                            UnitCore.Instance.MonTraceService.Save("PSKSRC", ret);
                                            var jpcdata = ACM4500Protocol.ParsePSK(ret);
                                            if (jpcdata != null) //全部接收
                                            {
                                                UnitCore.Instance.MonTraceService.Save("PSKJPC", jpcdata);
                                                /*
                                                if (Jp2KConverter.LoadJp2k(jpcdata))
                                                {
                                                    var imgbuf =
                                                        Jp2KConverter.SaveImg(UnitCore.Instance.MovConfigueService.MyExecPath +
                                                                              "\\" + "decode.jpg");
                                                    if (imgbuf != null)
                                                    {
                                                        UnitCore.Instance.MonTraceService.Save("IMG", imgbuf);
                                                        ACM4500Protocol.Results.Add(MovDataType.IMAGE, imgbuf);
                                                    }
                                                    UnitCore.Instance.EventAggregator.PublishMessage(new MovDataEvent(
                                                        ModuleType.MPSK, ACM4500Protocol.Results));
                                                }*/
                                            }
                                            break;
                                    }

                                }
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
                        ACM4500Protocol.UwvdataPool.Add(buffer, (MovDataType)id);
                        pro.Parse(buffer); //解析UDP数据
                        UnitCore.Instance.EventAggregator.PublishMessage(new SailEvent(
                                                (ExchageType)id, pro));
                    }
                    
                }
                catch (Exception ex)
                {             
                  //  App.Current.Dispatcher.Invoke(new Action(() =>
                  //  {
                       /* if (UnitCore.Instance.ACMMutex.WaitOne(100) == true)//如果能获取Mutex或已经获取Mutex就释放它
                        {
                            UnitCore.Instance.ACMMutex.ReleaseMutex();
                        }*/
                        UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));
                  //  }));
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
