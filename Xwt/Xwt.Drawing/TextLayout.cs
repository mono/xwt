// 
// TextLayout.cs
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
using Xwt.Engine;
using Xwt.Backends;

namespace Xwt.Drawing
{
	public class TextLayout: XwtObject
	{
		static ITextLayoutBackendHandler handler;
		
		Font font;
		string text;
		double width;
		
		static TextLayout ()
		{
			handler = WidgetRegistry.CreateSharedBackend<ITextLayoutBackendHandler> (typeof(TextLayout));
		}
		
		protected override IBackendHandler BackendHandler {
			get {
				return handler;
			}
		}
		
		public TextLayout (Canvas canvas)
		{
			Backend = handler.Create ((ICanvasBackend)WidgetRegistry.GetBackend (canvas));
			Font = canvas.Font;
		}
		
		public TextLayout (Context ctx)
		{
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
		
		public Size GetSize ()
		{
			return handler.GetSize (Backend);
		}
	}
}

