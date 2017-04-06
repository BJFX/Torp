using System.Net.PeerToPeer.Collaboration;
using GMap.NET.WindowsPresentation;
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
using LOUV.Torp.BaseType;
using LOUV.Torp.Monitor.Views;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Effects;
using System.Globalization;
using System.Windows.Media.Animation;

namespace LOUV.Torp.Monitor.Controls.MapCustom
{
    /// <summary>
    /// BuoyMarker.xaml 的交互逻辑
    /// </summary>
    public partial class BuoyMarker:UserControl
    {
        //Popup Popup;
        GMapMarker Marker;
        private Buoy _buoy;
        bool _popup;
        HomePageView MainWindow;
        readonly ScaleTransform scale = new ScaleTransform(1, 1);
        bool needAni = false;// set back to false after animation done!!
        public BuoyMarker(HomePageView window, GMapMarker marker, Buoy buoy)
        {
            this.InitializeComponent();

            this.MainWindow = window;
            this.Marker = marker;

            RenderTransform = scale;
            this.Loaded += new RoutedEventHandler(BuoyMarker_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(BuoyMarker_SizeChanged);
            this.MouseEnter += new MouseEventHandler(MarkerControl_MouseEnter);
            this.MouseLeave += new MouseEventHandler(MarkerControl_MouseLeave);
            //this.MouseMove += new MouseEventHandler(BuoyMarker_MouseMove);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(BuoyMarker_MouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(BuoyMarker_MouseLeftButtonDown);

            _popup = false;
            _buoy = buoy;
            Text = " ";
        }

        public void Refresh(Buoy buoy)
        {
            _buoy = buoy;
            InvalidateVisual();
        }
        public void StartAnimation()
        {
            needAni = true;
            InvalidateVisual();
        }
        public string Text { get; set; }
        public bool PopUp
        {
            get
            {
                return _popup;
            }
            set
            {
                _popup = value;
                InvalidateVisual();
            }
        }
        private bool MouseOver = false;
        private void BuoyMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        private void BuoyMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }
        public DropShadowEffect ShadowEffect;
        private void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Marker.ZIndex -= 10000;
            Cursor = Cursors.Arrow;
            this.Effect = null;

            scale.ScaleY = 1;
            scale.ScaleX = 1;
            MouseOver = false;
            InvalidateVisual();
        }

        private void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Marker.ZIndex += 10000;
            Cursor = Cursors.Hand;
            this.Effect = ShadowEffect;

            scale.ScaleY = 1.1;
            scale.ScaleX = 1.1;
            MouseOver = true;
            InvalidateVisual();
            //needAni = true;
        }

        private void BuoyMarker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height);
        }

        private void BuoyMarker_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            BroadCastAni(drawingContext, 100, TimeSpan.FromMilliseconds(5));   
            ShowTooltip(drawingContext);
        }
        protected void ShowTooltip(DrawingContext drawingContext)
        {
            if (PopUp || MouseOver)
            {
                Typeface tf = new Typeface("GenericSansSerif");
                System.Windows.FlowDirection fd = new System.Windows.FlowDirection();

                FormattedText ft = new FormattedText(_buoy.Name +"   "+ _buoy.Time + "(UTC)", CultureInfo.CurrentUICulture, fd, tf, 18, Brushes.Red);
                drawingContext.DrawText(ft, new Point(40, -40));
                string latlngstr = "经度:" + _buoy.gps.Longitude.ToString("F06") + "\r纬度:" + _buoy.gps.Latitude.ToString("F06");
                ft = new FormattedText(latlngstr, CultureInfo.CurrentUICulture, fd, tf, 16, Brushes.White);
                drawingContext.DrawText(ft, new Point(40, -20));
                ft = new FormattedText("测距:"+_buoy.Range.ToString("F02"), CultureInfo.CurrentUICulture, fd, tf, 16, Brushes.White);
                drawingContext.DrawText(ft, new Point(40, 20));
            }
        }
        protected void BroadCastAni(DrawingContext drawingContext, float radius, TimeSpan ts)
        {
            if (needAni)
            {
                needAni = false;

                Pen Stroke = new Pen(Brushes.Blue, 2.0);
                Pen myPen = new Pen(Brushes.Blue, 2.0);
                Typeface Font = new Typeface(new FontFamily("GenericSansSerif"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                FormattedText FText = new FormattedText(Text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Font, FontSize, Foreground);
                DoubleAnimationUsingKeyFrames myAnimation = new DoubleAnimationUsingKeyFrames();
                var keyFrames = myAnimation.KeyFrames;
                keyFrames.Add(new SplineDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
                keyFrames.Add(new SplineDoubleKeyFrame(radius, TimeSpan.FromSeconds(0.5), new KeySpline(0, 0, 1, 1)));
                keyFrames.Add(new SplineDoubleKeyFrame(0, TimeSpan.FromSeconds(0.5)));
                myAnimation.RepeatBehavior = new RepeatBehavior(ts);

                // Create a clock the for the animation.
                AnimationClock myClock = myAnimation.CreateClock();

                //drawingContext.DrawEllipse(null, Stroke, new Point(Width / 2, Height / 2), Width / 2 + Stroke.Thickness / 2, Height / 2 + Stroke.Thickness / 2);
                //drawingContext.DrawEllipse(Background, null, new Point(Width / 2, Height / 2), Width / 2, Height / 2);
                drawingContext.DrawEllipse(null, myPen, new Point(Width / 2, Height / 2), null, Width / 2, myClock, Height / 2, myClock);
                drawingContext.DrawText(FText, new Point(-FText.Width / 2, -FText.Height));
            }
        }
    }
}
