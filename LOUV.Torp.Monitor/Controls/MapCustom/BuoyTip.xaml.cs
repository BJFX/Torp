using LOUV.Torp.BaseType;
using System;
using System.Collections.Generic;
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

namespace LOUV.Torp.Monitor.Controls.MapCustom
{
    /// <summary>
    /// BuoyTip.xaml 的交互逻辑
    /// </summary>
    public partial class BuoyTip : UserControl
    {
        public BuoyTip()
        {
            InitializeComponent();
        }
        public void SetValues(Buoy buoy)
        {
            BuoyID.Text = buoy.Name;
            Range.Text = buoy.Range;
            Lat.Text = buoy.Latitude.ToString("F6");
            Long.Text = buoy.Longitude.ToString("F6");
            TimeGps.Text = buoy.GpsTime;
            Memo.Text = buoy.Memo;
        }
    }
}
