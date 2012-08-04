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
	public class ContextBackendHandler: IContextBackendHandler
	{
		public ContextBackendHandler ()
		{
		}
		
		ContextInfo GetContext (object backend)
		{
			var ctx = (ContextInfo) backend;
			ctx.SetFocus ();
			return ctx;
		}

		public void Save (object backend)
		{
			GetContext (backend);
			NSGraphicsContext.CurrentContext.SaveGraphicsState ();
		}
		
		public void Restore (object backend)
		{
			GetContext (backend);
			NSGraphicsContext.CurrentContext.RestoreGraphicsState ();
		}

		public void SetGlobalAlpha (object backend, double alpha)
		{
			// TODO
		}

		public void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2)
		{
			var ctx = GetContext (backend);
			ctx.Path.AppendPathWithArc (new System.Drawing.PointF ((float)xc, (float)yc), (float)radius, (float)angle1, (float)angle2);
		}

		public void Clip (object backend)
		{
			var ctx = GetContext (backend);
			ctx.Path.AddClip ();
			ctx.Path.Dispose ();
			ctx.Path = new NSBezierPath ();
		}

		public void ClipPreserve (object backend)
		{
			var ctx = GetContext (backend);
			ctx.Path.AddClip ();
		}

		public void ResetClip (object backend)
		{
			GetContext (backend);
			var path = new NSBezierPath ();
			path.AppendPathWithRect (new System.Drawing.RectangleF (0, 0, float.MaxValue, float.MaxValue));
			path.SetClip ();
		}
		
		public void ClosePath (object backend)
		{
			var ctx = GetContext (backend);
			ctx.Path.ClosePath ();
		}

		public void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			var ctx = GetContext (backend);
			ctx.Path.CurveTo (new System.Drawing.PointF ((float)x1, (float)y1),
				new System.Drawing.PointF ((float)x2, (float)y2),
				new System.Drawing.PointF ((float)x3, (float)y3));
		}

		public void Fill (object backend)
		{
			var ctx = GetContext (backend);
			if (ctx.Pattern is GradientInfo) {
				GradientInfo gr = (GradientInfo) ctx.Pattern;
				NSGradient g = new NSGradient (gr.Colors.ToArray (), gr.Stops.ToArray ());
				g.DrawInBezierPath (ctx.Path, 0f);
			}
			else if (ctx.Pattern is NSColor) {
				NSColor col = (NSColor) ctx.Pattern;
				col.Set ();
				col.SetFill ();
			}
			else {
				ctx.Path.Fill ();
			}
			ctx.Pattern = null;
			ctx.Path.Dispose ();
			ctx.Path = new NSBezierPath ();
		}

		public void FillPreserve (object backend)
		{
			var ctx = GetContext (backend);
			NSGraphicsContext.CurrentContext.SaveGraphicsState ();
			ctx.Path.Fill ();
			NSGraphicsContext.CurrentContext.RestoreGraphicsState ();
		}

		public void LineTo (object backend, double x, double y)
		{
			var ctx = GetContext (backend);
			ctx.Path.LineTo (new System.Drawing.PointF ((float)x, (float)y));
		}

		public void MoveTo (object backend, double x, double y)
		{
			var ctx = GetContext (backend);
			ctx.Path.MoveTo (new System.Drawing.PointF ((float)x, (float)y));
		}

		public void NewPath (object backend)
		{
			var ctx = GetContext (backend);
			ctx.Path = new NSBezierPath ();
		}

		public void Rectangle (object backend, double x, double y, double width, double height)
		{
			var ctx = GetContext (backend);
			ctx.Path.AppendPathWithRect (new System.Drawing.RectangleF ((float)x, (float)y, (float)width, (float)height));
		}

		public void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
		{
			var ctx = GetContext (backend);
			ctx.Path.RelativeCurveTo (new System.Drawing.PointF ((float)dx1, (float)dy1),
				new System.Drawing.PointF ((float)dx2, (float)dy2),
				new System.Drawing.PointF ((float)dx3, (float)dy3));
		}

		public void RelLineTo (object backend, double dx, double dy)
		{
			var ctx = GetContext (backend);
			ctx.Path.RelativeLineTo (new System.Drawing.PointF ((float)dx, (float)dy));
		}

		public void RelMoveTo (object backend, double dx, double dy)
		{
			var ctx = GetContext (backend);
			ctx.Path.RelativeMoveTo (new System.Drawing.PointF ((float)dx, (float)dy));
		}

		public void Stroke (object backend)
		{
			var ctx = GetContext (backend);
			ctx.Path.Stroke ();
			ctx.Path.Dispose ();
			ctx.Path = new NSBezierPath ();
		}

		public void StrokePreserve (object backend)
		{
			var ctx = GetContext (backend);
			ctx.Path.Stroke ();
		}
		
		public void SetColor (object backend, Xwt.Drawing.Color color)
		{
			GetContext (backend);
			NSColor col = NSColor.FromDeviceRgba ((float)color.Red, (float)color.Green, (float)color.Blue, (float)color.Alpha);
			col.Set ();
			col.SetFill ();
		}
		
		public void SetLineWidth (object backend, double width)
		{
			var ctx = GetContext (backend);
			ctx.Path.LineWidth = (float) width;
		}
		
		public void SetLineDash (object backend, double offset, params double[] pattern)
		{
			var ctx = GetContext (backend);
			float[] array = new float[pattern.Length];
			for (int n=0; n<pattern.Length; n++)
				array [n] = (float) pattern[n];
			if (array.Length == 0)
				array = new float [] { 0 };
			ctx.Path.SetLineDash (array, (float)offset);
		}
		
		public void SetPattern (object backend, object p)
		{
			var ctx = GetContext (backend);
			ctx.Pattern = p;
		}
		
		public void SetFont (object backend, Xwt.Drawing.Font font)
		{
		}
		
		public void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			GetContext (backend);
			TextLayoutBackendHandler.Draw (null, WidgetRegistry.GetBackend (layout), x, y);
		}
		
		public void DrawImage (object backend, object img, double x, double y, double alpha)
		{
			GetContext (backend);
			var image = (NSImage) img;
			image.Draw (new PointF ((float)x, (float)y), RectangleF.Empty, NSCompositingOperation.SourceOver, (float)alpha);
		}
		
		public void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha)
		{
			GetContext (backend);
			var image = (NSImage) img;
			image.DrawInRect (new RectangleF ((float)x, (float)y, (float)width, (float)height), RectangleF.Empty, NSCompositingOperation.SourceOver, (float)alpha);
		}

		public void DrawImage (object backend, object img, Rectangle srcRect, Rectangle destRect, double alpha)
		{
			// TODO
		}
		
		public void ResetTransform (object backend)
		{
			GetContext (backend);
			NSAffineTransform t = new NSAffineTransform ();
			t.Set ();
		}
		
		public void Rotate (object backend, double angle)
		{
			GetContext (backend);
			NSAffineTransform t = new NSAffineTransform ();
			t.RotateByDegrees ((float)angle);
			t.Concat ();
		}
		
		public void Scale (object backend, double scaleX, double scaleY)
		{
			GetContext (backend);
			NSAffineTransform t = new NSAffineTransform ();
			t.Scale ((float)scaleX, (float)scaleY);
			t.Concat ();
		}
		
		public void Translate (object backend, double tx, double ty)
		{
			GetContext (backend);
			NSAffineTransform t = new NSAffineTransform ();
			t.Translate ((float)tx, (float)ty);
			t.Concat ();
		}
		
		public void TransformPoint (object backend, ref double x, ref double y)
		{
			GetContext (backend);
			CGContext gp = NSGraphicsContext.CurrentContext.GraphicsPort;
			CGAffineTransform t = gp.GetCTM();

			PointF p = t.TransformPoint (new PointF ((float)x, (float)y));
			x = p.X;
			y = p.Y;
		}

		public void TransformDistance (object backend, ref double dx, ref double dy)
		{
			GetContext (backend);
			CGContext gp = NSGraphicsContext.CurrentContext.GraphicsPort;
			CGAffineTransform t = gp.GetCTM();
			// remove translational elements from CTM
			t.x0 = 0;
			t.y0 = 0;

			PointF p = t.TransformPoint (new PointF ((float)dx, (float)dy));
			dx = p.X;
			dy = p.Y;
		}

		public void TransformPoints (object backend, Point[] points)
		{
			GetContext (backend);
			CGContext gp = NSGraphicsContext.CurrentContext.GraphicsPort;
			CGAffineTransform t = gp.GetCTM();

			PointF p;
			for (int i = 0; i < points.Length; ++i) {
				p = t.TransformPoint (new PointF ((float)points[i].X, (float)points[i].Y));
				points[i].X = p.X;
				points[i].Y = p.Y;
			}
		}

		public void TransformDistances (object backend, Distance[] vectors)
		{
			GetContext (backend);
			CGContext gp = NSGraphicsContext.CurrentContext.GraphicsPort;
			CGAffineTransform t = gp.GetCTM();
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
			ContextInfo ctx = (ContextInfo) backend;
			ctx.Dispose ();
		}
	}
	
	public class ContextInfo
	{
		static ContextInfo CurrentFocus;
		
		public NSBezierPath Path = new NSBezierPath ();
		public object Pattern;
		public NSImage TargetImage;
		
		public ContextInfo ()
		{
		}
		
		public ContextInfo (NSImage targetImage)
		{
			this.TargetImage = targetImage;
		}
		
		public void SetFocus ()
		{
			if (CurrentFocus != this) {
				if (CurrentFocus != null)
					CurrentFocus.UnlockFocus ();
				CurrentFocus = this;
				LockFocus ();
			}
		}
		
		public void Dispose ()
		{
			Path.Dispose ();
			if (CurrentFocus == this)
				UnlockFocus ();
		}
		
		public virtual void LockFocus ()
		{
			if (TargetImage != null)
				TargetImage.LockFocus ();
		}
		
		public virtual void UnlockFocus ()
		{
			if (TargetImage != null)
				TargetImage.UnlockFocus ();
		}
	}
		
}

