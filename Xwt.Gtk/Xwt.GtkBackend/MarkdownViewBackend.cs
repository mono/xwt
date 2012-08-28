//
// MarkdownViewBackend.cs
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

namespace Xwt.GtkBackend
{
	public class MarkdownViewBackend : WidgetBackend, IMarkdownViewBackend
	{
		const string NewLine = "\n";
		Gtk.TextTagTable table;
		List<KeyValuePair<Gtk.TextChildAnchor, Gtk.Widget>> links = new List<KeyValuePair<Gtk.TextChildAnchor, Gtk.Widget>> ();

		public MarkdownViewBackend ()
		{
			Widget = new Gtk.TextView ();
			Widget.Show ();
			Widget.Editable = false;
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
				Indent = 14
			});
		}

		protected new Gtk.TextView Widget {
			get {
				return (Gtk.TextView)base.Widget;
			}
			set {
				base.Widget = value;
			}
		}

		public object CreateBuffer ()
		{
			return new Gtk.TextBuffer (table);
		}

		public void EmitText (object buffer, string text)
		{
			var b = ((Gtk.TextBuffer)buffer);
			var iter = b.EndIter;
			b.Insert (ref iter, text);
		}

		public void EmitStyledText (object buffer, string text, MarkdownInlineStyle style)
		{
			var b = ((Gtk.TextBuffer)buffer);
			var iter = b.EndIter;
			var tagName = string.Empty;
			switch (style) {
			case MarkdownInlineStyle.Bold:
				tagName = "bold";
				break;
			case MarkdownInlineStyle.Italic:
				tagName = "italic";
				break;
			case MarkdownInlineStyle.Monospace:
				tagName = "tt";
				break;
			}
			if (string.IsNullOrEmpty (tagName))
				EmitText (buffer, text);
			else
				b.InsertWithTagsByName (ref iter, text, tagName);
		}

		public void EmitHeader (object buffer, string title, int level)
		{
			var b = ((Gtk.TextBuffer)buffer);
			var iter = b.EndIter;
			b.Insert (ref iter, NewLine);
			b.InsertWithTagsByName (ref iter, title, "h" + level);
			b.Insert (ref iter, NewLine);
		}

		public void EmitStartParagraph (object buffer)
		{
			EmitText (buffer, NewLine);
		}

		public void EmitEndParagraph (object buffer)
		{
			EmitText (buffer, NewLine);
		}

		public void EmitOpenList (object buffer)
		{
			EmitText (buffer, NewLine);
		}

		public void EmitOpenBullet (object buffer)
		{
			var b = (Gtk.TextBuffer)buffer;
			var iter = b.EndIter;
			b.InsertWithTagsByName (ref iter, "• ", "li");
		}

		public void EmitCloseBullet (object buffer)
		{
			EmitText (buffer, NewLine);
		}

		public void EmitCloseList (object buffer)
		{

		}

		public void EmitLink (object buffer, string href, string text)
		{
			var b = (Gtk.TextBuffer)buffer;
			var iter = b.EndIter;
			var anchor = b.CreateChildAnchor (ref iter);
			var link = (ILinkLabelBackend) new LinkLabel (text) { Uri = new Uri (href, UriKind.RelativeOrAbsolute) };
			links.Add (new KeyValuePair<Gtk.TextChildAnchor, Gtk.Widget> (anchor, (Gtk.Widget) link.NativeWidget));
		}

		public void EmitCodeBlock (object buffer, string code)
		{
			var b = ((Gtk.TextBuffer)buffer);
			var iter = b.EndIter;
			b.Insert (ref iter, NewLine);
			b.InsertWithTagsByName (ref iter, code, "pre");
		}

		public void EmitHorizontalRuler (object buffer)
		{

		}

		public void SetBuffer (object buffer)
		{
			Widget.Buffer = (Gtk.TextBuffer)buffer;
			foreach (var l in links)
				Widget.AddChildAtAnchor (l.Value, l.Key);
			links.Clear ();
		}
	}
}

