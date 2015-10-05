//
// Gtk3FontChooserDialog.cs
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
using System.Runtime.InteropServices;

namespace Xwt.GtkBackend
{
	public class Gtk3FontChooserDialog : Gtk.Dialog
	{

		[DllImport(GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_font_chooser_dialog_new(IntPtr title, IntPtr parent);

		[DllImport(GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_font_chooser_widget_get_type();

		public Gtk3FontChooserDialog (IntPtr raw) : base(raw)
		{
		}

		public Gtk3FontChooserDialog (string title, Gtk.Window parent = null) : base (IntPtr.Zero)
		{
			IntPtr ptitle = GLib.Marshaller.StringToPtrGStrdup (title);
			Raw = gtk_font_chooser_dialog_new(ptitle, parent == null ? IntPtr.Zero : parent.Handle);
			GLib.Marshaller.Free (ptitle);

			using (GLib.Value val = new GLib.Value (true)) {
				SetProperty ("show-preview-entry", val);
			}
		}

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = gtk_font_chooser_widget_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		public string FontName {
			get  {
				using (GLib.Value property = GetProperty ("font")) {
					string result = (string)property;
					return result;
				}
			}
			set  {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("font", val);
				}
			}
		}

		public void SetFontName (string fontname)
		{
			FontName = fontname;
		}

		public string PreviewText {
			get {
				using (GLib.Value property = GetProperty ("preview-text")) {
					string result = (string)property;
					return result;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("preview-text", val);
				}
			}
		}

		[GLib.Signal("font-activated")]
		public event EventHandler FontActivated {
			add {
				AddSignalHandler ("font-activated", value, typeof (EventArgs));
			}
			remove {
				RemoveSignalHandler ("font-activated", value);
			}
		}
	}
}
