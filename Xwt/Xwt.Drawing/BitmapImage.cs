//
// Bitmap.cs
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
	public class BitmapImage: Image
	{
		public BitmapImage (BitmapImage image): base (image.ToolkitEngine.ImageBackendHandler.CopyBitmap (image.Backend), image.ToolkitEngine)
		{
		}
		
		internal BitmapImage (object backend): base (backend)
		{
		}
		
		internal BitmapImage (Image origImage): base (origImage)
		{
			// shares the backend of the image
		}

		void MakeWrittable ()
		{
			// If the bitmap only has one reference, that reference is the one held by this instance, so
			// no other image is referencing this one. In that case, the bitmap can be safely modified.
			// On the other hand, if the bitmap is being referenced by another image, an local copy
			// has to be made

			if (NativeRef.ReferenceCount > 1) {
				Backend = ToolkitEngine.ImageBackendHandler.CopyBitmap (Backend);
				NativeRef.ReleaseReference (true);
				NativeRef = new NativeImageRef (Backend, ToolkitEngine);
			}
		}
		
		public void SetPixel (int x, int y, Color color)
		{
			MakeWrittable ();
			ToolkitEngine.ImageBackendHandler.SetBitmapPixel (Backend, x, y, color);
		}
		
		public Color GetPixel (int x, int y)
		{
			return ToolkitEngine.ImageBackendHandler.GetBitmapPixel (Backend, x, y);
		}
		
		public void CopyArea (int srcX, int srcY, int width, int height, BitmapImage dest, int destX, int destY)
		{
			dest.MakeWrittable ();
			ToolkitEngine.ImageBackendHandler.CopyBitmapArea (Backend, srcX, srcY, width, height, dest.Backend, destX, destY);
		}
		
		public BitmapImage Crop (int srcX, int srcY, int width, int height)
		{
			return new BitmapImage (ToolkitEngine.ImageBackendHandler.CropBitmap (Backend, srcX, srcY, width, height));
		}
		
		public new BitmapImage WithAlpha (double alpha)
		{
			return new BitmapImage (ToolkitEngine.ImageBackendHandler.ChangeBitmapOpacity (Backend, alpha));
		}
	}
}

