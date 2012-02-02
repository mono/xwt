// 
// TextEntryBackend.cs
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
using MonoMac.AppKit;
using Xwt.Engine;

namespace Xwt.Mac
{
	public class TextEntryBackend: ViewBackend<NSTextField,ITextEntryEventSink>, ITextEntryBackend
	{
		public TextEntryBackend ()
		{
		}
		
		internal TextEntryBackend (MacComboBox field)
		{
			ViewObject = field;
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
			if (ViewObject is MacComboBox) {
				((MacComboBox)ViewObject).SetEntryEventSink (EventSink);
			} else {
				ViewObject = new CustomTextField (EventSink);
				Widget.SizeToFit ();
			}
		}

		#region ITextEntryBackend implementation
		public string Text {
			get {
				return Widget.StringValue;
			}
			set {
				Widget.StringValue = value;
			}
		}

		public bool ReadOnly {
			get {
				return !Widget.Editable;
			}
			set {
				Widget.Editable = !value;
			}
		}

		public bool ShowFrame {
			get {
				return Widget.Bordered;
			}
			set {
				Widget.Bordered = value;
			}
		}
		
		public string PlaceholderText {
			get {
				return ((NSTextFieldCell) Widget.Cell).PlaceholderString;
			}
			set {
				((NSTextFieldCell) Widget.Cell).PlaceholderString = value;
			}
		}
		#endregion
	}
	
	class CustomTextField: NSTextField, IViewObject<NSTextField>
	{
		ITextEntryEventSink eventSink;
		
		public CustomTextField (ITextEntryEventSink eventSink)
		{
			this.eventSink = eventSink;
		}
		
		public NSTextField View {
			get {
				return this;
			}
		}

		public Widget Frontend { get; set; }
		
		public override void DidChange (MonoMac.Foundation.NSNotification notification)
		{
			base.DidChange (notification);
			Toolkit.Invoke (delegate {
				eventSink.OnChanged ();
			});
		}
	}
}

