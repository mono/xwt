// 
// DrawingContext.cs
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

namespace Xwt.WPFBackend
{
	internal class DrawingContext
	{
		internal DrawingContext (Graphics graphics)
		{
			if (graphics == null)
				throw new ArgumentNullException ("graphics");

			Graphics = graphics;
		}

		internal DrawingContext (DrawingContext context)
		{
			Graphics = context.Graphics;
			
			var f = context.Font;
			Font = new Font (f.FontFamily, f.Size, f.Style, f.Unit, f.GdiCharSet, f.GdiVerticalFont);
			Pen = new Pen (context.Pen.Brush, context.Pen.Width);
			Brush = (Brush)context.Brush.Clone ();
			Path = (GraphicsPath) context.Path.Clone();
			
			CurrentX = context.CurrentX;
			CurrentY = context.CurrentY;
		}

		internal readonly Graphics Graphics;

		internal Font Font = new Font (FontFamily.GenericSansSerif, 12);
		internal Pen Pen = new Pen (Color.Black, 1);
		internal Brush Brush = new SolidBrush (Color.Black);

		internal float CurrentX;
		internal float CurrentY;

		internal GraphicsPath Path = new GraphicsPath();

		internal void SetColor (Color color)
		{
			Pen.Color = color;
			Brush = new SolidBrush (color);
		}

		internal void Save()
		{
			if (this.contexts == null)
				this.contexts = new Stack<DrawingContext> ();

			this.contexts.Push (new DrawingContext (this));
		}

		internal void Restore()
		{
			if (this.contexts == null || this.contexts.Count == 0)
				throw new InvalidOperationException();

			var dc = this.contexts.Pop ();

			Font = dc.Font;
			Pen = dc.Pen;
			Brush = dc.Brush;
			Path = dc.Path;

			CurrentX = dc.CurrentX;
			CurrentY = dc.CurrentY;
		}

		private Stack<DrawingContext> contexts;
	}
}