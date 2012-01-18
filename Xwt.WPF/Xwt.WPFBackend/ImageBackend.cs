// 
// ButtonBackend.cs
//  
// Author:
//	   Luís Reis <luiscubal@gmail.com>
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
using System.IO;
using Xwt.Backends;
using SWM = System.Windows.Media;
using SWMI = System.Windows.Media.Imaging;

namespace Xwt.WPFBackend
{
	public class ImageBackend: ImageBackendHandler
	{
		public override object LoadFromStream(Stream stream)
		{
			var img = new SWMI.BitmapImage();
			img.BeginInit();
			img.CacheOption = SWMI.BitmapCacheOption.OnLoad;
			img.StreamSource = stream;
			img.EndInit();

			return img;
		}

		public override object LoadFromIcon(string id, IconSize size)
		{
			throw new NotImplementedException();
		}

		private static double WidthToDPI(SWMI.BitmapSource img, double pixels)
		{
			return pixels * 96 / img.DpiX;
		}

		private static double HeightToDPI(SWMI.BitmapSource img, double pixels)
		{
			return pixels * 96 / img.DpiY;
		}

		public override Size GetSize(object handle)
		{
			var img = (SWMI.BitmapSource)handle;

			return new Size(img.PixelWidth, img.PixelHeight);
		}

		public override object Resize(object handle, double width, double height)
		{
			var oldImg = (SWMI.BitmapSource)handle;

			width = WidthToDPI(oldImg, width);
			height = HeightToDPI(oldImg, height);

			SWM.DrawingVisual visual = new SWM.DrawingVisual();
			using (SWM.DrawingContext ctx = visual.RenderOpen())
			{
				ctx.DrawImage(oldImg, new System.Windows.Rect(0, 0, width, height));
			}

			SWMI.RenderTargetBitmap bmp = new SWMI.RenderTargetBitmap((int)width, (int)height, oldImg.DpiX, oldImg.DpiY, oldImg.Format);
			bmp.Render(visual);

			return bmp;
		}

		public override object Copy(object handle)
		{
			return ((SWMI.BitmapSource)handle).Clone();
		}

		public override object Crop(object handle, int srcX, int srcY, int w, int h)
		{
			var oldImg = (SWMI.BitmapSource)handle;

			double width = WidthToDPI(oldImg, w);
			double height = HeightToDPI(oldImg, h);

			SWM.DrawingVisual visual = new SWM.DrawingVisual();
			using (SWM.DrawingContext ctx = visual.RenderOpen())
			{
				//Not sure whether this actually works, untested
				ctx.DrawImage(oldImg, new System.Windows.Rect(-srcX, -srcY, srcX+width, srcY+height));
			}

			SWMI.RenderTargetBitmap bmp = new SWMI.RenderTargetBitmap((int)width, (int)height, oldImg.DpiX, oldImg.DpiY, oldImg.Format);
			bmp.Render(visual);

			return bmp;
		}

		public override object ChangeOpacity(object backend, double opacity)
		{
			throw new NotImplementedException();
		}

		public override void CopyArea(object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY)
		{
			throw new NotImplementedException();
		}
	}
}
