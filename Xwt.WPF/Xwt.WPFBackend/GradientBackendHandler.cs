// 
// GradientBackendHandler.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

using Xwt.Backends;
using Color = Xwt.Drawing.Color;
using DrawingColor = System.Drawing.Color;

namespace Xwt.WPFBackend
{
	public class GradientBackendHandler
		: Backend, IGradientBackendHandler
	{
		public object CreateLinear (double x0, double y0, double x1, double y1)
		{
			return new LinearGradient (
				new PointF ((float) x0, (float) y0),
				new PointF ((float) x1, (float) y1));
		}

		public object CreateRadial (double cx0, double cy0, double radius0, double cx1, double cy1, double radius1)
		{
			return new RadialGradient (cx0, cy0, radius0, cx1, cy1, radius1);
		}

		public void AddColorStop (object backend, double position, Color color)
		{
			((GradientBase)backend).ColorStops.Add (new Tuple<double, Color> (position, color));
		}
	}

	internal class RadialGradient
		: GradientBase
	{
		GraphicsPath path;
		double cx0, cy0, r0;
		double cx1, cy1, r1;

		public RadialGradient (double cx0, double cy0, double radius0, double cx1, double cy1, double radius1)
		{
			this.cx0 = cx0;
			this.cy0 = cy0;
			this.r0 = radius0;

			this.cx1 = cx1;
			this.cy1 = cy1;
			this.r1 = radius1;
		}

		public override Brush CreateBrush ()
		{
			var path = new GraphicsPath ();
			path.AddEllipse (new RectangleF ((float)(cx0 - r0), (float)(cy0 - r0), (float)(r0 * 2), (float)(r0 * 2)));

			var brush = new PathGradientBrush (path);

			var orderedStops = ColorStops.OrderBy (t => t.Item1).ToArray ();
			var colors = orderedStops.Select (i => i.Item2.ToDrawingColor ()).ToArray ();
			var stops = orderedStops.Select (i => (float) (i.Item1)).ToArray ();

			brush.InterpolationColors = new ColorBlend (colors.Length) {
				Colors = colors,
				Positions = stops
			};
			return brush;
		}
	}

	internal class LinearGradient
		: GradientBase
	{
		public LinearGradient (PointF start, PointF end)
		{
			Start = start;
			End = end;
		}

		public override System.Drawing.Brush CreateBrush ()
		{
			if (ColorStops.Count == 0)
				throw new ArgumentException ();

			var stops = ColorStops.OrderBy (t => t.Item1).ToArray ();
			var first = stops[0];
			var last = stops[stops.Length - 1];

			var brush = new System.Drawing.Drawing2D.LinearGradientBrush (Start, End, first.Item2.ToDrawingColor (),
													last.Item2.ToDrawingColor ());
			return brush;

		}
		internal readonly PointF Start;
		internal readonly PointF End;
	}

	internal abstract class GradientBase
	{
		internal readonly List<Tuple<double, Color>> ColorStops = new List<Tuple<double, Color>> ();
		public abstract Brush CreateBrush ();
	}
}