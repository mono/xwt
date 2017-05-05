//
// GtkQuartz.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corporation
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
using ObjCRuntime;

namespace Xwt.Mac
{
	static class GtkQuartz
	{
		const string LIBQUARTZ = "libgtk-quartz-2.0";

		static System.Reflection.PropertyInfo gdkWindowHandleProp;

		public static NSView GetView (object widget)
		{
			if (widget == null)
				throw new ArgumentNullException (nameof (widget));

			var gdkWindowProp = widget.GetType ().GetProperty ("GdkWindow");
			if (gdkWindowProp?.PropertyType.FullName != "Gdk.Window")
				throw new ArgumentException ("Wigdet is not a valid Gtk.Windget", nameof (widget));

			var gdkWindow = gdkWindowProp.GetValue (widget, null);

			if (gdkWindow == null)
				throw new InvalidOperationException ("Wigdet is not realized");

			if (gdkWindowHandleProp == null)
				gdkWindowHandleProp = gdkWindow.GetType ().GetProperty ("Handle");

			if (gdkWindowHandleProp?.PropertyType != typeof (IntPtr))
				throw new ArgumentException ("Wigdet is not a valid Gtk.Windget", nameof (widget));

			var gdkPtr = gdkWindowHandleProp.GetValue (gdkWindow, null);

			var ptr = IntPtr.Zero;
			if (gdkPtr != null)
				try {
					ptr = gdk_quartz_window_get_nsview ((IntPtr)gdkPtr);
				} catch (Exception ex) {
					throw new InvalidOperationException ("Unsupported Gtk version", ex);
				}
			if (ptr == IntPtr.Zero)
				return null;
			return Runtime.GetNSObject (ptr) as NSView;
		}

		[DllImport (LIBQUARTZ)]
		static extern IntPtr gdk_quartz_window_get_nsview (IntPtr window);
	}
}
