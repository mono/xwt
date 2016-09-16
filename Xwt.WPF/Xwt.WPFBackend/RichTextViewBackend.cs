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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;

namespace Xwt.WPFBackend
{
	class RichTextViewBackend
		: WidgetBackend, IRichTextViewBackend
	{
		RichTextBuffer currentBuffer;
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

		public IRichTextBuffer CurrentBuffer {
			get	{
				return currentBuffer;
			}
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
			currentBuffer = buf;
			if (Widget.Document != null)
				Widget.Document.ClearValue (FlowDocument.PageWidthProperty);
			Widget.Document = buf.ToFlowDocument ();
			Widget.Document.SetBinding (FlowDocument.PageWidthProperty, new Binding ("ActualWidth") { Source = Widget });
			Widget.IsDocumentEnabled = true;
			Widget.Document.IsEnabled = true;
			Widget.IsReadOnly = true;
		}

		public bool ReadOnly { 
			get {
				return Widget.IsReadOnly;
			} 
			set {
				Widget.IsReadOnly = value;
			}
		}

		class RichTextBuffer : IRichTextBuffer
		{
			const int FontSize = 16;
			const int HeaderIncrement = 8;

			FlowDocument doc;

			public RichTextBuffer ()
			{
				doc = new FlowDocument();
			}

			Stack<InlineCollection> blockStack = new Stack<InlineCollection>();

			bool GetOrCreateInline (out InlineCollection block)
			{
				if (blockStack.Count == 0) {
					block = EmitStartParagraph().Inlines;
					return true;
				}
				block = blockStack.Peek();
				return false;
			}

			public void EmitText (string text)
			{
				if (string.IsNullOrEmpty(text))
					return;

				var lines = text.Split (new[] { Environment.NewLine }, StringSplitOptions.None);
				var first = true;
				InlineCollection block;
				var inlineCreated = GetOrCreateInline(out block);
				foreach (var line in lines) {
					if (!first)
						block.Add(new LineBreak ());
					if (!string.IsNullOrEmpty (line))
						block.Add(new Run (line));
					first = false;
				}
				if (inlineCreated)
					EmitEndParagraph();
			}

			public void EmitStartHeader (int level)
			{
				var block = EmitStartParagraph().Inlines;
				var header = new Span() { FontSize = FontSize + HeaderIncrement * level };
				block.Add(header);
				blockStack.Push(header.Inlines);
			}

			public void EmitEndHeader()
			{
				var span = blockStack.Pop();
				EmitEndParagraph();
			}

			List currentList;
			ListItem currentListItem;

			public void EmitOpenList ()
			{
				if (currentList != null)
					throw new InvalidOperationException("A new List can not be added to an existing List.");
				if (blockStack.Count > 0)
					throw new InvalidOperationException("A list can not be added to an other block.");
				currentList = new List();
				doc.Blocks.Add(currentList);
			}

			public void EmitOpenBullet ()
			{
				if (currentList == null)
					throw new InvalidOperationException("Not inside a List.");
				var paragraph = new Paragraph();
				currentListItem  = new ListItem(paragraph);
				currentList.ListItems.Add(currentListItem);
				blockStack.Push(paragraph.Inlines);
			}

			public void EmitCloseBullet ()
			{
				if (currentList == null)
					throw new InvalidOperationException("Not inside a List.");
				if (currentListItem == null)
					throw new InvalidOperationException("Not inside a ListItem.");
				currentListItem = null;
				blockStack.Pop();
			}

			public void EmitCloseList ()
			{
				if (currentList == null)
					throw new InvalidOperationException("Not inside a List.");
				if (currentListItem != null)
					EmitCloseBullet();
				currentList = null;
			}

			bool localLinkParagraph;

			public void EmitStartLink (string href, string title)
			{
				InlineCollection block;
				localLinkParagraph = GetOrCreateInline(out block);
				var link = new Hyperlink();
				link.NavigateUri = new Uri (href);
				if (!string.IsNullOrEmpty(title))
					link.ToolTip = title;
				block.Add(link);
				blockStack.Push(link.Inlines);
			}

			public void EmitEndLink()
			{
				blockStack.Pop();
				if (localLinkParagraph)
					EmitEndParagraph();
				localLinkParagraph = false;
			}

			int rtbCounter;
			public void EmitCodeBlock (string code)
			{
				if (blockStack.Count > 0)
					throw new InvalidOperationException("A Code Block can not be added to an other block.");

				var rtb = new RichTextBox();
				rtb.Name = "rtb" + (rtbCounter++);
				rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
				var codeDoc = new FlowDocument();
				var widthBinding = new Binding();
				widthBinding.Path = new PropertyPath("ActualWidth");
				widthBinding.ElementName = rtb.Name;
				codeDoc.SetBinding(FlowDocument.PageWidthProperty, widthBinding);
				var p = new Paragraph();
				p.FontFamily = new FontFamily("Global Monospace");
				p.Inlines.Add(code);
				codeDoc.Blocks.Add(p);
				rtb.Document = codeDoc;


				var container = new BlockUIContainer(rtb);
				container.Margin = new Thickness(15, 15, 15, 15);

				doc.Blocks.Add(container);
			}

			public void EmitText (string text, RichTextInlineStyle style)
			{
				InlineCollection block;
				var inlineCreated = GetOrCreateInline(out block);

				switch (style) {
				case RichTextInlineStyle.Bold:
					var bold = new Bold();
					blockStack.Push(bold.Inlines);
					EmitText(text);
					blockStack.Pop();
					break;
					
				case RichTextInlineStyle.Italic:
					var italic = new Italic();
					blockStack.Push(italic.Inlines);
					EmitText(text);
					blockStack.Pop();
					break;

				case RichTextInlineStyle.Monospace:
					var run = new Run(text);
					run.FontFamily = new FontFamily("Global Monospace");
					block.Add(run);
					break;

				default:
					EmitText (text);
					return;
				}
				if (inlineCreated)
					EmitEndParagraph();
			}

			public void EmitText (FormattedText markup)
			{
				EmitText (markup.Text, RichTextInlineStyle.Normal);
			}

			Paragraph EmitStartParagraph ()
			{
				var paragraph = new Paragraph();
				blockStack.Push(paragraph.Inlines);
				doc.Blocks.Add(paragraph);
				return paragraph;
			}

			public void EmitStartParagraph (int indentLevel)
			{
				//FIXME: indentLevel
				EmitStartParagraph();
			}

			public void EmitEndParagraph ()
			{
				blockStack.Pop();
			}

			public void EmitHorizontalRuler ()
			{
				if (blockStack.Count > 0)
					throw new InvalidOperationException("A Ruler can not be added to an other block.");
				var ruler = new BlockUIContainer(new System.Windows.Controls.Separator ());
				ruler.Margin = new Thickness (0, 0, 0, 0);
				doc.Blocks.Add(ruler);
			}

			public FlowDocument ToFlowDocument ()
			{
				return doc;
			}

			public string PlainText { 
				get {
					string text = new TextRange (doc.ContentStart, doc.ContentEnd).Text;
					return text;
				}
			}
		}
	}
}
