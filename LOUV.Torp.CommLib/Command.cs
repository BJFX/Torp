﻿using System.Threading.Tasks;
using LOUV.Torp.CommLib.Serial;
using LOUV.Torp.CommLib.TCP;

namespace LOUV.Torp.CommLib
{
    /// <summary>
    /// 命令模式的命令类实现，调用IComm
    /// </summary>
    public  class Command
    {
        public static string Error;

        public static Task<bool> SendSerialAsync(SerialBaseComm command)
        {
            return Task.Factory.StartNew(() => command.Send(out Error));
           
        }
        public static Task<CustomEventArgs> RecvSerialAsync(SerialBaseComm command)
        {
            return Task.Factory.StartNew(() => command.RecvData());

        }
        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Task<bool> SendTCPAsync(TCPBaseComm command)
        {
            return Task.Factory.StartNew(() => command.Send(out Error));

        }
        public static bool SendTCPSync(TCPBaseComm command)
        {
            return command.Send(out Error);

        }
        public static Task<CustomEventArgs> RecvTCPAsync(TCPBaseComm command)
        {
            return Task.Factory.StartNew(() => command.RecvData());

        }

    }

}
