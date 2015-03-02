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
using System.Linq;
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

		static int[] supportedScales = { 2 };

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


		internal ImageDescription GetImageDescription (Toolkit toolkit)
		{
			InitForToolkit (toolkit);
			return new ImageDescription () {
				Alpha = requestedAlpha,
				Size = Size,
				Backend = Backend
			};
		}

		internal void InitForToolkit (Toolkit t)
		{
			if (ToolkitEngine != t && NativeRef != null) {
				var nr = NativeRef.LoadForToolkit (t);
				ToolkitEngine = t;
				Backend = nr.Backend;
			}
		}

		/// <summary>
		/// Loads an image from a resource
		/// </summary>
		/// <returns>An image</returns>
		/// <param name="resource">Resource name</param>
		/// <remarks>
		/// This method will look for alternative versions of the image with different resolutions.
		/// For example, if a resource is named "foo.png", this method will load
		/// other resources with the name "foo@XXX.png", where XXX can be any arbitrary string. For example "foo@2x.png".
		/// Each of those resources will be considered different versions of the same image.
		/// </remarks>
		public static Image FromResource (string resource)
		{
			if (resource == null)
				throw new ArgumentNullException ("resource");

			return FromResource (Assembly.GetCallingAssembly (), resource);
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
			if (toolkit == null)
				throw new ToolkitNotInitializedException ();

			var name = Path.GetFileNameWithoutExtension (resource);

			var img = toolkit.ImageBackendHandler.LoadFromResource (assembly, resource);
			if (img == null)
				throw new InvalidOperationException ("Resource not found: " + resource);

			var reqSize = toolkit.ImageBackendHandler.GetSize (img);

			var ext = GetExtension (resource);
			var altImages = new List<Tuple<string,object>> ();

			foreach (var r in assembly.GetManifestResourceNames ()) {
				int i = r.LastIndexOf ('@');
				if (i != -1) {
					string rname = r.Substring (0, i);
					if (rname == resource || rname == name) {
						var rim = toolkit.ImageBackendHandler.LoadFromResource (assembly, r);
						if (rim != null)
							altImages.Add (new Tuple<string, object> (r, rim));
					}
				}
			}
			if (altImages.Count > 0) {
				altImages.Insert (0, new Tuple<string, object> (resource, img));
				if (ext == ".9.png")
					return CreateComposedNinePatch (toolkit, altImages);
				img = toolkit.ImageBackendHandler.CreateMultiResolutionImage (altImages.Select (i => i.Item2));
			}
			var res = new Image (img, toolkit) {
				requestedSize = reqSize
			};
			res.NativeRef.SetResourceSource (assembly, resource);
			if (ext == ".9.png")
				res = new NinePatchImage (res.ToBitmap ());
			return res;
		}

		public static Image CreateMultiSizeIcon (IEnumerable<Image> images)
		{
			if (Toolkit.CurrentEngine == null)
				throw new ToolkitNotInitializedException ();

			var allImages = images.ToArray ();

			var img = new Image (Toolkit.CurrentEngine.ImageBackendHandler.CreateMultiSizeIcon (allImages.Select (i => i.GetBackend ())));

			if (allImages.All (i => i.NativeRef.HasNativeSource)) {
				var sources = allImages.Select (i => i.NativeRef.NativeSource).ToArray ();
				img.NativeRef.SetSources (sources);
			}
			return img;
		}

		public static Image CreateMultiResolutionImage (IEnumerable<Image> images)
		{
			if (Toolkit.CurrentEngine == null)
				throw new ToolkitNotInitializedException ();
			return new Image (Toolkit.CurrentEngine.ImageBackendHandler.CreateMultiResolutionImage (images.Select (i => i.GetBackend ())));
		}

		public static Image FromFile (string file)
		{
			var toolkit = Toolkit.CurrentEngine;
			if (toolkit == null)
				throw new ToolkitNotInitializedException ();

			var ext = GetExtension (file);
			var img = toolkit.ImageBackendHandler.LoadFromFile (file);

			List<Tuple<string,object>> altImages = null;
			foreach (var s in supportedScales) {
				var fn = file.Substring (0, file.Length - ext.Length) + "@" + s + "x" + ext;
				if (File.Exists (fn)) {
					if (altImages == null) {
						altImages = new List<Tuple<string, object>> ();
						altImages.Add (new Tuple<string, object> (file, img));
					}
					altImages.Add (new Tuple<string, object> (fn, toolkit.ImageBackendHandler.LoadFromFile (fn)));
				}
			}

			if (altImages != null) {
				if (ext == ".9.png")
					return CreateComposedNinePatch (toolkit, altImages);
				img = toolkit.ImageBackendHandler.CreateMultiResolutionImage (altImages.Select (i => i.Item2));
			}

			var res = new Image (img, toolkit);
			if (ext == ".9.png")
				res = new NinePatchImage (res.ToBitmap ());
			return res;
		}

		static Image CreateComposedNinePatch (Toolkit toolkit, List<Tuple<string,object>> altImages)
		{
			var npImage = new NinePatchImage ();
			foreach (var fi in altImages) {
				int i = fi.Item1.LastIndexOf ('@');
				double scaleFactor;
				if (i == -1)
					scaleFactor = 1;
				else {
					int j = fi.Item1.IndexOf ('x', ++i);
					if (!double.TryParse (fi.Item1.Substring (i, j - i), out scaleFactor)) {
						toolkit.ImageBackendHandler.Dispose (fi.Item2);
						continue;
					}
				}
				npImage.AddFrame (new Image (fi.Item2, toolkit).ToBitmap (), scaleFactor);
			}
			return npImage;
		}
		
		public static Image FromStream (Stream stream)
		{
			var toolkit = Toolkit.CurrentEngine;
			if (toolkit == null)
				throw new ToolkitNotInitializedException ();
			return new Image (toolkit.ImageBackendHandler.LoadFromStream (stream), toolkit);
		}

		static string GetExtension (string fileName)
		{
			if (fileName.EndsWith (".9.png", StringComparison.Ordinal))
				return ".9.png";
			else
				return Path.GetExtension (fileName);
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
		/// Gets the width of the image
		/// </summary>
		/// <value>The width.</value>
		public double Width {
			get { return Size.Width; }
		}

		/// <summary>
		/// Gets the height of the image
		/// </summary>
		/// <value>The height.</value>
		public double Height {
			get { return Size.Height; }
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
			return ToBitmap (1d);
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
			return ToBitmap (renderTarget.ParentWindow.Screen.ScaleFactor, format);
		}

		/// <summary>
		/// Converts the image to a bitmap
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="renderTarget">Window to be used as reference for determining the resolution of the bitmap</param>
		/// <param name="format">Bitmap format</param>
		public BitmapImage ToBitmap (WindowFrame renderTarget, ImageFormat format = ImageFormat.ARGB32)
		{
			return ToBitmap (renderTarget.Screen.ScaleFactor, format);
		}

		/// <summary>
		/// Converts the image to a bitmap
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="renderTarget">Screen to be used as reference for determining the resolution of the bitmap</param>
		/// <param name="format">Bitmap format</param>
		public BitmapImage ToBitmap (Screen renderTarget, ImageFormat format = ImageFormat.ARGB32)
		{
			return ToBitmap (renderTarget.ScaleFactor, format);
		}

		/// <summary>
		/// Converts the image to a bitmap
		/// </summary>
		/// <returns>The bitmap.</returns>
		/// <param name="scaleFactor">Scale factor of the bitmap</param>
		/// <param name="format">Bitmap format</param>
		public BitmapImage ToBitmap (double scaleFactor, ImageFormat format = ImageFormat.ARGB32)
		{
			var s = GetFixedSize ();
			var bmp = ToolkitEngine.ImageBackendHandler.ConvertToBitmap (Backend, s.Width, s.Height, scaleFactor, format);
			return new BitmapImage (bmp, s, ToolkitEngine);
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

		NativeImageSource[] sources;

		public struct NativeImageSource {
			// Source file or resource name
			public string Source;

			// Assembly that contains the resource
			public Assembly ResourceAssembly;

			public Func<Stream[]> ImageLoader;

			public ImageDrawCallback DrawCallback;

			public string StockId;
		}

		public object Backend {
			get { return backend; }
		}

		public Toolkit Toolkit {
			get { return toolkit; }
		}

		public NativeImageSource NativeSource {
			get { return sources[0]; }
		}

		public bool HasNativeSource {
			get { return sources != null; }
		}

		public void SetSources (NativeImageSource[] sources)
		{
			this.sources = sources;
		}

		public void SetFileSource (string file)
		{
			sources = new [] { 
				new NativeImageSource {
					Source = file,
				}
			};
		}

		public void SetResourceSource (Assembly asm, string name)
		{
			sources = new [] { 
				new NativeImageSource {
					Source = name,
					ResourceAssembly = asm
				}
			};
		}

		public void SetStreamSource (Func<Stream[]> imageLoader)
		{
			sources = new [] { 
				new NativeImageSource {
					ImageLoader = imageLoader
				}
			};
		}

		public void SetCustomDrawSource (ImageDrawCallback drawCallback)
		{
			sources = new [] { 
				new NativeImageSource {
					DrawCallback = drawCallback
				}
			};
		}

		public void SetStockSource (string stockID)
		{
			sources = new [] {
				new NativeImageSource {
					StockId = stockID
				}
			};
		}

		public int ReferenceCount {
			get { return referenceCount; }
		}

		public NativeImageRef (object backend, Toolkit toolkit)
		{
			this.backend = backend;
			this.toolkit = toolkit;
			NextRef = this;

			if (toolkit.ImageBackendHandler.DisposeHandleOnUiThread)
				ResourceManager.RegisterResource (backend, toolkit.ImageBackendHandler.Dispose);
		}

		public NativeImageRef LoadForToolkit (Toolkit targetToolkit)
		{
			NativeImageRef newRef = null;
			var r = NextRef;
			while (r != this) {
				if (r.toolkit == targetToolkit) {
					newRef = r;
					break;
				}
				r = r.NextRef;
			}
			if (newRef != null)
				return newRef;

			object newBackend = null;

			if (sources != null) {
				var frames = new List<object> ();
				foreach (var s in sources) {
					if (s.ImageLoader != null) {
						var streams = s.ImageLoader ();
						try {
							if (streams.Length == 1) {
								newBackend = targetToolkit.ImageBackendHandler.LoadFromStream (streams [0]);
							} else {
								var backends = new object[streams.Length];
								for (int n = 0; n < backends.Length; n++) {
									backends [n] = targetToolkit.ImageBackendHandler.LoadFromStream (streams [n]);
								}
								newBackend = targetToolkit.ImageBackendHandler.CreateMultiResolutionImage (backends);
							}
						} finally {
							foreach (var st in streams)
								st.Dispose ();
						}
					} else if (s.ResourceAssembly != null) {
						targetToolkit.Invoke (() => newBackend = Image.FromResource (s.ResourceAssembly, s.Source).GetBackend());
					}
					else if (s.Source != null)
						targetToolkit.Invoke (() => newBackend = Image.FromFile (s.Source).GetBackend());
					else if (s.DrawCallback != null)
						newBackend = targetToolkit.ImageBackendHandler.CreateCustomDrawn (s.DrawCallback);
					else if (s.StockId != null)
						newBackend = targetToolkit.GetStockIcon (s.StockId).GetBackend ();
					else
						throw new NotSupportedException ();
					frames.Add (newBackend);
				}
				newBackend = targetToolkit.ImageBackendHandler.CreateMultiSizeIcon (frames);
			} else {
				using (var s = new MemoryStream ()) {
					toolkit.ImageBackendHandler.SaveToStream (backend, s, ImageFileType.Png);
					s.Position = 0;
					newBackend = targetToolkit.ImageBackendHandler.LoadFromStream (s);
				}
			}
			newRef = new NativeImageRef (newBackend, targetToolkit);
			newRef.NextRef = NextRef;
			NextRef = newRef;
			return newRef;
		}

		public void AddReference ()
		{
			System.Threading.Interlocked.Increment (ref referenceCount);
		}

		public void ReleaseReference (bool disposing)
		{
			if (System.Threading.Interlocked.Decrement (ref referenceCount) == 0) {
				if (disposing) {
					if (toolkit.ImageBackendHandler.DisposeHandleOnUiThread)
						ResourceManager.FreeResource (backend);
					else
						toolkit.ImageBackendHandler.Dispose (backend);
				} else
					ResourceManager.FreeResource (backend);
			}
		}

		/// <summary>
		/// Reference to the next native image, for a different toolkit
		/// </summary>
		public NativeImageRef NextRef { get; set; }
	}
}

