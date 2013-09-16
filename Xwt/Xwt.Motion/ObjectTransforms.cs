//
// ObjectInterpolators.cs
//
// Author:
//       Jérémie Laval <jeremie dotlaval at gmail dot com>
//
// Copyright (c) 2013 Jérémie Laval
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
using Xwt.Drawing;

namespace Xwt.Motion
{
	public static class ObjectTransforms
	{
		public static Func<double, Color> GetColorTransform (Color initialColor, Color endColor)
		{
			return ratio => new Color (Interpolate (ratio, initialColor.Red, endColor.Red),
			                           Interpolate (ratio, initialColor.Green, endColor.Green),
			                           Interpolate (ratio, initialColor.Blue, endColor.Blue),
			                           Interpolate (ratio, initialColor.Alpha, endColor.Alpha));
		}

		public static Func<double, Point> GetPointTransform (Point initialPoint, Point endPoint)
		{
			return ratio => new Point (Interpolate (ratio, initialPoint.X, endPoint.X),
			                           Interpolate (ratio, initialPoint.Y, endPoint.Y));
		}

		public static Func<double, Size> GetSizeTransform (Size initialSize, Size endSize)
		{
			return ratio => new Size (Interpolate (ratio, initialSize.Width, endSize.Width),
			                          Interpolate (ratio, initialSize.Height, endSize.Height));
		}

		public static Func<double, Rectangle> GetRectangleTransform (Rectangle initialRectangle, Rectangle endRectangle)
		{
			return ratio => new Rectangle (Interpolate (ratio, initialRectangle.X, endRectangle.X),
			                               Interpolate (ratio, initialRectangle.Y, endRectangle.Y),
			                               Interpolate (ratio, initialRectangle.Width, endRectangle.Width),
			                               Interpolate (ratio, initialRectangle.Height, endRectangle.Height));
		}

		static double Interpolate (double ratio, double beginValue, double endValue)
		{
			return beginValue + (endValue - beginValue) * ratio;
		}
	}
}
