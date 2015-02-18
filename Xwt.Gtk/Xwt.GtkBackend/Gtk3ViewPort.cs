//
// Gtk3ViewPort.cs
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

namespace Xwt.GtkBackend
{
	public class GtkViewPort: Gtk.Bin, Gtk.IScrollableImplementor
	{
		public GtkViewPort ()
		{
		}

		public GtkViewPort (IntPtr raw) : base (raw)
		{
		}

		Gtk.Adjustment hadjustment;
		public Gtk.Adjustment Hadjustment {
			get {
				return hadjustment;
			}
			set {
				hadjustment = value;
				if (vadjustment != null) {
					OnSetScrollAdjustments (value, vadjustment);
				}
			}
		}

		Gtk.Adjustment vadjustment;
		public Gtk.Adjustment Vadjustment {
			get {
				return vadjustment;
			}
			set {
				vadjustment = value;
				if (hadjustment != null) {
					OnSetScrollAdjustments (hadjustment, value);
				}
			}
		}

		public Gtk.ScrollablePolicy HscrollPolicy { get; set; }

		public Gtk.ScrollablePolicy VscrollPolicy { get; set; }

		protected override void OnAdded (Gtk.Widget widget)
		{
			base.OnAdded (widget);
			int width, height;
			widget.GetSizeRequest (out width, out height);
			SetSizeRequest (width, height);
		}

		protected virtual void OnSetScrollAdjustments (Gtk.Adjustment hadj, Gtk.Adjustment vadj)
		{
		}

		protected virtual void OnSizeRequested (ref Gtk.Requisition requisition)
		{
		}
	}
}

