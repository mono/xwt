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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AppKit;
using CoreText;
using Foundation;
using Xwt.Backends;
using Xwt.Drawing;

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
			return Create (GetDefaultMonospaceFontNames(Desktop.DesktopType), font.PointSize, FontStyle.Normal, FontWeight.Normal, FontStretch.Normal);
		}

		public override IEnumerable<string> GetInstalledFonts ()
		{
			return NSFontManager.SharedFontManager.AvailableFontFamilies;
		}

		public override IEnumerable<KeyValuePair<string, object>> GetAvailableFamilyFaces (string family)
		{
			foreach (var nsFace in NSFontManager.SharedFontManager.AvailableMembersOfFontFamily(family)) {
				var name = NSString.FromHandle(nsFace.ValueAt(1));
				var weight = ((NSNumber) NSValue.ValueFromPointer (nsFace.ValueAt (2)).NonretainedObjectValue).Int32Value;
				var traits = (NSFontTraitMask) ((NSNumber)NSValue.ValueFromPointer (nsFace.ValueAt (3)).NonretainedObjectValue).Int32Value;
				yield return new KeyValuePair<string, object>(name, FontData.FromFamily(family, traits, weight, 0));
			}
			yield break;
		}

		public override object Create (string fontName, double size, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			var t = GetStretchTrait (stretch) | GetStyleTrait (style);

			var names = fontName.Split (new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
			NSFont f = null;
			foreach (var name in names) {
				f = NSFontManager.SharedFontManager.FontWithFamily (name.Trim (), t, weight.ToMacValue (), (float)size);
				if (f != null) break;
			}
			if (f == null) return null;

			var fd = FontData.FromFont (NSFontManager.SharedFontManager.ConvertFont (f, t));
			fd.Style = style;
			fd.Weight = weight;
			fd.Stretch = stretch;
			return fd;
		}

		public override bool RegisterFontFromFile (string fontPath)
		{
			return CTFontManager.RegisterFontsForUrl (NSUrl.FromFilename (fontPath), CTFontManagerScope.Process) == null;
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

			var names = family.Split (new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
			NSFont font = null;
			foreach (var name in names) {
				font = NSFontManager.SharedFontManager.ConvertFontToFamily (f.Font, name.Trim ());
				if (font != null) {
					f.Font = font;
					break;
				}
			}

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
			case FontWeight.Thin:
				return 1;
			case FontWeight.Ultralight:
				return 2;
			case FontWeight.Light:
				return 3;
			case FontWeight.Book:
				return 4;
			case FontWeight.Normal:
				return 5;
			case FontWeight.Medium:
				return 6;
			case FontWeight.Semibold:
				return 8;
			case FontWeight.Bold:
				return 9;
			case FontWeight.Ultrabold:
				return 10;
			case FontWeight.Heavy:
				return 11;
			case FontWeight.Ultraheavy:
				return 12;
			default:
				return 13;
			}
		}

		internal static FontWeight GetWeightFromValue (nint w)
		{
			if (w <= 1)
				return FontWeight.Thin;
			if (w == 2)
				return FontWeight.Ultralight;
			if (w == 3)
				return FontWeight.Light;
			if (w == 4)
				return FontWeight.Book;
			if (w == 5)
				return FontWeight.Normal;
			if (w == 6)
				return FontWeight.Medium;
			if (w <= 8)
				return FontWeight.Semibold;
			if (w == 9)
				return FontWeight.Bold;
			if (w == 10)
				return FontWeight.Ultrabold;
			if (w == 11)
				return FontWeight.Heavy;
			return FontWeight.Ultraheavy;
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
			f.Font = f.Font.WithWeight (weight);
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

		public static FontData FromFamily (string family, NSFontTraitMask traits, int weight, float size)
		{
			var font = NSFontManager.SharedFontManager.FontWithFamily (family, traits, weight, size);
			var gentraits = NSFontManager.SharedFontManager.TraitsOfFont (font);
			// NSFontManager may loose the traits, restore them here
			if (traits > 0 && gentraits == 0)
				font = NSFontManager.SharedFontManager.ConvertFont (font, traits);
			return FromFont (font);
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

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder (Font.FamilyName);
			if (Style != FontStyle.Normal)
				sb.Append (' ').Append (Style.ToString ());
			if (Weight != FontWeight.Normal)
				sb.Append (' ').Append (Weight.ToString ());
			if (Stretch != FontStretch.Normal)
				sb.Append (' ').Append (Stretch.ToString ());
			sb.Append (' ').Append (Font.PointSize.ToString (CultureInfo.InvariantCulture));
			return sb.ToString ();
		}
	}

}


