//
// MarkdownTextFormat.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
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
		 *   - Headers in Atx-style i.e. prefixed with one or more '#' characters and in Setex-style i.e. underlined '=' or '-'
		 *   - Paragraph are separated by a blank line
		 *   - Line break inserted by a double space ("  ") at the end of the line
		 *   - A link has the syntax: "[This link](http://example.net/)" only
		 *   - Code blocks are normal paragraph with a 4-spaces or 1-tab space prepended
		 *   - A list is a number of text line with no newlines in between and prefixed by one of '+', '-' or '*' with whitespace immediately following. no nesting
		 *   - Italic is by putting a portion of text between '*' or '_'
		 *   - Bold is by putting a portion of text between '**' or '__'
		 *   - Inline code is wrapped between the '`' character
		 *   - horizontal ruler, a line with at least 3 hyphens
		 *
		 * Notable things we don't support (yet):
		 *
		 *   - Blockquotes syntax (lines starting in '>')
		 *   - Reference link syntax: [Google] [1]  ... [1]: http://google.com
		 *   - Images
		 *   - Inline HTML
		 */
		static void ParseMarkdown (string markdown, IRichTextBuffer buffer)
		{
			var lines = markdown.Replace ("\r\n", "\n").Split (new[] { '\n' });
			var wasParagraph = false;

			for (int i = 0; i < lines.Length; i++) {
				var line = lines[i];
				var trimmed = line.TrimStart ();
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
					buffer.EmitStartHeader (level);
					ParseInline (buffer, line.Trim (' ', '#'));
					buffer.EmitEndHeader ();
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
					buffer.EmitStartHeader (level);
					ParseInline (buffer, line);
					buffer.EmitEndHeader ();
					i++;
				}

				// Ruler
				else if (line.All (c => c == '-') && line.Length >= 3) {
					buffer.EmitHorizontalRuler ();
				}

				// Code blocks
				else if (line.StartsWith ("\t") || line.StartsWith ("    ") || line.StartsWith ("```")) {
					bool isFencedCodeBlock = line.StartsWith ("```");

					if (isFencedCodeBlock)
						i++;

					var codeblock = new StringBuilder ();
					for (; i < lines.Length; i++) {
						line = lines[i];
						if (!line.StartsWith ("\t") && !line.StartsWith ("    ") && !isFencedCodeBlock)
							break;
						if (isFencedCodeBlock && line.StartsWith ("```")) {
							i++;
							break;
						}

						if (isFencedCodeBlock && !line.StartsWith ("```"))
							codeblock.AppendLine (line);
						else
							codeblock.AppendLine (line.StartsWith ("\t") ? line.Substring (1) : line.Substring (4));
					}
					i--;
					if (wasParagraph) {
						buffer.EmitEndParagraph ();
						wasParagraph = false;
					}
					buffer.EmitCodeBlock (codeblock.ToString ());
				}

				// List
				else if ((trimmed [0] == '+' || trimmed [0] == '-' || trimmed [0] == '*') && (trimmed [1] == ' ' || trimmed [1] == '\t')) {
					buffer.EmitOpenList ();
					var bullet = trimmed[0].ToString ();
					for (; i < lines.Length; i++) {
						trimmed = lines[i].TrimStart ();
						if (!trimmed.StartsWith (bullet))
							break;
						buffer.EmitOpenBullet ();
						var lineBreaks = new List<string> ();
						lineBreaks.Add (trimmed.TrimStart ('+', '-', '*', ' ', '\t'));
						while (i + 1 < lines.Length)
						{
							string nextTrimmed = lines[i + 1].TrimStart ();
							if (nextTrimmed.Length > 0 && !nextTrimmed.StartsWith (bullet))
							{
								lineBreaks.Add (nextTrimmed);
								i++;
							}
							else
								break;
						}
						ParseInline (buffer, string.Join (" ", lineBreaks));
						buffer.EmitCloseBullet ();
					}
					i--;
					buffer.EmitCloseList ();
				}

				// Normal paragraph
				else {
					if (!wasParagraph)
						buffer.EmitStartParagraph (0);
					ParseInline (buffer, line.TrimEnd () + (line.EndsWith ("  ")? Environment.NewLine : " "));
					wasParagraph = true;
				}
			}

			// If we don't end in a newline we need to end the open paragrah
			if (wasParagraph)
				buffer.EmitEndParagraph ();
		}

		static void ParseInline (IRichTextBuffer buffer, string line)
		{
			var match = inline.Match (line);
			int currentIndex = 0;
			while (match.Success) {
				var escaped = match.Index != 0 && line [match.Index - 1] == '\\';
				if (!escaped) {

					var text = line.Substring (currentIndex, match.Index - currentIndex);
					if (!string.IsNullOrEmpty (text))
						ParseText (buffer, text);

					// Link
					{
						var url = match.Groups["url"].Value;
						var name = match.Groups["name"].Success? match.Groups["name"].Value : url;
						var title = match.Groups["title"].Value;
						buffer.EmitStartLink (url, title);
						ParseText (buffer, name);
						buffer.EmitEndLink ();
					}

					currentIndex = match.Index + match.Length;

				}
				match = match.NextMatch ();
			}
			// Add remaining text
			ParseText (buffer, line.Substring (currentIndex));
		}

		static void ParseText (IRichTextBuffer buffer, string line, RichTextInlineStyle style = RichTextInlineStyle.Normal)
		{
			var match = styles.Match (line);
			int currentIndex = 0;
			while (match.Success) {
				var escaped = match.Index != 0 && line [match.Index - 1] == '\\';
				if (!escaped) {

					var text = line.Substring (currentIndex, match.Index - currentIndex);
					if (!string.IsNullOrEmpty (text))
						EmitText (buffer, text, style);

					if (match.Groups["bold"].Success)
						ParseText (buffer, match.Groups["bold"].Value, style | RichTextInlineStyle.Bold);
					else if (match.Groups["italic"].Success)
						ParseText (buffer, match.Groups["italic"].Value, style | RichTextInlineStyle.Italic);
					else
						EmitText (buffer, match.Groups["code"].Value, style | RichTextInlineStyle.Monospace);

					currentIndex = match.Index + match.Length;

				}
				match = match.NextMatch ();
			}
			// Add remaining text
			EmitText (buffer, line.Substring (currentIndex), style);
		}

		static void EmitText (IRichTextBuffer buffer, string text, RichTextInlineStyle style)
		{
			text = escape.Replace (text, m => m.Groups["next"].Value);
			buffer.EmitText (text, style);
		}

		static readonly Regex escape = new Regex (@"\\(?<next>.)", RegexOptions.Singleline | RegexOptions.Compiled);
		static readonly Regex inline = new Regex (@"\[(?<name>[^\]]+)\]\((?<url>[^\s""\)]+)(?:[ \t]*""(?<title>.*)"")?\)" + //link
		                                          // See http://daringfireball.net/2010/07/improved_regex_for_matching_urls
		                                          @"|(?i)\b(?<url>(?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))"
		                                               //FIXME: image, etc...
		                                           , RegexOptions.Singleline | RegexOptions.Compiled);

		static readonly Regex styles = new Regex (@"(?<double>\*{2}|_{2})(?<bold>[^\s]+[^\*{2}_{2}]*)(?<!\s)\k<double>" + // emphasis: double ** or __ for bold
		                                          @"|(?<single>\*|_)(?<italic>[^\s]+[^\*_]*)(?<!\s)\k<single>" + // emphasis: single * or _ for italic
		                                          @"|`(?<code>[^`]+)`" // inline code
		                                          , RegexOptions.Compiled);
	}
}

