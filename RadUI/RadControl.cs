using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace RadUI
{
    public class RadControl : FrameworkElement
    {
        private readonly DispatcherTimer _timer;
        private double _sweepAngleDeg = 0;
        private double MaxDistanceCm { get; set; } = 200;

        private readonly List<Blip> _blips = new();
        private record Blip(double AngleDeg, double DistanceCm, DateTime TimeStamp);

        public RadControl(){
            //Konstruktor mit Timer 
            _timer = new DispatcherTimer{ Interval = TimeSpan.FromMilliseconds(16)}; //16ms ca. 60fps
            _timer.Tick += (_, __) =>
            {
                _sweepAngleDeg += 1.2;
                if (_sweepAngleDeg > 180) _sweepAngleDeg = 0;

                var cutoff = DateTime.UtcNow - TimeSpan.FromSeconds(1.2);
                _blips.RemoveAll(b => b.TimeStamp < cutoff);

                InvalidateVisual();
            };
            Loaded += (_, __) => _timer.Start();
            Unloaded += (_, __) => _timer.Stop();
        }

        public void AddMeasurement(double angleDeg, double distanceCm)
        {
            if (angleDeg < 0 || angleDeg > 180) return;
            if (distanceCm <= 0 || distanceCm > MaxDistanceCm) return;

            _blips.Add(new Blip(angleDeg, distanceCm, DateTime.UtcNow));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var width = ActualWidth;
            var height = ActualHeight;
            if (width <= 10 || height <= 10) return;

            var center = new Point(width/2.0 , height);
            var radius = Math.Min(width/2.0 , height) -10 ;

            //Hintergrund
            drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));

            var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(90, 0, 255, 0)), 1);
            gridPen.Freeze();

            var brightPen = new Pen(new SolidColorBrush(Color.FromArgb(200, 0, 255, 0)), 2);
            brightPen.Freeze();

            // Halbkreis

            // Grad- Linien 

            // Blips 
        }

        //Polarkoordinaten umwandeln in Punkt
        private static Point PolarToPoint(Point center, double r, double angleDeg)
        {
            double rad = angleDeg * Math.PI / 180;
            double x = center.X + r * Math.Cos(rad);
            double y = center.Y - r * Math.Sin(rad); //minus wichtig damit 0 Grad oben ist 

            return new Point(x, y);
        }

        private static void DrawArc(DrawingContext drawingContext, Point center, double radius, double startDeg, double endDeg, Pen pen)
        {
            if (radius <= 0) return;

            Point start = PolarToPoint(center, radius, startDeg);
            Point end = PolarToPoint(center, radius, endDeg);

            bool isLargeArc = (endDeg -  startDeg) >180;

            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                ctx.BeginFigure(start, false, false);
                ctx.ArcTo(end, new Size(radius, radius),0, isLargeArc, SweepDirection.Counterclockwise, true, false);

            }
            geom.Freeze();

            drawingContext.DrawGeometry(null, pen, geom);
        }
    }
}
