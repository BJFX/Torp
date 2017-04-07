using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using ImageProc;
using MahApps.Metro.Controls;
using TinyMetroWpfLibrary.Controller;
using MahApps.Metro.Controls.Dialogs;
using LOUV.Torp.Monitor.Core;
using LOUV.Torp.Monitor.ViewModel;

namespace LOUV.Torp.Monitor.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainFrame
    {
        [DllImport("user32.dll")]
        private extern static bool SwapMouseButton(bool fSwap);
        public MainFrame()
        {
            InitializeComponent();
            MainFrameViewModel.pMainFrame.DialogCoordinator = DialogCoordinator.Instance;
            Kernel.Instance.Controller.SetRootFrame(ContentFrame);
        }

        private async void ContentFrame_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
            Application.Current.MainWindow = this;
            
            await TaskEx.Run(()=>UnitCore.Instance.Start());
            Kernel.Instance.Controller.NavigateToPage("Views/HomePageView.xaml");
            LOUV.Torp.Monitor.Helpers.LogHelper.WriteLog("开始工作");
            
            //SwapMouseButton(false);//switch back
        }

        private void MetroWindow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void XmtValueBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        { 
            var newdialog = (BaseMetroDialog)Application.Current.MainWindow.Resources["BuoyCMDDialog"];
            if (newdialog == null)
                return;
            var XmtValueBox = newdialog.FindChild<NumericUpDown>("XmtValueBox");
            var XmtValuelabel = newdialog.FindChild<Label>("XmlAmpLabel");
            if (XmtValueBox.Value == 0)
            {
                XmtValuelabel.Content = "0%";
            }
            else if(XmtValueBox.Value.HasValue)
            {
                XmtValuelabel.Content = ((int)XmtValueBox.Value.Value * 100 / 32767)+"%";
            }
        }

        private async void CloseCMD_Click(object sender, RoutedEventArgs e)
        {
            var newdialog = (BaseMetroDialog)Application.Current.MainWindow.Resources["BuoyCMDDialog"];
            await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame,
                        newdialog);
        }

        private void SendCMD_Click(object sender, RoutedEventArgs e)
        {
            var newdialog = (BaseMetroDialog)Application.Current.MainWindow.Resources["BuoyCMDDialog"];
            var id = int.Parse(newdialog.Title.Substring(3, 1));
            //send cmd
            UnitCore.Instance.NetCore.UDPSend(UnitCore.Instance.MonConfigueService.GetNet().IP[id-1],);
        }
    }
}
