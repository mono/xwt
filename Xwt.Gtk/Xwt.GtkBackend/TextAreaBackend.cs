// 
// TextAreaBackend.cs
// 
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//       Lytico (http://www.limada.org)
// 
// Copyright (c) 2014 Vsevolod Kukol
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
	public partial class TextAreaBackend : WidgetBackend, ITextAreaBackend
	{
		public override void Initialize ()
		{
			textView = new Gtk.TextView ();
			textView.Indent = 3;
			Widget = new Gtk.Frame ();
			((Gtk.Frame)Widget).Add (textView);
			ShowFrame = true;
			Wrap = WrapMode.None;
			Widget.ShowAll ();
		}

		Gtk.TextView textView;
		protected virtual Gtk.TextView TextView {
			get { return textView; }
		}
		
		protected new ITextBoxEventSink EventSink {
			get { return (ITextBoxEventSink)base.EventSink; }
		}

		public string Text {
			get { return TextView.Buffer.Text; }
			set { TextView.Buffer.Text = value; }
		}

		public Alignment TextAlignment {
			get { return TextView.Justification.ToXwtValue (); }
			set { TextView.Justification = value.ToGtkJustification (); }
		}
		
		public override Color BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = value;
				TextView.ModifyBase (Gtk.StateType.Normal, value.ToGtkValue ());
			}
		}

		Pango.Layout layout;
		public override object Font {
			get { return base.Font; }
			set {
				base.Font = value;
				TextView.ModifyFont ((Pango.FontDescription)value);
				layout = null;
			}
		}

		public bool ReadOnly {
			get {
				return TextView.Editable;
			}
			set {
				TextView.Editable = value;
				TextView.CursorVisible = !value;
			}
		}

		public WrapMode Wrap {
			get {
				switch (TextView.WrapMode) {
					case Gtk.WrapMode.Char:
						return WrapMode.Character;
					case Gtk.WrapMode.Word:
						return WrapMode.Word;
					case Gtk.WrapMode.WordChar:
						return WrapMode.WordAndCharacter;
					default:
						return WrapMode.None;
				}
			}
			set {
				switch (value) {
					case WrapMode.Character:
						TextView.WrapMode = Gtk.WrapMode.Char;
						break;
					case WrapMode.Word:
						TextView.WrapMode = Gtk.WrapMode.Word;
						break;
					case WrapMode.WordAndCharacter:
						TextView.WrapMode = Gtk.WrapMode.WordChar;
						break;
					default:
						TextView.WrapMode = Gtk.WrapMode.None;
						break;
				}
			}
		}

		public bool ShowFrame {
			get { return ((Gtk.Frame)Widget).ShadowType != Gtk.ShadowType.None; }
			set {
				if (value) {
					((Gtk.Frame)Widget).ShadowType = Gtk.ShadowType.In;
					((Gtk.Frame)Widget).BorderWidth = 2;
				} else {
					((Gtk.Frame)Widget).ShadowType = Gtk.ShadowType.None;
					((Gtk.Frame)Widget).BorderWidth = 0;
				}
			}
		}

		public int CursorPosition {
			get {
				var iter = TextView.Buffer.GetIterAtMark (TextView.Buffer.InsertMark);
				return iter.Offset;
			}
			set {
				var iter = TextView.Buffer.GetIterAtOffset (value);
				TextView.Buffer.PlaceCursor (iter);
			}
		}

		public int SelectionStart {
			get {
				Gtk.TextIter start, end;
				TextView.Buffer.GetSelectionBounds (out start, out end);
				return start.Offset;
			}
			set {
				Gtk.TextIter start, end;
				TextView.Buffer.GetSelectionBounds (out start, out end);
				var cacheLength = end.Offset - start.Offset;
				start.Offset = value;
				end.Offset = value + cacheLength;
				TextView.GrabFocus ();
				TextView.Buffer.SelectRange (start, end);
				HandleSelectionChanged ();
			}
		}

		public int SelectionLength {
			get {
				Gtk.TextIter start, end;
				if (!TextView.Buffer.GetSelectionBounds (out start, out end))
					return 0;
				return end.Offset - start.Offset;

			}
			set {
				Gtk.TextIter start, end;
				if (!TextView.Buffer.GetSelectionBounds (out start, out end)) {
					start = TextView.Buffer.GetIterAtMark (TextView.Buffer.InsertMark);
					end = start;
				}
				end.Offset = start.Offset + value;
				TextView.GrabFocus ();
				TextView.Buffer.SelectRange (start, end);
				HandleSelectionChanged ();
			}
		}

		public string SelectedText {
			get {
				Gtk.TextIter start, end;
				if (!TextView.Buffer.GetSelectionBounds (out start, out end))
					return String.Empty;
				return TextView.Buffer.GetText (start, end, true);
			}
			set {
				Gtk.TextIter start, end;
				int cachedOffset;
				if (!TextView.Buffer.GetSelectionBounds (out start, out end)) {
					start = TextView.Buffer.GetIterAtMark (TextView.Buffer.InsertMark);
					cachedOffset = start.Offset;
				} else {
					cachedOffset = start.Offset;
					TextView.Buffer.DeleteSelection (true, true);
					start = TextView.Buffer.GetIterAtOffset (cachedOffset);
				}
				TextView.Buffer.Insert (ref start, value);
				start.Offset = cachedOffset;
				end = start;
				end.Offset = start.Offset + value.Length;
				TextView.GrabFocus ();
				TextView.Buffer.SelectRange (start, end);
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TextBoxEvent) {
				switch ((TextBoxEvent)eventId) {
				case TextBoxEvent.Changed: TextView.Buffer.Changed += HandleChanged; break;
				case TextBoxEvent.Activated: TextView.KeyPressEvent += HandleKeyPress; break;
				case TextBoxEvent.SelectionChanged:
					enableSelectionChangedEvent = true;
					TextView.MoveCursor += HandleMoveCursor;
					TextView.ButtonPressEvent += HandleButtonPressEvent;
					TextView.ButtonReleaseEvent += HandleButtonReleaseEvent;
					TextView.MotionNotifyEvent += HandleMotionNotifyEvent;
					break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TextBoxEvent) {
				switch ((TextBoxEvent)eventId) {
				case TextBoxEvent.Changed: TextView.Buffer.Changed -= HandleChanged; break;
				case TextBoxEvent.Activated: TextView.KeyPressEvent -= HandleKeyPress; break;
				case TextBoxEvent.SelectionChanged:
					enableSelectionChangedEvent = false;
					TextView.MoveCursor -= HandleMoveCursor;
					TextView.ButtonPressEvent -= HandleButtonPressEvent;
					TextView.ButtonReleaseEvent -= HandleButtonReleaseEvent;
					TextView.MotionNotifyEvent -= HandleMotionNotifyEvent;
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

		[GLib.ConnectBefore]
		void HandleKeyPress (object sender, Gtk.KeyPressEventArgs e)
		{
			if (e.Event.Key == Gdk.Key.Return ||
			    e.Event.Key == Gdk.Key.ISO_Enter ||
			    e.Event.Key == Gdk.Key.KP_Enter)
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnActivated ();
				});
		}

		bool enableSelectionChangedEvent;
		void HandleSelectionChanged ()
		{
			if (enableSelectionChangedEvent)
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnSelectionChanged ();
				});
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

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				var l = layout;
				if (l != null) {
					l.Dispose ();
					layout = null;
				}
			}
			base.Dispose (disposing);
		}
	}
}

