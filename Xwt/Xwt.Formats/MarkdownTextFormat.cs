//
// MarkdownTextFormat.cs
//
// Author:
//       Alex Corrado <corrado@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc.
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
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Xwt.Backends;

namespace Xwt.Formats
{
	public class MarkdownTextFormat : TextFormat
	{

		public override void Parse (Stream input, IRichTextBuffer buffer)
		{
			using (var reader = new StreamReader (input))
				ParseMarkdown (reader.ReadToEnd (), buffer);
		}

		/* The subset we support:
		 *   - Headers in Atx-style i.e. prefixed with the '#' character and in Setex-style i.e. underlined '=' or '-'
		 *   - Paragraph are separated by a new line
		 *   - A link has the syntax: "[This link](http://example.net/)" only
		 *   - Code blocks are normal paragraph with a 4-spaces or 1-tab space prepended
		 *   - A list is a number of text line with no newlines in between and prefixed by one of '+', '-' or '*', no nesting
		 *   - Emphasis is by putting a portion of text between '**' or '__' respectively for bold and italic
		 *   - Inline code is wrapped between the '`' character
		 *   - horizontal ruler, a line with at least 3 hyphens
		 */
		void ParseMarkdown (string markdown, IRichTextBuffer buffer)
		{
			var lines = markdown.Replace ("\r\n", "\n").Split (new[] { '\n' });
			var wasParagraph = false;

			for (int i = 0; i < lines.Length; i++) {
				var line = lines[i];
				// New paragraph
				if (string.IsNullOrWhiteSpace (line)) {
					if (wasParagraph) {
						buffer.EmitEndParagraph ();
						wasParagraph = false;
					}
				}

				// Title
				else if (line.StartsWith ("#")) {
					var level = line.TakeWhile (c => c == '#').Count ();
					buffer.EmitHeader (line.Trim (' ', '#'), level);
				}

				// Title (setex-style)
				else if (i < lines.Length - 1 && !string.IsNullOrEmpty (lines[i + 1]) && lines[i + 1].All (c => c == '=' || c == '-')) {
					var level = lines[i + 1][0] == '=' ? 1 : 2;
					//
					//	FooBarBaz
					//	SomeHeader
					//	=========

					// In the above Markdown snippet we generate a paragraph and then want to insert a header, so we
					// must close the paragraph containing 'FooBarBaz' first. Or we should disallow this construct
					if (wasParagraph) {
						wasParagraph = false;
						buffer.EmitEndParagraph ();
					}
					buffer.EmitHeader (line, level);
					i++;
				}

				// Ruler
				else if (line.All (c => c == '-') && line.Length >= 3) {
					buffer.EmitHorizontalRuler ();
				}

				// Code blocks
				else if (line.StartsWith ("\t") || line.StartsWith ("    ")) {
					var codeblock = new StringBuilder ();
					for (; i < lines.Length; i++) {
						line = lines[i];
						if (!line.StartsWith ("\t") && !line.StartsWith ("    "))
							break;
						codeblock.AppendLine (line.StartsWith ("\t") ? line.Substring (1) : line.Substring (4));
					}
					i--;
					if (wasParagraph) {
						buffer.EmitEndParagraph ();
						wasParagraph = false;
					}
					buffer.EmitCodeBlock (codeblock.Replace ("\n", Environment.NewLine).ToString ());
				}

				// List
				else if (new[] { '+', '-', '*' }.Contains (line.TrimStart()[0])) {
					buffer.EmitOpenList ();
					var bullet = line[0].ToString ();
					for (; i < lines.Length; i++) {
						line = lines[i];
						if (!line.StartsWith (bullet))
							break;
						buffer.EmitOpenBullet ();
						ParseText (buffer, line.TrimStart (' ', '-'));
						buffer.EmitCloseBullet ();
					}
					i--;
					buffer.EmitCloseList ();
				}

				// Normal paragraph
				else {
					if (!wasParagraph)
						buffer.EmitStartParagraph ();
					ParseText (buffer, line);
					wasParagraph = true;
				}
			}

			// If we don't end in a newline we need to end the open paragrah
			if (wasParagraph)
				buffer.EmitEndParagraph ();
		}

		void ParseText (IRichTextBuffer buffer, string line)
		{
			// First transform any embedded URL into a proper format
			line = autoUrl.Replace (line, m => string.Format ("[{0}]({1})", m.Value, m.Value));

			// Then do the rich text parsing
			var match = richText.Match (line);
			int currentIndex = 0;
			while (match.Success) {
				var text = line.Substring (currentIndex, match.Index - currentIndex);
				if (!string.IsNullOrEmpty (text))
					buffer.EmitText (text);
				// Emphasis
				if (match.Groups["char"].Success) {
					RichTextInlineStyle style = 0;
					switch (match.Groups["char"].Value[0]) {
					case '*':
						style |= RichTextInlineStyle.Bold;
						break;
					case '_':
						style |= RichTextInlineStyle.Italic;
						break;
					case '`':
						style |= RichTextInlineStyle.Monospace;
						break;
					}
					buffer.EmitStyledText (match.Groups["emph"].Value, style);
				}
				// Link
				else {
					var url = match.Groups["url"].Value;
					var name = match.Groups["name"].Value;
					buffer.EmitLink (url, name);
				}
				currentIndex = match.Index + match.Length;
				match = match.NextMatch ();
			}
			// Add remaining text
			buffer.EmitText (line.Substring (currentIndex));
		}

		static Regex richText = new Regex (@"\[(?<name>.+)\]\((?<url>.+)\)
		                                     |(?<char>\*)\*(?<emph>.+)\*\*
		                                     |(?<char>`)(?<emph>.+)`
		                                     |(?<char>_)_(?<emph>.+)__",
		                                   RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
		// See http://daringfireball.net/2010/07/improved_regex_for_matching_urls
		static Regex autoUrl = new Regex (@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))",
		                                  RegexOptions.Singleline | RegexOptions.Compiled);
	}
}

