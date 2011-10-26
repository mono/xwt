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
		{
			ViewObject = new TextFieldView ();
			Widget.Editable = false;
			Widget.Bezeled = false;
			Widget.DrawsBackground = false;
			Widget.SizeToFit ();
		}

		public string Text {
			get {
				return Widget.StringValue;
			}
			set {
				Widget.StringValue = value;
				Widget.SizeToFit ();
			}
		}
		
		public Alignment HorizontalAlignment {
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
	}
	
	class TextFieldView: NSTextField, IViewObject<NSTextField>
	{
		public Widget Frontend { get; set; }
		public NSTextField View {
			get { return this; }
		}
	}
}

