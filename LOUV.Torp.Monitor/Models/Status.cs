using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LOUV.Torp.CommLib;
using LOUV.Torp.Monitor.Core;
using LOUV.Torp.Monitor.Helpers;
namespace LOUV.Torp.Monitor.Models
{
    public class Status
    {

        public static string NetworkStatus
        {
            get
            {
                if (UnitCore.Instance.NetCore!=null&&UnitCore.Instance.NetCore.IsWorking)
                    return Properties.Resources.NETWORK_OK;
                return Properties.Resources.NETWORK_DOWN;
            }
        }
        
 

        public static string LastUpdateTime { get; set; }

        public static UInt16 ReceiveMsgCount { get; set; }

    }
}
