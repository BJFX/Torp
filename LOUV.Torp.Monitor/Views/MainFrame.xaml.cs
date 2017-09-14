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
using System;
using LOUV.Torp.BaseType;

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

        private async void SendCMD_Click(object sender, RoutedEventArgs e)
        {
            var newdialog = (BaseMetroDialog)Application.Current.MainWindow.Resources["BuoyCMDDialog"];
            var id = int.Parse(newdialog.Title.Substring(3, 1));
            var CmdTypeBox = newdialog.FindChild<ComboBox>("CmdTypeBox");
            var AUVIDBox = newdialog.FindChild<NumericUpDown>("AUVIDBox");
            var XmtValue3DBox = newdialog.FindChild<NumericUpDown>("XmtValue3DBox");
            var XmtValueBox = newdialog.FindChild<NumericUpDown>("XmtValueBox");
            byte[] cmd = new byte[1032];
            Array.Clear(cmd, 0,1032);
            cmd[0] = 0x2B;
            cmd[1] = 0x00;
            Buffer.BlockCopy(BitConverter.GetBytes((Int16)XmtValueBox.Value.Value), 0, cmd, 2,2);
            byte[] tinycmd = new byte[19];
            if(CmdTypeBox.SelectedIndex==0)
            {
                tinycmd[0] = 0x09;
                
                Buffer.BlockCopy(BitConverter.GetBytes((byte)AUVIDBox.Value.Value), 0, tinycmd, 1, 1);
                Buffer.BlockCopy(BitConverter.GetBytes((Int16)XmtValue3DBox.Value.Value), 0, tinycmd, 2, 2);
            }
            if(CmdTypeBox.SelectedIndex==1)
            {
                tinycmd[0] = 0x11;
                Buffer.BlockCopy(BitConverter.GetBytes((byte)AUVIDBox.Value.Value), 0, tinycmd, 1, 1);
            }
            Buffer.BlockCopy(tinycmd, 0, cmd, 8, 19);
            //send cmd
            Buoy b =  (Buoy)UnitCore.Instance.Buoy[id - 1];
            UnitCore.Instance.NetCore.UDPSend(b.IP, cmd);
            await TaskEx.Delay(500);
            await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame,
                        newdialog);
        }

        private void XmtValue3DBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            var newdialog = (BaseMetroDialog)Application.Current.MainWindow.Resources["BuoyCMDDialog"];
            if (newdialog == null)
                return;
            var XmtValueBox = newdialog.FindChild<NumericUpDown>("XmtValue3DBox");
            var XmtValuelabel = newdialog.FindChild<Label>("XmlAmp3DLabel");
            if (XmtValueBox.Value == 0)
            {
                XmtValuelabel.Content = "0%";
            }
            else if (XmtValueBox.Value.HasValue)
            {
                XmtValuelabel.Content = ((int)XmtValueBox.Value.Value * 100 / 32767) + "%";
            }
        }

    }
}
