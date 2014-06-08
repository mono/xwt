//
// GtkExtensions.cs
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
using System;
using Gdk;

namespace Xwt.GtkBackend
{
	public static class Gtk2Extensions
	{
		public static void SetHasWindow (this Gtk.Widget widget, bool value)
		{
			if (value)
				widget.WidgetFlags &= ~Gtk.WidgetFlags.NoWindow;
			else
				widget.WidgetFlags |= Gtk.WidgetFlags.NoWindow;
		}

		public static bool GetHasWindow (this Gtk.Widget widget)
		{
			return !widget.IsNoWindow;
		}

		public static void SetAppPaintable (this Gtk.Widget widget, bool value)
		{
			if (value)
				widget.WidgetFlags |= Gtk.WidgetFlags.AppPaintable;
			else
				widget.WidgetFlags &= ~Gtk.WidgetFlags.AppPaintable;
		}

		public static void SetStateActive(this Gtk.Widget widget)
		{
			widget.State = Gtk.StateType.Active;
		}

		public static void SetStateNormal(this Gtk.Widget widget)
		{
			widget.State = Gtk.StateType.Normal;
		}

		public static void AddSignalHandler (this Gtk.Widget widget, string name, Delegate handler, Type args_type)
		{
			var signal = GLib.Signal.Lookup (widget, name, args_type);
			signal.AddDelegate (handler);
		}

		public static Gdk.Pixbuf ToPixbuf (this Gdk.Window window, int src_x, int src_y, int width, int height)
		{
			return Gdk.Pixbuf.FromDrawable (window, Gdk.Colormap.System, src_x, src_y, 0, 0, width, height);
		}

		public static Gtk.CellRenderer[] GetCellRenderers (this Gtk.TreeViewColumn column)
		{
			return column.CellRenderers;
		}

		public static Gdk.DragAction GetSelectedAction (this Gdk.DragContext context)
		{
			return context.Action;
		}

		public static Gdk.Atom[] ListTargets (this Gdk.DragContext context)
		{
			return context.Targets;
		}

		public static void AddContent (this Gtk.Dialog dialog, Gtk.Widget widget)
		{
			dialog.VBox.PackStart (widget);
		}

		public static void SetContentSpacing (this Gtk.Dialog dialog, int spacing)
		{
			dialog.VBox.Spacing = spacing;
		}

		public static void SetTextColumn (this Gtk.ComboBox comboBox, int column)
		{
			((Gtk.ComboBoxEntry)comboBox).TextColumn = column;
		}

		public static void FixContainerLeak (this Gtk.Container c)
		{
			GtkWorkarounds.FixContainerLeak (c);
		}

		public static void SetBackgroundColor (this Gtk.Widget widget, Xwt.Drawing.Color color)
		{
			widget.ModifyBg (Gtk.StateType.Normal, color.ToGtkValue ());
		}

		public static void SetChildBackgroundColor (this Gtk.Container container, Xwt.Drawing.Color color)
		{
			foreach (var widget in container.Children)
				widget.ModifyBg (Gtk.StateType.Normal, color.ToGtkValue ());
		}
	}
}

