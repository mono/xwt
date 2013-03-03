// 
// TextLayout.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
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
using System.Collections.Generic;

namespace Xwt.Drawing
{
	public sealed class TextLayout: XwtObject, IDisposable
	{
		TextLayoutBackendHandler handler;
		
		Font font;
		string text;
		double width = -1;
		double height = -1;
		TextTrimming textTrimming;
		
		public TextLayout (Canvas canvas)
		{
			ToolkitEngine = canvas.Surface.ToolkitEngine;
			handler = ToolkitEngine.TextLayoutBackendHandler;
			Backend = handler.Create ((ICanvasBackend)Toolkit.GetBackend (canvas));
			Font = canvas.Font;
		}
		
		public TextLayout (Context ctx)
		{
			ToolkitEngine = ctx.ToolkitEngine;
			handler = ToolkitEngine.TextLayoutBackendHandler;
			Backend = handler.Create (ctx);
		}
		
		public Font Font {
			get { return font; }
			set { font = value; handler.SetFont (Backend, value); }
		}
		
		public string Text {
			get { return text; }
			set { text = value;
				handler.SetText (Backend, text); }
		}
		
		/// <summary>
		/// Gets or sets the desired width.
		/// </summary>
		/// <value>
		/// The width. A value of -1 uses GetSize().Width on drawings
		/// </value>
		public double Width {
			get { return width; }
			set { width = value; handler.SetWidth (Backend, value); }
		}
		
		/// <summary>
		/// Gets or sets desired Height.
		/// </summary>
		/// <value>
		/// The Height. A value of -1 uses GetSize().Height on drawings
		/// </value>
		public double Height {
			get { return this.height; }
			set { this.height = value; handler.SetHeight (Backend, value); }
		}
		
		/// <summary>
		/// measures the text
		/// if Width is other than -1, it measures the height according to Width
		/// Height is ignored
		/// </summary>
		/// <returns>
		/// The size.
		/// </returns>
		public Size GetSize ()
		{
			return handler.GetSize (Backend);
		}
		
		public TextTrimming Trimming {
			get { return textTrimming; }
			set { textTrimming = value; handler.SetTrimming (Backend, value); }
		}

		/// <summary>
		/// Converts from a X and Y position within the layout to the character at this position.
		/// </summary>
		/// <returns>The index of the character.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public int GetIndexFromCoordinates (double x, double y)
		{
			return handler.GetIndexFromCoordinates (Backend, x, y);
		}

		/// <summary>
		/// Converts from a Position within the layout to the character at this position.
		/// </summary>
		/// <returns>The index of the character.</returns>
		/// <param name="p">The position.</param>
		public int GetIndexFromCoordinates (Point p)
		{
			return handler.GetIndexFromCoordinates (Backend, p.X, p.Y);
		}

		/// <summary>
		/// Obtains the graphical coordinate of an character in the layout.
		/// </summary>
		/// <returns>The extends from the character at index.</returns>
		/// <param name="index">The index of the character.</param>
		public Point GetCoordinateFromIndex (int index)
		{
			return handler.GetCoordinateFromIndex (Backend, index);
		}

		/// <summary>
		/// Sets the foreground color of a part of text inside the <see cref="T:Xwt.Drawing.TextLayout"/> object.
		/// </summary>
		/// <param name="color">The color of the text.</param>
		/// <param name="startIndex">Start index of the first character to apply the foreground color to.</param>
		/// <param name="count">The number of characters to apply the foreground color to.</param>
		public void SetForeground (Color color, int startIndex, int count)
		{
			handler.SetForeground (Backend, color, startIndex, count);
		}
		
		/// <summary>
		/// Sets the background color of a part of text inside the <see cref="T:Xwt.Drawing.TextLayout"/> object.
		/// </summary>
		/// <param name="color">The color of the text background.</param>
		/// <param name="startIndex">Start index of the first character to apply the background color to.</param>
		/// <param name="count">The number of characters to apply the background color to.</param>
		public void SetBackgound (Color color, int startIndex, int count)
		{
			handler.SetBackgound (Backend, color, startIndex, count);
		}
		
		/// <summary>
		/// Sets the font weight of a part of text inside the <see cref="T:Xwt.Drawing.TextLayout"/> object.
		/// </summary>
		/// <param name="weight">The font weight of the text.</param>
		/// <param name="startIndex">Start index of the first character to apply the font weight to.</param>
		/// <param name="count">The number of characters to apply the font weight to.</param>
		public void SetFontWeight (FontWeight weight, int startIndex, int count)
		{
			handler.SetFontWeight (Backend, weight, startIndex, count);
		}

		/// <summary>
		/// Sets the font style of a part of text inside the <see cref="T:Xwt.Drawing.TextLayout"/> object.
		/// </summary>
		/// <param name="style">The font style of the text.</param>
		/// <param name="startIndex">Start index of the first character to apply the font style to.</param>
		/// <param name="count">The number of characters to apply the font style to.</param>
		public void SetFontStyle (FontStyle style, int startIndex, int count)
		{
			handler.SetFontStyle (Backend, style, startIndex, count);
		}

		/// <summary>
		/// Underlines a part of text inside the <see cref="T:Xwt.Drawing.TextLayout"/> object.
		/// </summary>
		/// <param name="startIndex">Start index of the first character to underline.</param>
		/// <param name="count">The number of characters to underline.</param>
		public void SetUnderline (int startIndex, int count)
		{
			handler.SetUnderline (Backend, startIndex, count);
		}

		/// <summary>
		/// Adds a strike-through to a part of text inside the <see cref="T:Xwt.Drawing.TextLayout"/> object.
		/// </summary>
		/// <param name="startIndex">Start index of the first character to strike-through.</param>
		/// <param name="count">The number of characters to strike-through.</param>
		public void SetStrikethrough (int startIndex, int count)
		{
			handler.SetStrikethrough (Backend, startIndex, count);
		}

		public void Dispose ()
		{
			handler.DisposeBackend (Backend);
		}
	}
	
	public enum TextTrimming {
		Word,
		WordElipsis
	}
}

