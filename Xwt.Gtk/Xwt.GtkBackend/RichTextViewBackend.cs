//
// RichTextViewBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Collections.Generic;

using Xwt;
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public class RichTextViewBackend : WidgetBackend, IRichTextViewBackend
	{
		Gtk.TextTagTable table;

		double ParagraphSpacing {
			get {
				var font = Font as Pango.FontDescription;
				var size = font.SizeIsAbsolute ? font.Size : font.Size / Pango.Scale.PangoScale;
				return size / 2; // default to 1/2 of the font/line size
			}
		}

		public RichTextViewBackend ()
		{
			Widget = new GtkTextView ();
			Widget.Show ();
			ReadOnly = true;
			Widget.WrapMode = Gtk.WrapMode.Word;
			InitTagTable ();
		}

		void InitTagTable ()
		{
			table = new Gtk.TextTagTable ();
			table.Add (new Gtk.TextTag ("bold") {
				Weight = Pango.Weight.Bold
			});
			table.Add (new Gtk.TextTag ("italic") {
				Style = Pango.Style.Italic
			});
			table.Add (new Gtk.TextTag ("tt") {
				Family = "Monospace"
			});
			table.Add (new Gtk.TextTag ("li") {
				LeftMargin = 14
			});
			table.Add (new Gtk.TextTag ("h1") {
				Weight = Pango.Weight.Bold,
				Scale = Pango.Scale.XXLarge
			});
			table.Add (new Gtk.TextTag ("h2") {
				Weight = Pango.Weight.Bold,
				Scale = Pango.Scale.XLarge
			});
			table.Add (new Gtk.TextTag ("h3") {
				Weight = Pango.Weight.Bold,
				Scale = Pango.Scale.Large
			});
			table.Add (new Gtk.TextTag ("h4") {
				Scale = Pango.Scale.Large
			});
			table.Add (new Gtk.TextTag ("pre") {
				Family = "Monospace",
				Indent = 14,
				WrapMode = Gtk.WrapMode.None
			});
			table.Add (new Gtk.TextTag ("p") {
				SizePoints = Math.Max (LineSpacing, ParagraphSpacing),
			});
			
			table.Add(new Gtk.TextTag ("textColor"));
		}

		private new GtkTextView Widget {
			get {
				return (GtkTextView)base.Widget;
			}
			set {
				base.Widget = value;
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is RichTextViewEvent) {
				switch ((RichTextViewEvent) eventId) {
				case RichTextViewEvent.NavigateToUrl:
					Widget.NavigateToUrl += HandleNavigateToUrl;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is RichTextViewEvent) {
				switch ((RichTextViewEvent) eventId) {
				case RichTextViewEvent.NavigateToUrl:
					Widget.NavigateToUrl -= HandleNavigateToUrl;
					break;
				}
			}
		}

		public IRichTextBuffer CreateBuffer ()
		{
			return new RichTextBuffer (table);
		}

		public void SetBuffer (IRichTextBuffer buffer)
		{
			var buf = buffer as RichTextBuffer;
			if (buf == null)
				throw new ArgumentException ("Passed buffer is of incorrect type", "buffer");

			Widget.Buffer = buf;
		}

		public IRichTextBuffer CurrentBuffer {
			get {
				return Widget.Buffer as RichTextBuffer;
			}
		}

		public bool ReadOnly { 
			get { 
				return !Widget.Editable;
			}
			set {
				Widget.Editable = !value;
				Widget.CursorVisible = !value;
			}
		}

		public bool Selectable {
			get {
				return Widget.Selectable;
			}
			set {
				Widget.Selectable = value;
			}
		}

		public int LineSpacing {
			get {
				return Widget.PixelsInsideWrap;
			}
			set {
				Widget.PixelsInsideWrap = value;
				Widget.PixelsBelowLines = value;
				var tag = table.Lookup ("p");
				tag.SizePoints = Math.Max (value, ParagraphSpacing);
			}
		}

		public Drawing.Color TextColor {
			get {
				var tag = table.Lookup ("textColor");
				return tag.ForegroundGdk.ToXwtValue ();
			}
			set {
				var tag = table.Lookup ("textColor");
				tag.ForegroundGdk = value.ToGtkValue ();
				var buffer = Widget.Buffer;
				buffer.ApplyTag (tag, buffer.StartIter, buffer.EndIter);
			}
		}

		public override object Font {
			get {
				return base.Font;
			}
			set {
				base.Font = value;
				var tag = table.Lookup ("p");
				tag.SizePoints = Math.Max (LineSpacing, ParagraphSpacing);
			}
		}

		protected override void OnSetBackgroundColor(Drawing.Color color)
		{
			base.OnSetBackgroundColor(color);
			Widget.SetBackgroundColor(Gtk.StateType.Normal, color);
			Widget.SetBackgroundColor(Gtk.StateType.Insensitive, color);
			#if !XWT_GTK3
			Widget.ModifyBase(Gtk.StateType.Normal, color.ToGtkValue());
			Widget.ModifyBase(Gtk.StateType.Insensitive, color.ToGtkValue());
			#endif
		}

		void HandleNavigateToUrl (object sender, NavigateToUrlEventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				((IRichTextViewEventSink)EventSink).OnNavigateToUrl (e.Uri);
			});
		}

		class Link {
			public string Title;
			public Uri Href;
			public Gtk.TextMark StartMark;
		}

		class RichTextBuffer : Gtk.TextBuffer, IRichTextBuffer
		{
			const string NewLine = "\n";

			bool needsParagraphBreak;
			bool needsListBreak;

			void BreakParagraph ()
			{
				if (needsParagraphBreak) {
					var iterEnd = EndIter;
					Insert (ref iterEnd, NewLine);

					var m = CreateMark (null, iterEnd, true);
					Insert (ref iterEnd, NewLine);
					var iterStart = GetIterAtMark (m);
					ApplyTag ("p", iterStart, iterEnd);
					DeleteMark (m);

					needsParagraphBreak = false;
					return;
				}
			}

			void BreakList ()
			{
				if (needsListBreak) {
					var end = EndIter;
					Insert (ref end, NewLine);
					needsListBreak = false;
					return;
				}
			}

			public Dictionary<Gtk.TextTag, Link> Links {
				get; private set;
			}

			struct StartState {
				public Gtk.TextMark Mark;
				public int Data;
				public StartState (Gtk.TextMark mark, int data)
				{
					Mark = mark;
					Data = data;
				}
			}
			Stack<StartState> openHeaders;
			Stack<Link> openLinks;

			public RichTextBuffer (Gtk.TextTagTable table) : base (table)
			{
				Links = new Dictionary<Gtk.TextTag, Link> ();
				openHeaders = new Stack<StartState> ();
				openLinks = new Stack<Link> ();
			}

			public string PlainText {
				get {
					return Text;
				}
			}

			public void EmitText (string text, RichTextInlineStyle style)
			{
				var iterEnd = EndIter;
				var m = CreateMark (null, iterEnd, true);
				Insert (ref iterEnd, text);
				var iterStart = GetIterAtMark (m);

				if ((style & RichTextInlineStyle.Bold) != 0)
					ApplyTag ("bold", iterStart, iterEnd);

				if ((style & RichTextInlineStyle.Italic) != 0)
					ApplyTag ("italic", iterStart, iterEnd);

				if ((style & RichTextInlineStyle.Monospace) != 0)
					ApplyTag ("tt", iterStart, iterEnd);

				DeleteMark (m);
			}

			public void EmitStartHeader (int level)
			{
				BreakParagraph ();
				var iter = EndIter;
				openHeaders.Push (new StartState (CreateMark (null, iter, true), level));
			}

			public void EmitEndHeader ()
			{
				var start = openHeaders.Pop ();
				var iterStart = GetIterAtMark (start.Mark);
				var iterEnd = EndIter;
				ApplyTag ("h" + start.Data, iterStart, iterEnd);
				DeleteMark (start.Mark);
				needsParagraphBreak = true;
			}

			public void EmitStartParagraph (int indentLevel)
			{
				BreakParagraph ();
				//FIXME: support indentLevel
			}

			public void EmitEndParagraph ()
			{
				needsParagraphBreak = true;
			}

			public void EmitOpenList ()
			{
				BreakParagraph ();
			}

			public void EmitOpenBullet ()
			{
				BreakList ();
				var iter = EndIter;
				InsertWithTagsByName (ref iter, "• ", "li");
			}

			public void EmitCloseBullet ()
			{
				needsListBreak = true;
			}

			public void EmitCloseList ()
			{
				needsListBreak = false;
				needsParagraphBreak = true;
			}

			public void EmitStartLink (string href, string title)
			{
				var iter = EndIter;

				Uri uri;
				if (!Uri.TryCreate (href, UriKind.RelativeOrAbsolute, out uri))
					uri = null;

				openLinks.Push (new Link ()
				{
					Title = title,
					Href = uri,
					StartMark = CreateMark (null, iter, true),
				});
			}

			public void EmitEndLink ()
			{
				var link = openLinks.Pop ();
				var tag = new Gtk.TextTag (null);
				tag.Underline = Pango.Underline.Single;
				tag.ForegroundGdk = Toolkit.CurrentEngine.Defaults.FallbackLinkColor.ToGtkValue ();
				TagTable.Add (tag);
				ApplyTag (tag, GetIterAtMark (link.StartMark), EndIter);
				Links[tag] = link;
			}

			public void EmitCodeBlock (string code)
			{
				BreakParagraph ();
				var iter = EndIter;
				InsertWithTagsByName (ref iter, code.Trim (), "pre");
				needsParagraphBreak = true;
			}

			public void EmitHorizontalRuler ()
			{
				//FIXME
			}

			public void EmitText (FormattedText markup)
			{
				var iterEnd = EndIter;
				var textmark = CreateMark (null, iterEnd, true);
				Insert (ref iterEnd, markup.Text);

				foreach (var attr in markup.Attributes) {
					var iterEndAttr = GetIterAtMark (textmark);
					iterEndAttr.ForwardChars (attr.StartIndex);
					var attrStart = CreateMark (null, iterEndAttr, true);
					iterEndAttr.ForwardChars (attr.Count);

					var tag = new Gtk.TextTag (null);

					if (attr is BackgroundTextAttribute) {
						var xa = (BackgroundTextAttribute)attr;
						tag.BackgroundGdk = xa.Color.ToGtkValue ();
					} else if (attr is ColorTextAttribute) {
						var xa = (ColorTextAttribute)attr;
						tag.ForegroundGdk = xa.Color.ToGtkValue ();
					} else if (attr is FontWeightTextAttribute) {
						var xa = (FontWeightTextAttribute)attr;
						tag.Weight = (Pango.Weight)(int)xa.Weight;
					} else if (attr is FontStyleTextAttribute) {
						var xa = (FontStyleTextAttribute)attr;
						tag.Style = (Pango.Style)(int)xa.Style;
					} else if (attr is UnderlineTextAttribute) {
						var xa = (UnderlineTextAttribute)attr;
						tag.Underline = xa.Underline ? Pango.Underline.Single : Pango.Underline.None;
					} else if (attr is StrikethroughTextAttribute) {
						var xa = (StrikethroughTextAttribute)attr;
						tag.Strikethrough = xa.Strikethrough;
					} else if (attr is FontTextAttribute) {
						var xa = (FontTextAttribute)attr;
						tag.FontDesc = (Pango.FontDescription)Toolkit.GetBackend (xa.Font);
					} else if (attr is LinkTextAttribute) {
						var xa = (LinkTextAttribute)attr;
						Uri uri = xa.Target;
						if (uri == null)
							Uri.TryCreate (markup.Text.Substring (xa.StartIndex, xa.Count), UriKind.RelativeOrAbsolute, out uri);
						var link = new Link { Href = uri };
						tag.Underline = Pango.Underline.Single;
						tag.ForegroundGdk = Toolkit.CurrentEngine.Defaults.FallbackLinkColor.ToGtkValue ();
						Links [tag] = link;
					}

					TagTable.Add (tag);
					ApplyTag (tag, GetIterAtMark (attrStart), iterEndAttr);
					DeleteMark (attrStart);
				}
				DeleteMark (textmark);
			}
		}

		class GtkTextView : Gtk.TextView
		{
			bool selectable = true;
			Link activeLink;
			Gdk.Cursor defaultCursor, handCursor;

			public bool Selectable {
				get {
					return selectable;
				}
				set {
					selectable = value;
					UpdateBackground ();
				}
			}

			public new RichTextBuffer Buffer {
				get {
					return base.Buffer as RichTextBuffer;
				}
				set {
					var buffer = value as RichTextBuffer;
					if (buffer == null)
						throw new InvalidOperationException ();
					base.Buffer = buffer;
				}
			}

			public event EventHandler<NavigateToUrlEventArgs> NavigateToUrl;

			Gdk.Cursor DefaultCursor {
				get {
					if (defaultCursor == null)
						defaultCursor = new Gdk.Cursor (Gdk.CursorType.Xterm);
					return defaultCursor;
				}
			}

			Gdk.Cursor HandCursor {
				get {
					if (handCursor == null)
						handCursor = new Gdk.Cursor (Gdk.CursorType.Hand1);
					return handCursor;
				}
			}

			protected override bool OnMotionNotifyEvent (Gdk.EventMotion evnt)
			{
				Link link = GetLinkAtPos (evnt.X, evnt.Y);
				if (link != null) {
					if (activeLink == null) {
						TooltipText = link.Title;

						GetWindow (Gtk.TextWindowType.Text).Cursor = HandCursor;
					} else if (activeLink.Title != link.Title) {
						TooltipText = link.Title;
					}
					activeLink = link;
				} else if (activeLink != null) {
					activeLink = null;
					TooltipText = null;
					SetDefaultCursor ();
				}

				if (selectable)
					return base.OnMotionNotifyEvent (evnt);
				return false;
			}

			protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
			{
				if (selectable)
					return base.OnButtonPressEvent (evnt);
				return false;
			}

			protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
			{
				if (NavigateToUrl != null && (PointerButton)evnt.Button == PointerButton.Left) {
					Link link = activeLink ?? GetLinkAtPos (evnt.X, evnt.Y);

					if (link?.Href != null)
						NavigateToUrl?.Invoke (this, new NavigateToUrlEventArgs (link.Href));
				}
				if (selectable)
					return base.OnButtonReleaseEvent (evnt);
				return false;
			}

			Link GetLinkAtPos (double mousex, double mousey)
			{
				int x, y;
				WindowToBufferCoords (Gtk.TextWindowType.Text, (int)mousex, (int)mousey, out x, out y);
				var iter = GetIterAtLocation (x, y);
				if (Buffer != null) {
					foreach (var l in Buffer.Links) {
						if (iter.HasTag (l.Key)) {
							return l.Value;
						}
					}
				}
				return null;
			}

			protected override void OnStateChanged (Gtk.StateType previous_state)
			{
				base.OnStateChanged (previous_state);
				SetDefaultCursor ();
				UpdateBackground ();
			}

			protected override void OnGrabNotify (bool was_grabbed)
			{
				base.OnGrabNotify (was_grabbed);
				if (!was_grabbed)
					SetDefaultCursor ();
			}

			protected override void OnRealized ()
			{
				base.OnRealized ();
				SetDefaultCursor ();
				UpdateBackground ();
				UpdateLinkColor ();
			}

			protected override void OnStyleSet (Gtk.Style previous_style)
			{
				base.OnStyleSet (previous_style);
				UpdateLinkColor ();
			}

			void SetDefaultCursor ()
			{
				if (!IsRealized)
					return;
				Gdk.Cursor cursor = Sensitive && Selectable ? DefaultCursor : null;
				GetWindow (Gtk.TextWindowType.Text).Cursor = cursor;

			}

			void UpdateLinkColor ()
			{
				if (!IsRealized)
					return;
				var objColor = StyleGetProperty ("link-color");
				var color = Gdk.Color.Zero;
				if (objColor != null)
					color = (Gdk.Color) objColor;
				if (color.Equals (Gdk.Color.Zero))
					color = Toolkit.CurrentEngine.Defaults.FallbackLinkColor.ToGtkValue ();
				if (Buffer != null)
					foreach (var linkTag in Buffer.Links.Keys)
						linkTag.ForegroundGdk = color;
			}

			void UpdateBackground ()
			{
				if (!IsRealized)
					return;
				var state = Selectable ? State : Gtk.StateType.Insensitive;

				var bg = Style.Background (state);
				var bbg = Style.Base (state);

				GetWindow (Gtk.TextWindowType.Widget).Background = bg;
				GetWindow (Gtk.TextWindowType.Text).Background = bbg;
			}
		}
	}
}

