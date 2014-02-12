//
// WebViewBackend.cs
//
// Author:
//       Cody Russell <cody@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc.
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
using System.Threading;
using System.Runtime.InteropServices;
using SWF = System.Windows.Forms;
using Xwt.GtkBackend;
using Xwt.Backends;
using GTK = Gtk;

namespace Xwt.Gtk.Windows
{
	public class WebViewBackend : WidgetBackend, IWebViewBackend
	{
		SWF.WebBrowser view;
		string url;
		GTK.Socket socket;

		[DllImportAttribute("user32.dll", EntryPoint = "SetParent")]
		internal static extern System.IntPtr SetParent([InAttribute] System.IntPtr hwndChild, [InAttribute] System.IntPtr hwndNewParent);

		public override void Initialize ()
		{
			base.Initialize ();

			socket = new GTK.Socket ();
			Widget = socket;

			GLib.Timeout.Add (50, delegate {
				// We need to wait until after this widget has been parented.
				Widget.Realize ();
				Widget.Show ();

				var size = new System.Drawing.Size (Widget.WidthRequest, Widget.HeightRequest);

				view = new SWF.WebBrowser ();
				view.Size = size;
				var browser_handle = view.Handle;
				IntPtr window_handle = (IntPtr)socket.Id;
				SetParent (browser_handle, window_handle);
				if (url != null)
					view.Navigate (url);

				return false;
			});
		}

		public string Url {
			get { return url; }
			set {
				url = value;
				if (view != null)
					view.Navigate (url);
			}
		}
	}
}

