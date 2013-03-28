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
			return NSFont.SystemFontOfSize (0);
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
			object o = NSFont.FromFontName (fontName, (float)size);
			o = SetStyle (o, style);
			o = SetWeight (o, weight);
			o = SetStretch (o, stretch);
			return o;
		}

		#region IFontBackendHandler implementation
		public override object Copy (object handle)
		{
			NSFont f = (NSFont) handle;
			return f.Copy ();
		}
		
		public override object SetSize (object handle, double size)
		{
			NSFont f = (NSFont) handle;
			return NSFontManager.SharedFontManager.ConvertFont (f, (float)size);
		}

		public override object SetFamily (object handle, string family)
		{
			NSFont f = (NSFont) handle;
			return NSFontManager.SharedFontManager.ConvertFontToFamily (f, family);
		}

		public override object SetStyle (object handle, FontStyle style)
		{
			NSFont f = (NSFont) handle;
			NSFontTraitMask mask;
			if (style == FontStyle.Italic || style == FontStyle.Oblique)
				mask = NSFontTraitMask.Italic;
			else
				mask = NSFontTraitMask.Unitalic;
			return NSFontManager.SharedFontManager.ConvertFont (f, mask);
		}

		public override object SetWeight (object handle, FontWeight weight)
		{
			NSFont f = (NSFont) handle;
			NSFontTraitMask mask;
			if (weight > FontWeight.Normal)
				mask = NSFontTraitMask.Bold;
			else
				mask = NSFontTraitMask.Unbold;
			return NSFontManager.SharedFontManager.ConvertFont (f, mask);
		}

		public override object SetStretch (object handle, FontStretch stretch)
		{
			NSFont f = (NSFont) handle;
			if (stretch < FontStretch.Normal) {
				f = NSFontManager.SharedFontManager.ConvertFont (f, NSFontTraitMask.Condensed);
				f = NSFontManager.SharedFontManager.ConvertFontToNotHaveTrait (f, NSFontTraitMask.Expanded);
			}
			else if (stretch > FontStretch.Normal) {
				f = NSFontManager.SharedFontManager.ConvertFont (f, NSFontTraitMask.Expanded);
				f = NSFontManager.SharedFontManager.ConvertFontToNotHaveTrait (f, NSFontTraitMask.Condensed);
			}
			else {
				f = NSFontManager.SharedFontManager.ConvertFontToNotHaveTrait (f, NSFontTraitMask.Condensed | NSFontTraitMask.Expanded);
			}
			return f;
		}
		
		public override double GetSize (object handle)
		{
			NSFont f = (NSFont) handle;
			return f.PointSize;
		}

		public override string GetFamily (object handle)
		{
			NSFont f = (NSFont) handle;
			return f.FamilyName;
		}

		public override FontStyle GetStyle (object handle)
		{
			NSFont f = (NSFont) handle;
			if ((f.FontDescriptor.SymbolicTraits & NSFontSymbolicTraits.ItalicTrait) != 0)
				return FontStyle.Italic;
			else
				return FontStyle.Normal;
		}

		public override FontWeight GetWeight (object handle)
		{
			NSFont f = (NSFont) handle;
			if ((f.FontDescriptor.SymbolicTraits & NSFontSymbolicTraits.BoldTrait) != 0)
				return FontWeight.Bold;
			else
				return FontWeight.Normal;
		}

		public override FontStretch GetStretch (object handle)
		{
			NSFont f = (NSFont) handle;
			var traits = NSFontManager.SharedFontManager.TraitsOfFont (f);
			if ((traits & NSFontTraitMask.Condensed) != 0)
				return FontStretch.Condensed;
			else if ((traits & NSFontTraitMask.Expanded) != 0)
				return FontStretch.Expanded;
			else
				return FontStretch.Normal;
		}
		#endregion


	}
}

