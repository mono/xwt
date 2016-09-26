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
		internal Size requestedSize;
		internal NativeImageRef NativeRef;
		internal double requestedAlpha = 1;
		internal StyleSet styles;

		internal static int[] SupportedScales = { 2 };

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
			requestedSize = image.requestedSize;
			requestedAlpha = image.requestedAlpha;
			styles = image.styles;
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
				Backend = Backend,
				Styles = styles
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

			var loader = new ResourceImageLoader (toolkit, assembly);
			return LoadImage (loader, resource, null);
		}

		static Image LoadImage (ImageLoader loader, string fileName, ImageTagSet tagFilter)
		{
			var toolkit = Toolkit.CurrentEngine;
			if (toolkit == null)
				throw new ToolkitNotInitializedException ();

			var img = loader.LoadImage (fileName);
			var reqSize = toolkit.ImageBackendHandler.GetSize (img);

			var ext = GetExtension (fileName);
			var name = fileName.Substring (0, fileName.Length - ext.Length);
			var altImages = new List<Tuple<string,ImageTagSet,bool,object>> ();
			var tags = Context.RegisteredStyles;

			foreach (var r in loader.GetAlternativeFiles (fileName, name, ext)) {
				int scale;
				ImageTagSet fileTags;
				if (ParseImageHints (name, r, ext, out scale, out fileTags) && (tagFilter == null || tagFilter.Equals (fileTags))) {
					var rim = loader.LoadImage (r);
					if (rim != null)
						altImages.Add (new Tuple<string, ImageTagSet, bool, object> (r, fileTags, scale > 1, rim));
				}
			}

			if (altImages.Count > 0) {
				altImages.Insert (0, new Tuple<string, ImageTagSet, bool, object> (fileName, ImageTagSet.Empty, false, img));
				var list = new List<Tuple<Image,string[]>> ();
				foreach (var imageGroup in altImages.GroupBy (t => t.Item2)) {
					Image altImg;
					if (ext == ".9.png")
						altImg = CreateComposedNinePatch (toolkit, imageGroup);
					else {
						var ib = toolkit.ImageBackendHandler.CreateMultiResolutionImage (imageGroup.Select (i => i.Item4));
						altImg = loader.WrapImage (fileName, imageGroup.Key, ib, reqSize);
					}
					list.Add (new Tuple<Image,string[]> (altImg, imageGroup.Key.AsArray));
				}
				if (list.Count == 1)
					return list [0].Item1;
				else {
					return new ThemedImage (list, reqSize);
				}
			} else {
				var res = loader.WrapImage (fileName, ImageTagSet.Empty, img, reqSize);
				if (ext == ".9.png")
					res = new NinePatchImage (res.ToBitmap ());
				return res;
			}
		}

		static bool ParseImageHints (string baseName, string fileName, string ext, out int scale, out ImageTagSet tags)
		{
			scale = 1;
			tags = ImageTagSet.Empty;

			if (!fileName.StartsWith (baseName, StringComparison.Ordinal) || fileName.Length <= baseName.Length + 1 || (fileName [baseName.Length] != '@' && fileName [baseName.Length] != '~'))
				return false;

			fileName = fileName.Substring (0, fileName.Length - ext.Length);

			int i = baseName.Length;
			if (fileName [i] == '~') {
				// For example: foo~dark@2x
				i++;
				var i2 = fileName.IndexOf ('@', i);
				if (i2 != -1) {
					int i3 = fileName.IndexOf ('x', i2 + 2);
					if (i3 == -1 || !int.TryParse (fileName.Substring (i2 + 1, i3 - i2 - 1), out scale))
						return false;
				} else
					i2 = fileName.Length;
				tags = new ImageTagSet (fileName.Substring (i, i2 - i));
				return true;
			}
			else {
				// For example: foo@2x~dark
				i++;
				var i2 = fileName.IndexOf ('~', i + 1);
				if (i2 == -1)
					i2 = fileName.Length;

				i2--;
				if (i2 < 0 || fileName [i2] != 'x')
					return false;
				
				var s = fileName.Substring (i, i2 - i);
				if (!int.TryParse (s, out scale)) {
					tags = null;
					return false;
				}
				if (i2 + 2 < fileName.Length)
					tags = new ImageTagSet (fileName.Substring (i2 + 2));
				return true;
			}
		}

		public static Image CreateMultiSizeIcon (IEnumerable<Image> images)
		{
			if (Toolkit.CurrentEngine == null)
				throw new ToolkitNotInitializedException ();

			var allImages = images.ToArray ();

			if (allImages.Length == 1)
				return allImages [0];

			if (allImages.Any (i => i is ThemedImage)) {
				// If one of the images is themed, then the whole resulting image will be themed.
				// To create the new image, we group images with the same theme but different size, and we create a multi-size icon for those.
				// The resulting image is the combination of those multi-size icons.
				var allThemes = allImages.OfType<ThemedImage> ().SelectMany (i => i.Images).Select (i => new ImageTagSet (i.Item2)).Distinct ().ToArray ();
				List<Tuple<Image, string []>> newImages = new List<Tuple<Image, string []>> ();
				foreach (var ts in allThemes) {
					List<Image> multiSizeImages = new List<Image> ();
					foreach (var i in allImages) {
						if (i is ThemedImage)
							multiSizeImages.Add (((ThemedImage)i).GetImage (ts.AsArray));
						else
							multiSizeImages.Add (i);
					}
					var img = CreateMultiSizeIcon (multiSizeImages);
					newImages.Add (new Tuple<Image, string []> (img, ts.AsArray));
				}
				return new ThemedImage (newImages);
			} else {
				var img = new Image (Toolkit.CurrentEngine.ImageBackendHandler.CreateMultiSizeIcon (allImages.Select (i => i.GetBackend ())));

				if (allImages.All (i => i.NativeRef.HasNativeSource)) {
					var sources = allImages.Select (i => i.NativeRef.NativeSource).ToArray ();
					img.NativeRef.SetSources (sources);
				}
				return img;
			}
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

			var loader = new FileImageLoader (toolkit);
			return LoadImage (loader, file, null);
		}

		static Image CreateComposedNinePatch (Toolkit toolkit, IEnumerable<Tuple<string,ImageTagSet,bool,object>> altImages)
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
						toolkit.ImageBackendHandler.Dispose (fi.Item4);
						continue;
					}
				}
				npImage.AddFrame (new Image (fi.Item4, toolkit).ToBitmap (), scaleFactor);
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

		public static Image FromCustomLoader (IImageLoader loader, string fileName)
		{
			return FromCustomLoader (loader, fileName, null);
		}

		internal static Image FromCustomLoader (IImageLoader loader, string fileName, ImageTagSet tags)
		{
			var toolkit = Toolkit.CurrentEngine;
			if (toolkit == null)
				throw new ToolkitNotInitializedException ();
			
			var ld = new StreamImageLoader (toolkit, loader);
			return LoadImage (ld, fileName, tags);
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
		/// Retuns a copy of the image with a set of specific styles
		/// </summary>
		/// <returns>A new image with the new styles</returns>
		/// <param name="styles">Styles to apply</param>
		/// <remarks>
		/// This is a lightweight operation.
		/// The method doesn't make a copy of the image data.
		/// </remarks>
		public Image WithStyles (params string[] styles)
		{
			return new Image (this) {
				styles = StyleSet.Empty.AddRange (styles)
			};
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
			var idesc = new ImageDescription {
				Alpha = requestedAlpha,
				Size = s,
				Styles = styles,
				Backend = Backend
			};
			var bmp = ToolkitEngine.ImageBackendHandler.ConvertToBitmap (idesc, scaleFactor, format);
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

			public IImageLoader CustomImageLoader;

			public ImageDrawCallback DrawCallback;

			public string StockId;

			public ImageTagSet Tags;
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

		public void SetFileSource (string file, ImageTagSet tags)
		{
			sources = new [] { 
				new NativeImageSource {
					Source = file,
					Tags = tags
				}
			};
		}

		public void SetResourceSource (Assembly asm, string name, ImageTagSet tags)
		{
			sources = new [] { 
				new NativeImageSource {
					Source = name,
					ResourceAssembly = asm,
					Tags = tags
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

		public void SetCustomLoaderSource (IImageLoader imageLoader, string fileName, ImageTagSet tags)
		{
			sources = new [] { 
				new NativeImageSource {
					CustomImageLoader = imageLoader,
					Source = fileName,
					Tags = tags
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
			if (Toolkit == targetToolkit)
				return this;
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
								var backends = new object [streams.Length];
								for (int n = 0; n < backends.Length; n++) {
									backends [n] = targetToolkit.ImageBackendHandler.LoadFromStream (streams [n]);
								}
								newBackend = targetToolkit.ImageBackendHandler.CreateMultiResolutionImage (backends);
							}
						} finally {
							foreach (var st in streams)
								st.Dispose ();
						}
					} else if (s.CustomImageLoader != null) {
						targetToolkit.Invoke (() => newBackend = Image.FromCustomLoader (s.CustomImageLoader, s.Source, s.Tags).GetBackend());
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

	class ImageTagSet
	{
		string tags;
		string[] tagsArray;

		public static readonly ImageTagSet Empty = new ImageTagSet (new string[0]);

		public ImageTagSet (string [] tagsArray)
		{
			this.tagsArray = tagsArray;
			Array.Sort (tagsArray);
		}

		public bool IsEmpty {
			get {
				return tagsArray.Length == 0;
			}
		}

		public ImageTagSet (string tags)
		{
			tagsArray = tags.Split (new [] { '~' }, StringSplitOptions.RemoveEmptyEntries);
			Array.Sort (AsArray);
		}

		public string AsString {
			get {
				if (tags == null)
					tags = string.Join ("~", tagsArray);
				return tags;
			}
		}

		public string [] AsArray {
			get {
				return tagsArray;
			}
		}

		public override bool Equals (object obj)
		{
			var other = obj as ImageTagSet;
			if (other == null || tagsArray.Length != other.tagsArray.Length)
				return false;
			for (int n = 0; n < tagsArray.Length; n++)
				if (tagsArray [n] != other.tagsArray [n])
					return false;
			return true;
		}

		public override int GetHashCode ()
		{
			unchecked {
				int c = 0;
				foreach (var s in tagsArray)
					c %= s.GetHashCode ();
				return c;
			}
		}
	}

	abstract class ImageLoader
	{
		public abstract object LoadImage (string fileName);
		public abstract IEnumerable<string> GetAlternativeFiles (string fileName, string baseName, string ext);
		public abstract Image WrapImage (string fileName, ImageTagSet tags, object img, Size reqSize);
	}

	class ResourceImageLoader : ImageLoader
	{
		Assembly assembly;
		Toolkit toolkit;

		public ResourceImageLoader (Toolkit toolkit, Assembly assembly)
		{
			this.assembly = assembly;
			this.toolkit = toolkit;
		}

		public override object LoadImage (string fileName)
		{
			var img = toolkit.ImageBackendHandler.LoadFromResource (assembly, fileName);
			if (img == null)
				throw new InvalidOperationException ("Resource not found: " + fileName);
			return img;
		}

		public override IEnumerable<string> GetAlternativeFiles (string fileName, string baseName, string ext)
		{
			return assembly.GetManifestResourceNames ().Where (f =>
				f.StartsWith (baseName, StringComparison.Ordinal) &&
				f.EndsWith (ext, StringComparison.Ordinal));
		}

		public override Image WrapImage (string fileName, ImageTagSet tags, object img, Size reqSize)
		{
			var res = new Image (img, toolkit) {
				requestedSize = reqSize
			};
			res.NativeRef.SetResourceSource (assembly, fileName, tags);
			return res;
		}
	}

	class FileImageLoader : ImageLoader
	{
		Toolkit toolkit;

		public FileImageLoader (Toolkit toolkit)
		{
			this.toolkit = toolkit;
		}

		public override object LoadImage (string fileName)
		{
			var img = toolkit.ImageBackendHandler.LoadFromFile (fileName);
			if (img == null)
				throw new InvalidOperationException ("File not found: " + fileName);
			return img;
		}

		public override IEnumerable<string> GetAlternativeFiles (string fileName, string baseName, string ext)
		{
			if (!Context.RegisteredStyles.Any ()) {
				foreach (var s in Image.SupportedScales) {
					var fn = baseName + "@" + s + "x" + ext;
					if (File.Exists (fn))
						yield return fn;
				}
			} else {
				if (Path.DirectorySeparatorChar == '\\') // windows)
					baseName = Path.GetFileName (baseName);
				var files = Directory.GetFiles (Path.GetDirectoryName (fileName), baseName + "*" + ext);
				foreach (var f in files)
					yield return f;
			}
		}

		public override Image WrapImage (string fileName, ImageTagSet tags, object img, Size reqSize)
		{
			var res = new Image (img, toolkit) {
				requestedSize = reqSize
			};
			res.NativeRef.SetFileSource (fileName, tags);
			return res;
		}
	}

	class StreamImageLoader : ImageLoader
	{
		IImageLoader loader;
		Toolkit toolkit;

		public StreamImageLoader (Toolkit toolkit, IImageLoader loader)
		{
			this.toolkit = toolkit;
			this.loader = loader;
		}

		public override IEnumerable<string> GetAlternativeFiles (string fileName, string baseName, string ext)
		{
			return loader.GetAlternativeFiles (fileName, baseName, ext);
		}

		public override object LoadImage (string fileName)
		{
			using (var s = loader.LoadImage (fileName))
				return toolkit.ImageBackendHandler.LoadFromStream (s);
		}

		public override Image WrapImage (string fileName, ImageTagSet tags, object img, Size reqSize)
		{
			var res = new Image (img, toolkit) {
				requestedSize = reqSize
			};
			var ld = loader;
			res.NativeRef.SetCustomLoaderSource (loader, fileName, tags);
			return res;
		}
	}

	
}

