//
// IMarkdownViewBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
using System;

namespace Xwt.Backends
{
	[Flags]
	public enum RichTextInlineStyle
	{
		Normal    = 0,
		Italic    = 1 << 0,
		Bold      = 1 << 1,
		Monospace = 1 << 2,
	}

	public interface IRichTextViewBackend : IWidgetBackend
	{
		IRichTextBuffer CreateBuffer ();

		// Display the passed buffer
		void SetBuffer (IRichTextBuffer buffer);

		bool ReadOnly { get; set; }

		IRichTextBuffer CurrentBuffer { get; }
	}

	public interface IRichTextBuffer
	{
		// Emit text using specified style mask
		void EmitText (string text, RichTextInlineStyle style);

		// Emit a header (h1, h2, ...)
		void EmitStartHeader (int level);
		void EmitEndHeader ();

		// What's outputed afterwards will be a in new paragrapgh
		void EmitStartParagraph (int indentLevel);
		void EmitEndParagraph ();

		// Emit a list
		// Chain is:
		// open-list, open-bullet, <above methods>, close-bullet, close-list
		void EmitOpenList ();
		void EmitOpenBullet ();
		void EmitCloseBullet ();
		void EmitCloseList ();

		// Emit a link opening the href URL with the mouseover title
		void EmitStartLink (string href, string title);
		void EmitEndLink ();

		// Emit code in a preformated blockquote
		void EmitCodeBlock (string code);

		// Emit an horizontal ruler
		void EmitHorizontalRuler ();

		string PlainText { get;}
	}

	public interface IRichTextViewEventSink : IWidgetEventSink
	{
		void OnNavigateToUrl (Uri uri);
	}

	public enum RichTextViewEvent
	{
		NavigateToUrl = 1
	}
}
