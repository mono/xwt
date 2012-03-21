// 
// GradientBackendHandler.cs
//  
// Author:
//       Lytico 
// 
// Copyright (c) 2012 Lytico (http://limada.sourceforge.net)
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

using System.Drawing;
using System.Drawing.Drawing2D;
using Xwt.Backends;

namespace Xwt.Gdi.Backend {
    /// <summary>
    /// todo
    /// </summary>
    public class GradientBackendHandler : IGradientBackendHandler {
        /// <summary>
        /// todo: make a GradientData-class
        /// </summary>
        public object CreateLinear(double x0, double y0, double x1, double y1) {
            var color1 = Color.Black;
            var color2 = Color.Black;
            return new LinearGradientBrush(new PointF((float)x0, (float)y0), new PointF((float)x1, (float)y1), color1,color2);
        }

        /// <summary>
        /// todo: make a GradientData-class and set a color
        /// </summary>
        public void AddColorStop(object backend, double position, Xwt.Drawing.Color color) {
            var g = (LinearGradientBrush)backend;
           
        }
    }
}