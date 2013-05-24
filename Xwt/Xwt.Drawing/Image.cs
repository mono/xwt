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

		/// <summary>
		/// Creates a new image that is a copy of another image
		/// </summary>
		/// <param name="image">Image.</param>
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
					Size = Size,
					Backend = Backend
				};
			}
		}

		/// <summary>
		/// Loads an image from a resource
		/// </summary>
		/// <returns>An image</returns>
		/// <param name="type">Type which identifies the assembly from which to load the image</param>
		/// <param name="resource">Resource name</param>
		/// <remarks>
		/// This method will look for alternative versions of the image with different resolutions.
		/// For example, if a resource is named "foo.png", this method will load
		/// other resources with the name "foo@XXX.png", where XXX can be any arbitrary string. For example "foo@2x.png".
		/// Each of those resources will be considered different versions of the same image.
		/// </remarks>
		public static Image FromResource (Type type, string resource)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (resource == null)
				throw new ArgumentNullException ("resource");

			return FromResource (type.Assembly, resource);
		}

		/// <summary>
		/// Loads an image from a resource
		/// </summary>
		/// <returns>An image</returns>
		/// <param name="assembly">The assembly from which to load the image</param>
		/// <param name="resource">Resource name</param>
		/// <remarks>
		/// This method will look for alternative versions of the image with different resolutions.
		/// For example, if a resource is named "foo.png", this method will load
		/// other resources with the name "foo@XXX.png", where XXX can be any arbitrary string. For example "foo@2x.png".
		/// Each of those resources will be considered different versions of the same image.
		/// </remarks>
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

			var reqSize = toolkit.ImageBackendHandler.GetSize (img);

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
			return new Image (img, toolkit) {
				requestedSize = reqSize
			};
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

		/// <summary>
		/// Gets a value indicating whether this image has fixed size.
		/// </summary>
		/// <value><c>true</c> if this image has fixed size; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Some kinds of images such as vector images or multiple-size icons don't have a fixed size,
		/// and a specific size has to be chosen before they can be used. A size can be chosen by using
		/// the WithSize method.
		/// </remarks>
		public bool HasFixedSize {
			get { return !Size.IsZero; }
		}

		/// <summary>
		/// Gets the size of the image
		/// </summary>
		/// <value>The size of the image, or Size.Zero if the image doesn't have an intrinsic size</value>
		public Size Size {
			get {
				return !requestedSize.IsZero ? requestedSize : GetDefaultSize ();
			}
			internal set {
				requestedSize = value;
			}
		}

		/// <summary>
		/// Applies an alpha filter to the image
		/// </summary>
		/// <returns>A new image with the alpha filter applied</returns>
		/// <param name="alpha">Alpha to apply</param>
		/// <remarks>This is a lightweight operation. The alpha filter is applied when the image is rendered.
		/// The method doesn't make a copy of the image data.</remarks>
		public Image WithAlpha (double alpha)
		{
			return new Image (this) {
				requestedSize = requestedSize,
				requestedAlpha = alpha
			};
		}

		/// <summary>
		/// Retuns a copy of the image with a specific size
		/// </summary>
		/// <returns>A new image with the new size</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <remarks>
		/// This is a lightweight operation. The image is scaled when it is rendered.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
		public Image WithSize (double width, double height)
		{
			return new Image (this) {
				requestedSize = new Size (width, height)
			};
		}

		/// <summary>
		/// Retuns a copy of the image with a specific size
		/// </summary>
		/// <returns>A new image with the new size</returns>
		/// <param name="size">The size.</param>
		/// <remarks>
		/// This is a lightweight operation. The image is scaled when it is rendered.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
		public Image WithSize (Size size)
		{
			return new Image (this) {
				requestedSize = size
			};
		}
		
		/// <summary>
		/// Retuns a copy of the image with a specific size
		/// </summary>
		/// <returns>A new image with the new size</returns>
		/// <param name="squaredSize">Width and height of the image (the image is expected to be squared)</param>
		/// <remarks>
		/// This is a lightweight operation. The image is scaled when it is rendered.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
		public Image WithSize (double squaredSize)
		{
			return new Image (this) {
				requestedSize = new Size (squaredSize, squaredSize)
			};
		}
		
		/// <summary>
		/// Retuns a copy of the image with a specific size
		/// </summary>
		/// <returns>A new image with the new size</returns>
		/// <param name="size">New size</param>
		/// <remarks>
		/// This is a lightweight operation. The image is scaled when it is rendered.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
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

		internal Size GetFixedSize ()
		{
			var size = Size;
			if (size.IsZero)
				throw new InvalidOperationException ("Image size has not been set and the image doesn't have a default size");
			return size;
		}

		/// <summary>
		/// Retuns a copy of the image with a size that fits the provided size limits
		/// </summary>
		/// <returns>The image</returns>
		/// <param name="maxWidth">Max width.</param>
		/// <param name="maxHeight">Max height.</param>
		/// <remarks>
		/// This is a lightweight operation. The image is scaled when it is rendered.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
		public Image WithBoxSize (double maxWidth, double maxHeight)
		{
			var size = GetFixedSize ();
			var ratio = Math.Min (maxWidth / size.Width, maxHeight / size.Height);

			return new Image (this) {
				requestedSize = new Size (size.Width * ratio, size.Height * ratio)
			};
		}
		
		/// <summary>
		/// Retuns a copy of the image with a size that fits the provided size limits
		/// </summary>
		/// <returns>The image</returns>
		/// <param name="maxSize">Max width and height (the image is expected to be squared)</param>
		/// <remarks>
		/// This is a lightweight operation. The image is scaled when it is rendered.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
		public Image WithBoxSize (double maxSize)
		{
			return WithBoxSize (maxSize, maxSize);
		}
		
		/// <summary>
		/// Retuns a copy of the image with a size that fits the provided size limits
		/// </summary>
		/// <returns>The image</returns>
		/// <param name="size">Max width and height</param>
		/// <remarks>
		/// This is a lightweight operation. The image is scaled when it is rendered.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
		public Image WithBoxSize (Size size)
		{
			return WithBoxSize (size.Width, size.Height);
		}
		
		/// <summary>
		/// Retuns a scaled copy of the image
		/// </summary>
		/// <returns>The image</returns>
		/// <param name="scale">Scale to apply to the image size</param>
		/// <remarks>
		/// This is a lightweight operation. The image is scaled when it is rendered.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
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
		
		/// <summary>
		/// Retuns a scaled copy of the image
		/// </summary>
		/// <returns>The image</returns>
		/// <param name="scaleX">Scale to apply to the width of the image</param>
		/// <param name="scaleY">Scale to apply to the height of the image</param>
		/// <remarks>
		/// This is a lightweight operation. The image is scaled when it is rendered.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
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

		/// <summary>
		/// Converts the image to a bitmap
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="format">Bitmap format</param>
		public BitmapImage ToBitmap (ImageFormat format = ImageFormat.ARGB32)
		{
			var s = GetFixedSize ();
			return ToBitmap ((int)s.Width, (int)s.Height);
		}

		/// <summary>
		/// Converts the image to a bitmap
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="renderTarget">Widget to be used as reference for determining the resolution of the bitmap</param>
		/// <param name="format">Bitmap format</param>
		public BitmapImage ToBitmap (Widget renderTarget, ImageFormat format = ImageFormat.ARGB32)
		{
			if (renderTarget.ParentWindow == null)
				throw new InvalidOperationException ("renderTarget is not bound to a window");
			return ToBitmap (renderTarget.ParentWindow, format);
		}

		/// <summary>
		/// Converts the image to a bitmap
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="renderTarget">Window to be used as reference for determining the resolution of the bitmap</param>
		/// <param name="format">Bitmap format</param>
		public BitmapImage ToBitmap (WindowFrame renderTarget, ImageFormat format = ImageFormat.ARGB32)
		{
			return ToBitmap (renderTarget.Screen, format);
		}

		/// <summary>
		/// Converts the image to a bitmap
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="renderTarget">Screen to be used as reference for determining the resolution of the bitmap</param>
		/// <param name="format">Bitmap format</param>
		public BitmapImage ToBitmap (Screen renderTarget, ImageFormat format = ImageFormat.ARGB32)
		{
			var s = GetFixedSize ();
			return ToBitmap ((int)(s.Width * renderTarget.ScaleFactor), (int)(s.Height * renderTarget.ScaleFactor), format);
		}

		/// <summary>
		/// Converts the image to a bitmap
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="pixelWidth">Width of the image in real pixels</param>
		/// <param name="pixelHeight">Height of the image in real pixels</param>
		/// <param name="format">Bitmap format</param>
		public BitmapImage ToBitmap (int pixelWidth, int pixelHeight, ImageFormat format = ImageFormat.ARGB32)
		{
			var bmp = ToolkitEngine.ImageBackendHandler.ConvertToBitmap (Backend, pixelWidth, pixelHeight, format);
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

