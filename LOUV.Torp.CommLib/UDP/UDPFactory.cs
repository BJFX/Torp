﻿using System;
using System.Net.Sockets;

namespace LOUV.Torp.CommLib.UDP
{
    public class UDPCommFactory:IUDPCommFactory
    {
        private UdpClient _udpClient;

        public UDPCommFactory(UdpClient udpClient)
        {
            _udpClient = udpClient;
        }
        public UDPBaseComm CreateUDPComm(ACNCommandMode mode, byte[] bytes,string str)
        {
            switch (mode)
            {
                case ACNCommandMode.CmdCharMode:
                    return new ACNUDPShellCommand(_udpClient,str);
                 
                case ACNCommandMode.CmdWithData:
                    return new ACNUDPDataCommand(_udpClient,bytes);
               
                default:
                    throw new InvalidOperationException("不支持的命令模式！");
            }
            
        }
    }
    public class UDPDebugServiceFactory:IUDPServiceFactory
    {
        public IUDPService CreateService()
        {
            return new ACNDebugUDPService();
        }
    }
    public class UDPDataServiceFactory : IUDPServiceFactory
    {
        public IUDPService CreateService()
        {
            return new ACNDataUDPService();
        }
    }
    public class ACMUWAServiceFactory : IUDPServiceFactory
    {
        public IUDPService CreateService()
        {
            return new ACMUWAService();
        }
    }
    public class ACMUSBLServiceFactory : IUDPServiceFactory
    {
        public IUDPService CreateService()
        {
            return new ACMUSBLService();
        }
    }
    public class ACMGPSServiceFactory : IUDPServiceFactory
    {
        public IUDPService CreateService()
        {
            return new ACMGPSService();
        }
    }
}
