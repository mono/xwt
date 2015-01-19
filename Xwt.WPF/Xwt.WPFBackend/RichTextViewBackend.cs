// 
// RichTextViewBackend.cs
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
using System.Windows.Navigation;

namespace Xwt.WPFBackend
{
	class RichTextViewBackend
		: WidgetBackend, IRichTextViewBackend
	{
		public new IRichTextViewEventSink EventSink {
			get { return (IRichTextViewEventSink) base.EventSink; }
		}

		public new ExRichTextBox Widget
		{
			get { return (ExRichTextBox) base.Widget; }
			set { base.Widget = value; }
		}

		public RichTextViewBackend ()
		{
			Widget = new ExRichTextBox ();
			Widget.BorderThickness = new System.Windows.Thickness (0);
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is RichTextViewEvent) {
				switch ((RichTextViewEvent) eventId) {
				case RichTextViewEvent.NavigateToUrl:
						Widget.AddHandler (Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler (HyperlinkNavigated), true);
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
						Widget.RemoveHandler (Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler (HyperlinkNavigated));
						break;
				}
			}
		}

		void HyperlinkNavigated (object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Context.InvokeUserCode (() => {
				EventSink.OnNavigateToUrl (e.Uri);
				e.Handled = true;
			});
		}

		public IRichTextBuffer CreateBuffer ()
		{
			return new RichTextBuffer ();
		}

		public void SetBuffer (IRichTextBuffer buffer)
		{
			var buf = buffer as RichTextBuffer;
			if (buf == null)
				throw new ArgumentException ("Passed buffer is of incorrect type", "buffer");

			if (Widget.Document != null)
				Widget.Document.ClearValue (FlowDocument.PageWidthProperty);
			Widget.Document = buf.ToFlowDocument ();
			Widget.Document.SetBinding (FlowDocument.PageWidthProperty, new Binding ("ActualWidth") { Source = Widget });
			Widget.IsDocumentEnabled = true;
			Widget.Document.IsEnabled = true;
			Widget.IsReadOnly = true;
		}

		class RichTextBuffer : IRichTextBuffer
		{
			const int FontSize = 16;
			const int HeaderIncrement = 8;

			StringBuilder builder;
			XmlWriter writer;
			FlowDocument doc;

			public RichTextBuffer ()
			{
				builder = new StringBuilder ();
				writer = XmlWriter.Create (builder, new XmlWriterSettings () { OmitXmlDeclaration = true, NewLineOnAttributes = true, Indent = true, IndentChars = "\t" });
				writer.WriteStartElement ("FlowDocument", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
			}

			public void EmitText (string text)
			{
				if (string.IsNullOrEmpty(text))
					return;

				var lines = text.Split (new[] { Environment.NewLine }, StringSplitOptions.None);
				var first = true;
				foreach (var line in lines) {
					if (!first) {
						writer.WriteStartElement ("LineBreak");
						writer.WriteEndElement ();
					}
					if (!string.IsNullOrEmpty (line)) {
						writer.WriteElementString ("Run", line);
					}
					first = false;
				}
			}

			public void EmitStartHeader (int level)
			{
				EmitStartParagraph (0);
				writer.WriteStartElement ("Span");
				writer.WriteAttributeString ("FontSize", (FontSize + HeaderIncrement * level).ToString ());
			}

			public void EmitEndHeader()
			{
				writer.WriteEndElement();
				EmitEndParagraph();
			}

			public void EmitOpenList ()
			{
				writer.WriteStartElement ("List");
			}

			public void EmitOpenBullet ()
			{
				writer.WriteStartElement ("ListItem");
				EmitStartParagraph (0);
			}

			public void EmitCloseBullet ()
			{
				writer.WriteEndElement ();
				writer.WriteEndElement ();
			}

			public void EmitCloseList ()
			{
				// Close the list
				writer.WriteEndElement ();
			}

			public void EmitStartLink (string href, string title)
			{
				writer.WriteStartElement ("Hyperlink");
				writer.WriteAttributeString ("NavigateUri", href);
				if (!string.IsNullOrEmpty (title))
					writer.WriteAttributeString ("ToolTip", title);
			}

			public void EmitEndLink()
			{
				writer.WriteEndElement();
			}

			int rtbCounter;
			public void EmitCodeBlock (string code)
			{
				writer.WriteStartElement ("BlockUIContainer");
				writer.WriteAttributeString ("Margin", "15");

				string name = "rtb" + (rtbCounter++);

				writer.WriteStartElement ("RichTextBox");
				writer.WriteAttributeString ("Name", name);
				writer.WriteAttributeString ("HorizontalScrollBarVisibility", "Hidden");

				writer.WriteStartElement ("FlowDocument");
				//writer.WriteAttributeString ("PageWidth", "1000");
				writer.WriteAttributeString ("PageWidth", "{Binding ElementName=" + name + ",Path=ActualWidth}");
				writer.WriteStartElement ("Paragraph");

				writer.WriteAttributeString ("xml", "space", null, "preserve");
				writer.WriteAttributeString ("FontFamily", "Global Monospace");
				writer.WriteString (code);

				writer.WriteEndElement ();
				writer.WriteEndElement ();
				writer.WriteEndElement ();
				writer.WriteEndElement ();
			}

			public void EmitText (string text, RichTextInlineStyle style)
			{
				switch (style) {
				case RichTextInlineStyle.Bold:
				case RichTextInlineStyle.Italic:
					writer.WriteStartElement (style.ToString ());
					EmitText (text);
					break;

				case RichTextInlineStyle.Monospace:
					writer.WriteStartElement ("Run");
					writer.WriteAttributeString ("FontFamily", "Global Monospace");
					writer.WriteString (text);
					break;

				default:
					EmitText (text);
					return;
				}
				writer.WriteEndElement ();
			}

			public void EmitStartParagraph (int indentLevel)
			{
				//FIXME: indentLevel
				writer.WriteStartElement ("Paragraph");
			}

			public void EmitEndParagraph ()
			{
				writer.WriteEndElement ();
			}

			public void EmitHorizontalRuler ()
			{
				writer.WriteStartElement("BlockUIContainer");
				writer.WriteAttributeString ("Margin", "0,0,0,0");
				writer.WriteElementString("Separator", "");
				writer.WriteEndElement();
			}

			public FlowDocument ToFlowDocument ()
			{
				if (doc == null) {
					writer.WriteEndElement ();
					writer.Flush ();
					doc = (FlowDocument) XamlReader.Parse (builder.ToString ());
					builder = null;
					writer = null;
				}
				return doc;
			}
		}


	}
}
