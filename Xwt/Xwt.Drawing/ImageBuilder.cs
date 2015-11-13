// 
// ImageBuilder.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt.Backends;
using System.Collections.Generic;


namespace Xwt.Drawing
{
	public sealed class ImageBuilder: XwtObject, IDisposable
	{
		Context ctx;
		VectorBackend backend;
		double width;
		double height;

		public ImageBuilder (double width, double height)
		{
			backend = new VectorContextBackend (ToolkitEngine, width, height);

			// Don't set the global styles to the context. The global styles will be used when rendering the image
			ctx = new Context (backend, ToolkitEngine, ToolkitEngine.VectorImageRecorderContextHandler, false);

			ctx.Reset (null);
			this.width = width;
			this.height = height;
		}
		
		public double Width {
			get { return width; } 
		}
		
		public double Height {
			get { return height; }
		}
		
		public void Dispose ()
		{
			ctx.Dispose ();
		}

		public Context Context {
			get {
				return ctx;
			}
		}
		
		public Image ToVectorImage ()
		{
			return new VectorImage (new Size (width, height), backend.ToVectorImageData ());
		}

		public BitmapImage ToBitmap (ImageFormat format = ImageFormat.ARGB32)
		{
			return ToVectorImage ().ToBitmap (format);
		}

		public BitmapImage ToBitmap (Widget renderTarget, ImageFormat format = ImageFormat.ARGB32)
		{
			return ToVectorImage ().ToBitmap (renderTarget, format);
		}

		public BitmapImage ToBitmap (Window renderTarget, ImageFormat format = ImageFormat.ARGB32)
		{
			return ToVectorImage ().ToBitmap (renderTarget, format);
		}

		public BitmapImage ToBitmap (Screen renderTarget, ImageFormat format = ImageFormat.ARGB32)
		{
			return ToVectorImage ().ToBitmap (renderTarget, format);
		}

		public BitmapImage ToBitmap (double scaleFactor, ImageFormat format = ImageFormat.ARGB32)
		{
			return ToVectorImage ().ToBitmap (scaleFactor, format);
		}
	}
}
