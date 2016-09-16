//
// MarkupTextFormat.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2016 Microsoft Corporation
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
using System.IO;
using Xwt.Backends;

namespace Xwt.Formats
{
	public class MarkupTextFormat : TextFormat
	{
		public override void Parse (Stream input, IRichTextBuffer buffer)
		{
			using (var reader = new StreamReader (input))
				ParseMarkup (reader.ReadToEnd (), buffer);
		}

		static void ParseMarkup (string markup, IRichTextBuffer buffer)
		{
			var paragraphs = markup.Replace ("\r\n", "\n").Split (new [] { "\n\n" }, StringSplitOptions.None);
			foreach (var p in paragraphs) {
				buffer.EmitStartParagraph (0);
				var formatted = FormattedText.FromMarkup (p);
				if (formatted.Attributes.Count > 0)
					buffer.EmitText (formatted);
				else
					buffer.EmitText (p, RichTextInlineStyle.Normal);
				buffer.EmitEndParagraph ();
			}
		}
	}
}
