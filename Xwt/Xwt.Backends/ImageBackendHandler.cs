// 
// ImageBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using System.Reflection;
using Xwt.Drawing;
using System.Collections.Generic;

namespace Xwt.Backends
{
	public abstract class ImageBackendHandler: DisposableResourceBackendHandler
	{
		public virtual object CreateBackend ()
		{
			throw new NotSupportedException ();
		}
		
		public virtual object LoadFromResource (Assembly asm, string name)
		{
			using (var s = asm.GetManifestResourceStream (name)) {
				if (s == null)
					throw new InvalidOperationException ("Resource not found: " + name);
				return LoadFromStream (s);
			}
		}
		
		public virtual object LoadFromFile (string file)
		{
			using (var s = File.OpenRead (file))
				return LoadFromStream (s);
		}

		/// <summary>
		/// Creates an image that is custom drawn
		/// </summary>
		/// <returns>The custom drawn.</returns>
		/// <param name="drawCallback">The callback to be used to draw the image. The arguments are: the context backend, the bounds where to draw</param>
		public virtual object CreateCustomDrawn (ImageDrawCallback drawCallback)
		{
			throw new NotSupportedException ();
		}

		/// <summary>
		/// Creates an image with multiple representations in different sizes
		/// </summary>
		/// <returns>The image backend</returns>
		/// <param name="images">Backends of the different image representations</param>
		/// <remarks>The first image of the list if the reference image, the one with scale factor = 1</remarks>
		public virtual object CreateMultiResolutionImage (IEnumerable<object> images)
		{
			throw new NotSupportedException ();
		}
		
		public virtual object CreateMultiSizeIcon (IEnumerable<object> images)
		{
			throw new NotSupportedException ();
		}

		public abstract object LoadFromStream (Stream stream);

		public abstract void SaveToStream (object backend, System.IO.Stream stream, ImageFileType fileType);

		public abstract Image GetStockIcon (string id);

		/// <summary>
		/// Determines whether this instance is a bitmap
		/// </summary>
		/// <param name="handle">Image handle</param>
		public abstract bool IsBitmap (object handle);

		/// <summary>
		/// Converts an image to a bitmap of the specified size
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="handle">Image handle.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public abstract object ConvertToBitmap (ImageDescription idesc, double scaleFactor, ImageFormat format);

		/// <summary>
		/// Returns True if the image has multiple representations of different sizes.
		/// </summary>
		/// <param name="handle">Image handle.</param>
		/// <remarks>For example, it would return True for a .ico file which as several representations of the image in different sizes</remarks>
		public abstract bool HasMultipleSizes (object handle);

		/// <summary>
		/// Gets the size of an image
		/// </summary>
		/// <returns>The size of the image, or a size of (0,0) if there is no known size for the image</returns>
		/// <param name="handle">Image handle</param>
		/// <remarks>
		/// This method should return a size of (0,0) if the image doesn't have an intrinsic size.
		/// For example: if the image is a vecor image, or if is an icon composed of images of different sizes.
		/// </remarks>
		public abstract Size GetSize (object handle);
		
		public abstract object CopyBitmap (object handle);

		public abstract void CopyBitmapArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY);

		public abstract object CropBitmap (object handle, int srcX, int srcY, int width, int height);

		public abstract void SetBitmapPixel (object handle, int x, int y, Color color);
		
		public abstract Color GetBitmapPixel (object handle, int x, int y);
	}

	public delegate void ImageDrawCallback (object contextBackend, Rectangle bounds, ImageDescription idesc, Toolkit toolkit);

	public struct ImageDescription
	{
		public static ImageDescription Null = new ImageDescription ();

		public bool IsNull {
			get { return Backend == null; }
		}

		public object Backend { get; set; }
		public Size Size { get; set; }
		public double Alpha { get; set; }
		public StyleSet Styles { get; set; }
	}
}

