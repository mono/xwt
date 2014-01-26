//
// CustomDrawnImage.cs
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
	public class ImageScaling: Canvas
	{
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			Image image = new CustomImage ();
			int x = 0;
			for (int n=4; n < 50; n += 4) {
				ctx.DrawImage (image.WithSize (n, n), x, 0);
				x += n;
			}

			int maxSize = 48;
			var warn = StockIcons.Error;
			x = 0;
			for (int n=8; n <= maxSize; n += 2) {
				ctx.DrawImage (warn, x, 50, n, n);
				x += n;
			}
			
			warn = StockIcons.Error.WithSize (maxSize).ToBitmap ();
			x = 0;
			for (int n=8; n <= maxSize; n += 2) {
				ctx.DrawImage (warn, x, 100, n, n);
				x += n;
			}

			ctx.DrawImage (image.WithSize (1000), new Rectangle (400, 0, 200, 1000), new Rectangle (0, 200, 200, 200));
			ctx.DrawImage (image.WithSize (1000), new Rectangle (400, 0, 200, 50), new Rectangle (210, 200, 200, 200));
		}
	}

	class CustomImage: DrawingImage
	{
		protected override void OnDraw (Context ctx, Rectangle bounds)
		{
			var lineWidth = bounds.Width / 32d;
			var section = ((bounds.Width / 2) - lineWidth / 2) / 3;

			ctx.SetLineWidth (lineWidth);

			ctx.SetColor (Colors.Black);
			ctx.Arc (bounds.Center.X, bounds.Center.Y, 1, 0, 360);
			ctx.Stroke ();
			
			ctx.SetColor (Colors.Red);
			ctx.Arc (bounds.Center.X, bounds.Center.Y, section, 0, 360);
			ctx.Stroke ();

			ctx.SetColor (Colors.Green);
			ctx.Arc (bounds.Center.X, bounds.Center.Y, section * 2, 0, 360);
			ctx.Stroke ();

			ctx.SetColor (Colors.Blue);
			ctx.Arc (bounds.Center.X, bounds.Center.Y, section * 3, 0, 360);
			ctx.Stroke ();
		}
	}
}

