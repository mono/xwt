//
// RichTextViewBackend.cs
//
// Author:
//       Marek Habersack <grendel@xamarin.com>
//
// Copyright (c) 2013 Xamarin, Inc.
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
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

using Xwt;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class RichTextViewBackend : ViewBackend <NSTextView, IRichTextViewEventSink>, IRichTextViewBackend
	{
		public RichTextViewBackend ()
		{
		}

		public override void Initialize ()
		{
			base.Initialize ();
			var tv = new MacTextView (EventSink, ApplicationContext);
			ViewObject = tv;
			tv.VerticallyResizable = false;
			tv.HorizontallyResizable = false;
		}

		double CalcHeight (double width)
		{
			var f = Widget.Frame;
			Widget.VerticallyResizable = true;
			Widget.Frame = new System.Drawing.RectangleF (Widget.Frame.X, Widget.Frame.Y, (float)width, 0);
			Widget.SizeToFit ();
			var h = Widget.Frame.Height;
			Widget.VerticallyResizable = false;
			Widget.Frame = f;
			return h;
		}

		public override WidgetSize GetPreferredWidth ()
		{
			var w = (double) Widget.TextStorage.Size.Width;
			if (minWidth != -1 && minWidth > w)
				w = minWidth;
			return new WidgetSize (w, w);
		}

		public override WidgetSize GetPreferredHeightForWidth (double width)
		{
			var h = CalcHeight (width);
			if (minHeight != -1 && minHeight > h)
				h = minHeight;
			return new Xwt.WidgetSize (h, h);
		}

		public IRichTextBuffer CreateBuffer ()
		{
			return new MacRichTextBuffer (Widget.Font);
		}
		
		public void SetBuffer (IRichTextBuffer buffer)
		{
			var macBuffer = buffer as MacRichTextBuffer;
			if (macBuffer == null)
				throw new ArgumentException ("Passed buffer is of incorrect type", "buffer");

			var tview = ViewObject as MacTextView;
			if (tview == null)
				return;

			tview.TextStorage.SetString (macBuffer.ToAttributedString ());
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (!(eventId is RichTextViewEvent))
				return;

			var tview = ViewObject as MacTextView;
			if (tview == null)
				return;
			tview.EnableEvent ((RichTextViewEvent)eventId);
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (!(eventId is RichTextViewEvent))
				return;

			var tview = ViewObject as MacTextView;
			if (tview == null)
				return;
			tview.DisableEvent ((RichTextViewEvent)eventId);
		}
	}

	class MacTextView : NSTextView, IViewObject
	{
		IRichTextViewEventSink eventSink;
		ApplicationContext context;

		public NSView View {
			get { return this; }
		}

		public ViewBackend Backend { get;  set; }

		public MacTextView (IntPtr p) : base (p)
		{
			CommonInit ();
		}

		public MacTextView (IRichTextViewEventSink eventSink, ApplicationContext context)
		{
			CommonInit ();
			this.eventSink = eventSink;
			this.context = context;
			EnableEvent (RichTextViewEvent.NavigateToUrl);
		}

		public void EnableEvent (RichTextViewEvent ev)
		{
			if (ev != RichTextViewEvent.NavigateToUrl)
				return;
			LinkClicked = TextLinkClicked;
		}

		public void DisableEvent (RichTextViewEvent ev)
		{
			if (ev != RichTextViewEvent.NavigateToUrl)
				return;
			LinkClicked = null;
		}

		bool TextLinkClicked (NSTextView textView, NSObject link, uint charIndex)
		{
			if (eventSink == null || context == null)
				return false;

			string linkUrl;

			if (link is NSString)
				linkUrl = (link as NSString);
			else if (link is NSUrl)
				linkUrl = (link as NSUrl).AbsoluteString;
			else
				linkUrl = null;

			Uri url = null;
			if (String.IsNullOrWhiteSpace (linkUrl) || !Uri.TryCreate (linkUrl, UriKind.RelativeOrAbsolute, out url))
				url = null;

			context.InvokeUserCode (delegate {
				eventSink.OnNavigateToUrl (url);
			});

			return true;
		}

		void CommonInit ()
		{
			Editable = false;
		}
	}

	class MacRichTextBuffer : IRichTextBuffer
	{
		const int HeaderIncrement = 8;

		static readonly string[] lineSplitChars = new string[] { Environment.NewLine };
		static readonly IntPtr selInitWithHTMLDocumentAttributes_Handle = Selector.GetHandle ("initWithHTML:documentAttributes:");

		StringBuilder text;
		XmlWriter xmlWriter;
		Stack <int> paragraphIndent;

		public MacRichTextBuffer (NSFont font)
		{
			text = new StringBuilder ();
			xmlWriter = XmlWriter.Create (text, new XmlWriterSettings {
				OmitXmlDeclaration = true,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t"
			});

			float fontSize;
			string fontFamily;

			if (font != null) {
				fontSize = font.PointSize;
				fontFamily = font.FontName;
			} else {
				fontSize = 16;
				fontFamily = "sans-serif";
			}

			xmlWriter.WriteDocType ("html", "-//W3C//DTD XHTML 1.0", "Strict//EN", null);
			xmlWriter.WriteStartElement ("html");
			xmlWriter.WriteAttributeString ("style", String.Format ("font-family: {0}; font-size: {1}pt", fontFamily, fontSize));
			xmlWriter.WriteStartElement ("meta");
			xmlWriter.WriteAttributeString ("http-equiv", "Content-Type");
			xmlWriter.WriteAttributeString ("content", "text/html; charset=utf-8");
		}

		public NSAttributedString ToAttributedString ()
		{
			xmlWriter.WriteEndElement ();
			xmlWriter.Flush ();
			if (text == null || text.Length == 0)
				return new NSAttributedString (String.Empty);

			NSDictionary docAttributes;
			try {
				return CreateStringFromHTML (text.ToString (), out docAttributes);
			} finally {
				text = null;
				xmlWriter.Dispose ();
				xmlWriter = null;
				docAttributes = null;
			}
		}

		NSAttributedString CreateStringFromHTML (string html, out NSDictionary docAttributes)
		{
			var data = NSData.FromString (html);
			IntPtr docAttributesPtr = Marshal.AllocHGlobal (4);
			Marshal.WriteInt32 (docAttributesPtr, 0);

			var attrString = new NSAttributedString ();
			attrString.Handle = Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr (attrString.Handle, selInitWithHTMLDocumentAttributes_Handle, data.Handle, docAttributesPtr);
			IntPtr docAttributesValue = Marshal.ReadIntPtr (docAttributesPtr);
			docAttributes = docAttributesValue != IntPtr.Zero ? Runtime.GetNSObject (docAttributesValue) as NSDictionary : null;
			Marshal.FreeHGlobal (docAttributesPtr);

			return attrString;
		}

		public void EmitText (string text, RichTextInlineStyle style)
		{
			if (String.IsNullOrEmpty (text))
				return;

			bool haveStyle = true;
			switch (style) {
				case RichTextInlineStyle.Bold:
					xmlWriter.WriteStartElement ("strong");
					break;

				case RichTextInlineStyle.Italic:
					xmlWriter.WriteStartElement ("em");
					break;

				case RichTextInlineStyle.Monospace:
					xmlWriter.WriteStartElement ("tt");
					break;

				default:
					haveStyle = false;
					break;
			}

			bool first = true;
			foreach (string line in text.Split (lineSplitChars, StringSplitOptions.None)) {
				if (!first) {
					xmlWriter.WriteStartElement ("br");
					xmlWriter.WriteEndElement ();
				} else
					first = false;
				xmlWriter.WriteString (line);
			}

			if (haveStyle)
				xmlWriter.WriteEndElement ();
		}

		public void EmitStartHeader (int level)
		{
			if (level < 1)
				level = 1;
			if (level > 6)
				level = 6;
			xmlWriter.WriteStartElement ("h" + level);
		}

		public void EmitEndHeader ()
		{
			xmlWriter.WriteEndElement ();
		}

		public void EmitStartParagraph (int indentLevel)
		{
			if (paragraphIndent == null)
				paragraphIndent = new Stack<int> ();
			paragraphIndent.Push (indentLevel);

			// FIXME: perhaps use CSS indent?
			for (int i = 0; i < indentLevel; i++)
				xmlWriter.WriteStartElement ("blockindent");
			xmlWriter.WriteStartElement ("p");
		}

		public void EmitEndParagraph ()
		{
			xmlWriter.WriteEndElement (); // </p>

			int indentLevel;
			if (paragraphIndent != null && paragraphIndent.Count > 0)
				indentLevel = paragraphIndent.Pop ();
			else
				indentLevel = 0;

			for (int i = 0; i < indentLevel; i++)
				xmlWriter.WriteEndElement (); // </blockindent>
		}

		public void EmitOpenList ()
		{
			xmlWriter.WriteStartElement ("ul");
		}

		public void EmitOpenBullet ()
		{
			xmlWriter.WriteStartElement ("li");
		}

		public void EmitCloseBullet ()
		{
			xmlWriter.WriteEndElement ();
		}

		public void EmitCloseList ()
		{
			xmlWriter.WriteEndElement ();
		}

		public void EmitStartLink (string href, string title)
		{
			xmlWriter.WriteStartElement ("a");
			xmlWriter.WriteAttributeString ("href", href);

			// FIXME: it appears NSTextView ignores 'title' when it's told to display
			// link tooltips - it will use the URL instead. See if there's a way to make
			// it show the actual title
			xmlWriter.WriteAttributeString ("title", title);
		}

		public void EmitEndLink ()
		{
			xmlWriter.WriteEndElement ();
		}

		public void EmitCodeBlock (string code)
		{
			xmlWriter.WriteStartElement ("pre");
			xmlWriter.WriteString (code);
			xmlWriter.WriteEndElement ();
		}

		public void EmitHorizontalRuler ()
		{
			xmlWriter.WriteStartElement ("hr");
			xmlWriter.WriteEndElement ();
		}
	}
}

