﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using LOUV.Torp.Monitor.Core;
using LOUV.Torp.Monitor.Events;
using MahApps.Metro.Controls.Dialogs;
using TinyMetroWpfLibrary.Events;
using TinyMetroWpfLibrary.ViewModel;
using System.Net;
using LOUV.Torp.Monitor.Helpers;
namespace LOUV.Torp.Monitor.ViewModel
{
    public class GlobalSettingViewModel : ViewModelBase
    {
        private bool bInitial = false;
        public override void Initialize()
        {
            
            SaveConfig = RegisterCommand(ExecuteSaveConfig, CanExecuteSaveConfig, true);
        }
        public override void InitializePage(object extraData)
        {
            if(!UnitCore.Instance.LoadConfiguration())
                return;
            var conf = UnitCore.Instance.MonConfigueService.GetNet();
            Buoy01IpAddr = conf.IP[0];
            Buoy02IpAddr = conf.IP[1];
            Buoy03IpAddr = conf.IP[2];
            Buoy04IpAddr = conf.IP[3];
            BuoyPort = conf.BroadPort;
            ListenPort = conf.RecvPort;

            AUVID1 = (byte)UnitCore.Instance.MonConfigueService.GetSetup().AUVID1;
            AUVID2 = (byte)UnitCore.Instance.MonConfigueService.GetSetup().AUVID2;
     
            Velocity = UnitCore.Instance.MonConfigueService.GetSetup().AcouVel;
            FixedOffset = UnitCore.Instance.MonConfigueService.GetSetup().Offset;
            TimeOut = UnitCore.Instance.MonConfigueService.GetSetup().TimeOut;
            ValidInterval = UnitCore.Instance.MonConfigueService.GetSetup().ValidInterval;
            PreAdjust = UnitCore.Instance.MonConfigueService.GetSetup().PreAdjust;
            SonarDepth = UnitCore.Instance.MonConfigueService.GetSetup().SonarDepth;
        }
        public string Buoy01IpAddr
        {
            get { return GetPropertyValue(() => Buoy01IpAddr); }
            set { SetPropertyValue(() => Buoy01IpAddr, value); }
        }
        public string Buoy02IpAddr
        {
            get { return GetPropertyValue(() => Buoy02IpAddr); }
            set { SetPropertyValue(() => Buoy02IpAddr, value); }
        }
        public string Buoy03IpAddr
        {
            get { return GetPropertyValue(() => Buoy03IpAddr); }
            set { SetPropertyValue(() => Buoy03IpAddr, value); }
        }
        public string Buoy04IpAddr
        {
            get { return GetPropertyValue(() => Buoy04IpAddr); }
            set { SetPropertyValue(() => Buoy04IpAddr, value); }
        }
        public int BuoyPort
        {
            get { return GetPropertyValue(() => BuoyPort); }
            set { SetPropertyValue(() => BuoyPort, value); }
        }
        public int ListenPort
        {
            get { return GetPropertyValue(() => ListenPort); }
            set { SetPropertyValue(() => ListenPort, value); }
        }
        public float Velocity
        {
            get { return GetPropertyValue(() => Velocity); }
            set { SetPropertyValue(() => Velocity, value); }
        }
        public float FixedOffset
        {
            get { return GetPropertyValue(() => FixedOffset); }
            set { SetPropertyValue(() => FixedOffset, value); }
        }
        public int TimeOut
        {
            get { return GetPropertyValue(() => TimeOut); }
            set { SetPropertyValue(() => TimeOut, value); }
        }
        public int ValidInterval
        {
            get { return GetPropertyValue(() => ValidInterval); }
            set { SetPropertyValue(() => ValidInterval, value); }
        }
        public int PreAdjust
        {
            get { return GetPropertyValue(() => PreAdjust); }
            set { SetPropertyValue(() => PreAdjust, value); }
        }
        public double SonarDepth
        {
            get { return GetPropertyValue(() => SonarDepth); }
            set { SetPropertyValue(() => SonarDepth, value); }
        }

        public byte AUVID1
        {
            get { return GetPropertyValue(() => AUVID1); }
            set { SetPropertyValue(() => AUVID1, value); }
        }
        public byte AUVID2
        {
            get { return GetPropertyValue(() => AUVID2); }
            set { SetPropertyValue(() => AUVID2, value); }
        }
       
        public ICommand SaveConfig
        {
            get { return GetPropertyValue(() => SaveConfig); }
            set { SetPropertyValue(() => SaveConfig, value); }
        }


        public void CanExecuteSaveConfig(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }


        public async void ExecuteSaveConfig(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if(Save()&& ReConnect())
            {
                var dialog = (BaseMetroDialog)App.Current.MainWindow.Resources["CustomInfoDialog"];
                dialog.Title = "设置";
                await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMetroDialogAsync(MainFrameViewModel.pMainFrame,
                    dialog);
                var textBlock = dialog.FindChild<TextBlock>("MessageTextBlock");
                textBlock.Text = "修改成功！";
                await TaskEx.Delay(1000);
                await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame, dialog);

            }
        }

        private bool Save()
        {

            if (UnitCore.Instance.LoadConfiguration() == false)
            {
                EventAggregator.PublishMessage(new LogEvent("访问配置文件出错！", LogType.Both));
                return false; 
            }
            IPAddress ip01, ip02, ip03, ip04;
            if (IPAddress.TryParse(Buoy01IpAddr, out ip01) == false)
            {
                EventAggregator.PublishMessage(new LogEvent("浮标01网络地址非法！", LogType.OnlyInfo));
                return false;
            }
            if (IPAddress.TryParse(Buoy02IpAddr, out ip02) == false)
            {
                EventAggregator.PublishMessage(new LogEvent("浮标02网络地址非法！", LogType.OnlyInfo));
                return false;
            }
            if (IPAddress.TryParse(Buoy03IpAddr, out ip03) == false)
            {
                EventAggregator.PublishMessage(new LogEvent("浮标03网络地址非法！", LogType.OnlyInfo));
                return false;
            }
            if (IPAddress.TryParse(Buoy04IpAddr, out ip04) == false)
            {
                EventAggregator.PublishMessage(new LogEvent("浮标04网络地址非法！", LogType.OnlyInfo));
                return false;
            }
            
            bool result = (UnitCore.Instance.MonConfigueService.SetNetIP(0, Buoy01IpAddr) &&
                   UnitCore.Instance.MonConfigueService.SetNetIP(1, Buoy02IpAddr)&&
                   UnitCore.Instance.MonConfigueService.SetNetIP(2, Buoy03IpAddr)&&
                   UnitCore.Instance.MonConfigueService.SetNetIP(3, Buoy04IpAddr));
            if (result == false)
            {
                EventAggregator.PublishMessage(new LogEvent("保存浮标地址出错", LogType.Both));
                return false;
            }
            var ans = (UnitCore.Instance.MonConfigueService.SetNetBroadPort(BuoyPort) &&
                  UnitCore.Instance.MonConfigueService.SetNetRecvPort(ListenPort));
            if (ans == false)
            {
                EventAggregator.PublishMessage(new LogEvent("保存端口参数出错", LogType.Both));
                return false;
            }
            if(!UnitCore.Instance.MonConfigueService.SetAcousticVel(Velocity))
            {
                EventAggregator.PublishMessage(new LogEvent("保存默认声速出错", LogType.Both));
                return false;
            }
            if (!UnitCore.Instance.MonConfigueService.SetOffset(FixedOffset))
            {
                EventAggregator.PublishMessage(new LogEvent("保存测距偏移出错", LogType.Both));
                return false;
            }
            //if (!UnitCore.Instance.MonConfigueService.SetTimeOut(TimeOut))
            //{
            //    EventAggregator.PublishMessage(new LogEvent("保存定位超时出错", LogType.Both));
            //    return false;
            //}
            //if (!UnitCore.Instance.MonConfigueService.SetValidInterval(ValidInterval))
            //{
            //    EventAggregator.PublishMessage(new LogEvent("保存定位间隔出错", LogType.Both));
            //    return false;
            //}
            if (!UnitCore.Instance.MonConfigueService.SetSonarDepth(SonarDepth))
            {
                EventAggregator.PublishMessage(new LogEvent("保存声呐深度出错", LogType.Both));
                return false;
            }
            if (!UnitCore.Instance.MonConfigueService.SetPreAdjust(PreAdjust))
            {
                EventAggregator.PublishMessage(new LogEvent("保存压力校准值出错", LogType.Both));
                return false;
            }
            if (!UnitCore.Instance.MonConfigueService.SetAUVID1(AUVID1))
            {
                EventAggregator.PublishMessage(new LogEvent("保存AUV 01 ID出错", LogType.Both));
                return false;
            }
            if (!UnitCore.Instance.MonConfigueService.SetAUVID2(AUVID2))
            {
                EventAggregator.PublishMessage(new LogEvent("保存AUV 02 ID出错", LogType.Both));
                return false;
            }
            BaseType.TeleRange.pressureadjust = PreAdjust;
            var interval = new ChangeValidIntervalEvent();
            interval.ValidInterval = ValidInterval;
            //UnitCore.Instance.EventAggregator.PublishMessage(interval);
            return true;
        }

        private bool ReConnect()
        {
            if (UnitCore.Instance.NetCore.IsUDPWorking)
            {
                if(UnitCore.Instance.NetCore.IsInitialize)
                    UnitCore.Instance.NetCore.StopUDPService();
            }
            try
            {
                if (!UnitCore.Instance.LoadConfiguration()) throw new Exception("无法读取配置");
                return UnitCore.Instance.NetCore.StartUDPService();
            }
            catch (Exception e)
            {
                EventAggregator.PublishMessage(new LogEvent(UnitCore.Instance.NetCore.Error, LogType.OnlyInfo));
                return false;
            }
        }
    }
}
