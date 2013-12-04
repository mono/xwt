// 
// FontBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Xwt.Mac
{
	public class MacFontBackendHandler: FontBackendHandler
	{
		public override object GetSystemDefaultFont ()
		{
			return FontData.FromFont (NSFont.SystemFontOfSize (0));
		}

		public override object GetSystemDefaultMonospaceFont ()
		{
			var font = NSFont.SystemFontOfSize (0);
			return Create ("Menlo", font.PointSize, FontStyle.Normal, FontWeight.Normal, FontStretch.Normal);
		}

		public override System.Collections.Generic.IEnumerable<string> GetInstalledFonts ()
		{
			return NSFontManager.SharedFontManager.AvailableFontFamilies;
		}

		public override object Create (string fontName, double size, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			var t = GetStretchTrait (stretch) | GetStyleTrait (style);
			var f = NSFontManager.SharedFontManager.FontWithFamily (fontName, t, GetWeightValue (weight), (float)size);
			var fd = FontData.FromFont (NSFontManager.SharedFontManager.ConvertFont (f, t));
			fd.Style = style;
			fd.Weight = weight;
			fd.Stretch = stretch;
			return fd;
		}

		#region IFontBackendHandler implementation
		public override object Copy (object handle)
		{
			FontData f = (FontData) handle;
			f = f.Copy ();
			f.Font = (NSFont) f.Font.Copy ();
			return f;
		}
		
		public override object SetSize (object handle, double size)
		{
			FontData f = (FontData) handle;
			f = f.Copy ();
			f.Font = NSFontManager.SharedFontManager.ConvertFont (f.Font, (float)size);
			return f;
		}

		public override object SetFamily (object handle, string family)
		{
			FontData f = (FontData) handle;
			f = f.Copy ();
			f.Font = NSFontManager.SharedFontManager.ConvertFontToFamily (f.Font, family);
			return f;
		}

		public override object SetStyle (object handle, FontStyle style)
		{
			FontData f = (FontData) handle;
			f = f.Copy ();
			NSFontTraitMask mask;
			if (style == FontStyle.Italic || style == FontStyle.Oblique)
				mask = NSFontTraitMask.Italic;
			else
				mask = NSFontTraitMask.Unitalic;
			f.Font = NSFontManager.SharedFontManager.ConvertFont (f.Font, mask);
			f.Style = style;
			return f;
		}

		static int GetWeightValue (FontWeight weight)
		{
			switch (weight) {
			case FontWeight.Ultralight:
				return 2;
			case FontWeight.Light:
				return 4;
			case FontWeight.Normal:
				return 5;
			case FontWeight.Semibold:
				return 7;
			case FontWeight.Bold:
				return 9;
			case FontWeight.Ultrabold:
				return 11;
			default:
				return 13;
			}
		}

		internal static FontWeight GetWeightFromValue (int w)
		{
			if (w <= 2)
				return FontWeight.Ultralight;
			if (w <= 4)
				return FontWeight.Light;
			if (w <= 6)
				return FontWeight.Normal;
			if (w <= 8)
				return FontWeight.Semibold;
			if (w == 9)
				return FontWeight.Bold;
			if (w <= 12)
				return FontWeight.Ultrabold;
			return FontWeight.Heavy;
		}

		NSFontTraitMask GetStretchTrait (FontStretch stretch)
		{
			switch (stretch) {
			case FontStretch.Condensed:
			case FontStretch.ExtraCondensed:
			case FontStretch.SemiCondensed:
				return NSFontTraitMask.Condensed;
			case FontStretch.Normal:
				return default (NSFontTraitMask);
			default:
				return NSFontTraitMask.Expanded;
			}
		}

		NSFontTraitMask GetStyleTrait (FontStyle style)
		{
			switch (style) {
			case FontStyle.Italic:
			case FontStyle.Oblique:
				return NSFontTraitMask.Italic;
			default:
				return default (NSFontTraitMask);
			}
		}

		public override object SetWeight (object handle, FontWeight weight)
		{
			FontData f = (FontData) handle;
			f = f.Copy ();
			int w = GetWeightValue (weight);
			f.Font = NSFontManager.SharedFontManager.FontWithFamily (f.Font.FamilyName, NSFontManager.SharedFontManager.TraitsOfFont (f.Font), w, f.Font.PointSize);
			f.Weight = weight;
			return f;
		}

		public override object SetStretch (object handle, FontStretch stretch)
		{
			FontData f = (FontData) handle;
			f = f.Copy ();

			NSFont font = f.Font;
			if (stretch < FontStretch.SemiCondensed) {
				font = NSFontManager.SharedFontManager.ConvertFont (font, NSFontTraitMask.Condensed);
				font = NSFontManager.SharedFontManager.ConvertFontToNotHaveTrait (font, NSFontTraitMask.Compressed | NSFontTraitMask.Expanded | NSFontTraitMask.Narrow);
			}
			if (stretch == FontStretch.SemiCondensed) {
				font = NSFontManager.SharedFontManager.ConvertFont (font, NSFontTraitMask.Narrow);
				font = NSFontManager.SharedFontManager.ConvertFontToNotHaveTrait (font, NSFontTraitMask.Compressed | NSFontTraitMask.Expanded | NSFontTraitMask.Condensed);
			}
			else if (stretch > FontStretch.Normal) {
				font = NSFontManager.SharedFontManager.ConvertFont (font, NSFontTraitMask.Expanded);
				font = NSFontManager.SharedFontManager.ConvertFontToNotHaveTrait (font, NSFontTraitMask.Compressed | NSFontTraitMask.Narrow | NSFontTraitMask.Condensed);
			}
			else {
				font = NSFontManager.SharedFontManager.ConvertFontToNotHaveTrait (font, NSFontTraitMask.Condensed | NSFontTraitMask.Expanded | NSFontTraitMask.Narrow | NSFontTraitMask.Compressed);
			}
			f.Font = font;
			f.Stretch = stretch;
			return f;
		}
		
		public override double GetSize (object handle)
		{
			FontData f = (FontData) handle;
			return f.Font.PointSize;
		}

		public override string GetFamily (object handle)
		{
			FontData f = (FontData) handle;
			return f.Font.FamilyName;
		}

		public override FontStyle GetStyle (object handle)
		{
			FontData f = (FontData) handle;
			return f.Style;
		}

		public override FontWeight GetWeight (object handle)
		{
			FontData f = (FontData) handle;
			return f.Weight;
		}

		public override FontStretch GetStretch (object handle)
		{
			FontData f = (FontData) handle;
			return f.Stretch;
		}
		#endregion
	}

	public class FontData
	{
		public NSFont Font;
		public FontStyle Style;
		public FontWeight Weight;
		public FontStretch Stretch;

		public FontData ()
		{
		}

		public static FontData FromFont (NSFont font)
		{
			var traits = NSFontManager.SharedFontManager.TraitsOfFont (font);

			FontStretch stretch;
			if ((traits & NSFontTraitMask.Condensed) != 0)
				stretch = FontStretch.Condensed;
			else if ((traits & NSFontTraitMask.Narrow) != 0)
				stretch = FontStretch.SemiCondensed;
			else if ((traits & NSFontTraitMask.Compressed) != 0)
				stretch = FontStretch.ExtraCondensed;
			else if ((traits & NSFontTraitMask.Expanded) != 0)
				stretch = FontStretch.Expanded;
			else
				stretch = FontStretch.Normal;

			FontStyle style;
			if ((traits & NSFontTraitMask.Italic) != 0)
				style = FontStyle.Italic;
			else
				style = FontStyle.Normal;

			return new FontData {
				Font = font,
				Style = style,
				Weight = MacFontBackendHandler.GetWeightFromValue (NSFontManager.SharedFontManager.WeightOfFont (font)),
				Stretch = stretch
			};
		}

		public FontData Copy ()
		{
			return new FontData {
				Font = Font,
				Style = Style,
				Weight = Weight,
				Stretch = Stretch
			};
		}
	}

}


