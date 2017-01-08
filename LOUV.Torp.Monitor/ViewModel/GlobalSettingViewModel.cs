using System;
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
            
            LinkCheckCommand = RegisterCommand(ExecuteLinkCheckCommand, CanExecuteLinkCheckCommand, true);
            LinkUnCheckCommand = RegisterCommand(ExecuteLinkUnCheckCommand, CanExecuteLinkUnCheckCommand, true);
            SaveConfig = RegisterCommand(ExecuteSaveConfig, CanExecuteSaveConfig, true);
            IsFetching = false;
            Version = "0.0.0"; 
            RefreshVisble = Visibility.Visible;
            IsUpdating = false;
            UpdatePercentange = 0;
            XmtIndex = 0;
            XMTValue = 0.1F;
            Gain = 39;
        }
        public override void InitializePage(object extraData)
        {
            bInitial = false;
            if(!UnitCore.Instance.LoadConfiguration())
                return;
            var conf = UnitCore.Instance.MovConfigueService.GetNet();
            
            if (conf != null)
            {
                
            }
            if (UnitCore.Instance.NetCore.IsTCPWorking)
            {
                
                
            }
            else
            {
                ShipConnected = false;
                UWVConnected = false;
            }
            bInitial = true;
        }
        public string ShipIpAddr
        {
            get { return GetPropertyValue(() => ShipIpAddr); }
            set { SetPropertyValue(() => ShipIpAddr, value); }
        }
        public string UWVIpAddr
        {
            get { return GetPropertyValue(() => UWVIpAddr); }
            set { SetPropertyValue(() => UWVIpAddr, value); }
        }
        public int SelectMode
        {
            get { return GetPropertyValue(() => SelectMode); }
            set
            {
                if (SelectMode != value && bInitial==true)
                {
                    EventAggregator.PublishMessage(new LogEvent("新的模式需在保存设置后生效！", LogType.OnlyInfo));
                }
                SetPropertyValue(() => SelectMode, value);
            }
        }

        public int SelectGMode
        {
            get { return GetPropertyValue(() => SelectGMode); }
            set
            {
                SetPropertyValue(() => SelectGMode, value);
            }
        }
        public string Version
        {
            get { return GetPropertyValue(() => Version); }
            set { SetPropertyValue(() => Version, value); }
        }
        public bool IsFetching
        {
            get { return GetPropertyValue(() => IsFetching); }
            set { SetPropertyValue(() => IsFetching, value); }
        }
        public Visibility RefreshVisble
        {
            get { return GetPropertyValue(() => RefreshVisble); }
            set { SetPropertyValue(() => RefreshVisble, value); }
        }
        public bool ShipConnected
        {
            get { return GetPropertyValue(() => ShipConnected); }
            set { SetPropertyValue(() => ShipConnected, value); }
        }
        public bool UWVConnected
        {
            get { return GetPropertyValue(() => UWVConnected); }
            set { SetPropertyValue(() => UWVConnected, value); }
        }
        public bool IsUpdating
        {
            get { return GetPropertyValue(() => IsUpdating); }
            set { SetPropertyValue(() => IsUpdating, value); }
        }
        public int UpdatePercentange
        {
            get { return GetPropertyValue(() => UpdatePercentange); }
            set { SetPropertyValue(() => UpdatePercentange, value); }
        }
        public int XmtIndex
        {
            get { return GetPropertyValue(() => XmtIndex); }
            set { SetPropertyValue(() => XmtIndex, value); }
        }
        public float XMTValue
        {
            get { return GetPropertyValue(() => XMTValue); }
            set { SetPropertyValue(() => XMTValue, value); }
        }

        public int Gain
        {
            get { return GetPropertyValue(() => Gain); }
            set { SetPropertyValue(() => Gain, value); }
        }
        //// command
        public ICommand LinkCheckCommand
        {
            get { return GetPropertyValue(() => LinkCheckCommand); }
            set { SetPropertyValue(() => LinkCheckCommand, value); }
        }


        public void CanExecuteLinkCheckCommand(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }


        public void ExecuteLinkCheckCommand(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            LogHelper.WriteLog("开始连接");
            if (UnitCore.Instance.NetCore.IsTCPWorking)
                return;
            else
            {
                try
                {
                    Save();
                    UnitCore.Instance.NetCore.StartTCPService();
                }
                catch (Exception e)
                {
                    EventAggregator.PublishMessage(new LogEvent(e.Message, LogType.OnlyInfo));
                }

                if (UnitCore.Instance.NetCore.IsTCPWorking)
                {
                    if (SelectMode == 0)
                    {
                        ShipConnected = true;
                        UWVConnected = false;
                    }
                    else
                    {
                        UWVConnected = true;
                        ShipConnected = false;
                    }
                }
                else
                {
                    LogHelper.WriteLog("连接失败！");
                    EventAggregator.PublishMessage(new LogEvent("网络连接失败！", LogType.OnlyInfo));
                    UWVConnected = false;
                    ShipConnected = false;
                }

            }
        }
        public ICommand LinkUnCheckCommand
        {
            get { return GetPropertyValue(() => LinkUnCheckCommand); }
            set { SetPropertyValue(() => LinkUnCheckCommand, value); }
        }


        public void CanExecuteLinkUnCheckCommand(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }


        public void ExecuteLinkUnCheckCommand(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if(UnitCore.Instance.NetCore.IsTCPWorking)
                UnitCore.Instance.NetCore.StopTCpService();
            UWVConnected = false;
            ShipConnected = false;
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
            Save();
            
            UnitCore.Instance.LoadConfiguration();
            var dialog = (BaseMetroDialog)App.Current.MainWindow.Resources["CustomInfoDialog"];
            dialog.Title = "设置";
            await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMetroDialogAsync(MainFrameViewModel.pMainFrame,
                dialog);
            var textBlock = dialog.FindChild<TextBlock>("MessageTextBlock");
            textBlock.Text = "修改成功！";
            await TaskEx.Delay(1000);
            await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame, dialog);
            
        }

        private void Save()
        {
            if (UnitCore.Instance.NetCore.IsTCPWorking)//修改参数前先断开连接
            {
                if (UnitCore.Instance.LoadConfiguration() == false)
                {
                    EventAggregator.PublishMessage(new LogEvent("读取配置出错！", LogType.Both));
                    return;
                }
                
            }
            
            
        }

        private void ReConnectToDSP()
        {
            if (UnitCore.Instance.NetCore.IsTCPWorking)
            {
                if(UnitCore.Instance.NetCore.IsInitialize)
                    UnitCore.Instance.NetCore.StopTCpService();
            }
            try
            {
                if (!UnitCore.Instance.LoadConfiguration()) throw new Exception("无法读取网络配置");
                UnitCore.Instance.NetCore.StartTCPService();
            }
            catch (Exception e)
            {
                EventAggregator.PublishMessage(new LogEvent(UnitCore.Instance.NetCore.Error, LogType.OnlyInfo));
                return;
            }
        }
    }
}
