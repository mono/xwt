//
// EmbeddedWidgetBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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

namespace Xwt.GtkBackend
{
	public class EmbeddedWidgetBackend: WidgetBackend, IEmbeddedWidgetBackend
	{
		public EmbeddedWidgetBackend ()
		{
		}

		public void SetContent (object nativeWidget)
		{
			if (nativeWidget is Gtk.Widget) {
				Widget = (Gtk.Widget)nativeWidget;
				return;
			}

			// Check if it is an NSView
			Type nsView = Type.GetType ("AppKit.NSView, Xamarin.Mac", false);
			if (nsView != null && nsView.IsInstanceOfType (nativeWidget)) {
				Widget = GtkMacInterop.NSViewToGtkWidget (nativeWidget);
				Widget.Show ();
				return;
			}

			Type frameworkElement = Type.GetType ("System.Windows.FrameworkElement, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", false);
			Type windowsHelper = Type.GetType ("Xwt.Gtk.Windows.GtkWin32Interop, Xwt.Gtk.Windows", false);
			if (frameworkElement != null && windowsHelper != null && frameworkElement.IsInstanceOfType (nativeWidget)) {
				var factoryMethod = windowsHelper.GetMethod ("ControlToGtkWidget",
				                                             System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
				Widget = (Gtk.Widget)factoryMethod.Invoke (null, new [] { nativeWidget });
				Widget.Show ();
				return;
			}
		}
	}
}

