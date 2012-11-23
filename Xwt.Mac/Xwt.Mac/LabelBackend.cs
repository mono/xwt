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

namespace Xwt.Mac
{
	public class LabelBackend: ViewBackend<NSTextField,IWidgetEventSink>, ILabelBackend
	{
		public LabelBackend ()
			: this (new TextFieldView ())
		{
		}

		protected LabelBackend (IViewObject viewObject)
		{
			ViewObject = viewObject;
			Widget.Editable = false;
			Widget.Bezeled = false;
			Widget.DrawsBackground = false;
			Widget.SizeToFit ();
		}

		public virtual string Text {
			get {
				return Widget.StringValue;
			}
			set {
				Widget.StringValue = value;
				Widget.SizeToFit ();
			}
		}

		public Xwt.Drawing.Color TextColor {
			get { return Widget.TextColor.ToXwtColor (); }
			set { Widget.TextColor = value.ToNSColor (); }
		}
		
		public Alignment TextAlignment {
			get {
				switch (Widget.Alignment) {
				case NSTextAlignment.Left: return Alignment.Start;
				case NSTextAlignment.Center: return Alignment.Center;
				case NSTextAlignment.Right: return Alignment.End;
				}
				return Alignment.Start;
			}
			set {
				switch (value) {
				case Alignment.Start: Widget.Alignment = NSTextAlignment.Left; break;
				case Alignment.Center: Widget.Alignment = NSTextAlignment.Center; break;
				case Alignment.End: Widget.Alignment = NSTextAlignment.Right; break;
				}
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
	
	class TextFieldView: NSTextField, IViewObject
	{
		public Widget Frontend { get; set; }
		public NSView View {
			get { return this; }
		}
	}
}

