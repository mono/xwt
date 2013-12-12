//
// ImagePatternBackendHandler.cs
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
using Xwt.Backends;
using System.Windows.Media;

namespace Xwt.WPFBackend
{
	public class WpfImagePatternBackendHandler
		: ImagePatternBackendHandler
	{
		public override object Create (ImageDescription img)
		{
			return new ImagePattern (ApplicationContext, img);
		}

		public override void Dispose (object img)
		{
		}
	}

	class ImagePattern
	{
		ApplicationContext actx;
		ImageDescription image;
		double scaleFactor;
		ImageBrush brush;

		public ImagePattern (ApplicationContext actx, ImageDescription im)
		{
			this.actx = actx;
			this.image = im;
		}

		public ImageBrush GetBrush (double scaleFactor)
		{
            if (brush == null || scaleFactor != this.scaleFactor)
            {
                this.scaleFactor = scaleFactor;
                var ib = (WpfImage)image.Backend;
                var bmp = ib.GetBestFrame(actx, scaleFactor, image.Size.Width, image.Size.Height, false);
                brush = new ImageBrush(bmp)
                {
                    TileMode = TileMode.Tile,
                    ViewportUnits = BrushMappingMode.Absolute,
                    AlignmentY = System.Windows.Media.AlignmentY.Top,
                    AlignmentX = System.Windows.Media.AlignmentX.Left,
                    Stretch = System.Windows.Media.Stretch.None,
					Viewport = new System.Windows.Rect(0, 0, bmp.Width, bmp.Height),
                    Opacity = image.Alpha
                };
				brush.RelativeTransform = new ScaleTransform(image.Size.Width / bmp.Width, image.Size.Height / bmp.Height);
            }
			return brush;
		}
	}
}
