//
// TextLayoutBackendHandler.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
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
using Xwt.Backends;
using Xwt.Drawing;

using Font = Xwt.Drawing.Font;
using System.Windows.Media;
using System.Windows;

namespace Xwt.WPFBackend
{
	public class WpfTextLayoutBackendHandler
		: TextLayoutBackendHandler
	{
		public override object Create ()
		{
			return new TextLayoutBackend ();
		}

		public override void SetWidth (object backend, double value)
		{
			var t = (TextLayoutBackend)backend;
			if (value >= 0)
				t.FormattedText.MaxTextWidth = value;
			else
				t.Rebuild (null, false, true);
		}

		public override void SetHeight (object backend, double value)
		{
			var t = (TextLayoutBackend)backend;
			if (value >= 0)
				t.FormattedText.MaxTextHeight = value;
			else
				t.Rebuild (null, true, false);
		}

		public override void SetText (object backend, string text)
		{
			var t = (TextLayoutBackend)backend;
			t.Rebuild (text);
		}

		public override void SetFont (object backend, Font font)
		{
			var t = (TextLayoutBackend)backend;
			t.SetFont (font);
		}

		public override void SetTrimming (object backend, Xwt.Drawing.TextTrimming textTrimming)
		{
			var t = (TextLayoutBackend)backend;
			switch (textTrimming) {
				case Xwt.Drawing.TextTrimming.WordElipsis:
					t.FormattedText.Trimming = System.Windows.TextTrimming.WordEllipsis;
					break;
				default:
					t.FormattedText.Trimming = System.Windows.TextTrimming.None;
					break;
			}
		}

		public override Size GetSize (object backend)
		{
			var t = (TextLayoutBackend)backend;
			return new Size (t.FormattedText.WidthIncludingTrailingWhitespace, t.FormattedText.Height);
		}

		public override Point GetCoordinateFromIndex (object backend, int index)
		{
			return Point.Zero;
		}

		public override int GetIndexFromCoordinates (object backend, double x, double y)
		{
			return 0;
		}

		public override void AddAttribute (object backend, TextAttribute attribute)
		{
			var t = (TextLayoutBackend)backend;
			if (attribute is FontStyleTextAttribute) {
				var xa = (FontStyleTextAttribute)attribute;
				t.FormattedText.SetFontStyle (xa.Style.ToWpfFontStyle (), xa.StartIndex, xa.Count);
			}
			else if (attribute is FontWeightTextAttribute) {
				var xa = (FontWeightTextAttribute)attribute;
				t.FormattedText.SetFontWeight (xa.Weight.ToWpfFontWeight (), xa.StartIndex, xa.Count);
			}
			else if (attribute is ColorTextAttribute) {
				var xa = (ColorTextAttribute)attribute;
				t.FormattedText.SetForegroundBrush (new SolidColorBrush (xa.Color.ToWpfColor ()), xa.StartIndex, xa.Count);
			}
			else if (attribute is StrikethroughTextAttribute) {
				var xa = (StrikethroughTextAttribute)attribute;
				var dec = new TextDecoration (TextDecorationLocation.Strikethrough, null, 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);
				TextDecorationCollection col = new TextDecorationCollection ();
				col.Add (dec);
				t.FormattedText.SetTextDecorations (col, xa.StartIndex, xa.Count);
			}
			else if (attribute is UnderlineTextAttribute) {
				var xa = (UnderlineTextAttribute)attribute;
				var dec = new TextDecoration (TextDecorationLocation.Underline, null, 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);
				TextDecorationCollection col = new TextDecorationCollection ();
				col.Add (dec);
				t.FormattedText.SetTextDecorations (col, xa.StartIndex, xa.Count);
			}
		}

		public override void ClearAttributes (object backend)
		{
		}

		public override void DisposeBackend (object backend)
		{
		}
	}

	class TextLayoutBackend
	{
		System.Windows.Media.FormattedText formattedText;

		public System.Windows.Media.FormattedText FormattedText
		{
			get
			{
				if (formattedText == null)
					Rebuild ("");
				return formattedText;
			}
		}

		public void Rebuild (string newText = null, bool keepWidth = true, bool keepHeight = true)
		{
			var font = new Typeface ("Verdana");
			var dir = System.Windows.FlowDirection.LeftToRight;
			var old = formattedText;
			if (old != null && old.Text != null && newText == null)
				newText = old.Text;
			formattedText = new System.Windows.Media.FormattedText(newText ?? "", System.Globalization.CultureInfo.CurrentCulture, dir, font, 36, System.Windows.Media.Brushes.Black);
			if (old != null) {
				if (old.MaxTextWidth != 0 && keepWidth)
					formattedText.MaxTextWidth = old.MaxTextWidth;
				if (old.MaxTextHeight != 0 && keepHeight)
					formattedText.MaxTextHeight = old.MaxTextHeight;
			}
			if (Font != null)
				SetFont (Font);
		}

		public void SetFont (Font font)
		{
			this.Font = font;
			var f = (FontData)Toolkit.GetBackend (font);
			FormattedText.SetFontFamily (f.Family);
			FormattedText.SetFontSize (f.GetDeviceIndependentPixelSize ());
			FormattedText.SetFontStretch (f.Stretch);
			FormattedText.SetFontStyle (f.Style);
			FormattedText.SetFontWeight (f.Weight);
		}

		public DrawingContext Context;
		public Font Font;
	}
}
