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
		public override object Create (string fontName, double size, FontSizeUnit sizeUnit, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			object o  = NSFont.FromFontName (fontName, (float)size);
			return o;
		}

		#region IFontBackendHandler implementation
		public override object Copy (object handle)
		{
			NSFont f = (NSFont) handle;
			return NSFont.FromDescription (f.FontDescriptor, f.FontDescriptor.Matrix);
		}
		
		public override object SetSize (object handle, double size, FontSizeUnit sizeUnit)
		{
			NSFont f = (NSFont) handle;
			var matrix = f.FontDescriptor.Matrix ?? new NSAffineTransform ();
			return NSFont.FromDescription (f.FontDescriptor.FontDescriptorWithSize ((float)size), matrix);
		}

		public override object SetFamily (object handle, string family)
		{
			NSFont f = (NSFont) handle;
			return NSFont.FromDescription (f.FontDescriptor.FontDescriptorWithFamily (family), f.FontDescriptor.Matrix);
		}

		public override object SetStyle (object handle, FontStyle style)
		{
			NSFont f = (NSFont) handle;
			NSFontSymbolicTraits traits = f.FontDescriptor.SymbolicTraits;
			if (style == FontStyle.Italic || style == FontStyle.Oblique)
				traits |= NSFontSymbolicTraits.ItalicTrait;
			else
				traits &= ~NSFontSymbolicTraits.ItalicTrait;
			
			return NSFont.FromDescription (f.FontDescriptor.FontDescriptorWithSymbolicTraits (traits), f.FontDescriptor.Matrix);
		}

		public override object SetWeight (object handle, FontWeight weight)
		{
			NSFont f = (NSFont) handle;
			NSFontSymbolicTraits traits = f.FontDescriptor.SymbolicTraits;
			if (weight > FontWeight.Normal)
				traits |= NSFontSymbolicTraits.BoldTrait;
			else
				traits &= ~NSFontSymbolicTraits.BoldTrait;
			
			return NSFont.FromDescription (f.FontDescriptor.FontDescriptorWithSymbolicTraits (traits), f.FontDescriptor.Matrix);
		}

		public override object SetStretch (object handle, FontStretch stretch)
		{
			NSFont f = (NSFont) handle;
			NSFontSymbolicTraits traits = f.FontDescriptor.SymbolicTraits;
			if (stretch < FontStretch.Normal) {
				traits |= NSFontSymbolicTraits.CondensedTrait;
				traits &= ~NSFontSymbolicTraits.ExpandedTrait;
			}
			else if (stretch > FontStretch.Normal) {
				traits |= NSFontSymbolicTraits.ExpandedTrait;
				traits &= ~NSFontSymbolicTraits.CondensedTrait;
			}
			else {
				traits &= ~NSFontSymbolicTraits.ExpandedTrait;
				traits &= ~NSFontSymbolicTraits.CondensedTrait;
			}
			
			return NSFont.FromDescription (f.FontDescriptor.FontDescriptorWithSymbolicTraits (traits), f.FontDescriptor.Matrix);
		}
		
		public override double GetSize (object handle)
		{
			NSFont f = (NSFont) handle;
			return f.FontDescriptor.PointSize;
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
			if ((f.FontDescriptor.SymbolicTraits & NSFontSymbolicTraits.CondensedTrait) != 0)
				return FontStretch.Condensed;
			else if ((f.FontDescriptor.SymbolicTraits & NSFontSymbolicTraits.ExpandedTrait) != 0)
				return FontStretch.Expanded;
			else
				return FontStretch.Normal;
		}
		#endregion


	}
}

