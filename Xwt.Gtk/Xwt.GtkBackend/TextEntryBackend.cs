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
using Xwt.Drawing;


namespace Xwt.GtkBackend
{
	public partial class TextEntryBackend : WidgetBackend, ITextEntryBackend
	{
		public override void Initialize ()
		{
			Widget = new Gtk.Entry ();
			Widget.Show ();
		}

		protected virtual Gtk.Entry TextEntry {
			get { return (Gtk.Entry)base.Widget; }
		}
		
		protected new Gtk.Entry Widget {
			get { return TextEntry; }
			set { base.Widget = value; }
		}
		
		protected new ITextEntryEventSink EventSink {
			get { return (ITextEntryEventSink)base.EventSink; }
		}

		public string Text {
			get { return Widget.Text; }
			set { Widget.Text = value ?? ""; } // null value causes GTK error
		}

		public Alignment TextAlignment {
			get {
				if (Widget.Xalign == 0)
					return Alignment.Start;
				else if (Widget.Xalign == 1)
					return Alignment.End;
				else
					return Alignment.Center;
			}
			set {
				switch (value) {
				case Alignment.Start: Widget.Xalign = 0; break;
				case Alignment.End: Widget.Xalign = 1; break;
				case Alignment.Center: Widget.Xalign = 0.5f; break;
				}
			}
		}

		public override Color BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = value;
				Widget.ModifyBase (Gtk.StateType.Normal, value.ToGtkValue ());
			}
		}

		public bool ReadOnly {
			get {
				return !Widget.IsEditable;
			}
			set {
				Widget.IsEditable = !value;
			}
		}
		
		public virtual bool ShowFrame {
			get {
				return Widget.HasFrame;
			}
			set {
				Widget.HasFrame = value;
			}
		}

		public int CursorPosition {
			get {
				return Widget.Position;
			}
			set {
				Widget.Position = value;
			}
		}

		public int SelectionStart {
			get {
				int start, end;
				Widget.GetSelectionBounds (out start, out end);
				return start;
			}
			set {
				int cacheStart = SelectionStart;
				int cacheLength = SelectionLength;
				Widget.GrabFocus ();
				if (String.IsNullOrEmpty (Text))
					return;
				Widget.SelectRegion (value, value + cacheLength);
				if (cacheStart != value)
					HandleSelectionChanged ();
			}
		}

		public int SelectionLength {
			get {
				int start, end;
				Widget.GetSelectionBounds (out start, out end);
				return end - start;
			}
			set {
				int cacheStart = SelectionStart;
				int cacheLength = SelectionLength;
				Widget.GrabFocus ();
				if (String.IsNullOrEmpty (Text))
					return;
				Widget.SelectRegion (cacheStart, cacheStart + value);
				if (cacheLength != value)
					HandleSelectionChanged ();
			}
		}

		public string SelectedText {
			get {
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
					Widget.DeleteSelection ();
				}
				Widget.InsertText (value, ref pos);
				Widget.GrabFocus ();
				Widget.SelectRegion (cacheSelStart, pos);
				HandleSelectionChanged ();
			}
		}

		public bool MultiLine {
			get; set;
		}

		public bool HasCompletions {
			get {
				return Widget?.Completion?.Model != null;
			}
		}

		/// <summary>
		/// Set the list of completions that will be shown by the entry
		/// </summary>
		/// <param name="completions">The list of completion or null if no completions should be shown</param>
		public void SetCompletions (string[] completions)
		{
			var widgetCompletion = Widget.Completion;

			if (completions == null || completions.Length == 0) {
				if (widgetCompletion != null)
					widgetCompletion.Model = null;
				return;
			}

			if (widgetCompletion == null)
				Widget.Completion = widgetCompletion = CreateCompletion ();

			var model = new Gtk.ListStore (typeof(string));
			foreach (var c in completions)
				model.SetValue (model.Append (), 0, c);
			widgetCompletion.Model = model;
		}

		/// <summary>
		/// Set a custom matching function used to decide which completion from SetCompletions list are shown
		/// for the given input
		/// </summary>
		/// <param name="matchFunc">A function which parameter are, in order, the current text entered by the user and a completion candidate.
		/// Returns true if the candidate should be included in the completion list.</param>
		public void SetCompletionMatchFunc (Func<string, string, bool> matchFunc)
		{
			var widgetCompletion = Widget.Completion;
			if (widgetCompletion == null)
				Widget.Completion = widgetCompletion = CreateCompletion ();
			widgetCompletion.MatchFunc = delegate (Gtk.EntryCompletion completion, string key, Gtk.TreeIter iter) {
				var completionText = (string)completion.Model.GetValue (iter, 0);
				return matchFunc (key, completionText);
			};
		}

		Gtk.EntryCompletion CreateCompletion ()
		{
			return new Gtk.EntryCompletion () {
				PopupCompletion = true,
				TextColumn = 0,
				InlineCompletion = true,
				InlineSelection = true
			};
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
				case TextEntryEvent.Changed: Widget.Changed += HandleChanged; break;
				case TextEntryEvent.Activated: Widget.Activated += HandleActivated; break;
				case TextEntryEvent.SelectionChanged:
					enableSelectionChangedEvent = true;
					Widget.MoveCursor += HandleMoveCursor;
					Widget.ButtonPressEvent += HandleButtonPressEvent;
					Widget.ButtonReleaseEvent += HandleButtonReleaseEvent;
					Widget.MotionNotifyEvent += HandleMotionNotifyEvent;
					break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
				case TextEntryEvent.Changed: Widget.Changed -= HandleChanged; break;
				case TextEntryEvent.Activated: Widget.Activated -= HandleActivated; break;
				case TextEntryEvent.SelectionChanged:
					enableSelectionChangedEvent = false;
					Widget.MoveCursor -= HandleMoveCursor;
					Widget.ButtonPressEvent -= HandleButtonPressEvent;
					Widget.ButtonReleaseEvent -= HandleButtonReleaseEvent;
					Widget.MotionNotifyEvent -= HandleMotionNotifyEvent;
					break;
				}
			}
		}

		void HandleChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnChanged ();
				EventSink.OnSelectionChanged ();
			});
		}

		void HandleActivated (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (EventSink.OnActivated);
		}

		bool enableSelectionChangedEvent;
		void HandleSelectionChanged ()
		{
			if (enableSelectionChangedEvent)
				ApplicationContext.InvokeUserCode (EventSink.OnSelectionChanged);
		}

		void HandleMoveCursor (object sender, EventArgs e)
		{
			HandleSelectionChanged ();
		}

		int cacheSelectionStart, cacheSelectionLength;
		bool isMouseSelection;

		[GLib.ConnectBefore]
		void HandleButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 1) {
				HandleSelectionChanged ();
				cacheSelectionStart = SelectionStart;
				cacheSelectionLength = SelectionLength;
				isMouseSelection = true;
			}
		}

		[GLib.ConnectBefore]
		void HandleMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			if (isMouseSelection)
			if (cacheSelectionStart != SelectionStart || cacheSelectionLength != SelectionLength)
				HandleSelectionChanged ();
			cacheSelectionStart = SelectionStart;
			cacheSelectionLength = SelectionLength;
		}

		[GLib.ConnectBefore]
		void HandleButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 1) {
				isMouseSelection = false;
				HandleSelectionChanged ();
			}
		}
	}
}

