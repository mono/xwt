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

		[DllImport (GtkInterop.LIBGDK, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gdk_pixbuf_get_from_surface (IntPtr surface, int src_x, int src_y, int width, int height);

		public static Gdk.Pixbuf GetFromSurface (Cairo.Surface surface, int src_x, int src_y, int width, int height)
		{
			IntPtr raw_ret = gdk_pixbuf_get_from_surface (surface.Handle, src_x, src_y, width, height);
			Gdk.Pixbuf ret;
			if (raw_ret == IntPtr.Zero)
				ret = null;
			else
				ret = (Gdk.Pixbuf)GLib.Object.GetObject (raw_ret);
			return ret;
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

		public static void AddContent (this Gtk.Dialog dialog, Gtk.Widget widget, bool expand = true, bool fill = true, uint padding = 0)
		{
			dialog.ContentArea.PackStart (widget, expand, fill, padding);
		}

		public static void AddContent (this Gtk.MessageDialog dialog, Gtk.Widget widget, bool expand = true, bool fill = true, uint padding = 0)
		{
			dialog.GetMessageArea().PackStart (widget, expand, fill, padding);
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

		public static Xwt.Drawing.Color GetBackgroundColor (this Gtk.Widget widget)
		{
			return widget.GetBackgroundColor (Gtk.StateFlags.Normal);
		}

		public static Xwt.Drawing.Color GetBackgroundColor (this Gtk.Widget widget, Gtk.StateType state)
		{
			return widget.GetBackgroundColor (state.ToGtk3StateFlags ());
		}

		public static Xwt.Drawing.Color GetBackgroundColor (this Gtk.Widget widget, Gtk.StateFlags state)
		{
			return widget.StyleContext.GetBackgroundColor (state).ToXwtValue ();
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

		public static void GetSize (this Gtk.CellRenderer cr, Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			cr.GetPadding(out x_offset, out y_offset);
			var minimum_size = Gtk.Requisition.Zero;
			var natural_size = Gtk.Requisition.Zero;
			cr.GetPreferredHeightForWidth (widget, cell_area.Width, out minimum_size.Height, out natural_size.Height);
			cr.GetPreferredWidthForHeight (widget, cell_area.Height, out minimum_size.Width, out natural_size.Width);
			width = Math.Max (minimum_size.Width, natural_size.Width);
			height = Math.Max (minimum_size.Height, natural_size.Height);
			x_offset += (int)(cr.Xalign * (cell_area.Width - width));
			y_offset += (int)(cr.Yalign * (cell_area.Height - height));
		}

		public static string GetText (this Gtk.TextInsertedArgs args)
		{
			return args.NewText;
		}

		public static double GetSliderPosition (this Gtk.Scale scale)
		{
			int start, end;
			scale.GetSliderRange (out start, out end);
			return start + ((end - start) / 2);
		}
	}
}

