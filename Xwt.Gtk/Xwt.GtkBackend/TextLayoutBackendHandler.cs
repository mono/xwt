// 
// TextLayoutBackendHandler.cs
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
using Xwt.Drawing;
using Xwt.Engine;
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	public class GtkTextLayoutBackendHandler: TextLayoutBackendHandler
	{
		static Cairo.Context SharedContext;
		
		public double Heigth = -1;
		
		static GtkTextLayoutBackendHandler ()
		{
			Cairo.Surface sf = new Cairo.ImageSurface (Cairo.Format.ARGB32, 1, 1);
			SharedContext = new Cairo.Context (sf);
		}
		
		public override object Create (Context context)
		{
			CairoContextBackend c = (CairoContextBackend) ToolkitEngine.GetBackend (context);
			return Pango.CairoHelper.CreateLayout (c.Context);
		}
		
		public override object Create (ICanvasBackend canvas)
		{
			return Pango.CairoHelper.CreateLayout (SharedContext);
		}

		public override void SetText (object backend, string text)
		{
			Pango.Layout tl = (Pango.Layout) backend;
			tl.SetText (text);
		}

		public override void SetFont (object backend, Xwt.Drawing.Font font)
		{
			Pango.Layout tl = (Pango.Layout)backend;
			tl.FontDescription = (Pango.FontDescription)ToolkitEngine.GetBackend (font);
		}
		
		public override void SetWidth (object backend, double value)
		{
			Pango.Layout tl = (Pango.Layout)backend;
			tl.Width = (int) (value * Pango.Scale.PangoScale);
		}
		
		public override void SetHeight (object backend, double value)
		{
			this.Heigth = value;
		}
		
		public override void SetTrimming (object backend, TextTrimming textTrimming)
		{
			Pango.Layout tl = (Pango.Layout)backend;
			if (textTrimming == TextTrimming.WordElipsis)
				tl.Ellipsize = Pango.EllipsizeMode.End;
			if (textTrimming == TextTrimming.Word)
				tl.Ellipsize = Pango.EllipsizeMode.None;
			
		}
		
		public override Size GetSize (object backend)
		{
			Pango.Layout tl = (Pango.Layout) backend;
			int w, h;
			tl.GetPixelSize (out w, out h);
			return new Size ((double)w, (double)h);
		}
	}
}

