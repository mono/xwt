// 
// ContextBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Alex Corrado <corrado@xamarin.com>
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
using Xwt.Backends;

using Xwt.Drawing;
using System.Drawing;
using System.Collections.Generic;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGRect = System.Drawing.RectangleF;
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
#else
using CoreGraphics;
using AppKit;
#endif

namespace Xwt.Mac
{
	class CGContextBackend {
		public CGContext Context;
		public SizeF Size;
		public CGAffineTransform? InverseViewTransform;
		public Stack<ContextStatus> StatusStack = new Stack<ContextStatus> ();
		public ContextStatus CurrentStatus = new ContextStatus ();
		public double ScaleFactor = 1;
	}

	class ContextStatus
	{
		public object Pattern;
	}

	public class MacContextBackendHandler: ContextBackendHandler
	{
		const double degrees = System.Math.PI / 180d;

		public override double GetScaleFactor (object backend)
		{
			var ct = (CGContextBackend) backend;
			return ct.ScaleFactor;
		}

		public override void Save (object backend)
		{
			var ct = (CGContextBackend) backend;
			ct.Context.SaveState ();
			ct.StatusStack.Push (ct.CurrentStatus);
			var newStatus = new ContextStatus ();
			newStatus.Pattern = ct.CurrentStatus.Pattern;
			ct.CurrentStatus = newStatus;
		}
		
		public override void Restore (object backend)
		{
			var ct = (CGContextBackend) backend;
			ct.Context.RestoreState ();
			ct.CurrentStatus = ct.StatusStack.Pop ();
		}

		public override void SetGlobalAlpha (object backend, double alpha)
		{
			((CGContextBackend)backend).Context.SetAlpha ((float)alpha);
		}

		public override void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			ctx.AddArc ((float)xc, (float)yc, (float)radius, (float)(angle1 * degrees), (float)(angle2 * degrees), false);
		}

		public override void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			ctx.AddArc ((float)xc, (float)yc, (float)radius, (float)(angle1 * degrees), (float)(angle2 * degrees), true);
		}

		public override void Clip (object backend)
		{
			((CGContextBackend)backend).Context.Clip ();
		}

		public override void ClipPreserve (object backend)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			using (CGPath oldPath = ctx.CopyPath ()) {
				ctx.Clip ();
				ctx.AddPath (oldPath);
			}
		}

		public override void ClosePath (object backend)
		{
			((CGContextBackend)backend).Context.ClosePath ();
		}

		public override void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			((CGContextBackend)backend).Context.AddCurveToPoint ((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3);
		}

		public override void Fill (object backend)
		{
			CGContextBackend gc = (CGContextBackend)backend;
			CGContext ctx = gc.Context;
			SetupContextForDrawing (ctx);

			if (gc.CurrentStatus.Pattern is GradientInfo) {
				MacGradientBackendHandler.Draw (ctx, ((GradientInfo)gc.CurrentStatus.Pattern));
			}
			else if (gc.CurrentStatus.Pattern is ImagePatternInfo) {
				SetupPattern (gc);
				ctx.DrawPath (CGPathDrawingMode.Fill);
			}
			else {
				ctx.DrawPath (CGPathDrawingMode.Fill);
			}
		}

		public override void FillPreserve (object backend)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			using (CGPath oldPath = ctx.CopyPath ()) {
				Fill (backend);
				ctx.AddPath (oldPath);
			}
		}

		public override void LineTo (object backend, double x, double y)
		{
			((CGContextBackend)backend).Context.AddLineToPoint ((float)x, (float)y);
		}

		public override void MoveTo (object backend, double x, double y)
		{
			((CGContextBackend)backend).Context.MoveTo ((float)x, (float)y);
		}

		public override void NewPath (object backend)
		{
			((CGContextBackend)backend).Context.BeginPath ();
		}

		public override void Rectangle (object backend, double x, double y, double width, double height)
		{
			((CGContextBackend)backend).Context.AddRect (new RectangleF ((float)x, (float)y, (float)width, (float)height));
		}

		public override void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			CGPoint p = ctx.GetPathCurrentPoint ();
			ctx.AddCurveToPoint ((float)(p.X + dx1), (float)(p.Y + dy1), (float)(p.X + dx2), (float)(p.Y + dy2), (float)(p.X + dx3), (float)(p.Y + dy3));
		}

		public override void RelLineTo (object backend, double dx, double dy)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			CGPoint p = ctx.GetPathCurrentPoint ();
			ctx.AddLineToPoint ((float)(p.X + dx), (float)(p.Y + dy));
		}

		public override void RelMoveTo (object backend, double dx, double dy)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			CGPoint p = ctx.GetPathCurrentPoint ();
			ctx.MoveTo ((float)(p.X + dx), (float)(p.Y + dy));
		}

		public override void Stroke (object backend)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			SetupContextForDrawing (ctx);
			ctx.DrawPath (CGPathDrawingMode.Stroke);
		}

		public override void StrokePreserve (object backend)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			SetupContextForDrawing (ctx);
			using (CGPath oldPath = ctx.CopyPath ()) {
				ctx.DrawPath (CGPathDrawingMode.Stroke);
				ctx.AddPath (oldPath);
			}
		}
		
		public override void SetColor (object backend, Xwt.Drawing.Color color)
		{
			CGContextBackend gc = (CGContextBackend)backend;
			gc.CurrentStatus.Pattern = null;
			CGContext ctx = gc.Context;
			ctx.SetFillColorSpace (Util.DeviceRGBColorSpace);
			ctx.SetStrokeColorSpace (Util.DeviceRGBColorSpace);
			ctx.SetFillColor ((float)color.Red, (float)color.Green, (float)color.Blue, (float)color.Alpha);
			ctx.SetStrokeColor ((float)color.Red, (float)color.Green, (float)color.Blue, (float)color.Alpha);
		}
		
		public override void SetLineWidth (object backend, double width)
		{
			((CGContextBackend)backend).Context.SetLineWidth ((float)width);
		}
		
		public override void SetLineDash (object backend, double offset, params double[] pattern)
		{
			var array = new nfloat[pattern.Length];
			for (int n=0; n<pattern.Length; n++)
				array [n] = (float) pattern[n];
			((CGContextBackend)backend).Context.SetLineDash ((nfloat)offset, array);
		}
		
		public override void SetPattern (object backend, object p)
		{
			CGContextBackend gc = (CGContextBackend)backend;
			gc.CurrentStatus.Pattern = p;
		}

		void SetupPattern (CGContextBackend gc)
		{
			gc.Context.SetPatternPhase (new SizeF (0, 0));

			if (gc.CurrentStatus.Pattern is GradientInfo)
				return;

			if (gc.CurrentStatus.Pattern is ImagePatternInfo) {

				var pi = (ImagePatternInfo) gc.CurrentStatus.Pattern;
				var bounds = new CGRect (CGPoint.Empty, new CGSize (pi.Image.Size.Width, pi.Image.Size.Height));
				var t = CGAffineTransform.Multiply (CGAffineTransform.MakeScale (1f, -1f), gc.Context.GetCTM ());

				CGPattern pattern;
				if (pi.Image is CustomImage) {
					pattern = new CGPattern (bounds, t, bounds.Width, bounds.Height, CGPatternTiling.ConstantSpacing, true, c => {
						c.TranslateCTM (0, bounds.Height);
						c.ScaleCTM (1f, -1f);
						((CustomImage)pi.Image).DrawInContext (c);
					});
				} else {
					var empty = CGRect.Empty;
					CGImage cgimg = pi.Image.AsCGImage (ref empty, null, null);
					pattern = new CGPattern (bounds, t, bounds.Width, bounds.Height,
					                         CGPatternTiling.ConstantSpacing, true, c => c.DrawImage (bounds, cgimg));
				}

				CGContext ctx = gc.Context;
				var alpha = new[] { (nfloat)pi.Alpha };
				ctx.SetFillColorSpace (Util.PatternColorSpace);
				ctx.SetStrokeColorSpace (Util.PatternColorSpace);
				ctx.SetFillPattern (pattern, alpha);
				ctx.SetStrokePattern (pattern, alpha);
			}
		}
		
		public override void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			SetupContextForDrawing (ctx);
			var li = ApplicationContext.Toolkit.GetSafeBackend (layout);
			MacTextLayoutBackendHandler.Draw (ctx, li, x, y);
		}

		public override void DrawImage (object backend, ImageDescription img, double x, double y)
		{
			var srcRect = new Rectangle (Point.Zero, img.Size);
			var destRect = new Rectangle (x, y, img.Size.Width, img.Size.Height);
			DrawImage (backend, img, srcRect, destRect);
		}

		public override void DrawImage (object backend, ImageDescription img, Rectangle srcRect, Rectangle destRect)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			NSImage image = img.ToNSImage ();
			ctx.SaveState ();
			ctx.SetAlpha ((float)img.Alpha);

			double rx = destRect.Width / srcRect.Width;
			double ry = destRect.Height / srcRect.Height;
			ctx.AddRect (new RectangleF ((float)destRect.X, (float)destRect.Y, (float)destRect.Width, (float)destRect.Height));
			ctx.Clip ();
			ctx.TranslateCTM ((float)(destRect.X - (srcRect.X * rx)), (float)(destRect.Y - (srcRect.Y * ry)));
			ctx.ScaleCTM ((float)rx, (float)ry);

			if (image is CustomImage) {
				((CustomImage)image).DrawInContext ((CGContextBackend)backend);
			} else {
				var rr = new CGRect (0, 0, image.Size.Width, image.Size.Height);
				ctx.ScaleCTM (1f, -1f);
				ctx.DrawImage (new CGRect (0, -image.Size.Height, image.Size.Width, image.Size.Height), image.AsCGImage (ref rr, NSGraphicsContext.CurrentContext, null));
			}

			ctx.RestoreState ();
		}
		
		public override void Rotate (object backend, double angle)
		{
			((CGContextBackend)backend).Context.RotateCTM ((float)(angle * degrees));
		}
		
		public override void Scale (object backend, double scaleX, double scaleY)
		{
			((CGContextBackend)backend).Context.ScaleCTM ((float)scaleX, (float)scaleY);
		}
		
		public override void Translate (object backend, double tx, double ty)
		{
			((CGContextBackend)backend).Context.TranslateCTM ((float)tx, (float)ty);
		}
		
		public override void ModifyCTM (object backend, Matrix m)
		{
			CGAffineTransform t = new CGAffineTransform ((float)m.M11, (float)m.M12,
			                                             (float)m.M21, (float)m.M22,
			                                             (float)m.OffsetX, (float)m.OffsetY);
			((CGContextBackend)backend).Context.ConcatCTM (t);
		}

		public override Matrix GetCTM (object backend)
		{
			CGAffineTransform t = GetContextTransform ((CGContextBackend)backend);
			Matrix ctm = new Matrix (t.xx, t.yx, t.xy, t.yy, t.x0, t.y0);
			return ctm;
		}

		public override object CreatePath ()
		{
			return new CGPath ();
		}

		public override object CopyPath (object backend)
		{
			return ((CGContextBackend)backend).Context.CopyPath ();
		}

		public override void AppendPath (object backend, object otherBackend)
		{
			CGContext dest = ((CGContextBackend)backend).Context;
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
			return ((CGContextBackend)backend).Context.PathContainsPoint (new PointF ((float)x, (float)y), CGPathDrawingMode.Fill);
		}

		public override bool IsPointInStroke (object backend, double x, double y)
		{
			return ((CGContextBackend)backend).Context.PathContainsPoint (new PointF ((float)x, (float)y), CGPathDrawingMode.Stroke);
		}

		public override void Dispose (object backend)
		{
			((CGContextBackend)backend).Context.Dispose ();
		}

		static CGAffineTransform GetContextTransform (CGContextBackend gc)
		{
			CGAffineTransform t = gc.Context.GetCTM ();

			// The CTM returned above actually includes the full view transform.
			//  We only want the transform that is applied to the context, so concat
			//  the inverse of the view transform to nullify that part.
			if (gc.InverseViewTransform.HasValue)
				t.Multiply (gc.InverseViewTransform.Value);

			return t;
		}

		static void SetupContextForDrawing (CGContext ctx)
		{
			if (ctx.IsPathEmpty ())
				return;

			// setup pattern drawing to better match the behavior of Cairo
			var drawPoint = ctx.GetCTM ().TransformPoint (ctx.GetPathBoundingBox ().Location);
			var patternPhase = new CGSize (drawPoint.X, drawPoint.Y);
			if (patternPhase != SizeF.Empty)
				ctx.SetPatternPhase (patternPhase);
		}
	}
}

