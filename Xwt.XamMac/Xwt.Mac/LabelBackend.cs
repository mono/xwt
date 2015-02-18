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

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGSize = System.Drawing.SizeF;
using CGRect = System.Drawing.RectangleF;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using MonoMac.CoreGraphics;
#else
using AppKit;
using ObjCRuntime;
using CoreGraphics;
#endif

namespace Xwt.Mac
{
	public class LabelBackend: ViewBackend<NSView,IWidgetEventSink>, ILabelBackend
	{

		public LabelBackend () : this (new TextFieldView ())
		{
		}

		protected LabelBackend (IViewObject view)
		{
			View = view;
		}

		IViewObject View;

		public override void Initialize ()
		{
			ViewObject = new CustomAlignedContainer (EventSink, ApplicationContext, (NSView)View);
			Widget.StringValue = string.Empty;
			Widget.Editable = false;
			Widget.Bezeled = false;
			Widget.DrawsBackground = false;
			Widget.BackgroundColor = NSColor.Clear;
			Wrap = WrapMode.None;
			Container.ExpandVertically = true;
			Widget.Cell.Scrollable = false;
		}

		public override Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			var r = new CGRect (0, 0, widthConstraint.IsConstrained ? (float)widthConstraint.AvailableSize : float.MaxValue, heightConstraint.IsConstrained ? (float)heightConstraint.AvailableSize : float.MaxValue);
			var s = Widget.Cell.CellSizeForBounds (r);
			return new Size (s.Width, s.Height);
		}

		CustomAlignedContainer Container {
			get { return (CustomAlignedContainer)base.Widget; }
		}

		public new NSTextField Widget {
			get { return (NSTextField) Container.Child; }
		}

		public virtual string Text {
			get {
				return Widget.StringValue;
			}
			set {
				Widget.StringValue = value ?? string.Empty;
				ResetFittingSize ();
			}
		}

		public void SetFormattedText (FormattedText text)
		{
			Widget.AllowsEditingTextAttributes = true;
			Widget.Selectable = true;
			Widget.AttributedStringValue = text.ToAttributedString ();
		}

		public Xwt.Drawing.Color TextColor {
			get { return Widget.TextColor.ToXwtColor (); }
			set { Widget.TextColor = value.ToNSColor (); }
		}
		
		public Alignment TextAlignment {
			get {
				return Widget.Alignment.ToAlignment ();
			}
			set {
				Widget.Alignment = value.ToNSTextAlignment ();
			}
		}
		
		public EllipsizeMode Ellipsize {
			get {
				switch (Widget.Cell.LineBreakMode) {
				case NSLineBreakMode.TruncatingHead: return Xwt.EllipsizeMode.Start;
				case NSLineBreakMode.TruncatingMiddle: return Xwt.EllipsizeMode.Middle;
				case NSLineBreakMode.TruncatingTail: return Xwt.EllipsizeMode.End;
				default: return Xwt.EllipsizeMode.None;
				}
			}
			set {
				switch (value) {
				case Xwt.EllipsizeMode.None: Widget.Cell.LineBreakMode = NSLineBreakMode.Clipping; break;
				case Xwt.EllipsizeMode.Start: Widget.Cell.LineBreakMode = NSLineBreakMode.TruncatingHead; break;
				case Xwt.EllipsizeMode.Middle: Widget.Cell.LineBreakMode = NSLineBreakMode.TruncatingMiddle; break;
				case Xwt.EllipsizeMode.End: Widget.Cell.LineBreakMode = NSLineBreakMode.TruncatingTail; break;
				}
			}
		}

		public override Xwt.Drawing.Color BackgroundColor {
			get { return Widget.BackgroundColor.ToXwtColor (); }
			set {
				Widget.BackgroundColor = value.ToNSColor ();
				((TextFieldView)Widget).SetBackgroundColor (value.ToCGColor ());
			}
		}

		// FIXME: technically not possible if we keep NSTextField
		// as the backend because it's one-line only
		public WrapMode Wrap {
			get {
				if (!Widget.Cell.Wraps)
					return WrapMode.None;
				switch (Widget.Cell.LineBreakMode) {
				case NSLineBreakMode.ByWordWrapping:
					return WrapMode.Word;
				case NSLineBreakMode.CharWrapping:
					return WrapMode.Character;
				default:
					return WrapMode.None;
				}
			}
			set {
				if (value == WrapMode.None) {
					Widget.Cell.Wraps = false;
					Widget.Cell.UsesSingleLineMode = true;
				} else {
					Widget.Cell.Wraps = true;
					Widget.Cell.UsesSingleLineMode = false;
					switch (value) {
					case WrapMode.Word:
					case WrapMode.WordAndCharacter:
						Widget.Cell.LineBreakMode = NSLineBreakMode.ByWordWrapping;
						break;
					case WrapMode.Character:
						Widget.Cell.LineBreakMode = NSLineBreakMode.CharWrapping;
						break;
					}
				}
			}
		}
	}

	sealed class CustomAlignedContainer: WidgetView
	{
		public NSView Child;

		public CustomAlignedContainer (IWidgetEventSink eventSink, ApplicationContext context, NSView child) : base (eventSink, context)
		{
			Child = child;
			AddSubview (child);
			UpdateTextFieldFrame ();
		}

		static readonly Selector sizeToFitSel = new Selector ("sizeToFit");

		public void SizeToFit ()
		{
			if (Child.RespondsToSelector (sizeToFitSel))
				Messaging.void_objc_msgSend (Child.Handle, sizeToFitSel.Handle);
			else
				throw new NotSupportedException ();
			Frame = new CGRect (Frame.X, Frame.Y, Child.Frame.Width, Child.Frame.Height);
		}

		bool expandVertically;
		public bool ExpandVertically {
			get {
				return expandVertically;
			}
			set {
				expandVertically = value;
				UpdateTextFieldFrame ();
			}
		}

		public override void SetFrameSize (CGSize newSize)
		{
			base.SetFrameSize (newSize);
			UpdateTextFieldFrame ();
		}

		void UpdateTextFieldFrame ()
		{
			if (expandVertically)
				Child.Frame = new CGRect (0, 0, Frame.Width, Frame.Height);
			else {
#if MONOMAC
				Child.Frame = new System.Drawing.RectangleF (0, (Frame.Height - Child.Frame.Height) / 2, Frame.Width, Child.Frame.Height);
#else
				Child.Frame = new CGRect (0, (Frame.Height - Child.Frame.Height) / 2, Frame.Width, Child.Frame.Height);
#endif
			}
			Child.NeedsDisplay = true;
		}
	}
	
	class TextFieldView: NSTextField, IViewObject
	{
		CustomTextFieldCell cell;

		public ViewBackend Backend { get; set; }
		public NSView View {
			get { return this; }
		}

		public TextFieldView ()
		{
			Cell = cell = new CustomTextFieldCell ();
		}

		public void SetBackgroundColor (CGColor c)
		{
			cell.SetBackgroundColor (c);
		}

		public override bool AcceptsFirstResponder ()
		{
			return false;
		}
	}

	class CustomTextFieldCell: NSTextFieldCell
	{
		CGColor bgColor;

		public void SetBackgroundColor (CGColor c)
		{
			bgColor = c;
			DrawsBackground = true;
		}

		public override CGRect TitleRectForBounds (CGRect theRect)
		{
			var rect = base.TitleRectForBounds (theRect);
			var textSize = CellSizeForBounds (theRect);
			nfloat dif = rect.Height - textSize.Height;	
			if (dif > 0) {
				rect.Height -= dif;
				rect.Y += (dif / 2);
			}
			return rect;
		}

		public override void DrawInteriorWithFrame (CGRect cellFrame, NSView inView)
		{
			if (DrawsBackground) {
				CGContext ctx = NSGraphicsContext.CurrentContext.GraphicsPort;
				ctx.AddRect (cellFrame);
				ctx.SetFillColorSpace (Util.DeviceRGBColorSpace);
				ctx.SetStrokeColorSpace (Util.DeviceRGBColorSpace);
				ctx.SetFillColor (bgColor);
				ctx.DrawPath (CGPathDrawingMode.Fill);
			}
			base.DrawInteriorWithFrame (TitleRectForBounds (cellFrame), inView);
		}
	}
}

