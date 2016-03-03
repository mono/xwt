// 
// IFontBackendHandler.cs
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
using Xwt.Drawing;
using System.Collections.Generic;

namespace Xwt.Backends
{
	public abstract class FontBackendHandler: BackendHandler
	{
		Font systemFont;
		Font systemMonospaceFont;
		Font systemSerifFont;
		Font systemSansSerifFont;

		protected static string GetDefaultMonospaceFontNames (DesktopType forDesktop)
		{
			switch(Desktop.DesktopType) {
				case DesktopType.Linux:
					return "FreeMono, Nimbus Mono L, Courier New, Courier, monospace";

				case DesktopType.Mac:
					return "Menlo, Monaco, Courier New, Courier, monospace";

				default:
					return "Lucida Console, Courier New, Courier, monospace";
			}
		}

		protected static string GetDefaultSerifFontNames (DesktopType forDesktop)
		{
			switch(forDesktop) {
				case DesktopType.Linux:
				return "FreeSerif, Bitstream Vera Serif, DejaVu Serif, Likhan, Norasi, Rekha, Times New Roman, Times, serif";

				case DesktopType.Mac:
				return "Georgia, Palatino, Times New Roman, Times, serif";

				default:
				return "Times New Roman, Times, serif";
			}
		}

		protected static string GetDefaultSansSerifFontNames (DesktopType forDesktop)
		{
			switch(forDesktop) {
				case DesktopType.Linux:
				return "FreeSans, Nimbus Sans L, Garuda, Utkal, Arial, Helvetica, sans-serif";

				case DesktopType.Mac:
				return "SF UI Text, Helvetica Neue, Helvetica, Lucida Grande, Lucida Sans Unicode, Arial, sans-serif";

				default:
				return "Segoe UI, Tahoma, Arial, Helvetica, Lucida Sans Unicode, Lucida Grande, sans-serif";
			}
		}

		internal Font SystemFont {
			get {
				if (systemFont == null)
					systemFont = new Font (GetSystemDefaultFont (), ApplicationContext.Toolkit);
				return systemFont;
			}
		}

		internal Font SystemMonospaceFont {
			get {
				if (systemMonospaceFont == null) {
					var f = GetSystemDefaultMonospaceFont ();
					if (f != null)
						systemMonospaceFont = new Font (f, ApplicationContext.Toolkit);
					else
						systemMonospaceFont = SystemFont.WithFamily (GetDefaultMonospaceFontNames(Desktop.DesktopType));
				}
				return systemMonospaceFont;
			}
		}

		internal Font SystemSerifFont {
			get {
				if (systemSerifFont == null) {
					var f = GetSystemDefaultSerifFont ();
					if (f != null)
						systemSerifFont = new Font (f, ApplicationContext.Toolkit);
					else
						systemSerifFont = SystemFont.WithFamily (GetDefaultSerifFontNames(Desktop.DesktopType));
				}
				return systemSerifFont;
			}
		}

		internal Font SystemSansSerifFont {
			get {
				if (systemSansSerifFont == null) {
					var f = GetSystemDefaultSansSerifFont ();
					if (f != null)
						systemSansSerifFont = new Font (f, ApplicationContext.Toolkit);
					else
						systemSansSerifFont = SystemFont.WithFamily (GetDefaultSansSerifFontNames(Desktop.DesktopType));
				}
				return systemSansSerifFont;
			}
		}

		public abstract object GetSystemDefaultFont ();

		/// <summary>
		/// Gets the system default serif font, or null if there is no default for such font
		/// </summary>
		public virtual object GetSystemDefaultSerifFont ()
		{
			return null;
		}

		/// <summary>
		/// Gets the system default sans-serif font, or null if there is no default for such font
		/// </summary>
		public virtual object GetSystemDefaultSansSerifFont ()
		{
			return null;
		}

		/// <summary>
		/// Gets the system default monospace font, or null if there is no default for such font
		/// </summary>
		public virtual object GetSystemDefaultMonospaceFont ()
		{
			return null;
		}

		public abstract IEnumerable<string> GetInstalledFonts ();

		public abstract IEnumerable<KeyValuePair<string, object>> GetAvailableFamilyFaces (string family);

		/// <summary>
		/// Creates a new font. Returns null if the font family is not available in the system
		/// </summary>
		/// <param name="fontName">Font family name</param>
		/// <param name="size">Size in points</param>
		/// <param name="style">Style</param>
		/// <param name="weight">Weight</param>
		/// <param name="stretch">Stretch</param>
		public abstract object Create (string fontName, double size, FontStyle style, FontWeight weight, FontStretch stretch);

		/// <summary>
		/// Register a font file with the system font manager that is then accessible through Create. The font is only
		/// available during the lifetime of the process.
		/// </summary>
		/// <returns><c>true</c>, if font from file was registered, <c>false</c> otherwise.</returns>
		/// <param name="fontPath">Font path.</param>
		public abstract bool RegisterFontFromFile (string fontPath);

		internal object WithSettings (object handle, double size, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			var backend = SetSize (Copy (handle), size);
			backend = SetStyle (backend, style);
			backend = SetWeight (backend, weight);
			backend = SetStretch (backend, stretch);
			return backend;
		}

		public abstract object Copy (object handle);
		
		public abstract object SetSize (object handle, double size);
		public abstract object SetFamily (object handle, string family);
		public abstract object SetStyle (object handle, FontStyle style);
		public abstract object SetWeight (object handle, FontWeight weight);
		public abstract object SetStretch (object handle, FontStretch stretch);
		
		public abstract double GetSize (object handle);
		public abstract string GetFamily (object handle);
		public abstract FontStyle GetStyle (object handle);
		public abstract FontWeight GetWeight (object handle);
		public abstract FontStretch GetStretch (object handle);
		
	}
}

