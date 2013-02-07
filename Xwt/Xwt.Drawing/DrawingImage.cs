//
// DrawingImage.cs
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

namespace Xwt.Drawing
{
	public class DrawingImage: Image
	{
		public DrawingImage ()
		{
		}

		protected override BitmapImage GenerateBitmap (Size size, double alpha)
		{
			using (ImageBuilder ib = new ImageBuilder ((int)size.Width, (int)size.Height)) {
				ib.Context.GlobalAlpha = alpha;
				OnDraw (ib.Context, new Rectangle (0, 0, size.Width, size.Height));
				return ib.ToImage ().ToBitmap ();
			}
		}

		internal override bool CanDrawInContext (double width, double height)
		{
			return true;
		}

		internal override void DrawInContext (Context ctx, double x, double y, double width, double height)
		{
			var oldAlpha = ctx.GlobalAlpha;
			ctx.GlobalAlpha *= requestedAlpha;
			OnDraw (ctx, new Rectangle (x, y, width, height));
			ctx.GlobalAlpha = oldAlpha;
		}

		protected virtual void OnDraw (Context ctx, Rectangle bounds)
		{
		}
	}
}

