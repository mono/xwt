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
		Italic,
		Bold,
		Monospace
	}

	public interface IRichTextViewBackend : IWidgetBackend
	{
		IRichTextBuffer CreateBuffer ();

		// Display the passed buffer
		void SetBuffer (IRichTextBuffer buffer);
	}

	public interface IRichTextBuffer
	{
		// Emit unstyled text
		void EmitText (string text);

		// Emit text using combination of the MarkdownInlineStyle
		void EmitStyledText (string text, RichTextInlineStyle style);

		// Emit a header (h1, h2, ...)
		void EmitHeader (string title, int level);

		// What's outputed afterwards will be a in new paragrapgh
		void EmitStartParagraph ();
		void EmitEndParagraph ();

		// Emit a list
		// Chain is:
		// open-list, open-bullet, <above methods>, close-bullet, close-list
		void EmitOpenList ();
		void EmitOpenBullet ();
		void EmitCloseBullet ();
		void EmitCloseList ();

		// Emit a link displaying text and opening the href URL
		void EmitLink (string href, string text);

		// Emit code in a preformated blockquote
		void EmitCodeBlock (string code);

		// Emit an horizontal ruler
		void EmitHorizontalRuler ();
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
