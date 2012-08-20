// 
// MouseScrolledEventArgs.cs
//  
// Author:
//       Wolfgang Silbermayr <wolfgang@silbermayr.at>
// 
// Copyright (c) 2012 Wolfgang Silbermayr
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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

