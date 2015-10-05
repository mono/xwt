//
// FontSelectorBackend.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2015 Vsevolod Kukol
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

namespace Xwt.GtkBackend
{
	public class FontSelectorBackend: WidgetBackend, IFontSelectorBackend
	{
		public FontSelectorBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = new GtkFontSelection ();
			base.Widget.Show ();
		}

		protected new GtkFontSelection Widget {
			get { return (GtkFontSelection)base.Widget; }
			set { base.Widget = value; }
		}

		protected new IFontSelectorEventSink EventSink {
			get { return (IFontSelectorEventSink)base.EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is FontSelectorEvent) {
				switch ((FontSelectorEvent)eventId) {
					case FontSelectorEvent.FontChanged: Widget.FontChanged += HandleFontChanged; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is FontSelectorEvent) {
				switch ((FontSelectorEvent)eventId) {
					case FontSelectorEvent.FontChanged: Widget.FontChanged -= HandleFontChanged; break;
				}
			}
		}

		void HandleFontChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnFontChanged ();
			});
		}

		public Xwt.Drawing.Font SelectedFont {
			get {
				return Xwt.Drawing.Font.FromName (Widget.FontName);
			}
			set {
				Widget.FontName = value.ToString ();
			}
		}

		public string PreviewText {
			get { return Widget.PreviewText; }
			set { Widget.PreviewText = value; }
		}
	}

	public class GtkFontSelection: Gtk.FontSelection
	{
		public event EventHandler FontChanged;

		public GtkFontSelection()
		{
			// there is no special font changed event
			// check whether the font changed on every redraw of the preview entry
			var entry = GetPreviewEntry();
			#if XWT_GTK3
			entry.Drawn += HandleExposeEvent;
			#else
			entry.ExposeEvent += HandleExposeEvent;
			#endif
		}

		string cachedFontName = String.Empty;
		void HandleExposeEvent (object o, EventArgs args)
		{
			if (cachedFontName !=  FontName) {
				cachedFontName = FontName;
				if (FontChanged != null)
					FontChanged (this, new EventArgs ());
			}
		}

		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr gtk_font_selection_get_preview_entry (IntPtr raw);

		Gtk.Entry GetPreviewEntry ()
		{
			IntPtr intPtr = gtk_font_selection_get_preview_entry (base.Handle);
			Gtk.Entry result;
			if (intPtr == IntPtr.Zero)
				result = null;
			else
				result = (Gtk.Entry)GLib.Object.GetObject (intPtr);
			return result;
		}
	}
}

