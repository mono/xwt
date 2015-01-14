// Util.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using SWC = System.Windows.Controls;
using SWD = System.Windows.Documents;
using SWM = System.Windows.Media;

namespace Xwt.WPFBackend
{
	public static class Util
	{
		public static Size GetPixelRatios (this Visual self)
		{
			var source = PresentationSource.FromVisual (self);
			if (source == null)
				return new Size (1, 1);

			Matrix m = source.CompositionTarget.TransformToDevice;
			return new Size (m.M11, m.M22);
		}

		public static double GetScaleFactor (this Visual self)
		{
			PresentationSource source = PresentationSource.FromVisual (self);
			if (source == null)
				return 1;

			Matrix m = source.CompositionTarget.TransformToDevice;
			return m.M11;
		}

		public static HorizontalAlignment ToWpfHorizontalAlignment(Alignment alignment)
		{
			switch (alignment) {
			case Alignment.Start:
				return HorizontalAlignment.Left;
			case Alignment.Center:
				return HorizontalAlignment.Center;
			case Alignment.End:
				return HorizontalAlignment.Right;
			}

			throw new InvalidOperationException("Invalid alignment value: " + alignment);
        }

        public static void SetFormattedText (this SWC.TextBlock textBlock, FormattedText text, object backend)
        {
            var atts = new List<Drawing.TextAttribute> (text.Attributes);
            atts.Sort ((a, b) => {
                var c = a.StartIndex.CompareTo (b.StartIndex);
                if (c == 0)
                    c = -(a.Count.CompareTo (b.Count));
                return c;
            });

            int i = 0, attrIndex = 0;
            textBlock.Inlines.Clear ();
            GenerateTextBlocks (backend, textBlock.Inlines, text.Text, ref i, text.Text.Length, atts, ref attrIndex);
        }

        static void GenerateTextBlocks (object backend, SWD.InlineCollection col, string text, ref int i, int spanEnd, List<Drawing.TextAttribute> attributes, ref int attrIndex)
        {
            while (attrIndex < attributes.Count) {
                var at = attributes[attrIndex];
                if (at.StartIndex > spanEnd) {
                    FlushText (col, text, ref i, spanEnd);
                    return;
                }

                FlushText (col, text, ref i, at.StartIndex);

                var s = new SWD.Span ();

                if (at is Drawing.BackgroundTextAttribute) {
                    s.Background = new SolidColorBrush (((Drawing.BackgroundTextAttribute)at).Color.ToWpfColor ());
                }
                else if (at is Drawing.FontWeightTextAttribute) {
                    s.FontWeight = ((Drawing.FontWeightTextAttribute)at).Weight.ToWpfFontWeight ();
                }
                else if (at is Drawing.FontStyleTextAttribute) {
                    s.FontStyle = ((Drawing.FontStyleTextAttribute)at).Style.ToWpfFontStyle ();
                }
                else if (at is Drawing.UnderlineTextAttribute) {
                    var xa = (Drawing.UnderlineTextAttribute)at;
                    var dec = new TextDecoration (TextDecorationLocation.Underline, null, 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);
                    s.TextDecorations.Add (dec);
                }
                else if (at is Drawing.StrikethroughTextAttribute) {
                    var xa = (Drawing.UnderlineTextAttribute)at;
                    var dec = new TextDecoration (TextDecorationLocation.Strikethrough, null, 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);
                    s.TextDecorations.Add (dec);
                }
                else if (at is Drawing.FontTextAttribute) {
                    var xa = (Drawing.FontTextAttribute)at;
                    s.FontFamily = new FontFamily (xa.Font.Family);
                    s.FontSize = WpfFontBackendHandler.GetPointsFromDeviceUnits (xa.Font.Size);
                    s.FontStretch = xa.Font.Stretch.ToWpfFontStretch ();
                    s.FontStyle = xa.Font.Style.ToWpfFontStyle ();
                    s.FontWeight = xa.Font.Weight.ToWpfFontWeight ();
                }
                else if (at is Drawing.ColorTextAttribute) {
                    s.Foreground = new SolidColorBrush (((Drawing.ColorTextAttribute)at).Color.ToWpfColor ());
                }
                else if (at is Drawing.LinkTextAttribute && backend is IWpfLinkNavigator) {
                    var link = new SWD.Hyperlink () {
                        NavigateUri = ((Drawing.LinkTextAttribute)at).Target
                    };
                    link.RequestNavigate += ((IWpfLinkNavigator)backend).LinkRequestNavigate;
                    s = link;
                }

                col.Add (s);

                var max = i + at.Count;
                if (max > spanEnd)
                    max = spanEnd;

                attrIndex++;
                GenerateTextBlocks (backend, s.Inlines, text, ref i, i + at.Count, attributes, ref attrIndex);
            }
            FlushText (col, text, ref i, spanEnd);
        }

        static void FlushText (SWD.InlineCollection col, string text, ref int i, int pos)
        {
            if (pos > i) {
                col.Add (text.Substring (i, pos - i));
                i = pos;
            }
        }
    }

    interface IWpfLinkNavigator
    {
        void LinkRequestNavigate (object sender, System.Windows.Navigation.RequestNavigateEventArgs e);
    }
}