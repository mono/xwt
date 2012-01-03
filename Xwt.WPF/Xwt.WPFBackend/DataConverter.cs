using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SW = System.Windows;

using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.WPFBackend
{
	internal static class DataConverter
	{
		//
		// Rect/Point
		//
		public static Rectangle ToXwtRect (SW.Rect rect)
		{
			return new Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static SW.Rect ToWpfRect (Rectangle rect)
		{
			return new SW.Rect (rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static Point ToXwtPoint (SW.Point point)
		{
			return new Point (point.X, point.Y);
		}

		public static SW.Point ToWpfPoint (Point point)
		{
			return new SW.Point (point.X, point.Y);
		}

		//
		// Color
		//
		public static Color ToXwtColor (SW.Media.Color color)
		{
			return Color.FromBytes (color.R, color.G, color.B, color.A);
		}

		public static SW.Media.Color ToWpfColor (Color color)
		{
			return SW.Media.Color.FromArgb (
				(byte)(color.Alpha * 255.0),
				(byte)(color.Red * 255.0),
				(byte)(color.Green * 255.0),
				(byte)(color.Blue * 255.0));
		}

		//
		// Font
		//
		public static FontStyle ToXwtFontStyle (SW.FontStyle value)
		{
			// No, SW.FontStyles is not an enum
			if (value == SW.FontStyles.Italic) return FontStyle.Italic;
			if (value == SW.FontStyles.Oblique) return FontStyle.Oblique;

			return FontStyle.Normal;
		}

		public static SW.FontStyle ToWpfFontStyle (FontStyle value)
		{
			if (value == FontStyle.Italic) return SW.FontStyles.Italic;
			if (value == FontStyle.Oblique) return SW.FontStyles.Oblique;
			
			return SW.FontStyles.Normal;
		}

		public static FontStretch ToXwtFontStretch (SW.FontStretch value)
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

		public static SW.FontStretch ToWpfFontStretch (FontStretch value)
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

		public static FontWeight ToXwtFontWeight (SW.FontWeight value)
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

		public static SW.FontWeight ToWpfFontWeight (FontWeight value)
		{
			if (value == FontWeight.Ultralight) return SW.FontWeights.UltraLight;
			if (value == FontWeight.Light) return SW.FontWeights.Light;
			if (value == FontWeight.Semibold) return SW.FontWeights.SemiBold;
			if (value == FontWeight.Bold) return SW.FontWeights.Bold;
			if (value == FontWeight.Ultrabold) return SW.FontWeights.UltraBold;
			if (value == FontWeight.Heavy) return SW.FontWeights.Black;
			
			return SW.FontWeights.Normal;
		}
	}
}
