// 
// ContextBackendHandler.cs
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
using Xwt.Backends;
using Xwt.Engine;
using MonoMac.AppKit;
using Xwt.Drawing;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
using System.Drawing;

namespace Xwt.Mac
{
	class CGContextBackend {
		public CGPath ClipPath;
		public CGContext Context;
		public SizeF Size;
		public GradientInfo Gradient;
	}

	public class ContextBackendHandler: IContextBackendHandler
	{
		const double degrees = System.Math.PI / 180d;

		public ContextBackendHandler ()
		{
		}

		public void Save (object backend)
		{
			((CGContextBackend)backend).Context.SaveState ();
		}
		
		public void Restore (object backend)
		{
			((CGContextBackend)backend).Context.RestoreState ();
		}

		public void SetGlobalAlpha (object backend, double alpha)
		{
			((CGContextBackend)backend).Context.SetAlpha ((float)alpha);
		}

		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			ctx.AddArc ((float)xc, (float)yc, (float)radius, (float)(angle1 * degrees), (float)(angle2 * degrees), false);
		}

		public void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			ctx.AddArc ((float)xc, (float)yc, (float)radius, (float)(angle1 * degrees), (float)(angle2 * degrees), true);
		}

		public void Clip (object backend)
		{
			ClipPreserve (backend);
			((CGContextBackend)backend).Context.BeginPath ();
		}

		public void ClipPreserve (object backend)
		{
			CGContextBackend gc = (CGContextBackend)backend;
			if (gc.ClipPath == null)
				gc.ClipPath = gc.Context.CopyPath ();
			//else
				//FIXME: figure out how to intersect existing ClipPath with the current path
		}

		public void ResetClip (object backend)
		{
			((CGContextBackend)backend).ClipPath = null;
		}
		
		public void ClosePath (object backend)
		{
			((CGContextBackend)backend).Context.ClosePath ();
		}

		public void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			((CGContextBackend)backend).Context.AddCurveToPoint ((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3);
		}

		public void Fill (object backend)
		{
			bool needsRestore;
			CGContextBackend gc = (CGContextBackend)backend;
			CGContext ctx = SetupContextForDrawing (gc, out needsRestore);
			if (gc.Gradient != null)
				GradientBackendHandler.Draw (ctx, gc.Gradient);
			else
				ctx.DrawPath (CGPathDrawingMode.Fill);
			if (needsRestore)
				ctx.RestoreState ();
		}

		public void FillPreserve (object backend)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			using (CGPath oldPath = ctx.CopyPath ()) {
				Fill (backend);
				ctx.AddPath (oldPath);
			}
		}

		public void LineTo (object backend, double x, double y)
		{
			((CGContextBackend)backend).Context.AddLineToPoint ((float)x, (float)y);
		}

		public void MoveTo (object backend, double x, double y)
		{
			((CGContextBackend)backend).Context.MoveTo ((float)x, (float)y);
		}

		public void NewPath (object backend)
		{
			((CGContextBackend)backend).Context.BeginPath ();
		}

		public void Rectangle (object backend, double x, double y, double width, double height)
		{
			((CGContextBackend)backend).Context.AddRect (new RectangleF ((float)x, (float)y, (float)width, (float)height));
		}

		public void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			PointF p = ctx.GetPathCurrentPoint ();
			ctx.AddCurveToPoint ((float)(p.X + dx1), (float)(p.Y + dy1), (float)(p.X + dx2), (float)(p.Y + dy2), (float)(p.X + dx3), (float)(p.Y + dy3));
		}

		public void RelLineTo (object backend, double dx, double dy)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			PointF p = ctx.GetPathCurrentPoint ();
			ctx.AddLineToPoint ((float)(p.X + dx), (float)(p.Y + dy));
		}

		public void RelMoveTo (object backend, double dx, double dy)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			PointF p = ctx.GetPathCurrentPoint ();
			ctx.MoveTo ((float)(p.X + dx), (float)(p.Y + dy));
		}

		public void Stroke (object backend)
		{
			bool needsRestore;
			CGContext ctx = SetupContextForDrawing ((CGContextBackend)backend, out needsRestore);
			ctx.DrawPath (CGPathDrawingMode.Stroke);
			if (needsRestore)
				ctx.RestoreState ();
		}

		public void StrokePreserve (object backend)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			using (CGPath oldPath = ctx.CopyPath ()) {
				Stroke (backend);
				ctx.AddPath (oldPath);
			}
		}
		
		public void SetColor (object backend, Xwt.Drawing.Color color)
		{
			CGContextBackend gc = (CGContextBackend)backend;
			gc.Gradient = null;
			CGContext ctx = gc.Context;
			ctx.SetFillColorSpace (Util.DeviceRGBColorSpace);
			ctx.SetStrokeColorSpace (Util.DeviceRGBColorSpace);
			ctx.SetFillColor ((float)color.Red, (float)color.Green, (float)color.Blue, (float)color.Alpha);
			ctx.SetStrokeColor ((float)color.Red, (float)color.Green, (float)color.Blue, (float)color.Alpha);
		}
		
		public void SetLineWidth (object backend, double width)
		{
			((CGContextBackend)backend).Context.SetLineWidth ((float)width);
		}
		
		public void SetLineDash (object backend, double offset, params double[] pattern)
		{
			float[] array = new float[pattern.Length];
			for (int n=0; n<pattern.Length; n++)
				array [n] = (float) pattern[n];
			if (array.Length == 0)
				array = new float [] { 1 };
			((CGContextBackend)backend).Context.SetLineDash ((float)offset, array);
		}
		
		public void SetPattern (object backend, object p)
		{
			CGContextBackend gc = (CGContextBackend)backend;
			gc.Gradient = p as GradientInfo;
			if (gc.Gradient != null || !(p is CGPattern))
				return;
			CGContext ctx = gc.Context;
			CGPattern pattern = (CGPattern)p;
			float[] alpha = new[] { 1.0f };
			ctx.SetFillColorSpace (Util.PatternColorSpace);
			ctx.SetStrokeColorSpace (Util.PatternColorSpace);
			ctx.SetFillPattern (pattern, alpha);
			ctx.SetStrokePattern (pattern, alpha);
		}
		
		public void SetFont (object backend, Xwt.Drawing.Font font)
		{
			((CGContextBackend)backend).Context.SelectFont (font.Family, (float)font.Size, CGTextEncoding.FontSpecific);
		}
		
		public void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			bool needsRestore;
			CGContext ctx = SetupContextForDrawing ((CGContextBackend)backend, out needsRestore);
			TextLayoutBackendHandler.Draw (ctx, WidgetRegistry.GetBackend (layout), x, y);
			if (needsRestore)
				ctx.RestoreState ();
		}
		
		public void DrawImage (object backend, object img, double x, double y, double alpha)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			NSImage image = (NSImage)img;
			var rect = new RectangleF (new PointF ((float)x, (float)y), image.Size);
			ctx.SaveState ();
			ctx.SetAlpha ((float)alpha);
			ctx.DrawImage (rect, image.AsCGImage (RectangleF.Empty, null, null));
			ctx.RestoreState ();
		}
		
		public void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha)
		{
			var srcRect = new Rectangle (Point.Zero, ((NSImage)img).Size.ToXwtSize ());
			var destRect = new Rectangle (x, y, width, height);
			DrawImage (backend, img, srcRect, destRect, alpha);
		}

		public void DrawImage (object backend, object img, Rectangle srcRect, Rectangle destRect, double alpha)
		{
			CGContext ctx = ((CGContextBackend)backend).Context;
			NSImage image = (NSImage) img;
			ctx.SaveState ();
			ctx.SetAlpha ((float)alpha);
			ctx.DrawImage (destRect.ToRectangleF (), image.AsCGImage (RectangleF.Empty, null, null).WithImageInRect (srcRect.ToRectangleF ()));
			ctx.RestoreState ();
		}
		
		public void Rotate (object backend, double angle)
		{
			((CGContextBackend)backend).Context.RotateCTM ((float)(angle * degrees));
		}
		
		public void Scale (object backend, double scaleX, double scaleY)
		{
			((CGContextBackend)backend).Context.ScaleCTM ((float)scaleX, (float)scaleY);
		}
		
		public void Translate (object backend, double tx, double ty)
		{
			((CGContextBackend)backend).Context.TranslateCTM ((float)tx, (float)ty);
		}
		
		public void TransformPoint (object backend, ref double x, ref double y)
		{
			CGAffineTransform t = ((CGContextBackend)backend).Context.GetCTM();

			PointF p = t.TransformPoint (new PointF ((float)x, (float)y));
			x = p.X;
			y = p.Y;
		}

		public void TransformDistance (object backend, ref double dx, ref double dy)
		{
			CGAffineTransform t = ((CGContextBackend)backend).Context.GetCTM();
			// remove translational elements from CTM
			t.x0 = 0;
			t.y0 = 0;

			PointF p = t.TransformPoint (new PointF ((float)dx, (float)dy));
			dx = p.X;
			dy = p.Y;
		}

		public void TransformPoints (object backend, Point[] points)
		{
			CGAffineTransform t = ((CGContextBackend)backend).Context.GetCTM();

			PointF p;
			for (int i = 0; i < points.Length; ++i) {
				p = t.TransformPoint (new PointF ((float)points[i].X, (float)points[i].Y));
				points[i].X = p.X;
				points[i].Y = p.Y;
			}
		}

		public void TransformDistances (object backend, Distance[] vectors)
		{
			CGAffineTransform t = ((CGContextBackend)backend).Context.GetCTM();
			t.x0 = 0;
			t.y0 = 0;
			PointF p;
			for (int i = 0; i < vectors.Length; ++i) {
				p = t.TransformPoint (new PointF ((float)vectors[i].Dx, (float)vectors[i].Dy));
				vectors[i].Dx = p.X;
				vectors[i].Dy = p.Y;
			}
		}

		public void Dispose (object backend)
		{
			((CGContextBackend)backend).Context.Dispose ();
		}

		static CGContext SetupContextForDrawing (CGContextBackend gc, out bool needsRestore)
		{
			CGContext ctx = gc.Context;
			if (!ctx.IsPathEmpty ()) {
				var drawPoint = ctx.GetCTM ().TransformPoint (ctx.GetPathBoundingBox ().Location);
				var patternPhase = new SizeF (drawPoint.X, drawPoint.Y);
				if (patternPhase != SizeF.Empty)
					ctx.SetPatternPhase (patternPhase);
			}
			if (gc.ClipPath == null) {
				needsRestore = false;
				return ctx;
			}
			ctx.SaveState ();
			using (CGPath oldPath = ctx.CopyPath ()) {
				ctx.BeginPath ();
				ctx.AddPath (gc.ClipPath);
				ctx.Clip ();
				ctx.AddPath (oldPath);
			}
			needsRestore = true;
			return ctx;
		}
	}
}

