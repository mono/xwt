// 
// Frame.cs
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
using System.ComponentModel;
using Xwt.Drawing;
using System.Windows.Markup;

namespace Xwt
{
	[ContentProperty("Content")]
	public class FrameBox: Widget
	{
		WidgetSpacing borderWidth;
		WidgetSpacing padding;
		FrameCanvas canvas;
		Color borderColor = Colors.Black;

		class FrameCanvas: Canvas
		{
			Widget child;

			public void Resize ()
			{
				OnPreferredSizeChanged ();
				QueueDraw ();
			}

			public Widget Child {
				get { return child; }
				set {
					if (child != null)
						RemoveChild (child);
					child = value;
					if (child != null)
						AddChild (child);
					Resize ();
					QueueForReallocate ();
				}
			}

			protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
			{
				FrameBox parent = (FrameBox)Parent;
				Size s = new Size (parent.Padding.HorizontalSpacing + parent.BorderWidth.HorizontalSpacing, parent.Padding.VerticalSpacing + parent.BorderWidth.VerticalSpacing);
				if (child != null)
					s += child.Surface.GetPreferredSize (widthConstraint - s.Width, heightConstraint - s.Height, true);
				return s;
			}

			protected override void OnReallocate ()
			{
				if (child != null) {
					FrameBox parent = (FrameBox)Parent;
					Rectangle rect = Bounds;
					var padding = parent.padding;
					var border = parent.borderWidth;
					rect.X += padding.Left + border.Left;
					rect.Y += padding.Top + border.Top;
					rect.Width -= padding.HorizontalSpacing + border.HorizontalSpacing;
					rect.Height -= padding.VerticalSpacing + border.VerticalSpacing;
					rect = child.Surface.GetPlacementInRect (rect);
					SetChildBounds (child, rect);
				}
			}

			protected override void OnDraw (Context ctx, Rectangle dirtyRect)
			{
				base.OnDraw (ctx, dirtyRect);

				FrameBox parent = (FrameBox)Parent;
				var border = parent.borderWidth;
				var r = Bounds;

				//ctx.SetLineDash (0);
				ctx.SetColor (parent.borderColor);

				if (border.Top > 0) {
					ctx.MoveTo (r.X, r.Y + border.Top / 2);
					ctx.RelLineTo (r.Width, 0);
					ctx.SetLineWidth (border.Top);
					ctx.Stroke ();
				}
				if (border.Bottom > 0) {
					ctx.MoveTo (r.X, r.Bottom - border.Bottom / 2);
					ctx.RelLineTo (r.Width, 0);
					ctx.SetLineWidth (border.Bottom);
					ctx.Stroke ();
				}
				if (border.Left > 0) {
					ctx.MoveTo (r.X + border.Left / 2, r.Y + border.Top);
					ctx.RelLineTo (0, r.Height - border.Top - border.Bottom);
					ctx.SetLineWidth (border.Left);
					ctx.Stroke ();
				}
				if (border.Right > 0) {
					ctx.MoveTo (r.Right - border.Right / 2, r.Y + border.Top);
					ctx.RelLineTo (0, r.Height - border.Top - border.Bottom);
					ctx.SetLineWidth (border.Right);
					ctx.Stroke ();
				}
			}
		}

		public FrameBox ()
		{
			canvas = SetInternalChild (new FrameCanvas ());
			base.Content = canvas;
		}

		public FrameBox (Widget content): this ()
		{
			VerifyConstructorCall (this);
			Content = content;
		}

		public WidgetSpacing Padding {
			get { return padding; }
			set {
				padding = value;
				canvas.Resize ();
			}
		}

		[DefaultValue (0d)]
		public double PaddingLeft {
			get { return padding.Left; }
			set {
				padding.Left = value;
				canvas.Resize ();
			}
		}

		[DefaultValue (0d)]
		public double PaddingRight {
			get { return padding.Right; }
			set {
				padding.Right = value;
				canvas.Resize ();
			}
		}

		[DefaultValue (0d)]
		public double PaddingTop {
			get { return padding.Top; }
			set {
				padding.Top = value;
				canvas.Resize ();
			}
		}

		[DefaultValue (0d)]
		public double PaddingBottom {
			get { return padding.Bottom; }
			set {
				padding.Bottom = value;
				canvas.Resize ();
			}
		}

		public WidgetSpacing BorderWidth {
			get { return borderWidth; }
			set {
				borderWidth = value;
				canvas.Resize ();
			}
		}

		[DefaultValue (0d)]
		public double BorderWidthLeft {
			get { return borderWidth.Left; }
			set {
				borderWidth.Left = value;
				canvas.Resize ();
			}
		}

		[DefaultValue (0d)]
		public double BorderWidthRight {
			get { return borderWidth.Right; }
			set {
				borderWidth.Right = value;
				canvas.Resize ();
			}
		}

		[DefaultValue (0d)]
		public double BorderWidthTop {
			get { return borderWidth.Top; }
			set {
				borderWidth.Top = value;
				canvas.Resize ();
			}
		}

		[DefaultValue (0d)]
		public double BorderWidthBottom {
			get { return borderWidth.Bottom; }
			set {
				borderWidth.Bottom = value;
				canvas.Resize ();
			}
		}

		public Color BorderColor {
			get { return borderColor; }
			set { borderColor = value; canvas.QueueDraw (); }
		}

		/// <summary>
		/// Removes all children of the Frame
		/// </summary>
		public void Clear ()
		{
			Content = null;
		}

		[DefaultValue (null)]
		public new Widget Content {
			get { return canvas.Child; }
			set {
 				var current = canvas.Child;
				canvas.Child = null;
				UnregisterChild (current);
 				RegisterChild (value);
				canvas.Child = value; 
			}
		}

		protected Widget InternalContent {
			get {
				return canvas;
			}
		}
	}
}

