//
// Gtk3PopoverWindow.cs
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
		protected bool supportAlpha = true;
		protected bool SupportAlpha
		{
			get
			{
				return supportAlpha;
			}
			private set
			{
				if (supportAlpha != value)
				{
					supportAlpha = value;
					OnSupportAlphaChanged();
				}
			}
		}

		protected virtual void OnSupportAlphaChanged()
		{
			QueueDraw();
		}

		Color? backgroundColor;

		public Color? BackgroundColor
		{
			get { return backgroundColor; }
			set {
				backgroundColor = value;
				// HACK: with AppPaintable == false Gtk3 doesn't draw the default background.
				//       Set AppPaintable = true only if a color is not null and is transparent.
				AppPaintable = backgroundColor?.A < 1.0;
			}
		}

		public GtkPopoverWindow (Gtk.WindowType type) : base (type)
		{
			UpdateColorMap();
		}

		void UpdateColorMap()
		{
			// To check if the display supports alpha channels, get the colormap
			var visual = Screen.RgbaVisual;
			if (visual == null)
			{
				visual = Screen.SystemVisual;
				SupportAlpha = false;
			}
			else
				SupportAlpha = true;
			Visual = visual;
		}

		protected override void OnScreenChanged(Gdk.Screen previous_screen)
		{
			UpdateColorMap();
			base.OnScreenChanged(previous_screen);
		}

		protected virtual bool OnDraw (Cairo.Context cr)
		{
			return false;
		}

		protected sealed override bool OnDrawn (Context cr)
		{
			if (BackgroundColor.HasValue)
			{
				// We clear the surface with a transparent color if possible
				if (SupportAlpha && BackgroundColor.Value.A < 1.0)
					cr.SetSourceRGBA(BackgroundColor.Value.R, BackgroundColor.Value.G, BackgroundColor.Value.B, BackgroundColor.Value.A);
				else
					cr.SetSourceRGB(BackgroundColor.Value.R, BackgroundColor.Value.G, BackgroundColor.Value.B);
				cr.Operator = Operator.Source;
				cr.Paint();
			}
			var handled = OnDraw (cr);
			cr.Restore();

			return handled || base.OnDrawn (cr);
		}
	}
}

