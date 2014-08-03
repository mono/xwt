// 
// HeaderBox.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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
using Gtk;
using Xwt.Drawing;
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	class HeaderBox: HeaderBoxGtk
	{
		Gtk.Widget child;
		int topMargin;
		int bottomMargin;
		int leftMargin;
		int rightMargin;
		
		int topPadding;
		int bottomPadding;
		int leftPadding;
		int rightPadding;
		
		Color? color;
		
		public HeaderBox ()
		{
		}
		
		public HeaderBox (IntPtr p): base (p)
		{
		}
		
		public HeaderBox (int topMargin, int bottomMargin, int leftMargin, int rightMargin)
		{
			SetMargins (topMargin, bottomMargin, leftMargin, rightMargin);
		}

		public void SetBorderColor (Color color)
		{
			this.color = color;
			QueueDraw ();
		}
		
		public void SetMargins (int topMargin, int bottomMargin, int leftMargin, int rightMargin)
		{
			this.topMargin = topMargin;
			this.bottomMargin = bottomMargin;
			this.leftMargin = leftMargin;
			this.rightMargin = rightMargin;
		}
		
		public void SetPadding (int topPadding, int bottomPadding, int leftPadding, int rightPadding)
		{
			this.topPadding = topPadding;
			this.bottomPadding = bottomPadding;
			this.leftPadding = leftPadding;
			this.rightPadding = rightPadding;
		}
		
		public bool GradientBackround { get; set; }
		public Color? BackgroundColor { get; set; }

		protected override void OnAdded (Gtk.Widget widget)
		{
			base.OnAdded (widget);
			child = widget;
		}

		protected override void OnSizeRequested (ref Requisition requisition)
		{
			if (child != null) {
				requisition = child.SizeRequest ();
				requisition.Width += leftMargin + rightMargin + leftPadding + rightPadding;
				requisition.Height += topMargin + bottomMargin + topPadding + bottomPadding;
			} else {
				requisition.Width = 0;
				requisition.Height = 0;
			}
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			if (allocation.Width > leftMargin + rightMargin + leftPadding + rightPadding) {
				allocation.X += leftMargin + leftPadding;
				allocation.Width -= leftMargin + rightMargin + leftPadding + rightPadding;
			}
			if (allocation.Height > topMargin + bottomMargin + topPadding + bottomPadding) {
				allocation.Y += topMargin + topPadding;
				allocation.Height -= topMargin + bottomMargin + topPadding + bottomPadding;
			}
			if (child != null)
				child.SizeAllocate (allocation);
		}

		protected override void OnDrawn (Cairo.Context cr, Gdk.Rectangle rect)
		{
			if (BackgroundColor.HasValue) {
				cr.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
				cr.SetSourceColor (BackgroundColor.Value.ToCairoColor ());
				cr.Fill ();
			}
		
			if (GradientBackround) {
				Color gcol = Style.Background (Gtk.StateType.Normal).ToXwtValue ();
			
				cr.NewPath ();
				cr.MoveTo (rect.X, rect.Y);
				cr.RelLineTo (rect.Width, 0);
				cr.RelLineTo (0, rect.Height);
				cr.RelLineTo (-rect.Width, 0);
				cr.RelLineTo (0, -rect.Height);
				cr.ClosePath ();
				using (var pat = new Cairo.LinearGradient (rect.X, rect.Y, rect.X, rect.Bottom)) {
					Cairo.Color color1 = gcol.ToCairoColor ();
					pat.AddColorStop (0, color1);
					gcol.Light -= 0.1;
					pat.AddColorStop (1, gcol.ToCairoColor ());
					cr.SetSource (pat);
					cr.FillPreserve ();
				}
			}
		
			cr.SetSourceColor (color.HasValue ? color.Value.ToCairoColor () : Style.Dark (Gtk.StateType.Normal).ToXwtValue ().ToCairoColor ());
			cr.Rectangle (rect.X, rect.Y, rect.Width, topMargin);
			cr.Rectangle (rect.X, rect.Y + rect.Height - bottomMargin, rect.Width, bottomMargin);
			cr.Rectangle (rect.X, rect.Y, leftMargin, rect.Height);
			cr.Rectangle (rect.X + rect.Width - rightMargin, rect.Y, rightMargin, rect.Height);
			cr.Fill ();
		}
	}
}

