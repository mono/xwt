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

#if USE_WPF_RENDERING

using System;
using System.Drawing;
using Xwt.Backends;
using Xwt.Drawing;

using Font = Xwt.Drawing.Font;
using System.Windows.Media;

namespace Xwt.WPFBackend
{
	public class WpfTextLayoutBackendHandler
		: TextLayoutBackendHandler
	{
		public override object Create (Context context)
		{
			var drawingContext = (DrawingContext)Toolkit.GetBackend (context);
			return new TextLayoutBackend () {
				Context = drawingContext,
				PixelRatio = drawingContext.PixelRatio
			};
		}

		public override object Create (ICanvasBackend canvas)
		{
			var backend = (WidgetBackend)canvas;
			return new TextLayoutBackend () {
				PixelRatio = backend.WidthPixelRatio
			};
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

		public override void SetTrimming (object backend, TextTrimming textTrimming)
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
	}

	class TextLayoutBackend
	{
		FormattedText formattedText;

		public double PixelRatio { get; set; }

		public FormattedText FormattedText
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
			formattedText = new FormattedText (newText ?? "", System.Globalization.CultureInfo.CurrentCulture, dir, font, 36, System.Windows.Media.Brushes.Black);
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
			FormattedText.SetFontSize (f.GetDeviceIndependentPixelSize (PixelRatio));
			FormattedText.SetFontStretch (f.Stretch);
			FormattedText.SetFontStyle (f.Style);
			FormattedText.SetFontWeight (f.Weight);
		}

		public DrawingContext Context;
		public Font Font;
	}
}
#endif