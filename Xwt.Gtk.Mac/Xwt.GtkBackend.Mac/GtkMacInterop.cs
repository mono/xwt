//
// GtkMacInterop.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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

namespace Xwt.GtkBackend.Mac
{
	public static class GtkMacInterop
	{
		const string LibGdk = "libgdk-quartz-2.0.dylib";
		const string LibGtk = "libgtk-quartz-2.0";

		[System.Runtime.InteropServices.DllImport (LibGdk)]
		static extern IntPtr gdk_quartz_window_get_nsview (IntPtr gdkwindow);

		[System.Runtime.InteropServices.DllImport (LibGdk)]
		static extern IntPtr gdk_quartz_window_get_nswindow (IntPtr gdkwindow);

		[System.Runtime.InteropServices.DllImport (LibGtk)]
		extern static IntPtr gtk_ns_view_new (IntPtr nsview);

		public static MonoMac.AppKit.NSView GetNSViewFromGdkWindow (Gdk.Window window)
		{
			if (!Platform.IsMac || window == null)
				return null;
			IntPtr nsView = gdk_quartz_window_get_nsview (window.Handle);
			return new MonoMac.AppKit.NSView (nsView);
		}

		[System.Runtime.InteropServices.DllImport (LibGtk)]
		static extern void gtk_widget_set_realized (IntPtr gtkwidget, bool realized);

		public static void SetRealized (Gtk.Widget widget, bool realized)
		{
			gtk_widget_set_realized (widget.Handle, realized);
		}

		public static IntPtr NSViewToGtkWidgetPtr (MonoMac.AppKit.NSView view)
		{
			return gtk_ns_view_new (view.Handle);
		}
	}
}