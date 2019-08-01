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


namespace Xwt.Mac
{
	public class TextEntryBackend: ViewBackend<NSView,ITextBoxEventSink>, ITextEntryBackend, ITextAreaBackend
	{
		int cacheSelectionStart, cacheSelectionLength;
		bool checkMouseSelection;

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
				var view = new CustomTextField (EventSink, ApplicationContext);
				ViewObject = new CustomAlignedContainer (EventSink, ApplicationContext, (NSView)view);
				if (Frontend is Xwt.TextArea)
					MultiLine = true;
				else
					MultiLine = false;
				Wrap = WrapMode.None;
			}

			Frontend.MouseEntered += delegate {
				checkMouseSelection = true;
			};
			Frontend.MouseExited += delegate {
				checkMouseSelection = false;
				HandleSelectionChanged ();
			};
			Frontend.MouseMoved += delegate {
				if (checkMouseSelection)
					HandleSelectionChanged ();
			};
		}
		
		protected override void OnSizeToFit ()
		{
			Container.SizeToFit ();
		}

		CustomAlignedContainer Container {
			get { return base.Widget as CustomAlignedContainer; }
		}

		public new NSTextField Widget {
			get { return (ViewObject is MacComboBox) ? (NSTextField)ViewObject : (NSTextField) Container.Child; }
		}

		protected override Size GetNaturalSize ()
		{
			var s = base.GetNaturalSize ();
			return new Size (EventSink.GetDefaultNaturalSize ().Width, s.Height);
		}

		#region ITextEntryBackend implementation
		public string Text {
			get {
				return Widget.StringValue;
			}
			set {
				Widget.StringValue = value ?? string.Empty;
			}
		}

		public Alignment TextAlignment {
			get {
				return Widget.Alignment.ToAlignment ();
			}
			set {
				Widget.Alignment = value.ToNSTextAlignment ();
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

		public bool MultiLine {
			get {
				if (Widget is MacComboBox)
					return false;
				return Widget.Cell.UsesSingleLineMode;
			}
			set {
				if (Widget is MacComboBox)
					return;
				if (value) {
					Widget.Cell.UsesSingleLineMode = false;
					Widget.Cell.Scrollable = false;
				} else {
					Widget.Cell.UsesSingleLineMode = true;
					Widget.Cell.Scrollable = true;
				}
				Container.ExpandVertically = value;
			}
		}

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

		public int CursorPosition { 
			get {
				if (Widget.CurrentEditor == null)
					return 0;
				return Widget.CurrentEditor.SelectedRange.Location;
			}
			set {
				Widget.CurrentEditor.SelectedRange = new MonoMac.Foundation.NSRange (value, SelectionLength);
				HandleSelectionChanged ();
			}
		}

		public int SelectionStart { 
			get {
				if (Widget.CurrentEditor == null)
					return 0;
				return Widget.CurrentEditor.SelectedRange.Location;
			}
			set {
				Widget.CurrentEditor.SelectedRange = new MonoMac.Foundation.NSRange (value, SelectionLength);
				HandleSelectionChanged ();
			}
		}

		public int SelectionLength { 
			get {
				if (Widget.CurrentEditor == null)
					return 0;
				return Widget.CurrentEditor.SelectedRange.Length;
			}
			set {
				Widget.CurrentEditor.SelectedRange = new MonoMac.Foundation.NSRange (SelectionStart, value);
				HandleSelectionChanged ();
			}
		}

		public string SelectedText { 
			get {
				if (Widget.CurrentEditor == null)
					return String.Empty;
				int start = SelectionStart;
				int end = start + SelectionLength;
				if (start == end) return String.Empty;
				try {
					return Text.Substring (start, end - start);
				} catch {
					return String.Empty;
				}
			}
			set {
				int cacheSelStart = SelectionStart;
				int pos = cacheSelStart;
				if (SelectionLength > 0) {
					Text = Text.Remove (pos, SelectionLength).Insert (pos, value);
				}
				SelectionStart = pos;
				SelectionLength = value.Length;
				HandleSelectionChanged ();
			}
		}

		void HandleSelectionChanged ()
		{
			if (cacheSelectionStart != SelectionStart ||
			    cacheSelectionLength != SelectionLength) {
				cacheSelectionStart = SelectionStart;
				cacheSelectionLength = SelectionLength;
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnSelectionChanged ();
				});
			}
		}

		public override void SetFocus ()
		{
			Widget.BecomeFirstResponder ();
		}
		#endregion
	}
	
	class CustomTextField: NSTextField, IViewObject
	{
		ITextBoxEventSink eventSink;
		ApplicationContext context;
		
		public CustomTextField (ITextBoxEventSink eventSink, ApplicationContext context)
		{
			this.context = context;
			this.eventSink = eventSink;
		}
		
		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }
		
		public override void DidChange (MonoMac.Foundation.NSNotification notification)
		{
			base.DidChange (notification);
			context.InvokeUserCode (delegate {
				eventSink.OnChanged ();
				eventSink.OnSelectionChanged ();
			});
		}

		int cachedCursorPosition;
		public override void KeyUp (NSEvent theEvent)
		{
			base.KeyUp (theEvent);
			if (cachedCursorPosition != CurrentEditor.SelectedRange.Location)
				context.InvokeUserCode (delegate {
				eventSink.OnSelectionChanged ();
			});
			cachedCursorPosition = CurrentEditor.SelectedRange.Location;
		}
	}
}

