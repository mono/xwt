//
// TextAreaBackendGtk3.cs
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
using System;
using Pango;
using Xwt.CairoBackend;

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
						TextView.Drawn += HandleDrawn;
					else if (value == null)
						TextView.Drawn -= HandleDrawn;
				}
				placeHolderText = value;
			}
		}

		void HandleDrawn (object o, Gtk.DrawnArgs args)
		{
			if (TextView.HasFocus)
				return;

			if (TextView.Buffer.Text.Length > 0)
				return;

			float xalign = 0;
			float yalign = 0;

			switch (TextView.Justification) {
				case Gtk.Justification.Center: xalign = 0.5f; break;
				case Gtk.Justification.Right: xalign = 1; break;
			}

			if (layout == null) {
				layout = new Pango.Layout (TextView.PangoContext);
				layout.FontDescription = TextView.PangoContext.FontDescription.Copy ();
			}

			int xpad = 3;
			int ypad = 0;

			int width, height;
			layout.SetText (placeHolderText);
			layout.GetPixelSize (out width, out height);

			int x = xpad + (int)((TextView.AllocatedWidth - width) * xalign);
			int y = ypad + (int)((TextView.AllocatedHeight - height) * yalign);
			args.Cr.MoveTo (x, y);

			Xwt.Drawing.Color color_a = TextView.Style.Base (Gtk.StateType.Normal).ToXwtValue ();
			Xwt.Drawing.Color color_b = TextView.Style.Text (Gtk.StateType.Normal).ToXwtValue ();
			args.Cr.SetSourceColor (color_b.BlendWith (color_a, 0.5).ToCairoColor());

			Pango.CairoHelper.ShowLayout (args.Cr, layout);
		}
	}
}

