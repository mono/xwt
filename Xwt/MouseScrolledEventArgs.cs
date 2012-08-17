using System;

namespace Xwt
{
    public enum ScrollDirection {
        Up,
        Down,
        Left,
        Right
    }

    public class MouseScrolledEventArgs: EventArgs
    {
        public MouseScrolledEventArgs (long timestamp, double x, double y, ScrollDirection direction)
        {
            X = x;
            Y = y;
            Timestamp  = timestamp;
            Direction = direction;
        }
        
        public bool Handled { get; set; }
        
        public double X { get; private set; }
        public double Y { get; private set; }
        public ScrollDirection Direction { get; private set; }
        
        public long Timestamp { get; private set; }
    }
}

