//
// DataConverter.cs
//
// Authors:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
//       Luís Reis <luiscubal@gmail.com>
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2011-2012 Carlos Alberto Cortez
// Copyright (c) 2012 Luís Reis
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
using System.Windows;
using System.Windows.Input;
using SW = System.Windows;
using SD = System.Drawing;
using Xwt.Drawing;
using FontStretch = Xwt.Drawing.FontStretch;
using FontStyle = Xwt.Drawing.FontStyle;
using FontWeight = Xwt.Drawing.FontWeight;

namespace Xwt.WPFBackend
{
	internal static class DataConverter
	{
		//
		// Rect/Point
		//
		public static Rectangle ToXwtRect (this SW.Rect rect)
		{
			return new Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static SW.Rect ToWpfRect (this Rectangle rect)
		{
			return new SW.Rect (rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static Int32Rect ToInt32Rect (this Rectangle rect)
		{
			return new Int32Rect ((int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height);
		}

		public static Point ToXwtPoint (this SW.Point point)
		{
			return new Point (point.X, point.Y);
		}

		public static SW.Point ToWpfPoint (this Point point)
		{
			return new SW.Point (point.X, point.Y);
		}

		//
		// Alignment
		//
		public static Alignment ToXwtAlignment (this SW.HorizontalAlignment alignment)
		{
			switch (alignment) {
				case SW.HorizontalAlignment.Left: return Alignment.Start;
				case SW.HorizontalAlignment.Center: return Alignment.Center;
				default: return Alignment.End;
			}
		}

		public static SW.HorizontalAlignment ToWpfAlignment (this Alignment alignment)
		{
			switch (alignment) {
				case Alignment.Start: return SW.HorizontalAlignment.Left;
				case Alignment.Center: return SW.HorizontalAlignment.Center;
				default: return SW.HorizontalAlignment.Right;
			}
		}

		//
		// Color
		//
		public static Color ToXwtColor (this SW.Media.Color color)
		{
			return Color.FromBytes (color.R, color.G, color.B, color.A);
		}

		public static Color ToXwtColor (this SW.Media.Brush brush)
		{
			var solidBrush = brush as SW.Media.SolidColorBrush;
			if (solidBrush == null)
				throw new ArgumentException();

			return solidBrush.Color.ToXwtColor();
		}

		public static SW.Media.Color ToWpfColor (this Color color)
		{
			return SW.Media.Color.FromArgb (
				(byte)(color.Alpha * 255.0),
				(byte)(color.Red * 255.0),
				(byte)(color.Green * 255.0),
				(byte)(color.Blue * 255.0));
		}

		public static System.Drawing.Color ToDrawingColor (this Color color)
		{
			return System.Drawing.Color.FromArgb (
				(byte) (color.Alpha * 255.0),
				(byte) (color.Red * 255.0),
				(byte) (color.Green * 255.0),
				(byte) (color.Blue * 255.0));
		}

		//
		// Font
		//
		public static SD.Font ToDrawingFont (this Font font)
		{
			SD.FontStyle style = font.Style.ToDrawingFontStyle ();
			if (font.Weight > FontWeight.Normal)
				style |= SD.FontStyle.Bold;

			return new SD.Font (font.Family, (float)font.Size, style);
		}
		
		public static FontStyle ToXwtFontStyle (this SW.FontStyle value)
		{
			// No, SW.FontStyles is not an enum
			if (value == SW.FontStyles.Italic) return FontStyle.Italic;
			if (value == SW.FontStyles.Oblique) return FontStyle.Oblique;

			return FontStyle.Normal;
		}

		public static SW.FontStyle ToWpfFontStyle (this FontStyle value)
		{
			if (value == FontStyle.Italic) return SW.FontStyles.Italic;
			if (value == FontStyle.Oblique) return SW.FontStyles.Oblique;
			
			return SW.FontStyles.Normal;
		}

		public static SD.FontStyle ToDrawingFontStyle (this FontStyle value)
		{
			switch (value) {
				case FontStyle.Normal:
					return SD.FontStyle.Regular;
				case FontStyle.Italic:
					return SD.FontStyle.Italic;
				
				default:
					throw new NotImplementedException();
			}
		}

		public static FontStretch ToXwtFontStretch (this SW.FontStretch value)
		{
			// No, SW.FontStretches is not an enum
			if (value == SW.FontStretches.UltraCondensed) return FontStretch.UltraCondensed;
			if (value == SW.FontStretches.ExtraCondensed) return FontStretch.ExtraCondensed;
			if (value == SW.FontStretches.Condensed) return FontStretch.Condensed;
			if (value == SW.FontStretches.SemiCondensed) return FontStretch.SemiCondensed;
			if (value == SW.FontStretches.SemiExpanded) return FontStretch.SemiExpanded;
			if (value == SW.FontStretches.Expanded) return FontStretch.Expanded;
			if (value == SW.FontStretches.ExtraExpanded) return FontStretch.ExtraExpanded;
			if (value == SW.FontStretches.UltraExpanded) return FontStretch.UltraExpanded;

			return FontStretch.Normal;
		}

		public static SW.FontStretch ToWpfFontStretch (this FontStretch value)
		{
			if (value == FontStretch.UltraCondensed) return SW.FontStretches.UltraCondensed;
			if (value == FontStretch.ExtraCondensed) return SW.FontStretches.ExtraCondensed;
			if (value == FontStretch.Condensed) return SW.FontStretches.Condensed;
			if (value == FontStretch.SemiCondensed) return SW.FontStretches.SemiCondensed;
			if (value == FontStretch.SemiExpanded) return SW.FontStretches.SemiExpanded;
			if (value == FontStretch.Expanded) return SW.FontStretches.Expanded;
			if (value == FontStretch.ExtraExpanded) return SW.FontStretches.ExtraExpanded;
			if (value == FontStretch.UltraExpanded) return SW.FontStretches.UltraExpanded;

			return SW.FontStretches.Normal;
		}

		public static FontWeight ToXwtFontWeight (this SW.FontWeight value)
		{
			// No, SW.FontWeights is not an enum
			if (value == SW.FontWeights.UltraLight) return FontWeight.Ultralight;
			if (value == SW.FontWeights.Light) return FontWeight.Light;
			if (value == SW.FontWeights.SemiBold) return FontWeight.Semibold;
			if (value == SW.FontWeights.Bold) return FontWeight.Bold;
			if (value == SW.FontWeights.UltraBold) return FontWeight.Ultrabold;
			if (value == SW.FontWeights.Black) return FontWeight.Heavy;

			return FontWeight.Normal;
		}

		public static SW.FontWeight ToWpfFontWeight (this FontWeight value)
		{
			if (value == FontWeight.Ultralight) return SW.FontWeights.UltraLight;
			if (value == FontWeight.Light) return SW.FontWeights.Light;
			if (value == FontWeight.Semibold) return SW.FontWeights.SemiBold;
			if (value == FontWeight.Bold) return SW.FontWeights.Bold;
			if (value == FontWeight.Ultrabold) return SW.FontWeights.UltraBold;
			if (value == FontWeight.Heavy) return SW.FontWeights.Black;
			
			return SW.FontWeights.Normal;
		}

		// Dock

		public static SW.Controls.Dock ToWpfDock (this ContentPosition value)
		{
			if (value == ContentPosition.Left) return SW.Controls.Dock.Left;
			if (value == ContentPosition.Top) return SW.Controls.Dock.Top;
			if (value == ContentPosition.Bottom) return SW.Controls.Dock.Bottom;

			return SW.Controls.Dock.Right;
		}

		// 
		// Mouse/Pointer Button
		//

		public static PointerButton ToXwtButton (this MouseButton value)
		{
			switch (value) {
				case MouseButton.Left: return PointerButton.Left;
				case MouseButton.Middle: return PointerButton.Middle;
				default: return PointerButton.Right;
			}
		}

		public static MouseButton ToWpfButton (this PointerButton value)
		{
			switch (value) {
				case PointerButton.Left: return MouseButton.Left;
				case PointerButton.Middle: return MouseButton.Middle;
				default: return MouseButton.Right;
			}
		}
	}
}
