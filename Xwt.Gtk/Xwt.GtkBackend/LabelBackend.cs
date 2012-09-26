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
using Xwt.CairoBackend;


namespace Xwt.GtkBackend
{
	class LabelBackend: WidgetBackend, ILabelBackend
	{
		Color? bgColor, textColor;
		int wrapHeight, wrapWidth;
		
		public LabelBackend ()
		{
			Widget = new Gtk.Label ();
			Label.Show ();
			Label.Xalign = 0;
			Label.Yalign = 0.5f;
		}
		
		protected Gtk.Label Label {
			get {
				if (Widget is Gtk.Label)
					return (Gtk.Label) Widget;
				else
					return (Gtk.Label) ((Gtk.EventBox)base.Widget).Child;
			}
		}
		
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
				ctx.Color = bgColor.Value.ToCairoColor ();
				ctx.Fill ();
			}
		}

		void HandleLabelDynamicSizeAllocate (object o, Gtk.SizeAllocatedArgs args)
		{
			int unused;
			Label.Layout.Width = Pango.Units.FromPixels (args.Allocation.Width);
			Label.Layout.GetPixelSize (out unused, out wrapHeight);
			if (wrapWidth != args.Allocation.Width) {
				wrapWidth = args.Allocation.Width;
				Label.QueueResize ();
			}
		}

		void HandleLabelDynamicSizeRequest (object o, Gtk.SizeRequestedArgs args)
		{
			if (wrapHeight > 0) {
				var req = args.Requisition;
				req.Width = 0;
				req.Height = wrapHeight;
				args.Requisition = req;
			}
		}
		
		public virtual string Text {
			get { return Label.Text; }
			set { Label.Text = value; }
		}


		public Xwt.Drawing.Color TextColor {
			get {
				return textColor.HasValue ? textColor.Value : Util.ToXwtColor (Widget.Style.Foreground (Gtk.StateType.Normal));
			}
			set {
				var color = value.ToGdkColor ();
				var attr = new Pango.AttrForeground (color.Red, color.Green, color.Blue);
				var attrs = new Pango.AttrList ();
				attrs.Insert (attr);

				Label.Attributes = attrs;

				textColor = value;
				Label.QueueDraw ();
			}
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
		
		public EllipsizeMode Ellipsize {
			get {
				var em = Label.Ellipsize;
				switch (em) {
				case Pango.EllipsizeMode.None: return Xwt.EllipsizeMode.None;
				case Pango.EllipsizeMode.Start: return Xwt.EllipsizeMode.Start;
				case Pango.EllipsizeMode.Middle: return Xwt.EllipsizeMode.Middle;
				case Pango.EllipsizeMode.End: return Xwt.EllipsizeMode.End;
				}
				throw new NotSupportedException ();
			}
			set {
				switch (value) {
				case Xwt.EllipsizeMode.None: Label.Ellipsize = Pango.EllipsizeMode.None; break;
				case Xwt.EllipsizeMode.Start: Label.Ellipsize = Pango.EllipsizeMode.Start; break;
				case Xwt.EllipsizeMode.Middle: Label.Ellipsize = Pango.EllipsizeMode.Middle; break;
				case Xwt.EllipsizeMode.End: Label.Ellipsize = Pango.EllipsizeMode.End; break;
				}
			}
		}

		public WrapMode Wrap {
			get {
				if (!Label.LineWrap)
					return WrapMode.None;
				else {
					switch (Label.LineWrapMode) {
					case Pango.WrapMode.Char:
						return WrapMode.Character;
					case Pango.WrapMode.Word:
						return WrapMode.Word;
					case Pango.WrapMode.WordChar:
						return WrapMode.WordAndCharacter;
					default:
						return WrapMode.None;
					}
				}
			}
			set {
				if (value == WrapMode.None){
					if (Label.LineWrap) {
						Label.LineWrap = false;
						Label.SizeAllocated -= HandleLabelDynamicSizeAllocate;
						Label.SizeRequested -= HandleLabelDynamicSizeRequest;
					}
				} else {
					if (!Label.LineWrap) {
						Label.LineWrap = true;
						Label.SizeAllocated += HandleLabelDynamicSizeAllocate;
						Label.SizeRequested += HandleLabelDynamicSizeRequest;
					}
					switch (value) {
					case WrapMode.Character:
						Label.LineWrapMode = Pango.WrapMode.Char;
						break;
					case WrapMode.Word:
						Label.LineWrapMode = Pango.WrapMode.Word;
						break;
					case WrapMode.WordAndCharacter:
						Label.LineWrapMode = Pango.WrapMode.WordChar;
						break;
					}
				}
			}
		}
	}
}

