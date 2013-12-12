//
// DrawingSurface.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin, Inc (http://www.xamarin.com)
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
	public class DrawingSurface: VBox
	{
		public DrawingSurface ()
		{
			Button run = new Button ("Run Test");
			PackStart (run);

			Label results = new Label ();
			PackStart (results);

			var st = new SurfaceTest ();
			PackStart (st);

			run.Clicked += delegate {
				run.Sensitive = false;
				st.StartTest ();
			};

			st.TestFinished += delegate {
				run.Sensitive = true;
				results.Text = string.Format ("Draw: {0} FPS\nBitmap: {1} FPS\nVector image: {2} FPS\nSurface: {3} FPS", st.DrawFPS, st.BitmapFPS, st.ImageFPS, st.SurfaceFPS);
				Console.WriteLine (results.Text);
			};
		}
	}

	class SurfaceTest: Canvas
	{
		bool testMode = false;

		Image vectorImage;
		Image bitmap;
		Surface surface;
		int testTime = 1000;
		double size = 500;
		double iterations = 20;
		Image cow;

		public int DrawFPS { get; private set; }
		public int BitmapFPS { get; private set; }
		public int ImageFPS { get; private set; }
		public int SurfaceFPS { get; private set; }

		public event EventHandler TestFinished;

		public SurfaceTest ()
		{
			cow = Image.FromResource ("cow.jpg").WithBoxSize (size - 50);
			var ib = new ImageBuilder (size, size);
			DrawScene (ib.Context);
			bitmap = ib.ToBitmap ();
			vectorImage = ib.ToVectorImage ();
			WidthRequest = size;
			HeightRequest = size;
		}

		public void StartTest ()
		{
			testMode = true;
			QueueDraw ();
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			if (surface == null) {
				surface = new Surface (size, size, this);
				DrawScene (surface.Context);
			}
			ctx.DrawSurface (surface, 0, 0);
			if (!testMode)
				return;

			DrawFPS = TimedDraw (delegate {
				DrawScene (ctx);
			});

			BitmapFPS = TimedDraw (delegate {
				ctx.DrawImage (bitmap, 0, 0);
			});

			ImageFPS = TimedDraw (delegate {
				ctx.DrawImage (vectorImage, 0, 0);
			});

			SurfaceFPS = TimedDraw (delegate {
				ctx.DrawSurface (surface, 0, 0);
			});

			testMode = false;
			if (TestFinished != null)
				TestFinished (this, EventArgs.Empty);
		}

		int TimedDraw (Action draw)
		{
			var t = DateTime.Now;
			var n = 0;
			while ((DateTime.Now - t).TotalMilliseconds < testTime) {
				draw ();
				n++;
			}
			return n;
		}

		void DrawScene (Context ctx)
		{
			ctx.SetLineWidth (1);
			ctx.SetColor (Colors.Black);
			for (int n = 1; n < iterations; n += 3) {
				ctx.Rectangle (0, 0, (size / iterations) * n, (size / iterations) * n);
				ctx.Stroke ();
				ctx.Arc (size/2, size/2, ((size / iterations) * n) / 2, 0, 360);
				ctx.Stroke ();
			}
			//ctx.DrawImage (cow, 20, 20);
		}
	}
}

