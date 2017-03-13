// 
// FrameBackend.cs
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
	public class FrameBackend:  WidgetBackend, IFrameBackend
	{
		Gtk.Alignment paddingAlign;
		string label;
		Color? borderColor;
		
		public FrameBackend ()
		{
			Widget = new Gtk.Frame ();
			Widget.Show ();
		}
		
		protected new Gtk.Bin Widget {
			get { return (Gtk.Bin)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IFrameEventSink EventSink {
			get { return (IFrameEventSink)base.EventSink; }
		}

		#region IFrameBackend implementation

		public void SetContent (IWidgetBackend child)
		{
			Gtk.Bin parent = paddingAlign != null ? paddingAlign : Widget;

			if (parent.Child != null) {
				RemoveChildPlacement (parent.Child);
				parent.Remove (parent.Child);
			}

			if (child != null) {
				var w = GetWidgetWithPlacement (child);
				parent.Child = w;
			} else {
				parent.Child = null;
			}
		}
		public void SetFrameType (FrameType type)
		{
			Frame f = (Frame) Frontend;
			
			switch (type) {
			case FrameType.Custom:
				if (!(Widget is HeaderBox)) {
					HeaderBox box = new HeaderBox ();
					box.Show ();
					box.BackgroundColor = UsingCustomBackgroundColor ? (Color?)BackgroundColor : null;
					#pragma warning disable CS0618 // Type or member is obsolete
					box.SetMargins ((int)f.BorderWidthTop, (int)f.BorderWidthBottom, (int)f.BorderWidthLeft, (int)f.BorderWidthRight);
					#pragma warning restore CS0618 // Type or member is obsolete
					box.SetPadding ((int)f.Padding.Top, (int)f.Padding.Bottom, (int)f.Padding.Left, (int)f.Padding.Right);
					if (borderColor != null)
						box.SetBorderColor (borderColor.Value);
					var c = paddingAlign != null ? paddingAlign.Child : Widget.Child;
					if (c != null) {
						((Gtk.Container)c.Parent).Remove (c);
						box.Add (c);
					}
					Widget = box;
					if (paddingAlign != null) {
						paddingAlign.Destroy ();
						paddingAlign = null;
					}
				}
				break;
			case FrameType.WidgetBox:
				if (!(Widget is Gtk.Frame)) {
					var c = Widget.Child;
					if (c != null)
						Widget.Remove (c);
					Gtk.Frame gf = new Gtk.Frame ();
					if (!string.IsNullOrEmpty (label))
						gf.Label = label;
					if (f.Padding.HorizontalSpacing != 0 || f.Padding.VerticalSpacing != 0) {
						paddingAlign = new Gtk.Alignment (0, 0, 1, 1);
						paddingAlign.Show ();
						UreatePaddingAlign (f.Padding.Top, f.Padding.Bottom, f.Padding.Left, f.Padding.Right);
						if (c != null)
							paddingAlign.Add (c);
						gf.Add (paddingAlign);
					} else {
						if (c != null)
							gf.Add (c);
					}
					gf.Show ();
					Widget = gf;
				}
				break;
			}
		}
		
		void UreatePaddingAlign (double top, double bottom, double left, double right)
		{
			paddingAlign.TopPadding = (uint) top;
			paddingAlign.BottomPadding = (uint) bottom;
			paddingAlign.LeftPadding = (uint) left;
			paddingAlign.RightPadding = (uint) right;
		}

		public void SetBorderSize (double left, double right, double top, double bottom)
		{
			HeaderBox hb = Widget as HeaderBox;
			if (hb != null) {
				hb.SetMargins ((int)top, (int)bottom, (int)left, (int)right);
			}
		}

		public void SetPadding (double left, double right, double top, double bottom)
		{
			if (Widget is HeaderBox) {
				HeaderBox hb = (HeaderBox) Widget;
				hb.SetPadding ((int)top, (int)bottom, (int)left, (int)right);
				return;
			}
			
			if (left == 0 && right == 0 && top == 0 && bottom == 0 && paddingAlign == null)
				return;

			if (paddingAlign == null) {
				paddingAlign = new Gtk.Alignment (0, 0, 1, 1);
				paddingAlign.Show ();
				var c = Widget.Child;
				if (c != null) {
					Widget.Remove (c);
					paddingAlign.Add (c);
				}
				Widget.Add (paddingAlign);
			}
			UreatePaddingAlign (top, bottom, left, right);
		}

		public Color BorderColor {
			get {
				if (borderColor == null)
					return Widget.Style.Dark (Gtk.StateType.Normal).ToXwtValue ();
				else
					return borderColor.Value;
			}
			set {
				borderColor = value;
				HeaderBox hb = Widget as HeaderBox;
				if (hb != null)
					hb.SetBorderColor (value);
			}
		}
		
		public override Color BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = value;
				if (Widget is HeaderBox) {
					((HeaderBox)Widget).BackgroundColor = value;
				}
			}
		}

		public string Label {
			get {
				return label;
			}
			set {
				label = value;
				if (Widget is Gtk.Frame)
					((Gtk.Frame)Widget).Label = value;
			}
		}
		#endregion
	}

	class FrameWidget: Gtk.Frame, IConstraintProvider
	{
		#if !XWT_GTK3
		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);
		}
		#endif

		public void GetConstraints (Gtk.Widget target, out SizeConstraint width, out SizeConstraint height)
		{
			width = height = SizeConstraint.Unconstrained;
		}
	}

	public interface IConstraintProvider
	{
		void GetConstraints (Gtk.Widget target, out SizeConstraint width, out SizeConstraint height);
	}
}

