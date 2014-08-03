//
// LabelBackendGtk2.cs
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
using Xwt.Drawing;
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	public partial class LabelBackend
	{
		Color? bgColor;
		int wrapHeight, wrapWidth;

		public override Xwt.Drawing.Color BackgroundColor {
			get {
				return bgColor.HasValue ? bgColor.Value : base.BackgroundColor;
			}
			set {
				if (!bgColor.HasValue)
					Label.ExposeEvent += HandleLabelExposeEvent;

				bgColor = value;
				Label.QueueDraw ();
			}
		}

		[GLib.ConnectBefore]
		void HandleLabelExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			using (var ctx = Gdk.CairoHelper.Create (Label.GdkWindow)) {
				ctx.Rectangle (Label.Allocation.X, Label.Allocation.Y, Label.Allocation.Width, Label.Allocation.Height);
				ctx.SetSourceColor (bgColor.Value.ToCairoColor ());
				ctx.Fill ();
			}
		}

		void HandleLabelDynamicSizeAllocate (object o, Gtk.SizeAllocatedArgs args)
		{
			int unused, oldHeight = wrapHeight;
			Label.Layout.Width = Pango.Units.FromPixels (args.Allocation.Width);
			Label.Layout.GetPixelSize (out unused, out wrapHeight);
			if (wrapWidth != args.Allocation.Width || oldHeight != wrapHeight) {
				wrapWidth = args.Allocation.Width;
				Label.QueueResize ();
			}
			// GTK renders the text using the calculated pixel width, not the allocated width.
			// If the calculated width is smaller and text is not left aligned, then a gap is
			// shown at the right of the label. We then have the adjust the allocation.
			if (Label.Justify == Gtk.Justification.Right) {
				var w = wrapWidth - unused;
				if (w != Label.Xpad)
					Label.Xpad = w;
			} else if (Label.Justify == Gtk.Justification.Center) {
				var w = (wrapWidth - unused) / 2;
				if (w != Label.Xpad)
					Label.Xpad = w;
			}
		}

		void HandleLabelDynamicSizeRequest (object o, Gtk.SizeRequestedArgs args)
		{
			if (wrapHeight > 0) {
				var req = args.Requisition;
				req.Width = Label.WidthRequest != -1 ? Label.WidthRequest : 0;
				req.Height = wrapHeight;
				args.Requisition = req;
			}
		}

		void SetAlignmentGtk ()
		{
			switch (TextAlignment) {
				case Alignment.Start:
					Label.Xalign = 0f;
					break;
				case Alignment.End:
					Label.Xalign = Label.LineWrap ? 0 : 1;
					break;
				case Alignment.Center:
					Label.Xalign = Label.LineWrap ? 0 : 0.5f;
					break;
			}
		}

		void ToggleSizeCheckEventsForWrap (WrapMode wrapMode)
		{
			if (wrapMode == WrapMode.None){
				if (Label.LineWrap) {
					Label.SizeAllocated -= HandleLabelDynamicSizeAllocate;
					Label.SizeRequested -= HandleLabelDynamicSizeRequest;
				}
			} else {
				if (!Label.LineWrap) {
					Label.SizeAllocated += HandleLabelDynamicSizeAllocate;
					Label.SizeRequested += HandleLabelDynamicSizeRequest;
				}
			}
		}
	}
}

