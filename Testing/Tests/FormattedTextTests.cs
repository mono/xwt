//
// FormattedTextTests.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc
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
using NUnit.Framework;
using Xwt.Drawing;

namespace Xwt
{
	public class FormattedTextTests
	{
		[Test]
		public void ParseEmptySpan ()
		{
			var s = "0<span>12</span>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (0, ft.Attributes.Count);
		}

		[Test]
		public void ParseIncorrectlyNestedSpan ()
		{
			var s = "0<b><u>12</b></u>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (1, ft.Attributes.Count);
			Assert.IsAssignableFrom<UnderlineTextAttribute> (ft.Attributes [0]);
		}

		[Test]
		public void UnclosedElements ()
		{
			var s = "0<b><u>12</u>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (1, ft.Attributes.Count);
			Assert.IsAssignableFrom<UnderlineTextAttribute> (ft.Attributes [0]);
		}

		[Test]
		public void ParseBrokenMarkup ()
		{
			var s = "0<b<u>12</u>>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (1, ft.Attributes.Count);
			Assert.IsAssignableFrom<UnderlineTextAttribute> (ft.Attributes [0]);
		}

		[Test]
		public void ParseFontSize ()
		{
			//relative checks
			foreach (var item in new string[] { "smaller", "larger" }) {
				AssertFirstAttribute (item, (float)Xwt.Drawing.Font.SystemFont.Size);
			}

			float currentSizeValue;
			//absolute size check
			foreach (var currentSize in FontSizeTextAttribute.SizeAbsoluteValues.Keys) {
				currentSizeValue = FontSizeTextAttribute.SizeAbsoluteValues[currentSize];
				AssertFirstAttribute (currentSize, currentSizeValue);
			}

			//pango size values
			currentSizeValue = 14.5f;
			var pagoSizeValue = currentSizeValue * PangoScale;
			AssertFirstAttribute (pagoSizeValue.ToString (), currentSizeValue);

			//pt values
			AssertFirstAttribute (currentSizeValue.ToString (), currentSizeValue);
		}

		void AssertFirstAttribute (string size, float resultSize)
		{
			var currentSpan = $"<span size='{size}'>(support-v7)</span>";
			var ft = FormattedText.FromMarkup (currentSpan);
			Assert.AreEqual (1, ft.Attributes.Count);
			Assert.IsAssignableFrom<FontSizeTextAttribute> (ft.Attributes[0]);
			var at = (FontSizeTextAttribute)ft.Attributes[0];
			Assert.AreEqual (resultSize, at.Size);
		}

		const int PangoScale = 1024;

		[Test]
		public void ParseFontWeight ()
		{
			var s = "0<b>12</b><span weight='ultrabold'>34</span><span font-weight='Light'>56</span>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (3, ft.Attributes.Count);

			Assert.IsAssignableFrom<FontWeightTextAttribute> (ft.Attributes [0]);
			var at = (FontWeightTextAttribute)ft.Attributes [0];
			Assert.AreEqual (1, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (FontWeight.Bold, at.Weight);

			Assert.IsAssignableFrom<FontWeightTextAttribute> (ft.Attributes [1]);
			at = (FontWeightTextAttribute)ft.Attributes [1];
			Assert.AreEqual (3, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (FontWeight.Ultrabold, at.Weight);

			Assert.IsAssignableFrom<FontWeightTextAttribute> (ft.Attributes [2]);
			at = (FontWeightTextAttribute)ft.Attributes [2];
			Assert.AreEqual (5, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (FontWeight.Light, at.Weight);
		}

		[Test]
		public void ParseItalic ()
		{
			var s = "0<i>12</i>3<span font-style='oBlique'>45</span>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (2, ft.Attributes.Count);

			Assert.IsAssignableFrom<FontStyleTextAttribute> (ft.Attributes [0]);
			var at = (FontStyleTextAttribute)ft.Attributes [0];
			Assert.AreEqual (1, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (FontStyle.Italic, at.Style);

			Assert.IsAssignableFrom<FontStyleTextAttribute> (ft.Attributes [1]);
			at = (FontStyleTextAttribute)ft.Attributes [1];
			Assert.AreEqual (4, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (FontStyle.Oblique, at.Style);
		}

		[Test]
		public void ParseStrikethrough ()
		{
			var s = "0<s>12</s>3<span strikethrough='true'>45</span>6<span strikethrough='f'>7</span><span strikethrough=''>8</span>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (4, ft.Attributes.Count);

			Assert.IsAssignableFrom<StrikethroughTextAttribute> (ft.Attributes [0]);
			var at = (StrikethroughTextAttribute)ft.Attributes [0];
			Assert.AreEqual (1, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.IsTrue (at.Strikethrough);

			Assert.IsAssignableFrom<StrikethroughTextAttribute> (ft.Attributes [1]);
			at = (StrikethroughTextAttribute)ft.Attributes [1];
			Assert.AreEqual (4, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.IsTrue (at.Strikethrough);

			Assert.IsAssignableFrom<StrikethroughTextAttribute> (ft.Attributes [2]);
			at = (StrikethroughTextAttribute)ft.Attributes [2];
			Assert.AreEqual (7, at.StartIndex);
			Assert.AreEqual (1, at.Count);
			Assert.IsFalse (at.Strikethrough);

			Assert.IsAssignableFrom<StrikethroughTextAttribute> (ft.Attributes [3]);
			at = (StrikethroughTextAttribute)ft.Attributes [3];
			Assert.AreEqual (8, at.StartIndex);
			Assert.AreEqual (1, at.Count);
			Assert.IsFalse (at.Strikethrough);
		}

		[Test]
		public void ParseUnderline ()
		{
			var s = "0<u>12</u>3<span underline='true'>45</span>6<span underline='f'>7</span><span underline=''>8</span>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (4, ft.Attributes.Count);

			Assert.IsAssignableFrom<UnderlineTextAttribute> (ft.Attributes [0]);
			var at = (UnderlineTextAttribute)ft.Attributes [0];
			Assert.AreEqual (1, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.IsTrue (at.Underline);

			Assert.IsAssignableFrom<UnderlineTextAttribute> (ft.Attributes [1]);
			at = (UnderlineTextAttribute)ft.Attributes [1];
			Assert.AreEqual (4, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.IsTrue (at.Underline);

			Assert.IsAssignableFrom<UnderlineTextAttribute> (ft.Attributes [2]);
			at = (UnderlineTextAttribute)ft.Attributes [2];
			Assert.AreEqual (7, at.StartIndex);
			Assert.AreEqual (1, at.Count);
			Assert.IsFalse (at.Underline);

			Assert.IsAssignableFrom<UnderlineTextAttribute> (ft.Attributes [3]);
			at = (UnderlineTextAttribute)ft.Attributes [3];
			Assert.AreEqual (8, at.StartIndex);
			Assert.AreEqual (1, at.Count);
			Assert.IsFalse (at.Underline);
		}

		[Test]
		public void ParseAnchor ()
		{
			var s = "<a href='http://foo.bar'>01</a><a href='#anchor'>23</a><a href='relative'>45</a><a href='custom:uri'>67</a><a href=''>89</a>";
			var ft = FormattedText.FromMarkup (s);

			Assert.AreEqual (5, ft.Attributes.Count);
			Assert.IsAssignableFrom<LinkTextAttribute> (ft.Attributes [0]);
			var at = (LinkTextAttribute)ft.Attributes [0];
			Assert.AreEqual (0, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (new Uri ("http://foo.bar", UriKind.RelativeOrAbsolute), at.Target);

			Assert.IsAssignableFrom<LinkTextAttribute> (ft.Attributes [1]);
			at = (LinkTextAttribute)ft.Attributes [1];
			Assert.AreEqual (2, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (new Uri ("#anchor", UriKind.RelativeOrAbsolute), at.Target);

			Assert.IsAssignableFrom<LinkTextAttribute> (ft.Attributes [2]);
			at = (LinkTextAttribute)ft.Attributes [2];
			Assert.AreEqual (4, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (new Uri ("relative", UriKind.RelativeOrAbsolute), at.Target);

			Assert.IsAssignableFrom<LinkTextAttribute> (ft.Attributes [3]);
			at = (LinkTextAttribute)ft.Attributes [3];
			Assert.AreEqual (6, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (new Uri ("custom:uri", UriKind.RelativeOrAbsolute), at.Target);

			Assert.IsAssignableFrom<LinkTextAttribute> (ft.Attributes [4]);
			at = (LinkTextAttribute)ft.Attributes [4];
			Assert.AreEqual (8, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (new Uri ("", UriKind.RelativeOrAbsolute), at.Target);
		}

		[Test]
		public void ParseColor ()
		{
			var s = "0<span color='#ff0000'>12</span>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (1, ft.Attributes.Count);

			Assert.IsAssignableFrom<ColorTextAttribute> (ft.Attributes [0]);
			var at = (ColorTextAttribute)ft.Attributes [0];
			Assert.AreEqual (1, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (Colors.Red, at.Color);
		}

		[Test]
		public void ParseBackgroundColor ()
		{
			var s = "0<span background-color='#0000ff'>12</span>";
			var ft = FormattedText.FromMarkup (s);
			Assert.AreEqual (1, ft.Attributes.Count);

			Assert.IsAssignableFrom<BackgroundTextAttribute> (ft.Attributes [0]);
			var at = (BackgroundTextAttribute)ft.Attributes [0];
			Assert.AreEqual (1, at.StartIndex);
			Assert.AreEqual (2, at.Count);
			Assert.AreEqual (Colors.Blue, at.Color);
		}
	}
}

