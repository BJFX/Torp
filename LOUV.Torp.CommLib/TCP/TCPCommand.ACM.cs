using System.Net.Sockets;
using LOUV.Torp.CommLib.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LOUV.Torp.Comm.TCP
{
    public class ACMShellCommand : TCPBaseComm
    {
        public ACMShellCommand(TcpClient tcpClient, string cmd)
        {
            if (!base.Init(tcpClient)) return;
            cmd = cmd.TrimEnd('\n');
            base.GetMsg(cmd + "\r");
        }
        public override bool Send(out string error)
        {
            return SendMsg(out error);
        }
    }
    public class ACMDataCommand : TCPBaseComm
    {

        public ACMDataCommand(TcpClient tcpClient, byte[] bytes)
        {
            if (!base.Init(tcpClient)) return;
            if (bytes == null) return;
            base.LoadData(bytes);
        }

        public override bool Send(out string error)
        {
            return SendData(out error);
        }
    }
}

