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

