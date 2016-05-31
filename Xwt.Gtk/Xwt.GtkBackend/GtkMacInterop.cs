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
using System.Linq;

namespace Xwt.GtkBackend
{
	public class GtkMacInterop
	{
		const string LibGdk = "libgdk-quartz-2.0.dylib";
		const string LibGtk = "libgtk-quartz-2.0";
		
		[System.Runtime.InteropServices.DllImport (LibGtk)]
		extern static IntPtr gtk_ns_view_new (IntPtr nsview);
		
		public static Gtk.Widget NSViewToGtkWidget (object view)
		{
			var prop = view.GetType ().GetProperty ("Handle");
			var handle = prop.GetValue (view, null);
			return new Gtk.Widget (gtk_ns_view_new ((IntPtr)handle));
		}

		public static Gtk.Window GetGtkWindow (object window)
		{
			if (window == null)
				return null;
			
			var prop = window.GetType ().GetProperty ("Handle");
			var handle = prop.GetValue (window, null);
			if (handle is IntPtr) {
				var toplevels = Gtk.Window.ListToplevels ();
				return toplevels.FirstOrDefault (w => w.IsRealized && GtkWorkarounds.GetGtkWindowNativeHandle (w) == (IntPtr)handle);
			}
			return null;
		}
	}
}

