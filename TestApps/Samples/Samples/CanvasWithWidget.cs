// 
// CanvasWithWidget.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//		 Hywel Thomas <hywel.w.thomas@gmail.com>
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class CanvasWithWidget_Linear: VBox
	{
		public CanvasWithWidget_Linear ()
		{
			MyCanvas c = new MyCanvas (true);
			PackStart (c, true);
		}
	}

	public class CanvasWithWidget_Radial : VBox
	{
		public CanvasWithWidget_Radial ()
		{
			MyCanvas c = new MyCanvas (false);
			PackStart (c, true);
		}
	}
	
	class MyCanvas: Canvas
	{
		Color stop1, stop2;
		double xStart, xEnd;

		Rectangle rect = new Rectangle (30, 30, 100, 30);

		bool Linear {
			get; set;
		}

		public MyCanvas (bool linear)
		{
			var entry = new TextEntry () { ShowFrame = false };
			AddChild (entry, rect);

			var box = new HBox ();
			box.PackStart (new Button ("..."));
			box.PackStart (new TextEntry (), true);
			AddChild (box, new Rectangle (30, 70, box.Surface.GetPreferredSize().Width, 30));
			Linear = linear;

			stop1 = new Color (0, 1, 0);
			stop2 = new Color (1, 0, 0);
			xStart = xEnd = 0;
		}

		protected override void OnMouseEntered (EventArgs args)
		{
			stop1 = new Color (1, 0, 0);	//Flip gradient
			stop2 = new Color (0, 1, 0);
			QueueDraw ();
		}

		protected override void OnMouseExited (EventArgs args)
		{
			stop1 = new Color (0, 1, 0);	//Flip back
			stop2 = new Color (1, 0, 0);
			QueueDraw ();
		}

		protected override void OnMouseMoved (MouseMovedEventArgs args)
		{
			xStart = args.X;
			xEnd = Bounds.Width - xStart;	// Change gradient endpoints
			QueueDraw ();
		}

		protected override void OnDraw (Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{
			if (Bounds.IsEmpty)
				return;

			ctx.Rectangle (0, 0, Bounds.Width, Bounds.Height);
			Gradient g = null;
			if (Linear)
				g = new LinearGradient (xStart, 0, xEnd, Bounds.Height);
			else
				g = new RadialGradient (Bounds.Width / 2, Bounds.Height / 2, Bounds.Width / 2, Bounds.Width / 2, Bounds.Height / 2, xStart/4 + Bounds.Width / 8);

			g.AddColorStop (0, stop1);
			g.AddColorStop (1, stop2);
			ctx.Pattern = g;
			ctx.Fill ();
			
			Rectangle r = rect.Inflate (5, 5);
			ctx.Rectangle (r);
			ctx.SetColor (new Color (0,0,1));
			ctx.SetLineWidth (1);
			ctx.Stroke ();
		}
	}
}

