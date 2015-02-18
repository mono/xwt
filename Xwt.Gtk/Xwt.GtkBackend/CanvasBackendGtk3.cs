//
// Gtk3CustomCanvas.cs
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
using System.Linq;
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	partial class CustomCanvas
	{
		protected override bool OnDrawn (Cairo.Context cr)
		{
			// ensure cr does not get disposed before it is passed back to Gtk
			var context = new TempCairoContextBackend (Util.GetScaleFactor (this));
			context.Context = cr;
			// Set context Origin from initial Cairo CTM (to ensure new Xwt CTM is Identity Matrix)
			context.Origin.X = cr.Matrix.X0;
			context.Origin.Y = cr.Matrix.Y0;
			// Gtk3 Cairo Context cannot access area to be redrawn, so use full Canvas area.
			// QueueDraw (rect) sets Clip (rect) internally, so drawing is limited correctly
			OnDraw (new Rectangle (0, 0, Allocation.Width, Allocation.Height), context);
			return base.OnDrawn (cr);
		}

		protected override Gtk.SizeRequestMode OnGetRequestMode ()
		{
			// always in fixed mode, since we have fixed width-height relation
			return (Gtk.SizeRequestMode)2;
		}

		protected override void OnGetPreferredHeight (out int minimum_height, out int natural_height)
		{
			if (HeightRequest > 0)
				minimum_height = natural_height = HeightRequest;
			else {
				minimum_height = natural_height = (Backend.Frontend.MinHeight > 0 ? (int) Backend.Frontend.MinHeight : 0);
				foreach (var cr in children.Where (c => c.Key.Visible)) {
					minimum_height = (int) Math.Max (minimum_height, cr.Value.Y + cr.Value.Height);
					natural_height = (int) Math.Max (natural_height, cr.Value.Y + cr.Value.Height);
				}
			}
		}

		protected override void OnGetPreferredWidth (out int minimum_width, out int natural_width)
		{
			if (WidthRequest > 0)
				minimum_width = natural_width = WidthRequest;
			else {
				minimum_width = natural_width = (Backend.Frontend.MinWidth > 0 ? (int)Backend.Frontend.MinWidth : 0);
				foreach (var cr in children.Where (c => c.Key.Visible)) {
					minimum_width = (int) Math.Max (minimum_width, cr.Value.X + cr.Value.Height);
					natural_width = (int) Math.Max (natural_width, cr.Value.X + cr.Value.Height);
				}
			}
		}
	}

	class TempCairoContextBackend : CairoContextBackend
	{
		public TempCairoContextBackend (double scaleFactor) : base (scaleFactor)
		{
		}

		public override void Dispose ()
		{
			// dispose only TempSurface and don't touch the Context itself
			IDisposable d = TempSurface;
			if (d != null) {
				d.Dispose ();
			}
		}
	}
}

