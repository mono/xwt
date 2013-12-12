//
// GtkSurfaceBackendHandler.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin, Inc (http://www.xamarin.com)
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
using System.Runtime.InteropServices;
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	public class GtkSurfaceBackendHandler: SurfaceBackendHandler
	{
		double scaleFactor;

		public GtkSurfaceBackendHandler ()
		{
		}

		#region implemented abstract members of SurfaceBackendHandler

		public override object CreateSurface (double width, double height, double scaleFactor)
		{
			this.scaleFactor = scaleFactor;
			// There is a bug in Cairo for OSX right now that prevents creating additional accellerated surfaces.
			if (Platform.IsMac) {
				return new QuartzSurface (Cairo.Format.ARGB32, (int)width, (int)height);
			} else {
				return new Cairo.ImageSurface (Cairo.Format.ARGB32, (int)width, (int)height);
			}
		}

		public override object CreateSurfaceCompatibleWithWidget (object widgetBackend, double width, double height)
		{
			var widget = ((WidgetBackend)widgetBackend).Widget;
			scaleFactor = Util.GetScaleFactor (widget);

			// There is a bug in Cairo for OSX right now that prevents creating additional accellerated surfaces.
			if (Platform.IsMac) {
				return new QuartzSurface (Cairo.Format.ARGB32, (int)width, (int)height);
			} else if (Platform.IsWindows) {
				using (var similar = Gdk.CairoHelper.Create (widget.GdkWindow))
				using (var target = similar.GetTarget ()) {
					return target.CreateSimilar (Cairo.Content.ColorAlpha, (int)width, (int)height);
				}
			} else {
				return new Cairo.ImageSurface (Cairo.Format.ARGB32, (int)width, (int)height);
			}
		}

		public override object CreateSurfaceCompatibleWithSurface (object surfaceBackend, double width, double height)
		{
			// There is a bug in Cairo for OSX right now that prevents creating additional accellerated surfaces.
			if (Platform.IsMac) {
				return new QuartzSurface (Cairo.Format.ARGB32, (int)width, (int)height);
			} else if (Platform.IsWindows) {
				var surface = (Cairo.Surface)surfaceBackend;
				return surface.CreateSimilar (Cairo.Content.ColorAlpha, (int)width, (int)height);
			} else {
				return new Cairo.ImageSurface (Cairo.Format.ARGB32, (int)width, (int)height);
			}
		}

		public override object CreateContext (object backend)
		{
			var sf = (Cairo.Surface)backend;
			CairoContextBackend ctx = new CairoContextBackend (scaleFactor);
			ctx.Context = new Cairo.Context (sf);
			return ctx;
		}

		#endregion
	}


	public class QuartzSurface : Cairo.Surface
	{
		const string CoreGraphics = "/System/Library/Frameworks/ApplicationServices.framework/Frameworks/CoreGraphics.framework/CoreGraphics";

		[DllImport (GtkInterop.LIBCAIRO, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr cairo_quartz_surface_create (Cairo.Format format, uint width, uint height);

		[DllImport (GtkInterop.LIBCAIRO, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr cairo_quartz_surface_get_cg_context (IntPtr surface);

		[DllImport (GtkInterop.LIBCAIRO, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr cairo_get_target (IntPtr context);

		[DllImport (CoreGraphics, EntryPoint="CGContextConvertRectToDeviceSpace", CallingConvention = CallingConvention.Cdecl)]
		static extern CGRect32 CGContextConvertRectToDeviceSpace32 (IntPtr contextRef, CGRect32 cgrect);

		[DllImport (CoreGraphics, EntryPoint="CGContextConvertRectToDeviceSpace", CallingConvention = CallingConvention.Cdecl)]
		static extern CGRect64 CGContextConvertRectToDeviceSpace64 (IntPtr contextRef, CGRect64 cgrect);

		public static double GetRetinaScale (Cairo.Context context)  {
			if (!Platform.IsMac)
				return 1;

			// Use C call to avoid dispose bug in cairo bindings for OSX
			var cgContext = cairo_quartz_surface_get_cg_context (cairo_get_target (context.Handle));

			if (IntPtr.Size == 8)
				return CGContextConvertRectToDeviceSpace64 (cgContext, CGRect64.Unit).X;

			return CGContextConvertRectToDeviceSpace32 (cgContext, CGRect32.Unit).X;
		}

		struct CGRect32
		{
			public CGRect32 (float x, float y, float width, float height)
			{
				this.X = x;
				this.Y = y;
				this.Width = width;
				this.Height = height;
			}

			public float X, Y, Width, Height;

			public static CGRect32 Unit {
				get {
					return new CGRect32 (1, 1, 1, 1);
				}
			}
		}

		struct CGRect64
		{
			public CGRect64 (double x, double y, double width, double height)
			{
				this.X = x;
				this.Y = y;
				this.Width = width;
				this.Height = height;
			}

			public double X, Y, Width, Height;

			public static CGRect64 Unit {
				get {
					return new CGRect64 (1, 1, 1, 1);
				}
			}
		}

		public QuartzSurface (Cairo.Format format, int width, int height)
			: base (cairo_quartz_surface_create (format, (uint)width, (uint)height), true)
		{
		}
	}
}

