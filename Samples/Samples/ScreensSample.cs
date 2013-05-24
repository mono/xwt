//
// ScreensSample.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
	public class ScreensSample: VBox
	{
		public ScreensSample ()
		{
			Label la = new Label (Desktop.Screens.Count + " screens found");
			PackStart (la);

			var c = new ScreensCanvas ();
			c.Margin = 30;
			PackStart (c, true);
		}
	}

	class ScreensCanvas: Canvas
	{
		bool pset;

		public ScreensCanvas ()
		{
			Desktop.ScreensChanged += HandleScreensChanged;
		}

		void HandleScreensChanged (object sender, EventArgs e)
		{
			QueueDraw ();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing)
				Desktop.ScreensChanged -= HandleScreensChanged;
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);

			if (!pset) {
				ParentWindow.BoundsChanged += delegate {
					QueueDraw ();
				};
				pset = true;
			}

			ctx.Rectangle (Bounds);
			ctx.SetColor (Colors.LightGray);
			ctx.Fill ();

			var size = Size;
			size.Width--;
			size.Height--;
			var fx = size.Width / Desktop.Bounds.Width;

			if (Desktop.Bounds.Height * fx > size.Height)
				fx = size.Height / Desktop.Bounds.Height;

			if (Desktop.Bounds.X < 0)
				ctx.Translate (-Desktop.Bounds.X * fx, 0);
			if (Desktop.Bounds.Y < 0)
				ctx.Translate (0, -Desktop.Bounds.Y * fx);

			ctx.SetLineWidth (1);
			foreach (var s in Desktop.Screens) {
				if (s.Bounds != s.VisibleBounds) {
					var vr = new Rectangle ((int)(s.Bounds.X * fx), (int)(s.Bounds.Y * fx), (int)(s.Bounds.Width * fx), (int)(s.Bounds.Height * fx));
					vr = vr.Offset (0.5, 0.5);
					ctx.Rectangle (vr);
					ctx.SetColor (Colors.White);
					ctx.FillPreserve ();
					ctx.SetColor (Colors.Black);
					ctx.Stroke ();
				}
				var r = new Rectangle ((int)(s.VisibleBounds.X * fx), (int)(s.VisibleBounds.Y * fx), (int)(s.VisibleBounds.Width * fx), (int)(s.VisibleBounds.Height * fx));
				r = r.Offset (0.5, 0.5);
				ctx.Rectangle (r);
				ctx.SetColor (new Color (0.4, 0.62, 0.83));
				ctx.FillPreserve ();
				ctx.SetColor (Colors.Black);
				ctx.Stroke ();

				TextLayout tl = new TextLayout (this);
				tl.Text = s.DeviceName;
				tl.Font = Font;
				ctx.DrawTextLayout (tl, r.Center.X - tl.Width / 2, r.Center.Y - tl.Height / 2);
			}

			var wr = ParentWindow.ScreenBounds;
			wr = new Rectangle ((int)(wr.X * fx), (int)(wr.Y * fx), (int)(wr.Width * fx), (int)(wr.Height * fx));
			ctx.Rectangle (wr);
			ctx.SetColor (Colors.Azure.WithAlpha (0.5));
			ctx.FillPreserve ();
			ctx.SetColor (Colors.Azure);
			ctx.Stroke ();
		}
	}
}

