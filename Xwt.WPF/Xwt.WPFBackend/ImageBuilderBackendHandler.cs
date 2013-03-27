//
// ImageBuilderBackendHandler.cs
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

using Xwt.Backends;
using Xwt.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Xwt.WPFBackend
{
	public class WpfImageBuilderBackendHandler
		: ImageBuilderBackendHandler
	{
		class ImageBuilder: DrawingVisual
		{
			public int Width;
			public int Height;
			public DrawingContext Context;
		}

		public override object CreateImageBuilder (int width, int height, ImageFormat format)
		{
			return new ImageBuilder () {
				Width = width,
				Height = height
			};
		}

		public override object CreateContext (object backend)
		{
			var visual = (ImageBuilder)backend;
			visual.Context = new DrawingContext (visual.RenderOpen (), visual.GetScaleFactor ());
			return visual.Context;
		}

		public override object CreateImage (object backend)
		{
			var visual = (ImageBuilder)backend;
			var ratios = visual.GetPixelRatios ();
			visual.Context.Dispose ();
			var bmp = new RenderTargetBitmap (visual.Width, visual.Height, ratios.Height * 96, ratios.Width * 96, PixelFormats.Pbgra32);
			bmp.Render(visual);
			return new WpfImage (bmp);
		}

		public override void Dispose (object backend)
		{
			var bmp = (ImageBuilder)backend;
			bmp.Context.Dispose ();
		}
	}
}
