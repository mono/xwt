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

		public static void RemoveSignalHandler (this Gtk.Widget widget, string name, Delegate handler)
		{
			var signal = GLib.Signal.Lookup (widget, name);
			signal.RemoveDelegate (handler);
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

		public static void AddContent (this Gtk.Dialog dialog, Gtk.Widget widget, bool expand = true, bool fill = true, uint padding = 0)
		{
			dialog.VBox.PackStart (widget, expand, fill, padding);
		}

		public static void AddContent (this Gtk.MessageDialog dialog, Gtk.Widget widget, bool expand = true, bool fill = true, uint padding = 0)
		{
			var messageArea = dialog.GetMessageArea () ?? dialog.VBox;
			messageArea.PackStart (widget, expand, fill, padding);
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

		public static Xwt.Drawing.Color GetBackgroundColor (this Gtk.Widget widget)
		{
			return widget.GetBackgroundColor (Gtk.StateType.Normal);
		}

		public static Xwt.Drawing.Color GetBackgroundColor (this Gtk.Widget widget, Gtk.StateType state)
		{
			return widget.Style.Background (state).ToXwtValue ();
		}

		public static void SetBackgroundColor (this Gtk.Widget widget, Xwt.Drawing.Color color)
		{
			widget.SetBackgroundColor (Gtk.StateType.Normal, color);
		}

		public static void SetBackgroundColor (this Gtk.Widget widget, Gtk.StateType state, Xwt.Drawing.Color color)
		{
			widget.ModifyBg (state, color.ToGtkValue ());
		}

		public static void SetChildBackgroundColor (this Gtk.Container container, Xwt.Drawing.Color color)
		{
			foreach (var widget in container.Children)
				widget.ModifyBg (Gtk.StateType.Normal, color.ToGtkValue ());
		}

		public static void RenderPlaceholderText (this Gtk.Entry entry, Gtk.ExposeEventArgs args, string placeHolderText, ref Pango.Layout layout)
		{
			// The Entry's GdkWindow is the top level window onto which
			// the frame is drawn; the actual text entry is drawn into a
			// separate window, so we can ensure that for themes that don't
			// respect HasFrame, we never ever allow the base frame drawing
			// to happen
			if (args.Event.Window == entry.GdkWindow)
				return;

			if (entry.Text.Length > 0)
				return;

			RenderPlaceholderText_internal (entry, args, placeHolderText, ref layout, entry.Xalign, 0.5f, 1, 0);
		}

		static void RenderPlaceholderText_internal (Gtk.Widget widget, Gtk.ExposeEventArgs args, string placeHolderText, ref Pango.Layout layout, float xalign, float yalign, int xpad, int ypad)
		{
			if (layout == null) {
				layout = new Pango.Layout (widget.PangoContext);
				layout.FontDescription = widget.PangoContext.FontDescription.Copy ();
			}

			int wh, ww;
			args.Event.Window.GetSize (out ww, out wh);

			int width, height;
			layout.SetText (placeHolderText);
			layout.GetPixelSize (out width, out height);

			int x = xpad + (int)((ww - width) * xalign);
			int y = ypad + (int)((wh - height) * yalign);

			using (var gc = new Gdk.GC (args.Event.Window)) {
				gc.Copy (widget.Style.TextGC (Gtk.StateType.Normal));
				Xwt.Drawing.Color color_a = widget.Style.Base (Gtk.StateType.Normal).ToXwtValue ();
				Xwt.Drawing.Color color_b = widget.Style.Text (Gtk.StateType.Normal).ToXwtValue ();
				gc.RgbFgColor = color_b.BlendWith (color_a, 0.5).ToGtkValue ();

				args.Event.Window.DrawLayout (gc, x, y, layout);
			}
		}

		public static double GetSliderPosition (this Gtk.Scale scale)
		{
			Gtk.Orientation orientation;
			if (scale is Gtk.HScale)
				orientation = Gtk.Orientation.Horizontal;
			else if (scale is Gtk.VScale)
				orientation = Gtk.Orientation.Vertical;
			else
				throw new InvalidOperationException ("Can not obtain slider position from " + scale.GetType ());

			var padding = (int)scale.StyleGetProperty ("focus-padding");
			var slwidth = Convert.ToDouble (scale.StyleGetProperty ("slider-width"));

			int orientationSize;
			if (orientation == Gtk.Orientation.Horizontal)
				orientationSize = scale.Allocation.Width - (2 * padding);
			else
				orientationSize = scale.Allocation.Height - (2 * padding);

			double prct = 0;
			if (scale.Adjustment.Lower >= 0) {
				prct = (scale.Value / (scale.Adjustment.Upper - scale.Adjustment.Lower));
			} else if (scale.Adjustment.Upper <= 0) {
				prct = (Math.Abs (scale.Value) / Math.Abs (scale.Adjustment.Lower - scale.Adjustment.Upper));
			} else if (scale.Adjustment.Lower < 0) {
				if (scale.Value >= 0)
					prct = 0.5 + ((scale.Value / 2) / scale.Adjustment.Upper);
				else
					prct = 0.5 - Math.Abs ((scale.Value / 2) / scale.Adjustment.Lower);
			}

			if (orientation == Gtk.Orientation.Vertical || scale.Inverted)
				prct = 1 - prct;

			return (int)(((orientationSize - (slwidth)) * prct) + (slwidth / 2));
		}
	}
}

