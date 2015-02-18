//
// CanvasBackendGtk.cs
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
using System.Linq;
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	partial class CustomCanvas
	{
		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);
			foreach (var cr in children.ToArray ())
				cr.Key.SizeRequest ();
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			var a = evnt.Area;
			using (var ctx = CreateContext ()) {
				// Set context Origin from initial Cairo CTM (to ensure new Xwt CTM is Identity Matrix)
				ctx.Origin.X = ctx.Context.Matrix.X0;
				ctx.Origin.Y = ctx.Context.Matrix.Y0;
				// Gdk Expose event supplies the area to be redrawn - but need to adjust X,Y for context Origin 
				Rectangle dirtyRect = new Rectangle (a.X-ctx.Origin.X, a.Y-ctx.Origin.Y, a.Width, a.Height);
				ctx.Context.Rectangle (dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
				ctx.Context.Clip ();
				OnDraw (dirtyRect, ctx);
			}
			return base.OnExposeEvent (evnt);
		}
	}
}

