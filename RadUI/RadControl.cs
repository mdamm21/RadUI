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

        //Konstruktor mit Timer 
        public RadControl(){ 
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
        //Zeichnen mit OnRender
        //Blips hinzufügen
    }
}
