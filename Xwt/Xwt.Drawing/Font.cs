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
using Xwt.Engine;

namespace Xwt.Drawing
{
	public sealed class Font: XwtObject
	{
		static IFontBackendHandler handler;
		FontSizeUnit unit;
		
		static Font ()
		{
			handler = WidgetRegistry.CreateSharedBackend<IFontBackendHandler> (typeof(Font));
		}
		
		protected override IBackendHandler BackendHandler {
			get {
				return handler;
			}
		}
		
		internal Font (object backend)
		{
			if (backend == null)
				throw new ArgumentNullException ("backend");
			Backend = backend;
		}

		/// <summary>
		/// Creates a new font description from a string representation in the form "[FAMILY] [SIZE]"
		/// </summary>
		/// <returns>
		/// The new font
		/// </returns>
		/// <param name='name'>
		/// Font description
		/// </param>
		public static Font FromName (string name)
		{
			name = name.Trim ();
			string[] parts = name.Split (new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
				throw new ArgumentException ("Font family name not specified");
			double size = 0;
			FontSizeUnit unit = FontSizeUnit.Points;
			if (parts.Length > 1) {
				var s = parts[parts.Length - 1];
				if (s.EndsWith ("px")) {
					s = s.Substring (0, s.Length - 2);
					unit = FontSizeUnit.Pixels;
				}
				if (!double.TryParse (s, out size))
					throw new ArgumentException ("Invalid font size: " + s);
			}
			return new Font (handler.Create (name, size, unit, FontStyle.Normal, FontWeight.Normal, FontStretch.Normal));
		}
		
		public Font WithFamily (string fontFamily)
		{
			return new Font (handler.SetFamily (Backend, fontFamily));
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

		public FontSizeUnit SizeUnit {
			get { return unit; }
		}


		public Font WithPointSize (double size)
		{
			return new Font (handler.SetSize (Backend, size, FontSizeUnit.Points));
		}
		
		public Font WithPixelSize (double size)
		{
			return new Font (handler.SetSize (Backend, size, FontSizeUnit.Pixels));
		}
		
		public FontStyle Style {
			get {
				return handler.GetStyle (Backend);
			}
		}
		
		public Font WithStyle (FontStyle style)
		{
			return new Font (handler.SetStyle (Backend, style));
		}
		
		public FontWeight Weight {
			get {
				return handler.GetWeight (Backend);
			}
		}
		
		public Font WithWeight (FontWeight weight)
		{
			return new Font (handler.SetWeight (Backend, weight));
		}
		
		public FontStretch Stretch {
			get {
				return handler.GetStretch (Backend);
			}
		}
		
		public Font WithStretch (FontStretch stretch)
		{
			return new Font (handler.SetStretch (Backend, stretch));
		}
	}

	public enum FontSizeUnit
	{
		Pixels,
		Points
	}
	
	public enum FontStyle
	{
		Normal,
		Oblique,
		Italic
	}
	
	public enum FontWeight
	{
		/// The ultralight weight (200)
		Ultralight = 200,
		/// The light weight (300)
		Light = 300,
		/// The default weight (400)
		Normal = 400,
		/// The semi bold weight (600)
		Semibold = 600,
		/// The bold weight (700)
		Bold = 700,
		/// The ultrabold weight (800)
		Ultrabold = 800,
		/// The heavy weight (900)
		Heavy = 900
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
}

