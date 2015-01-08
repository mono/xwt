//
// Gtk3Extensions.cs
//
// Author:
//       Vsevolod Kukol <v.kukol@rubologic.de>
//
// Copyright (c) 2014 Vsevolod Kukol
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

using Xwt.Backends;
using Xwt.CairoBackend;
using System;
using System.Runtime.InteropServices;
using Gtk;

namespace Xwt.GtkBackend
{
	public static class Gtk3Extensions
	{
		public static void SetHasWindow (this Gtk.Widget widget, bool value)
		{
			widget.HasWindow = value;
		}

		public static bool GetHasWindow (this Gtk.Widget widget)
		{
			return widget.HasWindow;
		}

		public static void SetAppPaintable (this Gtk.Widget widget, bool value)
		{
			widget.AppPaintable = value;
		}

		public static void SetStateActive(this Gtk.Widget widget)
		{
			widget.SetStateFlags(Gtk.StateFlags.Active, true);
		}

		public static void SetStateNormal(this Gtk.Widget widget)
		{
			widget.SetStateFlags(Gtk.StateFlags.Normal, true);
		}

		[DllImport (GtkInterop.LIBGDK)]
		static extern IntPtr gdk_pixbuf_get_from_window(IntPtr win, int src_x, int src_y, int width, int height);

		public static Gdk.Pixbuf ToPixbuf (this Gdk.Window window, int src_x, int src_y, int width, int height)
		{
			IntPtr raw_ret = gdk_pixbuf_get_from_window(window.Handle, src_x, src_y, width, height);
			Gdk.Pixbuf ret;
			if (raw_ret == IntPtr.Zero)
				ret = null;
			else
				ret = (Gdk.Pixbuf) GLib.Object.GetObject(raw_ret);
			return ret;
		}

		public static Gtk.CellRenderer[] GetCellRenderers (this Gtk.TreeViewColumn column)
		{
			return column.Cells;
		}

		public static Gdk.DragAction GetSelectedAction (this Gdk.DragContext context)
		{
			return context.SelectedAction;
		}

		public static void AddContent (this Gtk.Dialog dialog, Gtk.Widget widget)
		{
			dialog.ContentArea.Add (widget);
		}

		public static void SetContentSpacing (this Gtk.Dialog dialog, int spacing)
		{
			dialog.ContentArea.Spacing = spacing;
		}

		public static void GetSize (this Gdk.Window window, out int w, out int h)
		{
			w = window.Width;
			h = window.Height;
		}

		public static void SetTextColumn (this Gtk.ComboBox comboBox, int column)
		{
			comboBox.EntryTextColumn = column;
		}

		public static void FixContainerLeak (this Gtk.Container c)
		{
			// gtk3 is not affected by the container leak bug and there is no marker
			// method, so nothing to do.
			return;
		}

		public static void SetBackgroundColor (this Gtk.Widget widget, Xwt.Drawing.Color color)
		{
			widget.SetBackgroundColor (Gtk.StateFlags.Normal, color);
		}

		public static void SetChildBackgroundColor (this Gtk.Container container, Xwt.Drawing.Color color)
		{
			foreach (var widget in container.Children)
				widget.SetBackgroundColor (Gtk.StateFlags.Normal, color);
		}

		public static void SetBackgroundColor (this Gtk.Widget widget, Gtk.StateType state, Xwt.Drawing.Color color)
		{
			widget.SetBackgroundColor (state.ToGtk3StateFlags (), color);
		}

		public static void SetBackgroundColor (this Gtk.Widget widget, Gtk.StateFlags state, Xwt.Drawing.Color color)
		{
			widget.OverrideBackgroundColor (state, color.ToGtkRgbaValue ());
		}

		public static Gtk.StateFlags ToGtk3StateFlags (this Gtk.StateType state)
		{
			switch (state)
			{
				case Gtk.StateType.Active:
					return Gtk.StateFlags.Active;
				case Gtk.StateType.Prelight:
					return Gtk.StateFlags.Prelight;
				case Gtk.StateType.Insensitive:
					return Gtk.StateFlags.Insensitive;
				case Gtk.StateType.Focused:
					return Gtk.StateFlags.Active;
				case Gtk.StateType.Inconsistent:
					return Gtk.StateFlags.Normal;
				case Gtk.StateType.Selected:
					return Gtk.StateFlags.Selected;
			}
			return Gtk.StateFlags.Normal;
		}

		public static void RenderPlaceholderText (this Gtk.TextView textView, Cairo.Context cr, string placeHolderText, ref Pango.Layout layout)
		{
			if (textView.Buffer.Text.Length > 0)
				return;
			float xalign = 0;
			switch (textView.Justification) {
				case Gtk.Justification.Center: xalign = 0.5f; break;
				case Gtk.Justification.Right: xalign = 1; break;
			}
			RenderPlaceholderText_internal (textView, cr, placeHolderText, ref layout, xalign, 0.0f, 3, 0);
		}

		static void RenderPlaceholderText_internal (Gtk.Widget widget, Cairo.Context cr, string placeHolderText, ref Pango.Layout layout, float xalign, float yalign, int xpad, int ypad)
		{
			if (layout == null) {
				layout = new Pango.Layout (widget.PangoContext);
				layout.FontDescription = widget.PangoContext.FontDescription.Copy ();
			}
			int width, height;
			layout.SetText (placeHolderText);
			layout.GetPixelSize (out width, out height);
			int x = xpad + (int)((widget.AllocatedWidth - width) * xalign);
			int y = ypad + (int)((widget.AllocatedHeight - height) * yalign);
			Xwt.Drawing.Color color_a = widget.StyleContext.GetBackgroundColor (Gtk.StateFlags.Normal).ToXwtValue ();
			Xwt.Drawing.Color color_b = widget.StyleContext.GetColor (Gtk.StateFlags.Normal).ToXwtValue ();
			cr.SetSourceColor (color_b.BlendWith (color_a, 0.5).ToCairoColor ());
			cr.MoveTo (x, y);
			Pango.CairoHelper.ShowLayout (cr, layout);
		}
	}
}

