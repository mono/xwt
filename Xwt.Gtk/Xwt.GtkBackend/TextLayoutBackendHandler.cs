// 
// TextLayoutBackendHandler.cs
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
using Xwt.Drawing;
using Xwt.Engine;

namespace Xwt.GtkBackend
{
	public class TextLayoutBackendHandler: ITextLayoutBackendHandler
	{
		public object Create (Context context)
		{
			GtkContext c = (GtkContext) WidgetRegistry.GetBackend (context);
			return new Pango.Layout (c.Widget.PangoContext);
		}

		public void SetText (object backend, string text)
		{
			Pango.Layout tl = (Pango.Layout) backend;
			tl.SetText (text);
		}

		public void SetFont (object backend, Xwt.Drawing.Font font)
		{
			Pango.Layout tl = (Pango.Layout)backend;
			tl.FontDescription = (Pango.FontDescription)WidgetRegistry.GetBackend (font);
		}
		
		public void SetWidth (object backend, double value)
		{
			Pango.Layout tl = (Pango.Layout)backend;
			tl.Width = (int) value;
		}

		public Size GetSize (object backend)
		{
			Pango.Layout tl = (Pango.Layout) backend;
			int w, h;
			tl.GetSize (out w, out h);
			return new Size ((double)w / (double)Pango.Scale.PangoScale, (double)h / (double)Pango.Scale.PangoScale);
		}
	}
}

