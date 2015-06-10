// 
// Font.cs
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
using System.Windows.Markup;
using System.ComponentModel;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace Xwt.Drawing
{
	[TypeConverter (typeof(FontValueConverter))]
	[ValueSerializer (typeof(FontValueSerializer))]
	public sealed class Font: XwtObject
	{
		FontBackendHandler handler;

		internal Font (object backend): this (backend, null)
		{
		}
		
		internal Font (object backend, Toolkit toolkit)
		{
			if (toolkit != null)
				ToolkitEngine = toolkit;
			handler = ToolkitEngine.FontBackendHandler;
			if (backend == null)
				throw new ArgumentNullException ("backend");
			Backend = backend;
		}

		internal void InitForToolkit (Toolkit tk)
		{
			if (ToolkitEngine != tk) {
				// Gather existing font property before switching handler
				var fname = Family;
				var size = Size;
				var style = Style;
				var weight = Weight;
				var stretch = Stretch;
				ToolkitEngine = tk;
				handler = tk.FontBackendHandler;
				var fb = handler.Create (fname, size, style, weight, stretch);
				Backend = fb ?? handler.GetSystemDefaultFont ();
			}
		}

		/// <summary>
		/// Register a font file with the system font manager that is then accessible through FromName.
		/// The font is only available during the lifetime of the process.
		/// </summary>
		/// <returns><c>true</c>, if font from file was registered, <c>false</c> otherwise.</returns>
		/// <param name="fontPath">Font path.</param>
		public static bool RegisterFontFromFile (string fontPath)
		{
			return RegisterFontFromFile (fontPath, Toolkit.CurrentEngine);
		}

		static bool RegisterFontFromFile (string fontPath, Toolkit toolkit)
		{
			var handler = toolkit.FontBackendHandler;
			var result = handler.RegisterFontFromFile (fontPath);
			if (result) {
				installedFonts = null;
				installedFontsArray = null;
			}
			return result;
		}

		/// <summary>
		/// Creates a new font description from a string representation in the form "[FAMILY-LIST] [STYLE-OPTIONS] [SIZE]"
		/// </summary>
		/// <returns>
		/// The new font
		/// </returns>
		/// <param name='name'>
		/// Font description
		/// </param>
		/// <remarks>
		/// Creates a new font description from a string representation in the form "[FAMILY-LIST] [STYLE-OPTIONS] [SIZE]", 
		/// where FAMILY-LIST is a comma separated list of families optionally terminated by a comma, STYLE_OPTIONS is a
		/// whitespace separated list of words where each WORD describes one of style, weight or stretch, and SIZE is a
		/// decimal number (size in points). Any one of the options may be absent. If FAMILY-LIST is absent, the default
		/// font family will be used. If STYLE-OPTIONS is missing, then all style options will be set to the default values.
		/// If SIZE is missing, the size in the resulting font description will be set to the default font size.
		/// If the font doesn't exist, it returns the system font.
		/// </remarks>
		public static Font FromName (string name)
		{
			var toolkit = Toolkit.CurrentEngine;
			return FromName (name, toolkit);
		}

		internal static Font FromName (string name, Toolkit toolkit)
		{
			var handler = toolkit.FontBackendHandler;

			double size = -1;
			FontStyle style = FontStyle.Normal;
			FontWeight weight = FontWeight.Normal;
			FontStretch stretch = FontStretch.Normal;

			int i = name.LastIndexOf (' ');
			int lasti = name.Length;
			do {
				string token = name.Substring (i + 1, lasti - i - 1);
				FontStyle st;
				FontWeight fw;
				FontStretch fs;
				double siz;
				if (double.TryParse (token, NumberStyles.Any, CultureInfo.InvariantCulture, out siz)) // Try parsing the number first, since Enum.TryParse can also parse numbers
					size = siz;
				else if (Enum.TryParse<FontStyle> (token, true, out st) && st != FontStyle.Normal)
					style = st;
				else if (Enum.TryParse<FontWeight> (token, true, out fw) && fw != FontWeight.Normal)
					weight = fw;
				else if (Enum.TryParse<FontStretch> (token, true, out fs) && fs != FontStretch.Normal)
					stretch = fs;
				else if (token.Length > 0)
					break;

				lasti = i;
				if (i <= 0)
					break;

				i = name.LastIndexOf (' ', i - 1);
			} while (true);

			string fname = lasti > 0 ? name.Substring (0, lasti) : string.Empty;
			fname = fname.Length > 0 ? GetSupportedFont (fname) : Font.SystemFont.Family;

			if (size == -1)
				size = SystemFont.Size;

			var fb = handler.Create (fname, size, style, weight, stretch);
			if (fb != null)
				return new Font (fb, toolkit);
			else
				return Font.SystemFont;
		}

		static string GetSupportedFont (string fontNames)
		{
			LoadInstalledFonts ();

			int i = fontNames.IndexOf (',');
			if (i == -1) {
				var f = fontNames.Trim ();
				string nf;
				if (installedFonts.TryGetValue (f, out nf))
					return nf;
				else
					return GetDefaultFont (f);
			}

			string[] names = fontNames.Split (new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
			if (names.Length == 0)
				throw new ArgumentException ("Font family name not provided");

			foreach (var name in names) {
				var n = name.Trim ();
				if (installedFonts.ContainsKey (n))
					return n;
			}
			return GetDefaultFont (fontNames.Trim (' ',','));
		}

		static string GetDefaultFont (string unknownFont)
		{
			Console.WriteLine ("Font '" + unknownFont + "' not available in the system. Using '" + Font.SystemFont.Family + "' instead");
			return Font.SystemFont.Family;
		}

		static Dictionary<string,string> installedFonts;
		static ReadOnlyCollection<string> installedFontsArray;


		static void LoadInstalledFonts ()
		{
			if (installedFonts == null) {
				installedFonts = new Dictionary<string,string> (StringComparer.OrdinalIgnoreCase);
				foreach (var f in Toolkit.CurrentEngine.FontBackendHandler.GetInstalledFonts ())
					installedFonts [f] = f;
				installedFontsArray = new ReadOnlyCollection<string> (installedFonts.Values.ToArray ());
			}
		}

		public static ReadOnlyCollection<string> AvailableFontFamilies {
			get {
				LoadInstalledFonts ();
				return installedFontsArray;
			}
		}

		public static Font SystemFont {
			get { return Toolkit.CurrentEngine.FontBackendHandler.SystemFont; }
		}

		public static Font SystemMonospaceFont {
			get {
				return Toolkit.CurrentEngine.FontBackendHandler.SystemMonospaceFont;
			}
		}

		public static Font SystemSerifFont {
			get {
				return Toolkit.CurrentEngine.FontBackendHandler.SystemSerifFont;
			}
		}

		public static Font SystemSansSerifFont {
			get {
				return Toolkit.CurrentEngine.FontBackendHandler.SystemSansSerifFont;
			}
		}

		/// <summary>
		/// Gets the available family/font variants with varying weight, style and stretch.
		/// </summary>
		/// <returns>All available font variants for a specific family/font.</returns>
		/// <param name="fontFamily">A comma separated list of families</param>
		/// <remarks>
		/// Not all weights, styles or strech variants and combinations are available
		/// for every font or family. In case of an invalid combination, most toolkits
		/// fallback to a system default variant. GetAvailableFontFaces helps to retrieve
		/// only valid combinations for a specific font family.
		/// </remarks>
		public static ReadOnlyCollection<FontFace> GetAvailableFontFaces (string fontFamily)
		{
			fontFamily = GetSupportedFont (fontFamily);
			return new ReadOnlyCollection<FontFace>(Toolkit.CurrentEngine.FontBackendHandler.GetAvailableFamilyFaces(fontFamily).Select (f => new FontFace(f.Key, f.Value)).ToList ());
		}

		/// <summary>
		/// Gets the available variants of the font with varying weight, style and stretch.
		/// </summary>
		/// <returns>All available font variants for a specific family/font.</returns>
		/// <remarks>
		/// Not all weights, styles or strech variants and combinations are available
		/// for every font or family. In case of an invalid combination, most toolkits
		/// fallback to a system default variant. GetAvailableFontFaces helps to retrieve
		/// only valid combinations for a specific font family.
		/// </remarks>
		public ReadOnlyCollection<FontFace> GetAvailableFontFaces ()
		{
			return GetAvailableFontFaces (Family);
		}

		/// <summary>
		/// Returns a copy of the font using the provided font family
		/// </summary>
		/// <returns>The new font</returns>
		/// <param name="fontFamily">A comma separated list of families</param>
		public Font WithFamily (string fontFamily)
		{
			fontFamily = GetSupportedFont (fontFamily);
			return new Font (ToolkitEngine.FontBackendHandler.SetFamily (Backend, fontFamily), ToolkitEngine);
		}
		
		public string Family {
			get {
				return handler.GetFamily (Backend);
			}
		}
		
		/// <summary>
		/// Font size. It can be points or pixels, depending on the value of the SizeUnit property
		/// </summary>
		public double Size {
			get {
				return handler.GetSize (Backend);
			}
		}

		public Font WithSize (double size)
		{
			return new Font (handler.SetSize (Backend, size));
		}
		
		public Font WithScaledSize (double scale)
		{
			return new Font (handler.SetSize (Backend, Size * scale), ToolkitEngine);
		}
		
		public FontStyle Style {
			get {
				return handler.GetStyle (Backend);
			}
		}
		
		public Font WithStyle (FontStyle style)
		{
			return new Font (handler.SetStyle (Backend, style), ToolkitEngine);
		}
		
		public FontWeight Weight {
			get {
				return handler.GetWeight (Backend);
			}
		}
		
		public Font WithWeight (FontWeight weight)
		{
			return new Font (handler.SetWeight (Backend, weight), ToolkitEngine);
		}
		
		public FontStretch Stretch {
			get {
				return handler.GetStretch (Backend);
			}
		}
		
		public Font WithStretch (FontStretch stretch)
		{
			return new Font (handler.SetStretch (Backend, stretch), ToolkitEngine);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder (Family);
			if (Style != FontStyle.Normal)
				sb.Append (' ').Append (Style);
			if (Weight != FontWeight.Normal)
				sb.Append (' ').Append (Weight);
			if (Stretch != FontStretch.Normal)
				sb.Append (' ').Append (Stretch);
			sb.Append (' ').Append (Size.ToString (CultureInfo.InvariantCulture));
			return sb.ToString ();
		}

		public override bool Equals (object obj)
		{
			var other = obj as Font;
			if (other == null)
				return false;

			return Family == other.Family && Style == other.Style && Weight == other.Weight && Stretch == other.Stretch && Size == other.Size;
		}

		public override int GetHashCode ()
		{
			return ToString().GetHashCode ();
		}
	}

	/// <summary>
	/// The FontFace class describes a variant of a specific font family with a name and its Xwt representation.
	/// </summary>
	public class FontFace
	{
		/// <summary>
		/// The specific font variant/face name, unique for the font family on the local system.
		/// </summary>
		/// <value>The variant/face name.</value>
		/// <remarks>
		/// On most systems the name is a combination of the written weight, style and stretch
		/// of the font variant without the default values.
		/// The name is only valid for the specific font, backend and system. On some systems
		/// the names can be (partially) localized.
		/// Examples: "Regular", "Bold", "Bold Italic", "Condensed"</remarks>
		public string Name { get; private set; }

		/// <summary>
		/// The font with the family face/variant specific settings (weight, style, stretch).
		/// </summary>
		/// <value>The <see cref="Xwt.Drawing.Font"/> representation of the font variant/face .</value>
		public Font Font { get; private set; }

		internal FontFace (string name, Font font)
		{
			Name = name;
			Font = font;
		}

		internal FontFace (string name, object backend)
		{
			Name = name;
			Font = new Font (backend);
		}
	}

	public enum FontStyle
	{
		Normal,
		Oblique,
		Italic
	}
	
	public enum FontWeight
	{
		/// The thin weight (100)
		Thin = 100,
		/// The ultra light weight (200)
		Ultralight = 200,
		/// The light weight (300)
		Light = 300,
		/// The semi light weight (350)
		Semilight = 350,
		/// The book weight (380)
		Book = 350,
		/// The default weight (400)
		Normal = 400,
		/// The medium weight (500)
		Medium = 500,
		/// The semi bold weight (600)
		Semibold = 600,
		/// The bold weight (700)
		Bold = 700,
		/// The ultrabold weight (800)
		Ultrabold = 800,
		/// The heavy weight (900)
		Heavy = 900,
		/// The ultra heavy weight (1000)
		Ultraheavy = 1000
	}
	
	public enum FontStretch
	{
		/// <summary>
		/// 4x more condensed than Pango.Stretch.Normal
		/// </summary>
		UltraCondensed,
		/// <summary>
		/// 3x more condensed than Pango.Stretch.Normal
		/// </summary>
		ExtraCondensed,
		/// <summary>
		/// 2x more condensed than Pango.Stretch.Normal
		/// </summary>
		Condensed,
		/// <summary>
		/// 1x more condensed than Pango.Stretch.Normal
		/// </summary>
		SemiCondensed,
		/// <summary>
		/// The normal width
		/// </summary>
		Normal,
		/// <summary>
		/// 1x more expanded than Pango.Stretch.Normal
		/// </summary>
		SemiExpanded,
		/// <summary>
		/// 2x more expanded than Pango.Stretch.Normal
		/// </summary>
		Expanded,
		/// <summary>
		/// 3x more expanded than Pango.Stretch.Normal
		/// </summary>
		ExtraExpanded,
		/// <summary>
		/// 4x more expanded than Pango.Stretch.Normal
		/// </summary>
		UltraExpanded
	}

	
	class FontValueConverter: TypeConverter
	{
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}
		
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}
	}
	
	class FontValueSerializer: ValueSerializer
	{
		public override bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			return true;
		}
		
		public override bool CanConvertToString (object value, IValueSerializerContext context)
		{
			return true;
		}
		
		public override string ConvertToString (object value, IValueSerializerContext context)
		{
			return value.ToString ();
		}
		
		public override object ConvertFromString (string value, IValueSerializerContext context)
		{
			return Font.FromName (value);
		}
	}
}

