//
// Image9Patch.cs
//
// Author:
//       lluis <>
//
// Copyright (c) 2013 lluis
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
	public class Image9Patch: Canvas
	{
		Image img_ss, img_tt, img_st, img_ts;

		public Image9Patch ()
		{
			img_ss = Image.FromResource ("ninep-ss.9.png");
			img_tt = Image.FromResource ("ninep-tt.9.png");
			img_st = Image.FromResource ("ninep-st.9.png");
			img_ts = Image.FromResource ("ninep-ts.9.png");
			Margin = 30;
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			var w = Math.Truncate (Bounds.Width / 2);
			var h = Math.Truncate (Bounds.Height / 2);
			ctx.DrawImage (img_ss, new Rectangle (0, 0, w, h).Inflate (-10, -10));
			ctx.DrawImage (img_tt, new Rectangle (w, 0, w, h).Inflate (-10, -10));
			ctx.DrawImage (img_st, new Rectangle (0, h, w, h).Inflate (-10, -10));
			ctx.DrawImage (img_ts, new Rectangle (w, h, w, h).Inflate (-10, -10));
		}
	}
}

