//
// GtkQuartz.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2016 Vsevolod Kukol
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
using System.Runtime.InteropServices;
using AppKit;

namespace Xwt.Gtk.Mac
{
	public static class GtkQuartz
	{
		const string LIBQUARTZ = "libgtk-quartz-2.0.dylib";

		public static NSWindow GetWindow (global::Gtk.Window window)
		{
			if (window.GdkWindow == null)
				return null;
			var ptr = gdk_quartz_window_get_nswindow (window.GdkWindow.Handle);
			if (ptr == IntPtr.Zero)
				return null;
			return ObjCRuntime.Runtime.GetNSObject<NSWindow> (ptr);
		}

		public static NSView GetView (global::Gtk.Widget widget)
		{
			var ptr = gdk_quartz_window_get_nsview (widget.GdkWindow.Handle);
			if (ptr == IntPtr.Zero)
				return null;
			return ObjCRuntime.Runtime.GetNSObject<NSView> (ptr);
		}

		[DllImport (LIBQUARTZ)]
		static extern IntPtr gdk_quartz_window_get_nsview (IntPtr window);

		[DllImport (LIBQUARTZ)]
		static extern IntPtr gdk_quartz_window_get_nswindow (IntPtr window);
	}
}

