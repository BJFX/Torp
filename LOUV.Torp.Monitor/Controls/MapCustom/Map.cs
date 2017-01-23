using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public string MapName { get; set; }
        readonly Typeface tf = new Typeface("GenericSansSerif");
        readonly System.Windows.FlowDirection fd = new System.Windows.FlowDirection();
        Pen cross = new Pen(Brushes.Red, 1);
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            FormattedText text = new FormattedText(MapName, CultureInfo.CurrentUICulture, fd, tf, 24, Brushes.Blue);
            drawingContext.DrawText(text, new Point((ActualWidth -text.Width)/ 2, ActualHeight - text.Height));

            drawingContext.DrawLine(cross, new Point((ActualWidth / 2) - 10, ActualHeight / 2), new Point((ActualWidth / 2) + 10, ActualHeight / 2));
            drawingContext.DrawLine(cross, new Point(ActualWidth / 2, (ActualHeight / 2) - 10), new Point(ActualWidth / 2, (ActualHeight / 2) + 10));
        }
    }
}
