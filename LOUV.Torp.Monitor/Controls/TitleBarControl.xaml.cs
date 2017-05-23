using LOUV.Torp.Monitor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LOUV.Torp.Monitor.Core;
using LOUV.Torp.Monitor.Events;
using MahApps.Metro.Controls.Dialogs;
using TinyMetroWpfLibrary.Controls;
using LOUV.Torp.Monitor.ViewModel;
namespace LOUV.Torp.Monitor.Controls
{
    /// <summary>
    /// Interaction logic for TitleBarControl.xaml
    /// </summary>
    public partial class TitleBarControl : UserControl
    {
        public TitleBarControl()
        {
            InitializeComponent();
        }

        private MainFrameViewModel _vm;
        public MainFrameViewModel VM
        {
            get
            {
                if (_vm == null)
                {
                    _vm = MainFrameViewModel.pMainFrame;
                     
                }
                return _vm;
            }
        }


        private Window _mainWindow;
        public Window MainWindow
        {
            get
            {
                if (_mainWindow == null)
                {
                    _mainWindow = TreeHelper.TryFindParent<Window>(this);
                }
                return _mainWindow; 
            }
        }
        private async void GoToGlobalSettings(object sender, RoutedEventArgs e)
        {
            if (UnitCore.Instance.IsReplay)
            {
                var md = new MetroDialogSettings();
                md.AffirmativeButtonText = "确定";
                md.ColorScheme = MetroDialogColorScheme.Accented;
                var result = await VM.DialogCoordinator.ShowMessageAsync(VM, "无法修改配置",
                "请先退出回放模式", MessageDialogStyle.Affirmative, md);
            }
            if (!UnitCore.Instance.IsReplay)
                VM.GoToGlobalSettings();
        }


        private async void ShowAbout(object sender, RoutedEventArgs e)
        {
            if (UnitCore.Instance.IsReplay)
            {
                var md = new MetroDialogSettings();
                md.AffirmativeButtonText = "确定";
                md.ColorScheme = MetroDialogColorScheme.Accented;
                var result = await VM.DialogCoordinator.ShowMessageAsync(VM, "无法修改地图",
                "请先退出回放模式", MessageDialogStyle.Affirmative, md);
            }
            if (!UnitCore.Instance.IsReplay)
                UnitCore.Instance.EventAggregator.PublishMessage(new ShowAboutSlide(true));
        }
        private void GoBack(object sender, RoutedEventArgs e)
        {
            if(!UnitCore.Instance.IsReplay)
                VM.GoBack();
        }
        
       
        private async void Minimize(object sender, RoutedEventArgs e)
        {
            if(UnitCore.Instance.IsReplay)
            {
                var md = new MetroDialogSettings();
                md.AffirmativeButtonText = "确定";
                md.ColorScheme = MetroDialogColorScheme.Accented;
                var result = await VM.DialogCoordinator.ShowMessageAsync(VM, "无法最小化",
                "请先退出回放模式", MessageDialogStyle.Affirmative, md);
            }
            if (MainWindow != null&& !UnitCore.Instance.IsReplay)
            {
                MainWindow.WindowState = WindowState.Minimized;
            }
        }

        private async void ExitProgram(object sender, RoutedEventArgs e)
        {
            var md = new MetroDialogSettings();
            md.AffirmativeButtonText = "退出";
            md.NegativeButtonText = "取消";
            md.ColorScheme = MetroDialogColorScheme.Accented;
            var result = await VM.DialogCoordinator.ShowMessageAsync(VM, "退出",
                "真的要退出监控程序吗？", MessageDialogStyle.AffirmativeAndNegative, md);
            if (result == MessageDialogResult.Affirmative)
                VM.ExitProgram();
        }

        public Visibility BackButtonVisibility
        {
            get { return (Visibility)GetValue(BackButtonVisibilityProperty); }
            set { SetValue(BackButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackButtonVisibilityProperty =
            DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(TitleBarControl), new PropertyMetadata(Visibility.Visible));

        public ImageSource TitleImageSource
        {
            get { return (ImageSource)GetValue(TitleImageSourceProperty); }
            set { SetValue(TitleImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleImageSourceProperty =
            DependencyProperty.Register("TitleImageSource", typeof(ImageSource), typeof(TitleBarControl), new PropertyMetadata(null));


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TitleBarControl), new PropertyMetadata(string.Empty));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
     
        }

        private void SwitchMap(object sender, RoutedEventArgs e)
        {
            UnitCore.Instance.EventAggregator.PublishMessage(new SwitchMapModeEvent());
        }

        private async void BeginReplay(object sender, RoutedEventArgs e)
        {
            if(!UnitCore.Instance.IsReplay)
            {
                var md = new MetroDialogSettings();
                md.AffirmativeButtonText = "确定";
                md.NegativeButtonText = "取消";
                md.ColorScheme = MetroDialogColorScheme.Accented;
                var result = await VM.DialogCoordinator.ShowMessageAsync(VM, "进入回放模式",
                "实时网络通信将暂停", MessageDialogStyle.AffirmativeAndNegative, md);
                if (result == MessageDialogResult.Affirmative)
                {
                    UnitCore.Instance.IsReplay = true;
                    UnitCore.Instance.EventAggregator.PublishMessage(new ReplayModeEvent());

                }
            }
        }
    }
}
