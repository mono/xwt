//
// TextAreaBackendGtk2.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
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

namespace Xwt.GtkBackend
{
	public partial class TextAreaBackend
	{
		string placeHolderText;

		public string PlaceholderText {
			get { return placeHolderText; }
			set {
				if (placeHolderText != value) {
					if (placeHolderText == null)
						TextView.ExposeEvent += HandleWidgetExposeEvent;
					else if (value == null)
						TextView.ExposeEvent -= HandleWidgetExposeEvent;
				}
				placeHolderText = value;
			}
		}

		void HandleWidgetExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			RenderPlaceholderText (TextView, args, placeHolderText, ref layout);
		}

		public static void RenderPlaceholderText (Gtk.TextView widget, Gtk.ExposeEventArgs args, string placeHolderText, ref Pango.Layout layout)
		{
			if (args.Event.Window != widget.GetWindow (Gtk.TextWindowType.Text))
				return;

			if (widget.Buffer.Text.Length > 0)
				return;

			float xalign = 0;
			float yalign = 0;

			switch (widget.Justification) {
				case Gtk.Justification.Center: xalign = 0.5f; break;
				case Gtk.Justification.Right: xalign = 1; break;
			}

			if (layout == null) {
				layout = new Pango.Layout (widget.PangoContext);
				layout.FontDescription = widget.PangoContext.FontDescription.Copy ();
			}

			int wh, ww;
			int xpad = 3;
			int ypad = 0;
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
	}
}

