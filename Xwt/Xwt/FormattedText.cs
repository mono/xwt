//
// FormattedText.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using Xwt.Drawing;
using System.Text;
using System.Globalization;

namespace Xwt
{
	public class FormattedText
	{
		List<TextAttribute> attributes = new List<TextAttribute> ();

		public List<TextAttribute> Attributes {
			get { return attributes; }
		}

		public string Text { get; set; }

		class SpanInfo: List<TextAttribute>
		{
			public string Tag;
		}

		public static FormattedText FromMarkup (string markup)
		{
			FormattedText t = new FormattedText ();
			t.ParseMarkup (markup);
			return t;
		}

		void ParseMarkup (string markup)
		{
			Stack<SpanInfo> formatStack = new Stack<SpanInfo> ();
			StringBuilder sb = new StringBuilder ();
			int last = 0;
			int i = markup.IndexOf ('<');
			while (i != -1) {
				sb.Append (markup, last, i - last);
				if (PushSpan (formatStack, markup, sb.Length, ref i)) {
					last = i;
					i = markup.IndexOf ('<', i);
					continue;
				}
				if (PopSpan (formatStack, markup, sb.Length, ref i)) {
					last = i;
					i = markup.IndexOf ('<', i);
					continue;
				}
				last = i;
				i = markup.IndexOf ('<', i + 1);
			}
			sb.Append (markup, last, markup.Length - last);
			Text = sb.ToString ();
		}

		bool PushSpan (Stack<SpanInfo> formatStack, string markup, int textIndex, ref int i)
		{
			// <span kk="jj">

			int k = i;
			k++; // Skip the <

			string tag;
			if (!ReadId (markup, ref k, out tag))
				return false;

			SpanInfo span = new SpanInfo ();
			span.Tag = tag;

			switch (tag) {
			case "b":
				span.Add (new FontWeightTextAttribute () { Weight = FontWeight.Bold });
				break;
			case "i":
				span.Add (new FontStyleTextAttribute () { Style = FontStyle.Italic });
				break;
			case "s":
				span.Add (new StrikethroughTextAttribute ());
				break;
			case "u":
				span.Add (new UnderlineTextAttribute ());
				break;
			case "a":
				Uri href = null;
				ReadXmlAttributes (markup, ref k, (name, val) => {
					if (name == "href") {
						href = new Uri (val, UriKind.RelativeOrAbsolute);
						return true;
					}
					return false;
				});
				span.Add (new LinkTextAttribute () { Target = href });
				break;

			case "span":
				ParseAttributes (span, markup, ref k);
				break;
//			case "small":
//			case "big":
//			case "tt":
			}
			if (span.Count == 0)
				return false;

			if (!ReadCharToken (markup, '>', ref k))
				return false;

			foreach (var att in span)
				att.StartIndex = textIndex;

			formatStack.Push (span);
			i = k;
			return true;
		}

		bool PopSpan (Stack<SpanInfo> formatStack, string markup, int textIndex, ref int i)
		{
			if (formatStack.Count == 0)
				return false;

			int k = i;
			k++; // Skip the <

			if (!ReadCharToken (markup, '/', ref k))
				return false;

			string tag;
			if (!ReadId (markup, ref k, out tag))
				return false;

			// Make sure the closing tag matches the opened tag
			if (!string.Equals (tag, formatStack.Peek ().Tag, StringComparison.InvariantCultureIgnoreCase))
				return false;

			if (!ReadCharToken (markup, '>', ref k))
				return false;

			var span = formatStack.Pop ();
			foreach (var attr in span) {
				attr.Count = textIndex - attr.StartIndex;
				if (attr.Count > 0)
					attributes.Add (attr);
			}
			i = k;
			return true;
		}

		bool ParseAttributes (SpanInfo span, string markup, ref int i)
		{
			return ReadXmlAttributes (markup, ref i, (name, val) => {
				var attr = CreateAttribute (name, val);
				if (attr != null) {
					span.Add (attr);
					return true;
				}
				return false;
			});
		}

		bool ReadXmlAttributes (string markup, ref int i, Func<string,string,bool> callback)
		{
			int k = i;

			while (true) {
				string name;
				if (!ReadId (markup, ref k, out name))
					return true; // No more attributes

				if (!ReadCharToken (markup, '=', ref k))
					return false;

				char endChar;
				if (ReadCharToken (markup, '"', ref k))
					endChar = '"';
				else if (ReadCharToken (markup, '\'', ref k))
					endChar = '\'';
				else
					return false;

				int n = markup.IndexOf (endChar, k);
				if (n == -1)
					return false;

				string val = markup.Substring (k, n - k);
				if (callback (name, val))
					i = n + 1;
				else
					return false;
				k = i;
			}
		}

		TextAttribute CreateAttribute (string name, string val)
		{
			switch (name) {
			case "font":
			case "font-desc":
			case "font_desc":
				return new FontTextAttribute () { Font = Font.FromName (val) };

			case "size":
			case "font_size":
				var fontSize = new FontSizeTextAttribute ();
				fontSize.SetSize (val);
				return fontSize;
			case "font_weight":
			case "font-weight":
			case "weight":
				FontWeight w;
				if (!Enum.TryParse<FontWeight> (val, true, out w))
					return null;
				return new FontWeightTextAttribute () { Weight = w };

			case "font_style":
			case "font-style":
				FontStyle s;
				if (!Enum.TryParse<FontStyle> (val, true, out s))
					return null;
				return new FontStyleTextAttribute () { Style = s };

			case "foreground":
			case "fgcolor":
			case "color":
				Color c;
				if (!Color.TryParse (val, out c))
					return null;
				return new ColorTextAttribute () { Color = c };

			case "background":
			case "background-color":
			case "bgcolor":
				Color bc;
				if (!Color.TryParse (val, out bc))
					return null;
				return new BackgroundTextAttribute () { Color = bc };

			case "underline":
				return new UnderlineTextAttribute () {
					Underline = ParseBool (val, false)
				};

			case "strikethrough":
				return new StrikethroughTextAttribute () {
					Strikethrough = ParseBool (val, false)
				};
			}
			return null;
		}

		bool ParseBool (string s, bool defaultValue)
		{
			if (s.Length == 0)
				return defaultValue;
			return string.Equals (s, "true", StringComparison.OrdinalIgnoreCase);
		}
		
		bool ReadId (string markup, ref int i, out string tag)
		{
			tag = null;

			int k = i;
			if (!SkipWhitespace (markup, ref k))
				return false;

			var start = k;
			while (k < markup.Length) {
				char c = markup [k];
				if (!char.IsLetterOrDigit (c) && c != '_' && c != '-')
					break;
				k++;
			}
			if (start == k)
				return false;

			tag = markup.Substring (start, k - start);
			i = k;
			return true;
		}

		bool ReadCharToken (string markup, char c, ref int i)
		{
			int k = i;
			if (!SkipWhitespace (markup, ref k))
				return false;

			if (markup [k] == c) {
				i = k + 1;
				return true;
			} else
				return false;
		}

		bool SkipWhitespace (string markup, ref int k)
		{
			while (k < markup.Length) {
				if (!char.IsWhiteSpace (markup [k]))
					return true;
				k++;
			}
			return false;
		}
	}
}

