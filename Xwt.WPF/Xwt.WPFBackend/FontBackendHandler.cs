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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Xwt.Backends;
using Xwt.Drawing;

using FontFamily = System.Windows.Media.FontFamily;
using SW = System.Windows;

namespace Xwt.WPFBackend
{
	public class WpfFontBackendHandler : FontBackendHandler
	{
		static ConcurrentDictionary<string, FontFamily> registeredFonts = new ConcurrentDictionary<string, FontFamily>();

		public override object GetSystemDefaultFont ()
		{
			double size = GetPointsFromDeviceUnits (SW.SystemFonts.MessageFontSize);

			return new FontData (SW.SystemFonts.MessageFontFamily, size) {
				Style = SW.SystemFonts.MessageFontStyle,
				Weight = SW.SystemFonts.MessageFontWeight
			};
		}

		public override IEnumerable<string> GetInstalledFonts ()
		{
			foreach (var fontName in Fonts.SystemFontFamilies.Select(f => f.Source)) {
				yield return fontName;
			}

			foreach (var fontName in registeredFonts.Keys) {
				yield return fontName;
			}
		}

		public override IEnumerable<KeyValuePair<string, object>> GetAvailableFamilyFaces (string family)
		{
			FontFamily wpfFamily;
			if (!registeredFonts.TryGetValue (family, out wpfFamily)) // check for custom fonts
				wpfFamily = new FontFamily (family);

			foreach (var face in wpfFamily.GetTypefaces ()) {
				var langCurrent = SW.Markup.XmlLanguage.GetLanguage (CultureInfo.CurrentCulture.IetfLanguageTag);
				var langInvariant = SW.Markup.XmlLanguage.GetLanguage ("en-us");;
				string name;
				if (face.FaceNames.TryGetValue (langCurrent, out name) || face.FaceNames.TryGetValue (langInvariant, out name)) {
					var fontData = new FontData (wpfFamily, 0) {
						Style = face.Style,
						Weight = face.Weight,
						Stretch = face.Stretch
					};
					yield return new KeyValuePair<string, object> (name, fontData);
				}
			}
			yield break;
		}

		public override object Create (string fontName, double size, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			FontFamily fontFamily;
			if (!registeredFonts.TryGetValue (fontName, out fontFamily)) {
				fontFamily = new FontFamily (fontName);
			}

			size = GetPointsFromDeviceUnits (size);
			return new FontData (fontFamily, size) {
				Style = style.ToWpfFontStyle (),
				Weight = weight.ToWpfFontWeight (),
				Stretch = stretch.ToWpfFontStretch ()
			};
		}

		[System.Runtime.InteropServices.DllImport ("gdi32.dll")]
		static extern int AddFontResourceEx (string lpszFilename, uint fl, System.IntPtr pdv);

		public override bool RegisterFontFromFile (string fontPath)
		{
			string absoluteFontPath = Path.GetFullPath (fontPath);

			AddFontResourceEx (absoluteFontPath, 0x10 /* FR_PRIVATE */, System.IntPtr.Zero);

			// Get font name from font file.
			ICollection<FontFamily> fontInfo = Fonts.GetFontFamilies (absoluteFontPath);

			var fontFamily = fontInfo.SingleOrDefault ();
			if (fontFamily == null) {
				return false;
			}

			if (fontFamily.FamilyNames.Count == 0) {
				return false;
			}

			string fontName = fontFamily.FamilyNames.First ().Value;
			registeredFonts[fontName] = fontFamily;

			return true;
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
			font.Size = GetPointsFromDeviceUnits (size);
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
			return GetDeviceUnitsFromPoints (font.Size);
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
			double size = WpfFontBackendHandler.GetPointsFromDeviceUnits (control.FontSize);

			return new FontData (control.FontFamily, size) {
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
