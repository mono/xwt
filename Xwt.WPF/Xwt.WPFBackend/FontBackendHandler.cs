//
// FontBackendHandler.cs
//
// Authors:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2011-2012 Carlos Alberto Cortez
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

using SW = System.Windows;

using Xwt.Backends;
using Xwt.Drawing;

using FontFamily = System.Windows.Media.FontFamily;

namespace Xwt.WPFBackend
{
	public class FontBackendHandler : IFontBackendHandler
	{
		public object Create (string fontName, double size, FontSizeUnit sizeUnit, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			return new FontData (new FontFamily (fontName), size);
		}

		public object Copy (object handle)
		{
			var font = (FontData)handle;
			return font.Clone ();
		}

		public object SetSize (object handle, double size, FontSizeUnit sizeUnit)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Size = size;
			return font;
		}

		public object SetFamily (object handle, string family)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Family = new FontFamily (family);
			return font;
		}

		public object SetStyle (object handle, FontStyle style)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Style = DataConverter.ToWpfFontStyle (style);
			return font;
		}

		public object SetWeight (object handle, FontWeight weight)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Weight = DataConverter.ToWpfFontWeight (weight);
			return font;
		}

		public object SetStretch (object handle, FontStretch stretch)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Stretch = DataConverter.ToWpfFontStretch (stretch);
			return font;
		}

		public double GetSize (object handle)
		{
			var font = (FontData)handle;
			return font.Size;
		}

		public string GetFamily (object handle)
		{
			var font = (FontData)handle;
			return font.Family.Source;
		}

		public FontStyle GetStyle (object handle)
		{
			var font = (FontData)handle;
			return DataConverter.ToXwtFontStyle (font.Style);
		}

		public FontStretch GetStretch (object handle)
		{
			var font = (FontData)handle;
			return DataConverter.ToXwtFontStretch (font.Stretch);
		}

		public FontWeight GetWeight (object handle)
		{
			var font = (FontData)handle;
			return DataConverter.ToXwtFontWeight (font.Weight);
		}

		internal static double GetPointsFromPixels (double pixels, double dpi)
		{
			return (pixels / dpi) * 72;
		}

		internal static double GetPointsFromPixels (SW.Controls.Control control)
		{
			Size pixelRatios = control.GetPixelRatios ();
			double dpi = (pixelRatios.Width * 96); // 96 DPI is WPF's unit

			return GetPointsFromPixels (control.FontSize, dpi);
		}

		internal static double GetPixelsFromPoints (double points, double dpi)
		{
			return points * (dpi / 72);
		}
	}

	internal class FontData
	{
		public FontData (FontFamily family, double size)
		{
			Family = family;
			Size = size;
		}

		public FontData (string family, double size) :
			this (new FontFamily (family), size)
		{
		}

		public FontFamily Family { get; set; }
		public double Size { get; set; }
		public SW.FontWeight Weight { get; set; }
		public SW.FontStyle Style { get; set; }
		public SW.FontStretch Stretch { get; set; }

		public static FontData FromControl (SW.Controls.Control control)
		{
			return new FontData (control.FontFamily, FontBackendHandler.GetPointsFromPixels (control)) {
				Style = control.FontStyle,				
				Stretch = control.FontStretch,
				Weight = control.FontWeight
			};
		}

		// Didn't implement IClone on purpose (recommended by the Framework Design guidelines)
		public FontData Clone ()
		{
			return new FontData (Family, Size) {
				Style = Style,
				Stretch = Stretch,
				Weight = Weight
			};
		}
	}
}
