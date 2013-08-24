// 
// PartialImages.cs
//  
// Author:
//       Luís Reis <luiscubal@gmail.com>
// 
// Copyright (c) 2012 Luís Reis
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
	public class PartialImages: VBox
	{
		public PartialImages ()
		{
			PartialImageCanvas canvas = new PartialImageCanvas ();
			PackStart (canvas, true);
		}
	}

	internal class PartialImageCanvas : Canvas
	{
		private Image img;

		public PartialImageCanvas ()
		{
			img = Image.FromResource (GetType (), "cow.jpg");
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			
			for (int y = 0; y < img.Size.Height / 50; ++y) {
				for (int x = 0; x < img.Size.Width / 50; ++x) {
					ctx.DrawImage (img, new Rectangle (x*50, y*50, 50, 50), new Rectangle (x*55, y*55, 50, 50));
				}
			}
		}
	}
}
