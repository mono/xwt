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
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class TextEntryBackend: ViewBackend<NSView,ITextEntryEventSink>, ITextEntryBackend
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
			} else if (ViewObject == null) {
				var view = new CustomTextField (EventSink, ApplicationContext) { Backend = this };
				ViewObject = new CustomAlignedContainer (EventSink, ApplicationContext, (NSView)view) { DrawsBackground = false };
				Container.ExpandVertically = true;
				MultiLine = false;
			}
			Widget.StringValue = string.Empty;

			canGetFocus = Widget.AcceptsFirstResponder ();
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
				if (value)
					Widget.AbortEditing ();
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
				return !Widget.Cell.UsesSingleLineMode;
			}
			set {
				if (Widget is MacComboBox)
					return;
				if (value) {
					Widget.Cell.UsesSingleLineMode = false;
					Widget.Cell.Scrollable = false;
					Widget.Cell.Wraps = true;
				} else {
					Widget.Cell.UsesSingleLineMode = true;
					Widget.Cell.Scrollable = true;
					Widget.Cell.Wraps = false;
				}
			}
		}

		public int CursorPosition { 
			get {
				if (Widget.CurrentEditor == null)
					return 0;
				return (int)Widget.CurrentEditor.SelectedRange.Location;
			}
			set {
				Widget.CurrentEditor.SelectedRange = new NSRange (value, SelectionLength);
				HandleSelectionChanged ();
			}
		}

		public int SelectionStart { 
			get {
				if (Widget.CurrentEditor == null)
					return 0;
				return (int)Widget.CurrentEditor.SelectedRange.Location;
			}
			set {
				Widget.CurrentEditor.SelectedRange = new NSRange (value, SelectionLength);
				HandleSelectionChanged ();
			}
		}

		public int SelectionLength { 
			get {
				if (Widget.CurrentEditor == null)
					return 0;
				return (int)Widget.CurrentEditor.SelectedRange.Length;
			}
			set {
				Widget.CurrentEditor.SelectedRange = new NSRange (SelectionStart, value);
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
                ApplicationContext.InvokeUserCode (EventSink.OnSelectionChanged);
			}
		}

		string[] completions;
		Func<string, string, bool> completionsMatchFunc;

		public bool HasCompletions {
			get {
				return completions?.Length > 0;
			}
		}

		public void SetCompletions (string[] completions)
		{
			this.completions = completions;
			if (completions != null) {
				var entryDelegate = Widget.Delegate;
				if (entryDelegate == null) {
					entryDelegate = Widget.Delegate = new TextFieldDelegate ();
				}
				if (entryDelegate is TextFieldDelegate) {
					((TextFieldDelegate)entryDelegate).Backend = this;
				}
			}
			if (completionsMatchFunc == null) {
				completionsMatchFunc = DefaultCompletionMatchFunc;
			}
		}

		public void SetCompletionMatchFunc (Func<string, string, bool> matchFunc)
		{
			completionsMatchFunc = matchFunc ?? DefaultCompletionMatchFunc;
		}

		bool DefaultCompletionMatchFunc (string word, string completion)
		{
			if (word == null || completion == null)
				return false;
			return completion.StartsWith (word, StringComparison.CurrentCulture);
		}

		internal string [] GetCompletions (string word)
		{
			if (completions?.Length > 0)
			{
				return completions.Where (c => completionsMatchFunc(word, c)).ToArray ();
			}
			return new string[0];
		}

		#endregion
	

		#region Gross Hack
		// The 'Widget' property is not virtual and the one on the base class holds
		// the 'CustomAlignedContainer' object and *not* the NSTextField object. As
		// such everything that uses the 'Widget' property in the base class might be
		// working on the wrong object. The focus methods definitely need to work on
		// the NSTextField directly, so i've overridden those and made them interact
		// with the NSTextField instead of the CustomAlignedContainer.
		bool canGetFocus = true;
		public override bool CanGetFocus {
			get { return canGetFocus; }
			set { canGetFocus = value && Widget.AcceptsFirstResponder (); }
		}

		public override void SetFocus ()
		{
			if (Widget.Window != null && CanGetFocus)
				Widget.Window.MakeFirstResponder (Widget);
		}

		public override bool HasFocus {
			get {
				return Widget.Window != null && Widget.Window.FirstResponder == Widget;
			}
		}
		#endregion

		public override Drawing.Color BackgroundColor {
			get {
				return Widget.BackgroundColor.ToXwtColor ();
			}
			set {
				Widget.BackgroundColor = value.ToNSColor ();
				Widget.Cell.DrawsBackground = true;
				Widget.Cell.BackgroundColor = value.ToNSColor ();
			}
		}

		protected override void Dispose(bool disposing)
		{
			completions = null;
			base.Dispose (disposing);
		}
	}

	class TextFieldDelegate : NSTextFieldDelegate
	{
		WeakReference weakBackend;

		public TextEntryBackend Backend
		{
			get { return weakBackend?.Target as TextEntryBackend; }
			set { weakBackend = new WeakReference (value); }
		}

		public override string[] GetCompletions (NSControl control, NSTextView textView, string[] words, NSRange charRange, ref nint index)
		{
			var backend = Backend;
			if (backend != null)
			{
				string word;
				try {
					word = textView.String.Substring ((int)charRange.Location, (int)charRange.Length);
				} catch (ArgumentOutOfRangeException) {
					return new string[0];
				}
				return backend.GetCompletions (word);
			}
			return new string[0];
		}

		bool isCompleting;
		[Export("controlTextDidChange:")]
		public void DidChange (NSNotification notification)
		{
			var editor = notification.Object as NSTextView ?? (notification.Object as NSTextField)?.CurrentEditor as NSTextView;
			if (!isCompleting && editor != null && editor.String.Length > 0 && Backend?.HasCompletions == true) {
				// Cocoa will call DidChange for each completion, even if the text didn't change
				// avoid an infinite loop with an isCompleting check.
				isCompleting = true;
				editor.Complete (null);
				isCompleting = false;
			}
			if (Backend.EventSink != null) {
				Backend.ApplicationContext.InvokeUserCode (delegate {
					Backend.EventSink.OnChanged ();
					Backend.EventSink.OnSelectionChanged ();
				});
			}
		}

		protected override void Dispose(bool disposing)
		{
			weakBackend = null;
			base.Dispose(disposing);
		}
	}
	
	class CustomTextField: NSTextField, IViewObject
	{
		ITextEntryEventSink eventSink;
		ApplicationContext context;
		#pragma warning disable CS0414 // The private field is assigned but its value is never used
		CustomCell cell;
		#pragma warning disable CS0414

		public CustomTextField (ITextEntryEventSink eventSink, ApplicationContext context)
		{
			this.context = context;
			this.eventSink = eventSink;
			this.Cell = cell = new CustomCell {
				BezelStyle = NSTextFieldBezelStyle.Square,
				Bezeled = true,
				Editable = true,
				EventSink = eventSink,
				Context = context,
			};
		}

		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }

		public override string StringValue
		{
			get { return base.StringValue; }
			set {
				if (base.StringValue != value)
				{
					base.StringValue = value;
					context.InvokeUserCode (delegate
					{
						eventSink.OnChanged ();
						eventSink.OnSelectionChanged ();
					});
				}
			}
		}

		class CustomCell : NSTextFieldCell
		{
			NSTextView editor;
			NSObject selChangeObserver;
			public ApplicationContext Context {
				get; set;
			}

			public ITextEntryEventSink EventSink {
				get; set;
			}

			public CustomCell ()
			{

			}

			protected CustomCell(NativeHandle ptr) : base(ptr)
			{
			}

			/// <summary>
			/// Like what happens for the ios designer, AppKit can sometimes clone the native `NSTextFieldCell` using the Copy (NSZone)
			/// method. We *need* to ensure we can create a new managed wrapper for the cloned native object so we need the IntPtr
			/// constructor. NOTE: By keeping this override in managed we ensure the new wrapper C# object is created ~immediately,
			/// which makes it easier to debug issues.
			/// </summary>
			/// <returns>The copy.</returns>
			/// <param name="zone">Zone.</param>
			public override NSObject Copy(NSZone zone)
			{
				// Don't remove this override because the comment on this explains why we need this!
				var newCell = (CustomCell)base.Copy(zone);
				newCell.editor = editor;
				newCell.selChangeObserver = selChangeObserver;
				newCell.Context = Context;
				newCell.EventSink = EventSink;
				return newCell;
			}

			public override NSTextView FieldEditorForView (NSView aControlView)
			{
				if (editor == null) {
					editor = new CustomTextFieldCellEditor {
						Context = this.Context,
						EventSink = this.EventSink,
						FieldEditor = true,
						Editable = true,
					};
					using (var key = new NSString("NSTextViewDidChangeSelectionNotification"))
						selChangeObserver = NSNotificationCenter.DefaultCenter.AddObserver (key, HandleSelectionDidChange, editor);
				}
				return editor;
			}

			void HandleSelectionDidChange (NSNotification notif)
			{
				Context.InvokeUserCode (EventSink.OnSelectionChanged);
			}

			public override void DrawInteriorWithFrame (CGRect cellFrame, NSView inView)
			{
				base.DrawInteriorWithFrame (VerticalCenteredRectForBounds(cellFrame), inView);
			}

			public override void EditWithFrame (CGRect aRect, NSView inView, NSText editor, NSObject delegateObject, NSEvent theEvent)
			{
				base.EditWithFrame (VerticalCenteredRectForBounds(aRect), inView, editor, delegateObject, theEvent);
			}

			public override void SelectWithFrame (CGRect aRect, NSView inView, NSText editor, NSObject delegateObject, nint selStart, nint selLength)
			{
				base.SelectWithFrame (VerticalCenteredRectForBounds(aRect), inView, editor, delegateObject, selStart, selLength);
			}

			CGRect VerticalCenteredRectForBounds (CGRect aRect)
			{
				// multiline entries should always align on top
				if (!UsesSingleLineMode)
					return aRect;

				var textHeight = CellSizeForBounds (aRect).Height;
				var offset = (aRect.Height - textHeight) / 2;
				if (offset <= 0) // do nothing if the frame is too small
					return aRect;
				var rect = new Rectangle (aRect.X, aRect.Y, aRect.Width, aRect.Height).Inflate (0.0, -offset);
				return rect.ToCGRect ();
			}
		}
	}

	class CustomTextFieldCellEditor : NSTextView
	{
		public ApplicationContext Context
		{
			get; set;
		}

		public ITextEntryEventSink EventSink
		{
			get; set;
		}

		public override void KeyDown(NSEvent theEvent)
		{
			Context.InvokeUserCode(delegate {
				EventSink.OnKeyPressed(theEvent.ToXwtKeyEventArgs());
			});
			base.KeyDown(theEvent);
		}

		nint cachedCursorPosition;
		public override void KeyUp(NSEvent theEvent)
		{
			if (cachedCursorPosition != SelectedRange.Location)
			{
				cachedCursorPosition = SelectedRange.Location;
				Context.InvokeUserCode(delegate {
					EventSink.OnSelectionChanged();
					EventSink.OnKeyReleased(theEvent.ToXwtKeyEventArgs());
				});
			}
			base.KeyUp(theEvent);
		}

		public override bool BecomeFirstResponder()
		{
			var result = base.BecomeFirstResponder();
			if (result)
			{
				Context.InvokeUserCode(() => {
					EventSink.OnGotFocus();
				});
			}
			return result;
		}

		public override bool ResignFirstResponder()
		{
			var result = base.ResignFirstResponder();
			if (result)
			{
				Context.InvokeUserCode(() => {
					EventSink.OnLostFocus();
				});
			}
			return result;
		}
	}
}
