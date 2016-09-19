﻿using System.Windows;
using LOUV.Torp.Monitor.ViewModel;
using TinyMetroWpfLibrary.Controller;
using TinyMetroWpfLibrary.EventAggregation;
using LOUV.Torp.Monitor.Events;
namespace LOUV.Torp.Monitor.Core.Controllers
{
    /// <summary>
    /// 和页面导航相关消息处理函数，包括页面导航，导航传值，关闭页面，页面呈现
    /// BaseController已完成系统基础导航信息以及一些空间消息处理机制。
    /// </summary>
    internal class UnitNavigationController : BaseController,
        IHandleMessage<GoHomePageNavigationRequest>,
        IHandleMessage<GoLiveCaptureNavigationRequest>,
        IHandleMessage<GoSettingNavigation>
    {

        public void Handle(GoHomePageNavigationRequest message)
        {
            NavigateToPage("Views/HomePageView.xaml");
        }
        public void Handle(GoLiveCaptureNavigationRequest message)
        {
            NavigateToPage("Views/LiveCaptureView.xaml");
        }

        public void Handle(GoSettingNavigation message)
        {
            NavigateToPage("Views/GlobalSettingView.xaml");
        }
    }
}