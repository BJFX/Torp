using System;
using System.Net.Sockets;
using System.Text;

using LOUV.Torp.CommLib;
using LOUV.Torp.Monitor.Events;
using LOUV.Torp.Monitor.Helpers;
using DevExpress.Xpf.Core;
using Microsoft.Win32;
using System.Threading;

namespace LOUV.Torp.Monitor.Core
{
    public class MonitorDataObserver:Observer<CustomEventArgs>
    {
  
        public void Handle(object sender, CustomEventArgs e)
        {
            if (e.ParseOK)
            {
              //
                try
                {
                    int id = 0;
                    byte[] buffer = null;
                    
                    //类型标志
                    if (e.Mode == CallMode.GPS) //水下航控或ADCP或BP
                    {
                       
                            

                    }
                    else if (e.Mode == CallMode.Range)
                    {
                        //datatype = "USBL";
                    }
                    else if (e.Mode == CallMode.TeleRange)
                    {
                        //datatype = "GPS";
                    }
                    else if (e.Mode == CallMode.DataMode) //payload or ssb
                    {
                        
                    }
                    //保持数据
                    
                    
                    
                }
                catch (Exception ex)
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(ex, LogType.Both));
                    }));
                }


            }
            else
            {
                if (e.Mode == CallMode.ErrMode)
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        UnitCore.Instance.NetCore.StopUDPService();
                       // UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(e.Ex, LogType.Both));
                    }));
                    
                }
            }
        }


    }
}
