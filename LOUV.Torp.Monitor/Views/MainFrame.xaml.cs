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
            var FixTypeBox = newdialog.FindChild<ComboBox>("ModeTypeBox");
            var FixtimeBox = newdialog.FindChild<TextBox>("timevalue");
            var FixdistBox = newdialog.FindChild<TextBox>("distvalue");
            var FixdepBox = newdialog.FindChild<TextBox>("depthvalue");
            var FixdirBox = newdialog.FindChild<TextBox>("dirvalue");
            var FixspeedBox = newdialog.FindChild<NumericUpDown>("speedvalue");
            byte[] cmd = new byte[1032];
            Array.Clear(cmd, 0,1032);
            cmd[0] = 0x2B;
            cmd[1] = 0x00;
            Buffer.BlockCopy(BitConverter.GetBytes((Int16)XmtValueBox.Value.Value*32767/100), 0, cmd, 2,2);
            byte[] tinycmd = new byte[19];
            if(CmdTypeBox.SelectedIndex==0)
            {
                tinycmd[0] = 0x09;
                
                Buffer.BlockCopy(BitConverter.GetBytes((byte)AUVIDBox.Value.Value), 0, tinycmd, 1, 1);
                Buffer.BlockCopy(BitConverter.GetBytes((Int16)XmtValue3DBox.Value.Value*32767/100), 0, tinycmd, 2, 2);
            }
            if(CmdTypeBox.SelectedIndex==1)
            {
                tinycmd[0] = 0x11;
                Buffer.BlockCopy(BitConverter.GetBytes((byte)AUVIDBox.Value.Value), 0, tinycmd, 1, 1);
            }
            if (CmdTypeBox.SelectedIndex == 2)
            {
                tinycmd[0] = 0x19;
                Buffer.BlockCopy(BitConverter.GetBytes((byte)AUVIDBox.Value.Value), 0, tinycmd, 1, 1);
                tinycmd[2] = 0x05;
                tinycmd[3] = 0x81;
                if(AUVIDBox.Value.Value==0)
                {
                    tinycmd[4] = 0x04;
                }
                if(AUVIDBox.Value.Value==1)
                {
                    tinycmd[4] = 0x16;
                }
                tinycmd[5] = 0x0;
                tinycmd[6] = 0x46;
                tinycmd[7] = 0x0;
                Buffer.BlockCopy(BitConverter.GetBytes((byte)AUVIDBox.Value.Value), 0, tinycmd, 1, 1);
            }
            if (CmdTypeBox.SelectedIndex == 3)
            {
                tinycmd[0] = 0x19;
                Buffer.BlockCopy(BitConverter.GetBytes((byte)AUVIDBox.Value.Value), 0, tinycmd, 1, 1);
                tinycmd[2] = 0x0D;
                tinycmd[3] = 0x81;
                if (AUVIDBox.Value.Value == 0)
                {
                    tinycmd[4] = 0x04;
                }
                if (AUVIDBox.Value.Value == 1)
                {
                    tinycmd[4] = 0x16;
                }
                tinycmd[5] = 0x0;
                tinycmd[6] = 0x43;
                tinycmd[7] = 0x08;
                if (FixTypeBox.SelectedIndex == 0)
                {
                    tinycmd[8] = 2;
                    var timebytes = BitConverter.GetBytes((UInt16)(float.Parse(FixtimeBox.Text)*10));
                    tinycmd[13] = timebytes[1];
                    tinycmd[14] = timebytes[0];
                }

                if (FixTypeBox.SelectedIndex == 1)
                {
                    tinycmd[8] = 1;
                    var distbytes = BitConverter.GetBytes((UInt16)(float.Parse(FixdistBox.Text) * 10));
                    tinycmd[13] = distbytes[1];
                    tinycmd[14] = distbytes[0];
                }
                var deptbytes = BitConverter.GetBytes((UInt16)(float.Parse(FixdepBox.Text) * 10));
                tinycmd[9] = deptbytes[1];
                tinycmd[10] = deptbytes[0];
                var dirbytes = BitConverter.GetBytes((UInt16)(float.Parse(FixdirBox.Text) * 10));
                tinycmd[11] = deptbytes[1];
                tinycmd[12] = deptbytes[0];
                tinycmd[15] = BitConverter.GetBytes(FixspeedBox.Value.Value)[0];

            }
            Buffer.BlockCopy(tinycmd, 0, cmd, 8, 19);
            //send cmd
            Buoy b =  (Buoy)UnitCore.Instance.Buoy[id - 1];
            UnitCore.Instance.NetCore.UDPSend(b.IP, cmd);
            await TaskEx.Delay(500);
            await MainFrameViewModel.pMainFrame.DialogCoordinator.HideMetroDialogAsync(MainFrameViewModel.pMainFrame,
                        newdialog);
        }

    }
}
