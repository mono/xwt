// 
// Image.cs
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
using Xwt.Backends;

using System.Reflection;
using System.IO;

namespace Xwt.Drawing
{
	public class Image: XwtObject, IDisposable
	{
		Size requestedSize;
		internal NativeImageRef NativeRef;
		bool preserveAspectRatio;
		Image sourceImage;
		 
		static readonly object NoBackend = new object ();

		internal Image (object backend): base (backend)
		{
			Init ();
		}
		
		internal Image (object backend, Toolkit toolkit): base (backend, toolkit)
		{
			Init ();
		}
		
		public Image (Image image): base (image.Backend, image.ToolkitEngine)
		{
			if (image.NativeRef != null) {
				NativeRef = image.NativeRef;
				Init ();
			}
			else
				sourceImage = image;
		}

		protected Image (Toolkit toolkit): base (NoBackend, toolkit)
		{
		}

		protected Image (): base (NoBackend)
		{
		}
		
		void Init ()
		{
			if (NativeRef == null) {
				NativeRef = new NativeImageRef (Backend, ToolkitEngine);
			} else
				NativeRef.AddReference ();
		}
		
		~Image ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (NativeRef != null)
				NativeRef.ReleaseReference (disposing);
		}

		public static Image FromResource (Type type, string resource)
		{
			var toolkit = Toolkit.CurrentEngine;
			var img = toolkit.ImageBackendHandler.LoadFromResource (type.Assembly, resource);
			if (img == null)
				throw new InvalidOperationException ("Resource not found: " + resource);
			return new Image (img, toolkit);
		}
		
		public static Image FromResource (Assembly asm, string resource)
		{
			var toolkit = Toolkit.CurrentEngine;
			var img = toolkit.ImageBackendHandler.LoadFromResource (asm, resource);
			if (img == null)
				throw new InvalidOperationException ("Resource not found: " + resource);
			return new Image (img, toolkit);
		}
		
		public static Image FromFile (string file)
		{
			var toolkit = Toolkit.CurrentEngine;
			return new Image (toolkit.ImageBackendHandler.LoadFromFile (file), toolkit);
		}
		
		public static Image FromStream (Stream stream)
		{
			var toolkit = Toolkit.CurrentEngine;
			return new Image (toolkit.ImageBackendHandler.LoadFromStream (stream), toolkit);
		}
		
		public static Image FromMultipleSizes (params Image[] images)
		{
			return new ImageSet (images);
		}

		public BitmapImage ToBitmap ()
		{
			if (sourceImage != null)
				return sourceImage.ToBitmap (Size);
			else
				return ToBitmap (Size);
		}
		
		public Size Size {
			get {
				if (!requestedSize.IsZero)
					return requestedSize;
				if (Backend != NoBackend && ToolkitEngine.ImageBackendHandler.IsBitmap (Backend))
					return ToolkitEngine.ImageBackendHandler.GetBitmapSize (Backend);
				else
					return GetSize ();
			}
		}
		
		public Image WithSize (double width, double height)
		{
			return new Image (this) {
				requestedSize = new Size (width, height)
			};
		}
		
		public Image WithSize (Size size)
		{
			return new Image (this) {
				requestedSize = size
			};
		}
		
		public Image WithSize (double squaredSize)
		{
			return new Image (this) {
				requestedSize = new Size (squaredSize, squaredSize)
			};
		}
		
		public Image WithSize (IconSize size)
		{
			Size s;

			switch (size) {
			case IconSize.Small: s = new Size (16, 16); break;
			case IconSize.Medium: s = new Size (24, 24); break;
			case IconSize.Large: s = new Size (32, 32); break;
			default: throw new ArgumentOutOfRangeException ("size");
			}

			return new Image (this) {
				requestedSize = s
			};
		}
		
		public Image WithBoxSize (double maxWidth, double maxHeight)
		{
			return new Image (this) {
				requestedSize = new Size (maxWidth, maxHeight),
				preserveAspectRatio = true
			};
		}
		
		public Image WithBoxSize (double maxSize)
		{
			return new Image (this) {
				requestedSize = new Size (maxSize, maxSize),
				preserveAspectRatio = true
			};
		}
		
		public bool HasFixedSize {
			get { return !Size.IsZero; }
		}

		public Image Scale (double scale)
		{
			if (!HasFixedSize)
				throw new InvalidOperationException ("Image must have a size in order to be scaled");
			
			double w = Size.Width * scale;
			double h = Size.Height * scale;
			return new Image (this) {
				requestedSize = new Size (w, h)
			};
		}
		
		public Image Scale (double scaleX, double scaleY)
		{
			if (!HasFixedSize)
				throw new InvalidOperationException ("Image must have a size in order to be scaled");

			double w = Size.Width * scaleX;
			double h = Size.Height * scaleY;
			return new Image (this) {
				requestedSize = new Size (w, h)
			};
		}

		BitmapImage ToBitmap (Size size)
		{
			// TODO: Caching
			return GenerateBitmap (size);
		}
		
		protected virtual BitmapImage GenerateBitmap (Size size)
		{
			if (Backend != NoBackend) {
				if (ToolkitEngine.ImageBackendHandler.IsBitmap (Backend)) {
					if (size == ToolkitEngine.ImageBackendHandler.GetBitmapSize (Backend)) {
						// The backend can be shared
						return new BitmapImage (this);
					}
					else {
						// Create a new backend with the new size
						var bmp = ToolkitEngine.ImageBackendHandler.ResizeBitmap (Backend, size.Width, size.Height);
						return new BitmapImage (bmp);
					}
				}
				else
					return new BitmapImage (ToolkitEngine.ImageBackendHandler.ConvertToBitmap (Backend, size.Width, size.Height, preserveAspectRatio));
			}
			
			throw new NotSupportedException ();
		}

		protected virtual Size GetSize ()
		{
			return Size.Zero;
		}

		internal virtual bool CanDrawInContext (double width, double height)
		{
			return sourceImage != null ? sourceImage.CanDrawInContext (width, height) : false;
		}
		
		internal virtual void DrawInContext (Context ctx, double x, double y, double width, double height)
		{
			if (sourceImage != null)
				sourceImage.DrawInContext (ctx, x, y, width, height);
		}
	}

	class NativeImageRef
	{
		object backend;
		int referenceCount = 1;
		Toolkit toolkit;

		public int ReferenceCount {
			get { return referenceCount; }
		}

		public NativeImageRef (object backend, Toolkit toolkit)
		{
			this.backend = backend;
			this.toolkit = toolkit;
		}

		public void AddReference ()
		{
			System.Threading.Interlocked.Increment (ref referenceCount);
		}

		public void ReleaseReference (bool disposing)
		{
			if (System.Threading.Interlocked.Decrement (ref referenceCount) == 0 && disposing)
				toolkit.ImageBackendHandler.Dispose (backend);
		}
	}
}

