using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace LOUV.Torp.Monitor.Controls.MapCustom
{
    /// <summary>
    /// 自定义地图类，可以显示自定义文字和标识
    /// </summary>
    public class Map:GMapControl
    {
        private string name = "地图";

        public string MapName
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                }
            }
        }
        public PointLatLng MousePosition { get; set; }

        readonly Typeface tf = new Typeface("GenericSansSerif");
        readonly System.Windows.FlowDirection fd = new System.Windows.FlowDirection();
        Pen cross = new Pen(Brushes.Red, 1);
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            FormattedText text = new FormattedText(MapName, CultureInfo.CurrentUICulture, fd, tf, 24, Brushes.White);
            FormattedText centerpos = new FormattedText("Center:"+Position.ToString(), CultureInfo.CurrentUICulture, fd, tf, 16, Brushes.WhiteSmoke);
            drawingContext.DrawText(text, new Point(20, ActualHeight - text.Height-20));
            drawingContext.DrawText(centerpos, new Point((ActualWidth - centerpos.Width) / 2-40, ActualHeight - centerpos.Height));
            FormattedText mousepos = new FormattedText("mouse:"+MousePosition.ToString(), CultureInfo.CurrentUICulture, fd, tf, 16, Brushes.WhiteSmoke);
            drawingContext.DrawText(mousepos, new Point(ActualWidth - mousepos.Width-60, ActualHeight - mousepos.Height));
            drawingContext.DrawLine(cross, new Point((ActualWidth / 2) - 10, ActualHeight / 2), new Point((ActualWidth / 2) + 10, ActualHeight / 2));
            drawingContext.DrawLine(cross, new Point(ActualWidth / 2, (ActualHeight / 2) - 10), new Point(ActualWidth / 2, (ActualHeight / 2) + 10));
        }
    }
}
