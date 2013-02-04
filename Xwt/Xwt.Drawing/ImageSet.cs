//
// ImageSet.cs
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
using System.Linq;

namespace Xwt.Drawing
{
	class ImageSet: Image
	{
		Image[] images;

		public ImageSet (Image[] images)
		{
			if (images.Count (i => !i.HasFixedSize) > 1)
				throw new InvalidOperationException ("There is more than one image with unbound size");
			this.images = images.OrderBy (i => i.HasFixedSize ? i.Size.Width + i.Size.Height : int.MaxValue).ToArray ();
		}

		Image SelectImage (double width, double height)
		{
			foreach (var img in images) {
				if (!img.HasFixedSize || (width + height <= img.Size.Width + img.Size.Height))
					return img;
			}
			return images [images.Length - 1];
		}

		internal override bool CanDrawInContext (double width, double height)
		{
			var img = SelectImage (width, height);
			return img.CanDrawInContext (width, height);
		}

		internal override void DrawInContext (Context ctx, double x, double y, double width, double height)
		{
			var img = SelectImage (width, height);
			img.DrawInContext (ctx, x, y, width, height);
		}

		protected override BitmapImage GenerateBitmap (Size size)
		{
			var img = SelectImage (size.Width, size.Height);
			if (img.Size.Width != size.Width || img.Size.Height != size.Height)
				img = img.WithSize (size);
			return img.ToBitmap ();
		}
	}
}

