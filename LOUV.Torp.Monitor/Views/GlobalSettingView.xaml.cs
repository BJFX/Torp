using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using LOUV.Torp.ICore;
using LOUV.Torp.Monitor.Core;
using LOUV.Torp.Monitor.Events;
using DevExpress.Xpf.Core;
using Microsoft.Win32;

namespace LOUV.Torp.Monitor.Views
{
    /// <summary>
    /// GlobalSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class GlobalSettingView : Page
    {
        private DispatcherTimer t;
        private DispatcherTimer modet;
        private Stream Updatefile;
        public GlobalSettingView()
        {
            InitializeComponent();
        }

    }
}
