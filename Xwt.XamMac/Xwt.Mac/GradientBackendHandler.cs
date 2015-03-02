// 
// GradientBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using System.Drawing;
using Xwt.Backends;
using Xwt.Drawing;
using System.Collections.Generic;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.CoreGraphics;
#else
using CoreGraphics;
#endif

namespace Xwt.Mac
{
	public class MacGradientBackendHandler: GradientBackendHandler
	{
		public override object CreateLinear (double x0, double y0, double x1, double y1)
		{
			return new GradientInfo () {
				Linear = true,
				Start = new PointF ((float)x0, (float)y0),
				End = new PointF ((float)x1, (float)y1)
			};
		}

		public override void Dispose (object backend)
		{
		}

		public override object CreateRadial (double cx0, double cy0, double radius0, double cx1, double cy1, double radius1)
		{
			return new GradientInfo () {
				Linear = false,
				Start = new PointF ((float)cx0, (float)cy0),
				End = new PointF ((float)cx1, (float)cy1),
				StartRadius = (float)radius0,
				EndRadius = (float)radius1
			};
		}

		public override void AddColorStop (object backend, double position, Xwt.Drawing.Color color)
		{
			GradientInfo gr = (GradientInfo) backend;
			gr.Colors.Add (color.ToCGColor ());
			gr.Stops.Add ((float)position);
		}

		internal static void Draw (CGContext ctx, GradientInfo gradient)
		{
			ctx.SaveState ();
			ctx.Clip ();
			using (var cg = new CGGradient (Util.DeviceRGBColorSpace, gradient.Colors.ToArray (), gradient.Stops.ToArray ())) {
				if (gradient.Linear)
					ctx.DrawLinearGradient (cg, gradient.Start, gradient.End, CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation);
				else
					ctx.DrawRadialGradient (cg, gradient.Start, gradient.StartRadius, gradient.End, gradient.EndRadius, CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation);
			}
			ctx.RestoreState ();
		}
	}

	class GradientInfo
	{
		public bool Linear;
		public PointF Start, End;
		public float StartRadius, EndRadius;
		public List<CGColor> Colors = new List<CGColor> ();
		public List<nfloat> Stops = new List<nfloat> ();
	}
}

