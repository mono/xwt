//
// ComposedImage.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2019 Microsoft
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
using System.Linq;

namespace Xwt.Drawing
{
	class ComposedImage : DrawingImage
	{
		readonly Image [] images;

		public ComposedImage (IEnumerable<Image> images, Size size = default (Size))
		{
			if (images == null)
				throw new ArgumentNullException (nameof (images));
			this.images = images.ToArray ();
			if (this.images.Length == 0)
				throw new ArgumentException ("The enumeration does not contain any images", nameof (images));
			Size = size;
		}

		protected override void OnDraw (Context ctx, Rectangle bounds)
		{
			var size = !Size.IsZero ? Size : images [0].Size;

			ctx.Save ();
			ctx.Translate (bounds.Location);
			if (!size.IsZero) {
				ctx.Scale (bounds.Width / size.Width, bounds.Height / size.Height);
			}

			for (int n = 0; n < images.Length; n++) {
				var image = images [n];
				if (image.Size != size)
					image = image.WithSize (size);
				ctx.DrawImage (image, 0, 0);
			}

			ctx.Restore ();
		}
	}
}

