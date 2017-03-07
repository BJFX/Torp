using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LOUV.Torp.Monitor.Core;
using LOUV.Torp.Monitor.Events;
using LOUV.Torp.Monitor.Models;
using LOUV.Torp.Monitor.Views;
using TinyMetroWpfLibrary.Controller;
using TinyMetroWpfLibrary.Events;
using TinyMetroWpfLibrary.Frames;
using TinyMetroWpfLibrary.ViewModel;
using MahApps.Metro.Controls.Dialogs;

namespace LOUV.Torp.Monitor.ViewModel
{
    public class MainFrameViewModel : MainWindowViewModelBase
    {
        public static MainFrameViewModel pMainFrame { get; set; }
        private IDialogCoordinator _dialogCoordinator { get; set; }
        //private DispatcherTimer t = null;
        public override void Initialize()
        {
            base.Initialize();
            pMainFrame = this;
        }

        
        
        #region action
        internal void GoToGlobalSettings()
        {
            EventAggregator.PublishMessage(new GoSettingNavigation());
        }
        internal void GoBack()
        {
            EventAggregator.PublishMessage(new GoBackNavigationRequest());
        }
        internal void GoCommandWin()
        {
            
        }
        internal void ExitProgram()
        {
            
            Application.Current.Shutdown();
        }

        #endregion
        #region binding property
        
        public IDialogCoordinator DialogCoordinator
        {
            get { return _dialogCoordinator; }
            set { _dialogCoordinator = value; }
        }


        #endregion

    }
}
