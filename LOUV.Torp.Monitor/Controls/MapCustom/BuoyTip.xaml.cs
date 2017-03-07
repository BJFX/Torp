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
        public void SetBuoy(Buoy buoy)
        {
            BuoyID.Text = buoy.Name;
            Lat.Text = buoy.gps.Latitude.ToString("F6");
            Long.Text = buoy.gps.Longitude.ToString("F6");
            TimeGps.Text = buoy.gps.UTCTime.ToLocalTime().ToShortTimeString();
            Range.Text = buoy.Range.ToString("F6");
        }
        public void SetTarget(Target target)
        {
            BuoyID.Text = target.Name;
            Lat.Text = target.Latitude.ToString("F6");
            Long.Text = target.Longitude.ToString("F6");
            TimeGps.Text = target.UTCTime.ToLocalTime().ToShortTimeString();
            Range.Text = target.Status;
        }
    }
}
