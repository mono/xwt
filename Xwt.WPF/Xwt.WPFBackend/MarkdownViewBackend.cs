// 
// MarkdownViewBackend.cs
//  
// Author:
//       Alan McGovern <alan@xamarin.com>
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
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using Xwt.Backends;

using Xwt.WPFBackend.Utilities;
using System.Windows.Data;

namespace Xwt.WPFBackend
{
	class MarkdownViewBackend
		: WidgetBackend, IMarkdownViewBackend
	{
		int FontSize = 16;
		int HeaderIncrement = 8;

		StringBuilder Builder {
			get; set;
		}

		public new ExRichTextBox Widget
		{
			get { return (ExRichTextBox) base.Widget; }
			set { base.Widget = value; }
		}

		XmlWriter Writer {
			get; set;
		}

		public MarkdownViewBackend ()
		{
			Widget = new ExRichTextBox ();
		}

		object IMarkdownViewBackend.CreateBuffer ()
		{
			Builder = new StringBuilder ();
			Writer = XmlWriter.Create (Builder, new XmlWriterSettings () { OmitXmlDeclaration = true, NewLineOnAttributes = true, Indent = true, IndentChars = "\t" });
			Writer.WriteStartElement ("FlowDocument", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
			return Writer;
		}

		void IMarkdownViewBackend.EmitText (object buffer, string text)
		{
			if (!string.IsNullOrEmpty (text))
				Writer.WriteElementString ("Run", text);
		}

		void IMarkdownViewBackend.EmitHeader (object buffer, string title, int level)
		{
			((IMarkdownViewBackend) this).EmitStartParagraph (buffer);
			Writer.WriteStartElement ("Run");
			Writer.WriteAttributeString ("FontSize", (FontSize + HeaderIncrement * level).ToString ());
			Writer.WriteString (title);
			Writer.WriteEndElement ();
			((IMarkdownViewBackend) this).EmitEndParagraph (buffer);
		}

		void IMarkdownViewBackend.EmitOpenList (object buffer)
		{
			Writer.WriteStartElement ("List");
		}

		void IMarkdownViewBackend.EmitOpenBullet (object buffer)
		{
			Writer.WriteStartElement ("ListItem");
			((IMarkdownViewBackend) this).EmitStartParagraph (buffer);
		}

		void IMarkdownViewBackend.EmitCloseBullet (object buffer)
		{
			Writer.WriteEndElement ();
			Writer.WriteEndElement ();
		}

		void IMarkdownViewBackend.EmitCloseList (object buffer)
		{
			// Close the list
			Writer.WriteEndElement ();
		}

		void IMarkdownViewBackend.EmitLink (object buffer, string href, string text)
		{
			Writer.WriteStartElement ("Hyperlink");
			Writer.WriteAttributeString ("NavigateUri", href);
			((IMarkdownViewBackend) this).EmitText (buffer, text);
			Writer.WriteEndElement ();
		}

		void IMarkdownViewBackend.EmitCodeBlock (object buffer, string code)
		{
			((IMarkdownViewBackend) this).EmitStartParagraph (buffer);
			Writer.WriteAttributeString ("xml", "space", null, "preserve");
			Writer.WriteAttributeString ("TextIndent", "0");
			Writer.WriteAttributeString ("Margin", "50,0,0,0");
			Writer.WriteAttributeString ("FontFamily", "GlobalMonospace.CompositeFont");
			Writer.WriteString (code);
			((IMarkdownViewBackend) this).EmitEndParagraph (buffer);
		}

		void IMarkdownViewBackend.SetBuffer (object buffer)
		{
			Writer.WriteEndElement ();
			Writer.Flush ();

			if (Widget.Document != null)
				Widget.Document.ClearValue (FlowDocument.PageWidthProperty);
			Widget.Document = (FlowDocument) XamlReader.Parse (Builder.ToString ());
			Widget.Document.SetBinding (FlowDocument.PageWidthProperty, new Binding ("ActualWidth") { Source = Widget });
		}

		void IMarkdownViewBackend.EmitStyledText (object buffer, string text, MarkdownInlineStyle style)
		{
			switch (style) {
				case MarkdownInlineStyle.Bold:
				case MarkdownInlineStyle.Italic:
					Writer.WriteStartElement (style.ToString ());
					((IMarkdownViewBackend) this).EmitText (buffer, text);
					break;

				case MarkdownInlineStyle.Monospace:
					Writer.WriteStartElement ("Run");
					Writer.WriteAttributeString ("FontFamily", "GlobalMonospace.CompositeFont");
					Writer.WriteString (text);
					break;
			}
			Writer.WriteEndElement ();
		}

		void IMarkdownViewBackend.EmitStartParagraph (object buffer)
		{
			Writer.WriteStartElement ("Paragraph");
		}

		void IMarkdownViewBackend.EmitEndParagraph (object buffer)
		{
			Writer.WriteEndElement ();
		}

		void IMarkdownViewBackend.EmitHorizontalRuler (object buffer)
		{
			//Writer.WriteStartElement("BlockUIContainer");
			//Writer.WriteElementString("Separator", "");
			//Writer.WriteEndElement();
		}
	}
}
