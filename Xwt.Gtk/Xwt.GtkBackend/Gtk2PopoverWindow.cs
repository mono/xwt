//
// Gtk2PopoverWindow.cs
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

using Cairo;

namespace Xwt.GtkBackend
{
	public class GtkPopoverWindow : Gtk.Window
	{
		bool supportAlpha;
		protected bool SupportAlpha {
			get {
				return supportAlpha;
			}
			private set {
				if (supportAlpha != value) {
					supportAlpha = value;
					OnSupportAlphaChanged ();
				}
			}
		}

		protected virtual void OnSupportAlphaChanged ()
		{
			QueueDraw ();
		}

		public Color? BackgroundColor { get; set; }

		public GtkPopoverWindow (Gtk.WindowType type) : base (type)
		{
			AppPaintable = true;
			UpdateColorMap ();
		}

		void UpdateColorMap ()
		{
			// To check if the display supports alpha channels, get the colormap
			var colormap = Screen.RgbaColormap;
			if (colormap == null) {
				colormap = Screen.RgbColormap;
				SupportAlpha = false;
			} else
				SupportAlpha = true;
			Colormap = colormap;
		}

		protected override void OnScreenChanged (Gdk.Screen previous_screen)
		{
			UpdateColorMap ();
			base.OnScreenChanged (previous_screen);
		}

		protected virtual bool OnDraw (Cairo.Context cr)
		{
			return false;
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			bool handled;
			using (Context ctx = Gdk.CairoHelper.Create (this.GdkWindow)) {
				if (BackgroundColor.HasValue) {
					// We clear the surface with a transparent color if possible
					if (SupportAlpha && BackgroundColor.Value.A < 1.0)
						ctx.SetSourceRGBA (BackgroundColor.Value.R, BackgroundColor.Value.G, BackgroundColor.Value.B, BackgroundColor.Value.A);
					else
						ctx.SetSourceRGB (BackgroundColor.Value.R, BackgroundColor.Value.G, BackgroundColor.Value.B);
					ctx.Operator = Operator.Source;
					ctx.Paint ();
				}
				handled = OnDraw (ctx);
			}

			return handled || base.OnExposeEvent (evnt);
		}
	}
}

