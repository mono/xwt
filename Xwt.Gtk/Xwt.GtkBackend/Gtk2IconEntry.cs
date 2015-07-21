//
// Gtk2IconEntry.cs
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
using Xwt.GtkBackend;

namespace Xwt.GtkBackend
{
	public class GtkEntry : Gtk.Entry
	{
		enum EntryIconPosition {
			Primary,
			Secondary,
		}

		static GtkEntry ()
		{
			var tIconPosition = new GLib.GType (gtk_entry_icon_position_get_type ());
			if (GLib.GType.LookupType (tIconPosition.Val) == null)
				GLib.GType.Register(tIconPosition, typeof(EntryIconPosition));
		}

		public string PrimaryIconName {
			get {
				using (GLib.Value val = GetProperty ("primary-icon-name")) {
					return (string)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("primary-icon-name", val);
				}
			}
		}

		public bool PrimaryIconActivatable {
			get {
				using (GLib.Value val = GetProperty ("primary-icon-activatable")) {
					return (bool)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("primary-icon-activatable", val);
				}
			}
		}

		public bool PrimaryIconSensitive {
			get {
				using (GLib.Value val = GetProperty ("primary-icon-sensitive")) {
					return (bool)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("primary-icon-sensitive", val);
				}
			}
		}

		public string PrimaryIconTooltipText {
			get {
				using (GLib.Value val = GetProperty ("primary-icon-tooltip-text")) {
					return (string)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("primary-icon-tooltip-text", val);
				}
			}
		}

		public string PrimaryIconTooltipMarkup {
			get {
				using (GLib.Value val = GetProperty ("primary-icon-tooltip-markup")) {
					return (string)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("primary-icon-tooltip-markup", val);
				}
			}
		}

		public Gdk.Pixbuf PrimaryIconPixbuf {
			get {
				using (GLib.Value val = GetProperty ("primary-icon-pixbuf")) {
					return (Gdk.Pixbuf)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("primary-icon-pixbuf", val);
				}
			}
		}

		public string SecondaryIconName {
			get {
				using (GLib.Value val = GetProperty ("secondary-icon-name")) {
					return (string)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("secondary-icon-name", val);
				}
			}
		}

		public bool SecondaryIconActivatable {
			get {
				using (GLib.Value val = GetProperty ("secondary-icon-activatable")) {
					return (bool)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("secondary-icon-activatable", val);
				}
			}
		}

		public bool SecondaryIconSensitive {
			get {
				using (GLib.Value val = GetProperty ("secondary-icon-sensitive")) {
					return (bool)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("secondary-icon-sensitive", val);
				}
			}
		}

		public string SecondaryIconTooltipText {
			get {
				using (GLib.Value val = GetProperty ("secondary-icon-tooltip-text")) {
					return (string)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("secondary-icon-tooltip-text", val);
				}
			}
		}

		public string SecondaryIconTooltipMarkup {
			get {
				using (GLib.Value val = GetProperty ("secondary-icon-tooltip-markup")) {
					return (string)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("secondary-icon-tooltip-markup", val);
				}
			}
		}

		public Gdk.Pixbuf SecondaryIconPixbuf {
			get {
				using (GLib.Value val = GetProperty ("secondary-icon-pixbuf")) {
					return (Gdk.Pixbuf)val;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					SetProperty ("secondary-icon-pixbuf", val);
				}
			}
		}

		[GLib.Signal("icon-press")]
		public event EventHandler<GLib.SignalArgs> IconPress {
			add {
				this.AddSignalHandler ("icon-press", value, typeof (GLib.SignalArgs));
			}
			remove {
				this.RemoveSignalHandler ("icon-press", value);
			}
		}

		[GLib.Signal ("icon-release")]
		public event EventHandler<GLib.SignalArgs> IconRelease {
			add {
				this.AddSignalHandler ("icon-release", value, typeof (GLib.SignalArgs));
			}
			remove {
				this.RemoveSignalHandler ("icon-release", value);
			}
		}

		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_entry_icon_position_get_type ();
	}
}
