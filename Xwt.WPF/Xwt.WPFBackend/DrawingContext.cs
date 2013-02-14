// 
// DrawingContext.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
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

#if !USE_WPF_RENDERING

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Xwt.WPFBackend
{
	internal class DrawingContext:IDisposable
	{
		internal DrawingContext (Graphics graphics)
		{
			if (graphics == null)
				throw new ArgumentNullException ("graphics");
			
			graphics.SmoothingMode = SmoothingMode.None;
			graphics.PixelOffsetMode = PixelOffsetMode.Half;
			graphics.CompositingQuality = CompositingQuality.HighSpeed;

			// necessary for correct text rendering with System.Drawing
			//graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
			// necessary for none-pixelated text drawing in images, revert to above line if it introduces a performance problem
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

			Graphics = graphics;
		}

		internal DrawingContext (DrawingContext context)
		{
			
			context.CopyTo (this, false);
			
			CurrentX = context.CurrentX;
			CurrentY = context.CurrentY;
		}

		internal DrawingContext()
		{
		}

		internal readonly Graphics Graphics;
		Font font = null;

		internal Font Font {
			get { return font ?? (font = new Font (FontFamily.GenericSansSerif, 12));}
			set { font = value;}
		}

		Color color = Color.Black;
		float width = 1;
		Pen pen = null;

		internal Pen Pen {
			get { return pen ?? (pen = new Pen (color, width));}
			set { pen = value;}
		}

		Brush brush = null;

		internal Brush Brush {
			get { return brush ?? (brush = new SolidBrush (color));}
			set { brush = value;}
		}

		internal GraphicsState State;
		internal float CurrentX;
		internal float CurrentY;
		GraphicsPath path = null;

		internal GraphicsPath Path {
			get { return path ?? (path = new GraphicsPath ());}
			set { path = value;}
		}

		internal void SetColor (Color color)
		{
			if (this.color == color)
				return;
			
			if (pen != null)
				pen.Color = color;

			if (brush != null) {
				SolidBrush solidBrush = brush as SolidBrush;
				if (solidBrush == null) {
					brush.Dispose ();
					brush = null;
				} else
					solidBrush.Color = color;
			}

			this.color = color;
		}
		
		internal void SetWidth (float width)
		{
			if (this.width != width) {
				if (pen != null) {
					pen.Width = width;
				}
			}
			this.width = width;
		}
		
		internal void CopyTo (DrawingContext dc, bool toCurrent)
		{
			if (toCurrent) 
				dc.Graphics.Restore (this.State);
			else 
				dc.State = this.Graphics.Save ();
			dc.Font = this.font;
			dc.Brush = this.brush;
			dc.Pen = this.pen;
			dc.SetWidth (this.width);
			dc.SetColor (this.color);
			dc.CurrentX = this.CurrentX;
			dc.CurrentY = this.CurrentY;
			if (this.path != null && this.path.PointCount > 0)
				dc.Path = (GraphicsPath) this.path.Clone ();
		}
		
		internal void Save ()
		{
			if (this.contexts == null)
				this.contexts = new Stack<DrawingContext> ();

			this.contexts.Push (new DrawingContext (this));
		}
		
		internal void Restore ()
		{
			if (this.contexts == null || this.contexts.Count == 0)
				throw new InvalidOperationException ();

			var dc = this.contexts.Pop ();

			dc.CopyTo (this, true);
			dc.Dispose (true);

		}

		private Stack<DrawingContext> contexts;

		public void Dispose (bool stacked)
		{
			if (!stacked) {
				if (font != null)
					font.Dispose ();
				if (brush != null)
					brush.Dispose ();
				if (pen != null)
					pen.Dispose ();
			}
			
			if (path != null)
				path.Dispose ();
			
			if (contexts != null)
				while (contexts.Count!=0) {
					var c = contexts.Pop ();
					c.Dispose (true);
				}
			font = null;
			brush = null;
			pen = null;
			path = null;
		}
		
		public void Dispose ()
		{
			Dispose (false);
		}
	}
}

#endif