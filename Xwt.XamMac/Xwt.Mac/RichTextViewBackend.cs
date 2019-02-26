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
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class RichTextViewBackend : ViewBackend <NSTextView, IRichTextViewEventSink>, IRichTextViewBackend
	{
		NSFont font;
		MacRichTextBuffer currentBuffer;

		public override object Font {
			get { return base.Font; }
			set {
				var fd = value as FontData;
				if (fd != null)
					font = fd.Font;
				else
					font = null;
				base.Font = value;
			}
		}

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
			// Use cached font since Widget.Font size increases for each LoadText... It has to do
			// with the 'style' attribute for the 'body' element - not sure why that happens
			font = tv.Font;
		}

		double CalcHeight (double width)
		{
			var f = Widget.Frame;
			Widget.VerticallyResizable = true;
			Widget.Frame = new CGRect (Widget.Frame.X, Widget.Frame.Y, (float)width, 0);
			Widget.SizeToFit ();
			var h = Widget.Frame.Height;
			Widget.VerticallyResizable = false;
			Widget.Frame = f;
			return h;
		}

		public override Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			// set initial width to 0 to force text wrapping if inside ScrollView with disabled horizontal scrolling
			var width = (Widget.EnclosingScrollView?.HasHorizontalScroller == false) ? 0 : (double)Widget.TextStorage.Size.Width;
			if (widthConstraint.IsConstrained)
				width = widthConstraint.AvailableSize;
			if (minWidth != -1 && minWidth > width)
				width = minWidth;

			var height = CalcHeight (width);
			if (minHeight != -1 && minHeight > height)
				height = minHeight;
			return new Size (width, height);
		}

		public IRichTextBuffer CreateBuffer ()
		{
			return new MacRichTextBuffer ();
		}

		public bool ReadOnly { 
			get { 
				return !Widget.Editable;
			}
			set {
				Widget.Editable = !value;
			}
		}

		public bool Selectable {
			get {
				return Widget.Selectable;
			}
			set {
				Widget.Selectable = value;
				// force NSTextView not to draw its (white) background
				// making it look like a label, which is the default Gtk/Wpf behaviour
				// the background color can still be set manually with the BackgroundColor property
				Widget.DrawsBackground = value;
			}
		}

		public override Drawing.Color BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = value;
				Widget.BackgroundColor = value.ToNSColor ();
			}
		}

		public Drawing.Color TextColor {
			get { return Widget.TextColor.ToXwtColor (); }
			set { Widget.TextColor = value.ToNSColor (); }
		}

		int? lineSpacing = null;
		public int LineSpacing {
			get {
				return lineSpacing.HasValue ? (int)lineSpacing : 0;
			}
			set {
				lineSpacing = value;

				if (currentBuffer != null)
					Widget.SetAttributedString (currentBuffer.ToAttributedString (font, lineSpacing), !currentBuffer.HasForegroundAttributes);
			}
		}

		public IRichTextBuffer CurrentBuffer {
			get {
				return currentBuffer;
			}
			private set {
				if (currentBuffer != null)
					currentBuffer.Dispose ();
				currentBuffer = value as MacRichTextBuffer;
			}
		}

		public void SetBuffer (IRichTextBuffer buffer)
		{
			var macBuffer = buffer as MacRichTextBuffer;
			if (macBuffer == null)
				throw new ArgumentException ("Passed buffer is of incorrect type", "buffer");
			CurrentBuffer = macBuffer;
			var tview = ViewObject as MacTextView;
			if (tview == null)
				return;


			tview.SetAttributedString (macBuffer.ToAttributedString (font, lineSpacing), !macBuffer.HasForegroundAttributes);
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

		protected override void Dispose (bool disposing)
		{
			if (currentBuffer != null)
				currentBuffer.Dispose ();
			base.Dispose (disposing);
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

		bool TextLinkClicked (NSTextView textView, NSObject link, nuint charIndex)
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

		public override void ViewDidMoveToWindow ()
		{
			base.ViewDidMoveToWindow ();
			if (MacSystemInformation.OsVersion < MacSystemInformation.Mavericks)
				return;
			// FIXME: the NSAppearance does not define a color for links,
			//        this may change in the future, but for now use the fallback color
			if (Window?.EffectiveAppearance?.Name == NSAppearance.NameVibrantDark) {
				var ns = new NSMutableDictionary (LinkTextAttributes);
				ns [NSStringAttributeKey.ForegroundColor] = Backend.Frontend.Surface.ToolkitEngine.Defaults.FallbackLinkColor.ToNSColor ();
				LinkTextAttributes = ns;
			}
		}

		public override void MouseUp (NSEvent theEvent)
		{
			if (!Selectable) {
				var uri = GetLinkAtPos (theEvent);
				string linkUrl = uri?.AbsoluteString ?? null;
				if (!string.IsNullOrEmpty (linkUrl)) {
					Uri url = null;
					if (string.IsNullOrWhiteSpace (linkUrl) || !Uri.TryCreate (linkUrl, UriKind.RelativeOrAbsolute, out url))
						url = null;

					context.InvokeUserCode (delegate {
						eventSink.OnNavigateToUrl (url);
					});
				}
			}
			base.MouseUp (theEvent);
		}


		NSUrl GetLinkAtPos (NSEvent theEvent)
		{
			var i = GetCharacterIndex (Window.ConvertRectToScreen (new CGRect (theEvent.LocationInWindow, CGSize.Empty)).Location);
			if (i >= 0) {
				NSRange r;
				var attr = TextStorage.GetAttribute (NSStringAttributeKey.Link, (nint)i, out r) as NSUrl;
				if (attr != null && r.Length > 0)
					return attr;
			}
			return null;
		}
		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
			// NSTextView sets the link cursors only in selectable mode
			// Do the same when Selectable == false
			if (!Selectable && TextStorage?.Length > 0) {
				TextStorage.EnumerateAttributes (new NSRange (0, TextStorage.Length), NSAttributedStringEnumeration.None, (NSDictionary attrs, NSRange range, ref bool stop) => {
					stop = false;
					if (attrs.ContainsKey (NSStringAttributeKey.Link)) {
						var rects = RectsForCharacterRange (range);
						for (nuint i = 0; i < rects.Count; i++)
							AddCursorRect (rects.GetItem<NSValue> (i).CGRectValue, NSCursor.PointingHandCursor);
					}
				});
			}
		}

		void CommonInit ()
		{
			Editable = false;
		}
	}

	class MacRichTextBuffer : IRichTextBuffer, IDisposable
	{
		const int HeaderIncrement = 8;

		static readonly string[] lineSplitChars = new string[] { Environment.NewLine };
		static readonly IntPtr selInitWithHTMLDocumentAttributes_Handle = Selector.GetHandle ("initWithHTML:documentAttributes:");

		readonly StringBuilder text;
		readonly XmlWriter xmlWriter;
		Stack <int> paragraphIndent;

		/// <summary>
		/// Used to identify whether we can safely update TextView's TextColor.
		/// Otherwise such update can override all custom ForegroundColor attributes.
		/// </summary>
		/// <value></value>
		public bool HasForegroundAttributes { get; private set; }

		public MacRichTextBuffer ()
		{
			text = new StringBuilder ();
			xmlWriter = XmlWriter.Create (text, new XmlWriterSettings {
				OmitXmlDeclaration = true,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				ConformanceLevel = ConformanceLevel.Fragment
			});
		}

		public NSAttributedString ToAttributedString (NSFont font, int? lineSpacing)
		{
			xmlWriter.Flush ();

			var finaltext = new StringBuilder ();
			var finalxmlWriter = XmlWriter.Create (finaltext, new XmlWriterSettings {
				OmitXmlDeclaration = true,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t"
			});


			float fontSize;
			string fontFamily;

			if (font != null) {
				fontSize = (float) font.PointSize;
				fontFamily = font.FontName;
			} else {
				fontSize = 16;
				fontFamily = "sans-serif";
			}

			finalxmlWriter.WriteDocType ("html", "-//W3C//DTD XHTML 1.0", "Strict//EN", null);
			finalxmlWriter.WriteStartElement ("html");
			finalxmlWriter.WriteStartElement ("meta");
			finalxmlWriter.WriteAttributeString ("http-equiv", "Content-Type");
			finalxmlWriter.WriteAttributeString ("content", "text/html; charset=utf-8");
			finalxmlWriter.WriteEndElement ();
			finalxmlWriter.WriteStartElement ("body");

			string style = String.Format ("font-family: {0}; font-size: {1}", fontFamily, fontSize);
			if (lineSpacing.HasValue)
				style += "; line-height: " + (lineSpacing.Value + fontSize) + "px";

			finalxmlWriter.WriteAttributeString ("style", style);
			finalxmlWriter.WriteRaw (text.ToString ());
			finalxmlWriter.WriteEndElement (); // body
			finalxmlWriter.WriteEndElement (); // html
			finalxmlWriter.Flush ();

			if (finaltext == null || finaltext.Length == 0)
				return new NSAttributedString (String.Empty);

			NSDictionary docAttributes;
			try {
				return CreateStringFromHTML (finaltext.ToString (), out docAttributes);
			} finally {
				finaltext = null;
				((IDisposable)finalxmlWriter).Dispose ();
				finalxmlWriter = null;
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

		public string PlainText {
			get {
				return text.ToString ();
			}
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

		public void EmitText (FormattedText text)
		{
			if (text.Attributes.Count == 0) {
				EmitText (text.Text, RichTextInlineStyle.Normal);
				return;
			}
			var s = text.ToAttributedString ();
			var options = new NSAttributedStringDocumentAttributes ();
			options.DocumentType = NSDocumentType.HTML;
			var exclude = NSArray.FromObjects (new [] { "doctype", "html", "head", "meta", "xml", "body", "p" });
			options.Dictionary [NSExcludedElementsDocumentAttribute] = exclude;
			NSError err;
			var d = s.GetData (new NSRange (0, s.Length), options, out err);
			var str = (string)NSString.FromData (d, NSStringEncoding.UTF8);

			//bool first = true;
			foreach (string line in str.Split (lineSplitChars, StringSplitOptions.None)) {
				//if (!first) {
				//	xmlWriter.WriteStartElement ("br");
				//	xmlWriter.WriteEndElement ();
				//} else
				//	first = false;
				xmlWriter.WriteRaw (line);
			}

			HasForegroundAttributes |= text.Attributes.Any (a => a is Xwt.Drawing.ColorTextAttribute);
		}

		private static readonly IntPtr _AppKitHandle = Dlfcn.dlopen ("/System/Library/Frameworks/AppKit.framework/AppKit", 0);

		private static NSString _NSExcludedElementsDocumentAttribute;
		private static NSString NSExcludedElementsDocumentAttribute {
			get {
				if (_NSExcludedElementsDocumentAttribute == null) {
					_NSExcludedElementsDocumentAttribute = Dlfcn.GetStringConstant (_AppKitHandle, "NSExcludedElementsDocumentAttribute");
				}
				return _NSExcludedElementsDocumentAttribute;
			}
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

		public void Dispose ()
		{
			((IDisposable)xmlWriter).Dispose ();
		}
	}
}

