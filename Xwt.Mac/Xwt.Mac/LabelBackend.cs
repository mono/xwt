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
using MonoMac.AppKit;
using Xwt.Backends;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace Xwt.Mac
{
	public class LabelBackend: ViewBackend<NSView,IWidgetEventSink>, ILabelBackend
	{
		public LabelBackend ()
			: this (new TextFieldView ())
		{
		}

		protected LabelBackend (IViewObject viewObject)
		{
			ViewObject = new CustomAlignedContainer ((NSView)viewObject);
			Widget.Editable = false;
			Widget.Bezeled = false;
			Widget.DrawsBackground = false;
		}

		protected override void OnSizeToFit ()
		{
			Container.SizeToFit ();
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
				Widget.DrawsBackground = true;
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
				} else {
					Widget.Cell.Wraps = true;
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

	sealed class CustomAlignedContainer: NSView, IViewObject
	{
		public NSView Child;

		public CustomAlignedContainer (NSView child)
		{
			Child = child;
			AddSubview (child);
			UpdateTextFieldFrame ();
		}

		public ViewBackend Backend { get; set; }

		public NSView View {
			get { return this; }
		}

		static readonly Selector sizeToFitSel = new Selector ("sizeToFit");

		public void SizeToFit ()
		{
			if (Child.RespondsToSelector (sizeToFitSel))
				Messaging.void_objc_msgSend (Child.Handle, sizeToFitSel.Handle);
			else
				throw new NotSupportedException ();
			Frame = new System.Drawing.RectangleF (Frame.X, Frame.Y, Child.Frame.Width, Child.Frame.Height);
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

		public override void SetFrameSize (System.Drawing.SizeF newSize)
		{
			base.SetFrameSize (newSize);
			UpdateTextFieldFrame ();
		}

		void UpdateTextFieldFrame ()
		{
			if (expandVertically)
				Child.Frame = Frame;
			else
				Child.Frame = new System.Drawing.RectangleF (0, (Frame.Height - Child.Frame.Height) / 2, Frame.Width, Child.Frame.Height);
		}
	}
	
	class TextFieldView: NSTextField, IViewObject
	{
		public ViewBackend Backend { get; set; }
		public NSView View {
			get { return this; }
		}
	}
}

