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
using CoreGraphics;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class MacPathBackendHandler: DrawingPathBackendHandler
	{
		const double degrees = System.Math.PI / 180d;

		public MacPathBackendHandler ()
		{
		}

		public override void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			((CGPath)backend).AddArc ((float)xc, (float)yc, (float)radius, (float)(angle1 * degrees), (float)(angle2 * degrees), false);
		}

		public override void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			((CGPath)backend).AddArc ((float)xc, (float)yc, (float)radius, (float)(angle1 * degrees), (float)(angle2 * degrees), true);
		}

		public override void ClosePath (object backend)
		{
			((CGPath)backend).CloseSubpath ();
		}

		public override void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			((CGPath)backend).AddCurveToPoint ((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3);
		}

		public override void LineTo (object backend, double x, double y)
		{
			((CGPath)backend).AddLineToPoint ((float)x, (float)y);
		}

		public override void MoveTo (object backend, double x, double y)
		{
			((CGPath)backend).MoveToPoint ((float)x, (float)y);
		}

		public override void Rectangle (object backend, double x, double y, double width, double height)
		{
			((CGPath)backend).AddRect (new CGRect ((nfloat)x, (nfloat)y, (nfloat)width, (nfloat)height));
		}

		public override void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			CGPath path = (CGPath)backend;
			CGPoint p = path.CurrentPoint;
			path.AddCurveToPoint ((float)(p.X + dx1), (float)(p.Y + dy1), (float)(p.X + dx2), (float)(p.Y + dy2), (float)(p.X + dx3), (float)(p.Y + dy3));
		}

		public override void RelLineTo (object backend, double dx, double dy)
		{
			CGPath path = (CGPath)backend;
			CGPoint p = path.CurrentPoint;
			path.AddLineToPoint ((float)(p.X + dx), (float)(p.Y + dy));
		}

		public override void RelMoveTo (object backend, double dx, double dy)
		{
			CGPath path = (CGPath)backend;
			CGPoint p = path.CurrentPoint;
			path.MoveToPoint ((float)(p.X + dx), (float)(p.Y + dy));
		}

		public override object CreatePath ()
		{
			return new CGPath ();
		}

		public override object CopyPath (object backend)
		{
			return new CGPath ((CGPath)backend);
		}

		public override void AppendPath (object backend, object otherBackend)
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

		public override bool IsPointInFill (object backend, double x, double y)
		{
			return ((CGPath)backend).ContainsPoint (new CGPoint ((nfloat)x, (nfloat)y), false);
		}

		public override void Dispose (object backend)
		{
			((CGPath)backend).Dispose ();
		}
	}
}

