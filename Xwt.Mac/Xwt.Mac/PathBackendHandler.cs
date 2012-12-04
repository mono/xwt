// 
// PathBackendHandler.cs
//  
// Author:
//       Alex Corrado <corrado@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt.Backends;
using Xwt.Engine;
using MonoMac.AppKit;
using Xwt.Drawing;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
using System.Drawing;

namespace Xwt.Mac
{
	public class PathBackendHandler: IPathBackendHandler
	{
		const double degrees = System.Math.PI / 180d;

		public PathBackendHandler ()
		{
		}

		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			((CGPath)backend).AddArc ((float)xc, (float)yc, (float)radius, (float)(angle1 * degrees), (float)(angle2 * degrees), false);
		}

		public void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			((CGPath)backend).AddArc ((float)xc, (float)yc, (float)radius, (float)(angle1 * degrees), (float)(angle2 * degrees), true);
		}

		public void ClosePath (object backend)
		{
			((CGPath)backend).CloseSubpath ();
		}

		public void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			((CGPath)backend).AddCurveToPoint ((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3);
		}

		public void LineTo (object backend, double x, double y)
		{
			((CGPath)backend).AddLineToPoint ((float)x, (float)y);
		}

		public void MoveTo (object backend, double x, double y)
		{
			((CGPath)backend).MoveToPoint ((float)x, (float)y);
		}

		public void Rectangle (object backend, double x, double y, double width, double height)
		{
			((CGPath)backend).AddRect (new RectangleF ((float)x, (float)y, (float)width, (float)height));
		}

		public void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			CGPath path = (CGPath)backend;
			PointF p = path.CurrentPoint;
			path.AddCurveToPoint ((float)(p.X + dx1), (float)(p.Y + dy1), (float)(p.X + dx2), (float)(p.Y + dy2), (float)(p.X + dx3), (float)(p.Y + dy3));
		}

		public void RelLineTo (object backend, double dx, double dy)
		{
			CGPath path = (CGPath)backend;
			PointF p = path.CurrentPoint;
			path.AddLineToPoint ((float)(p.X + dx), (float)(p.Y + dy));
		}

		public void RelMoveTo (object backend, double dx, double dy)
		{
			CGPath path = (CGPath)backend;
			PointF p = path.CurrentPoint;
			path.MoveToPoint ((float)(p.X + dx), (float)(p.Y + dy));
		}

		public object CreatePath ()
		{
			return new CGPath ();
		}

		public void AppendPath (object backend, object otherBackend)
		{
			CGPath dest = (CGPath)backend;
			CGContextBackend src = otherBackend as CGContextBackend;

			if (src != null) {
				using (var path = src.Context.CopyPath ())
					dest.AddPath (path);
			} else {
				dest.AddPath ((CGPath)otherBackend);
			}
		}

		public bool IsPointInFill (object backend, double x, double y)
		{
			return ((CGPath)backend).ContainsPoint (new PointF ((float)x, (float)y), false);
		}

		public void Dispose (object backend)
		{
			((CGPath)backend).Dispose ();
		}
	}
}

