using System;
using MahApps.Metro.Controls.Dialogs;
using TinyMetroWpfLibrary.Controller;
using TinyMetroWpfLibrary.EventAggregation;
using LOUV.Torp.Monitor.Events;
using LOUV.Torp.ICore;
using LOUV.Torp.Monitor.Helpers;
using LOUV.Torp.Monitor.ViewModel;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LOUV.Torp.Monitor.Core.Controllers
{
    /// <summary>
    /// 模块间消息处理类，包括WriteLog，系统消息广播，报警等
    /// 由于不像导航消息处理类那样已经由BaseController处理了一些基本消息
    /// 因此需要自己将消息处理函数完成并完成IMessageController接口
    /// </summary>
    class UnitMessageController : IMessageController, IHandleMessage<LogEvent>, IHandleMessage<ErrorEvent>
    {
        #region 构造
        private IEventAggregator eventAggregator;
        public UnitMessageController()
        {
            eventAggregator = Kernel.Instance.EventAggregator;
            //将类实例注册到EventAggregator
            eventAggregator.Subscribe(this);
        }

        

        ~UnitMessageController()
        {
            eventAggregator.Unsubscribe(this);
        }
        #endregion

        #region IMessage接口实现
        //初始化消息处理类
         public void Init()
        {
            
        }
        
        public void SendMessage(string message)
        {
            
        }

        public void WriteLog(string message)
        {
            LogHelper.WriteLog(message);

        }

        public void ErrorLog(string message, Exception ex)
        {
            LogHelper.ErrorLog(message, ex);
            
        }

        public void Alert(string message, Exception ex = null)
        {
            var md = new MetroDialogSettings();
            md.AffirmativeButtonText = "确定";
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                if(ex==null)
                {
                    MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame, "程序错误",
                    message, MessageDialogStyle.Affirmative, md);
                }
                else
                {
                    MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame, message,
                    ex.StackTrace, MessageDialogStyle.Affirmative, md);
                }
            }));
        }

        public void BroadCast(string message)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region IHandle
        //负责写入记录文件
        public void Handle(LogEvent message)
        {
            switch (message.Type)
            {
                case LogType.Both:
                    WriteLog(message.Message);
                    Notice(message.Message);
                    break;
                case LogType.OnlyInfo:
                    //WriteLog(message.Message);
                    Notice(message.Message);
                    break;
                case LogType.OnlyInfoandClose:
                    Notice(message.Message,true);
                    break;
                default:
                    WriteLog(message.Message);
                    break;
            }
        }
        public void Handle(ErrorEvent message)
        {
            switch (message.Type)
            {
                case LogType.OnlyLog:
                    ErrorLog(message.Message, message.Ex);
                    break;
                case LogType.Both:
                    ErrorLog(message.Message, message.Ex);
                    Alert(message.Message, message.Ex);
                    break;
                default:
                    //ErrorLog(message.Message, message.Ex);
                    Alert(message.Message, message.Ex);
                    break;
            }
        }
        #endregion


        public void Notice(string message,bool autoclose = false)
        {
            var md = new MetroDialogSettings();
            md.AffirmativeButtonText = "确定";
            App.Current.Dispatcher.Invoke(new Action(async () =>
            {
                if (autoclose)
                {
                    var dialog = (BaseMetroDialog)App.Current.MainWindow.Resources["CustomInfoDialog"];
                    dialog.Title = "消息";
                    await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMetroDialogAsync(MainFrameViewModel.pMainFrame,
                        dialog);
                    var textBlock = dialog.FindChild<TextBlock>("MessageTextBlock");
                    textBlock.Text = message;
                    await TaskEx.Delay(2000);
                    await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame, dialog);

                }
                else
                {
                    await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame, "提示",
                message, MessageDialogStyle.Affirmative, md);
                }
            }));
        }
    }
}
