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
	enum MarkdownInlineStyle
	{
		Italic,
		Bold,
		Monospace
	}

	public interface IMarkdownViewBackend : IWidgetBackend
	{
		object CreateBuffer ();

		// Emit unstyled text
		void EmitText (object buffer, string text);

		// Emit a header (h1, h2, ...)
		void EmitHeader (object buffer, string title, int level);

		// What's outputed afterwards will be a in new paragrapgh
		void EmitStartParagraph (object buffer);
		void EmitEndParagraph (object buffer);

		// Emit a list
		// Chain is:
		// open-list, open-bullet, <above methods>, close-bullet, close-list
		void EmitOpenList (object buffer);
		void EmitOpenBullet (object buffer);
		void EmitCloseBullet (object buffet);
		void EmitCloseList (object buffer);

		// Emit a link displaying text and opening the href URL
		void EmitLink (object buffer, string href, string text);

		// Emit code in a preformated blockquote
		void EmitCodeBlock (object buffer, string code);

		// Emit an horizontal ruler
		void EmitHorizontalRuler (object buffer);

		// Display the passed buffer
		void SetBuffer (object buffer);
	}
}
