// 
// LabelBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt.Backends;
using Xwt.Drawing;


namespace Xwt.GtkBackend
{
	class LabelBackend: WidgetBackend, ILabelBackend
	{
		public LabelBackend ()
		{
			Widget = new Gtk.Label ();
			Label.Show ();
			Label.Xalign = 0;
			Label.Yalign = 0.5f;
		}
		
		Gtk.Label Label {
			get {
				if (Widget is Gtk.Label)
					return (Gtk.Label) Widget;
				else
					return (Gtk.Label) ((Gtk.EventBox)base.Widget).Child;
			}
		}
		
		public override Xwt.Drawing.Color BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				CustomLabel cla = Widget as CustomLabel;
				if (cla == null) {
					cla = new CustomLabel ();
					cla.Text = Label.Text;
					cla.Xalign = Label.Xalign;
					cla.Visible = Label.Visible;
					Widget = cla;
				}
				cla.BackgroundColor = value;
				cla.QueueDraw ();
			}
		}
		
		public string Text {
			get { return Label.Text; }
			set { Label.Text = value; }
		}

		public Alignment TextAlignment {
			get {
				if (Label.Xalign == 0)
					return Alignment.Start;
				else if (Label.Xalign == 1)
					return Alignment.End;
				else
					return Alignment.Center;
			}
			set {
				switch (value) {
				case Alignment.Start: Label.Xalign = 0; break;
				case Alignment.End: Label.Xalign = 1; break;
				case Alignment.Center: Label.Xalign = 0.5f; break;
				}
			}
		}
	}
	
	class CustomLabel: Gtk.Label
	{
		public Color BackgroundColor;
		
		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			using (var ctx = Gdk.CairoHelper.Create (this.GdkWindow)) {
				ctx.Rectangle (Allocation.X, Allocation.Y, Allocation.Width, Allocation.Height);
				ctx.Color = Util.ToCairoColor (BackgroundColor);
				ctx.Fill ();
			}
			
			return base.OnExposeEvent (evnt);
		}
	}
}

