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
	public class WpfFontBackendHandler : FontBackendHandler
	{
		public override object Create (string fontName, double size, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			return new FontData (new FontFamily (fontName), size) {
				Style = style.ToWpfFontStyle (),
				Weight = weight.ToWpfFontWeight (),
				Stretch = stretch.ToWpfFontStretch ()
			};
		}

		public override object Copy (object handle)
		{
			var font = (FontData)handle;
			return font.Clone ();
		}

		public override object SetSize (object handle, double size)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Size = size;
			return font;
		}

		public override object SetFamily (object handle, string family)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Family = new FontFamily (family);
			return font;
		}

		public override object SetStyle (object handle, FontStyle style)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Style = DataConverter.ToWpfFontStyle (style);
			return font;
		}

		public override object SetWeight (object handle, FontWeight weight)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Weight = DataConverter.ToWpfFontWeight (weight);
			return font;
		}

		public override object SetStretch (object handle, FontStretch stretch)
		{
			var font = (FontData)handle;
			font = font.Clone ();
			font.Stretch = DataConverter.ToWpfFontStretch (stretch);
			return font;
		}

		public override double GetSize (object handle)
		{
			var font = (FontData)handle;
			return font.Size;
		}

		public override string GetFamily (object handle)
		{
			var font = (FontData)handle;
			return font.Family.Source;
		}

		public override FontStyle GetStyle (object handle)
		{
			var font = (FontData)handle;
			return DataConverter.ToXwtFontStyle (font.Style);
		}

		public override FontStretch GetStretch (object handle)
		{
			var font = (FontData)handle;
			return DataConverter.ToXwtFontStretch (font.Stretch);
		}

		public override FontWeight GetWeight (object handle)
		{
			var font = (FontData)handle;
			return DataConverter.ToXwtFontWeight (font.Weight);
		}

		internal static double GetDeviceUnitsFromPoints (double points)
		{
			return points * (96d / 72d);
		}

		internal static double GetPointsFromDeviceUnits (double deviceUnits)
		{
			return deviceUnits * (72d / 96d);
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

		public double GetDeviceIndependentPixelSize (SW.Controls.Control control)
		{
			return WpfFontBackendHandler.GetDeviceUnitsFromPoints (Size);
		}

		public double GetDeviceIndependentPixelSize ()
		{
			return WpfFontBackendHandler.GetDeviceUnitsFromPoints (Size);
		}

		public static FontData FromControl (SW.Controls.Control control)
		{
			var pixelSize = control.FontSize;

			return new FontData (control.FontFamily, pixelSize) {
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
