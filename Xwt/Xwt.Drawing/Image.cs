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
using System.Collections.Generic;

namespace Xwt.Drawing
{
	public class Image: XwtObject, IDisposable
	{
		Size requestedSize;
		internal NativeImageRef NativeRef;
		BitmapImage cachedBitmap;
		internal double requestedAlpha = 1;

		internal Image ()
		{
		}
		
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
			NativeRef = image.NativeRef;
			Init ();
		}

		internal void Init ()
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


		internal ImageDescription ImageDescription {
			get {
				return new ImageDescription () {
					Alpha = requestedAlpha,
					Size = requestedSize,
					Backend = Backend
				};
			}
		}

		public static Image FromResource (Type type, string resource)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (resource == null)
				throw new ArgumentNullException ("resource");

			return FromResource (type.Assembly, resource);
		}
		
		public static Image FromResource (Assembly assembly, string resource)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			if (resource == null)
				throw new ArgumentNullException ("resource");
			
			var toolkit = Toolkit.CurrentEngine;

			var name = Path.GetFileNameWithoutExtension (resource);

			var img = toolkit.ImageBackendHandler.LoadFromResource (assembly, resource);
			if (img == null)
				throw new InvalidOperationException ("Resource not found: " + resource);

			List<object> altImages = new List<object> ();
			foreach (var r in assembly.GetManifestResourceNames ()) {
				int i = r.LastIndexOf ('@');
				if (i != -1) {
					string rname = r.Substring (0, i);
					if (rname == resource || rname == name) {
						var rim = toolkit.ImageBackendHandler.LoadFromResource (assembly, r);
						if (rim != null)
							altImages.Add (rim);
					}
				}
			}
			if (altImages.Count > 0) {
				altImages.Insert (0, img);
				img = toolkit.ImageBackendHandler.CreateMultiSizeImage (altImages);
			}
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
		
		public void Save (string file, ImageFileType fileType)
		{
			using (var f = File.OpenWrite (file))
				Save (f, fileType);
		}

		public void Save (Stream stream, ImageFileType fileType)
		{
			ToolkitEngine.ImageBackendHandler.SaveToStream (ToBitmap ().Backend, stream, fileType);
		}
		
		public bool HasFixedSize {
			get { return !Size.IsZero; }
		}

		public Size Size {
			get {
				if (!requestedSize.IsZero)
					return requestedSize;
				if (!ToolkitEngine.ImageBackendHandler.HasMultipleSizes (Backend))
					return ToolkitEngine.ImageBackendHandler.GetSize (Backend);
				else
					return Size.Zero;
			}
			internal set {
				requestedSize = value;
			}
		}
		
		public Image WithAlpha (double alpha)
		{
			return new Image (this) {
				requestedSize = requestedSize,
				requestedAlpha = alpha
			};
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

		Size DefaultSize {
			get {
				if (!requestedSize.IsZero)
					return requestedSize;
				else
					return GetDefaultSize ();
			}
		}

		internal Size GetFixedSize ()
		{
			var size = !Size.IsZero ? Size : DefaultSize;
			if (size.IsZero)
				throw new InvalidOperationException ("Image size has not been set and the image doesn't have a default size");
			return size;
		}
		
		public Image WithBoxSize (double maxWidth, double maxHeight)
		{
			var size = GetFixedSize ();
			var ratio = Math.Min (maxWidth / size.Width, maxHeight / size.Height);

			return new Image (this) {
				requestedSize = new Size (size.Width * ratio, size.Height * ratio)
			};
		}
		
		public Image WithBoxSize (double maxSize)
		{
			return WithBoxSize (maxSize, maxSize);
		}
		
		public Image WithBoxSize (Size size)
		{
			return WithBoxSize (size.Width, size.Height);
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

		public BitmapImage ToBitmap ()
		{
			var size = GetFixedSize ();
			var bmp = GetCachedBitmap (size, requestedAlpha);
			if (bmp != null)
				return bmp;

			bmp = ToBitmap (size, requestedAlpha);

			return cachedBitmap = bmp;
		}
		
		BitmapImage ToBitmap (Size size, double alpha)
		{
			// Don't cache the bitmap here since this method may be called by other image instances
			return GetCachedBitmap (size, alpha) ?? GenerateBitmap (size, alpha);
		}
		
		BitmapImage GetCachedBitmap (Size size, double alpha)
		{
			return cachedBitmap != null && size == cachedBitmap.Size && this.requestedAlpha == alpha ? cachedBitmap : null;
		}
		
		protected virtual BitmapImage GenerateBitmap (Size size, double alpha)
		{
			object bmp = Backend;
			if (ToolkitEngine.ImageBackendHandler.IsBitmap (bmp)) {
				if (size == ToolkitEngine.ImageBackendHandler.GetSize (bmp)) {
					if (alpha == 1)
						// The backend can be shared
						return new BitmapImage (this);
				}
				else
					// Create a new backend with the new size
					bmp = ToolkitEngine.ImageBackendHandler.ResizeBitmap (Backend, size.Width, size.Height);
			}
			else
				bmp = ToolkitEngine.ImageBackendHandler.ConvertToBitmap (Backend, size.Width, size.Height);

			if (alpha != 1) {
				var oldBmp = bmp;
				bmp = ToolkitEngine.ImageBackendHandler.ChangeBitmapOpacity (bmp, alpha);
				ToolkitEngine.ImageBackendHandler.Dispose (oldBmp);
			}
			return new BitmapImage (bmp);
		}

		protected virtual Size GetDefaultSize ()
		{
			return ToolkitEngine.ImageBackendHandler.GetSize (Backend);
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

