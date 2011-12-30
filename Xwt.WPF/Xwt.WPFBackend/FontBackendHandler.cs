using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SW = System.Windows;

using Xwt.Backends;
using Xwt.Drawing;

using FontFamily = System.Windows.Media.FontFamily;

namespace Xwt.WPFBackend
{
	public class FontBackendHandler : IFontBackendHandler
	{
		public object CreateFromName (string fontName, double size)
		{
			return new FontData (new FontFamily (fontName), size);
		}

		public object Copy (object handle)
		{
			var font = (FontData)handle;
			return font.Clone ();
		}

		public object SetSize (object handle, double size)
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
			return new FontData (control.FontFamily, control.FontSize) {
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
