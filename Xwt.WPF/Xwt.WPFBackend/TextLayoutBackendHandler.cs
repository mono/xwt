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
using System.Collections.Generic;

namespace Xwt.WPFBackend
{
	public class WpfTextLayoutBackendHandler
		: TextLayoutBackendHandler
	{
		public override object Create ()
		{
			return new TextLayoutBackend (ApplicationContext);
		}

		public override void SetWidth (object backend, double value)
		{
			var t = (TextLayoutBackend)backend;
			t.SetWidth(value);
		}

		public override void SetHeight (object backend, double value)
		{
			var t = (TextLayoutBackend)backend;
			t.SetHeight(value);
		}

		public override void SetText (object backend, string text)
		{
			var t = (TextLayoutBackend)backend;
			t.SetText(text);
		}

		public override void SetFont (object backend, Font font)
		{
			var t = (TextLayoutBackend)backend;
			t.SetFont (font);
		}

		public override void SetTrimming (object backend, Xwt.Drawing.TextTrimming textTrimming)
		{
			var t = (TextLayoutBackend)backend;
			t.SetTrimming(textTrimming);
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

		public override double GetBaseline (object backend)
		{
			var t = (TextLayoutBackend)backend;
			return t.FormattedText.Baseline;
		}

		public override double GetMeanline (object backend)
		{
			var t = (TextLayoutBackend)backend;
			var fd = (FontData)ApplicationContext.Toolkit.GetSafeBackend(t.Font);
			var tf = new Typeface (fd.Family, fd.Style, fd.Weight, fd.Stretch);

			return t.FormattedText.Baseline - tf.StrikethroughPosition * WpfFontBackendHandler.GetDeviceUnitsFromPoints (fd.Size);
		}

		public override void AddAttribute (object backend, TextAttribute attribute)
		{
			var t = (TextLayoutBackend)backend;
			t.AddAttribute(attribute);
		}

		public override void ClearAttributes (object backend)
		{
			var t = (TextLayoutBackend)backend;
			t.ClearAttributes();
		}
	}

	class TextLayoutBackend
	{
		static Typeface defaultFont = new Typeface("Verdana");

		System.Windows.Media.FormattedText formattedText;
		List<TextAttribute> attributes;
		SolidColorBrush brush = System.Windows.Media.Brushes.Black;
		double width;
		double height;
		string text = String.Empty;
		Xwt.Drawing.TextTrimming? textTrimming;
		bool needsRebuild;

		readonly ApplicationContext ApplicationContext;

		public TextLayoutBackend (ApplicationContext actx)
		{
			this.ApplicationContext = actx;
		}

		public System.Windows.Media.FormattedText FormattedText
		{
			get
			{
				if (formattedText == null || needsRebuild)
					Rebuild ();
				return formattedText;
			}
		}

		public void SetWidth(double value)
		{
			if (width != value)
			{
				width = value;
				if (formattedText != null)
				{
					if (value >= 0)
						FormattedText.MaxTextWidth = value;
					else
						needsRebuild = true;
				}
			}
		}

		public void SetHeight(double value)
		{
			if (height != value)
			{
				height = value;
				if (formattedText != null)
				{
					if (value >= 0)
						FormattedText.MaxTextHeight = value;
					else
						needsRebuild = true;
				}
			}
		}

		public void SetText(string text)
		{
			if (this.text != text)
			{
				this.text = text ?? String.Empty;
				needsRebuild = true;
			}
		}

		public void SetFont(Font font)
		{
			if (!font.Equals(this.Font))
			{
				this.Font = font;
				if (formattedText != null)
					ApplyFont();
			}
		}

		void ApplyFont ()
		{
			var f = (FontData)ApplicationContext.Toolkit.GetSafeBackend (Font);
			FormattedText.SetFontFamily(f.Family);
			FormattedText.SetFontSize(f.GetDeviceIndependentPixelSize());
			FormattedText.SetFontStretch(f.Stretch);
			FormattedText.SetFontStyle(f.Style);
			FormattedText.SetFontWeight(f.Weight);
		}

		public void SetTrimming(Xwt.Drawing.TextTrimming textTrimming)
		{
			if (this.textTrimming != textTrimming)
			{
				this.textTrimming = textTrimming;
				if (formattedText != null)
					ApplyTrimming();
			}
		}

		void ApplyTrimming ()
		{
			switch (textTrimming)
			{
				case Xwt.Drawing.TextTrimming.WordElipsis:
					FormattedText.Trimming = System.Windows.TextTrimming.WordEllipsis;
					break;
				default:
					FormattedText.Trimming = System.Windows.TextTrimming.None;
					break;
			}
		}

		public void SetDefaultForeground(SolidColorBrush fg)
		{
			if (fg.Color != brush.Color) {
				brush = fg;
				needsRebuild = true;
			}
		}

		public void AddAttribute(TextAttribute attribute)
		{
			if (attributes == null)
				attributes = new List<TextAttribute>();
			attributes.Add(attribute);
			if (formattedText != null)
				ApplyAttribute(attribute);
		}

		public void ClearAttributes ()
		{
			attributes = null;
			needsRebuild = true;
		}

		void ApplyAttribute(TextAttribute attribute)
		{
			if (attribute is FontStyleTextAttribute)
			{
				var xa = (FontStyleTextAttribute)attribute;
				FormattedText.SetFontStyle(xa.Style.ToWpfFontStyle(), xa.StartIndex, xa.Count);
			}
			else if (attribute is FontWeightTextAttribute)
			{
				var xa = (FontWeightTextAttribute)attribute;
				FormattedText.SetFontWeight(xa.Weight.ToWpfFontWeight(), xa.StartIndex, xa.Count);
			}
			else if (attribute is ColorTextAttribute)
			{
				var xa = (ColorTextAttribute)attribute;
				FormattedText.SetForegroundBrush(new SolidColorBrush(xa.Color.ToWpfColor()), xa.StartIndex, xa.Count);
			}
			else if (attribute is StrikethroughTextAttribute)
			{
				var xa = (StrikethroughTextAttribute)attribute;
				var dec = new TextDecoration(TextDecorationLocation.Strikethrough, null, 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);
				TextDecorationCollection col = new TextDecorationCollection();
				col.Add(dec);
				FormattedText.SetTextDecorations(col, xa.StartIndex, xa.Count);
			}
			else if (attribute is UnderlineTextAttribute)
			{
				var xa = (UnderlineTextAttribute)attribute;
				var dec = new TextDecoration(TextDecorationLocation.Underline, null, 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);
				TextDecorationCollection col = new TextDecorationCollection();
				col.Add(dec);
				FormattedText.SetTextDecorations(col, xa.StartIndex, xa.Count);
			}
		}

		public void Rebuild ()
		{
			needsRebuild = false;
			var dir = System.Windows.FlowDirection.LeftToRight;
			formattedText = new System.Windows.Media.FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, dir, defaultFont, 36, brush);
			if (width > 0)
				formattedText.MaxTextWidth = width;
			if (height > 0)
				formattedText.MaxTextHeight = height;
			if (Font != null)
				ApplyFont();
			if (textTrimming != null)
				ApplyTrimming();

			if (attributes != null)
				foreach (var at in attributes)
					ApplyAttribute(at);
		}

		public DrawingContext Context;
		public Font Font { get; private set; }
	}
}
