using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET.WindowsPresentation;
using LOUV.Torp.BaseType;
using LOUV.Torp.Monitor.Views;
using System.Globalization;
using System.Windows.Media.Animation;

namespace LOUV.Torp.Monitor.Controls.MapCustom
{
    /// <summary>
    /// ObjectMarker.xaml 的交互逻辑
    /// </summary>
    public partial class ObjectMarker
    {
        
        private Target _target;
        GMapMarker Marker;
        bool _popup;
        HomePageView MainWindow;
        readonly ScaleTransform scale = new ScaleTransform(1, 1);
        bool needAni = false;// set back to false after animation done!!
        public ObjectMarker(HomePageView window, GMapMarker marker, Target target)
        {
            this.InitializeComponent();

            this.MainWindow = window;
            this.Marker = marker;
            RenderTransform = scale;
            this.Loaded += new RoutedEventHandler(ObjectMarker_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(ObjectMarker_SizeChanged);
            this.MouseEnter += new MouseEventHandler(ObjectMarker_MouseEnter);
            this.MouseLeave += new MouseEventHandler(ObjectMarker_MouseLeave);

            this.MouseLeftButtonUp += new MouseButtonEventHandler(ObjectMarker_MouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ObjectMarker_MouseLeftButtonDown);

            _popup = false;
            _target = target;
        }

        public void Refresh(Target target)
        {
            _target = target;
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
        private void ObjectMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        private void ObjectMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }
        public DropShadowEffect ShadowEffect;
        private void ObjectMarker_MouseLeave(object sender, MouseEventArgs e)
        {
            Marker.ZIndex -= 10000;
            Cursor = Cursors.Arrow;
            this.Effect = null;

            scale.ScaleY = 1;
            scale.ScaleX = 1;
            MouseOver = false;
            InvalidateVisual();
        }

        private void ObjectMarker_MouseEnter(object sender, MouseEventArgs e)
        {
            Marker.ZIndex += 10000;
            Cursor = Cursors.Hand;
            this.Effect = ShadowEffect;

            scale.ScaleY = 1.1;
            scale.ScaleX = 1.1;
            MouseOver = true;
            InvalidateVisual();
        }

        private void ObjectMarker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height);
        }

        private void ObjectMarker_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            BroadCastAni(drawingContext, 100, TimeSpan.FromMilliseconds(5000));
            ShowTooltip(drawingContext);
        }
        protected void ShowTooltip(DrawingContext drawingContext)
        {
            if (PopUp || MouseOver)
            {
                Typeface tf = new Typeface("GenericSansSerif");
                System.Windows.FlowDirection fd = new System.Windows.FlowDirection();

                FormattedText ft = new FormattedText(_target.Name + "    " + _target.Time+"(UTC)", CultureInfo.CurrentUICulture, fd, tf, 18, Brushes.Red);
                drawingContext.DrawText(ft, new Point(40, -60));
                string latlngstr = "经度:" + _target.Longitude.ToString("F06") + " | 纬度:" + _target.Latitude.ToString("F06");
                ft = new FormattedText(latlngstr, CultureInfo.CurrentUICulture, fd, tf, 16, Brushes.White);
                drawingContext.DrawText(ft, new Point(40, -40));
                ft = new FormattedText("测算深度:" + _target.Depth.ToString("F02"), CultureInfo.CurrentUICulture, fd, tf, 16, Brushes.White);
                drawingContext.DrawText(ft, new Point(40, -20));
                ft = new FormattedText("测距状态:" + _target.Status, CultureInfo.CurrentUICulture, fd, tf, 16, Brushes.White);
                drawingContext.DrawText(ft, new Point(40, 0));
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

                drawingContext.DrawEllipse(null, myPen, new Point(Width / 2, Height / 2), null, Width / 2, myClock, Height / 2, myClock);
                drawingContext.DrawText(FText, new Point(-FText.Width / 2, -FText.Height));
            }
        }
        }
}
