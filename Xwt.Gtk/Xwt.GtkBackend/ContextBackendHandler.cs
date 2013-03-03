// 
// ContextBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt.CairoBackend;
using Xwt.Drawing;

using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class ContextBackendHandler: CairoContextBackendHandler
	{
		protected override object ResolveImage (object img, double width, double height)
		{
			if (img is String)
				return ImageHandler.CreateBitmap ((string)img, width, height);
			else
				return img;
		}

		protected override void SetSourceImage (Cairo.Context ctx, object img, double x, double y)
		{
			Gdk.CairoHelper.SetSourcePixbuf (ctx, (Gdk.Pixbuf)img, x, y);
		}
		
		protected override Size GetImageSize (object img)
		{
			Gdk.Pixbuf pb = (Gdk.Pixbuf)img;
			return new Size (pb.Width, pb.Height);
		}
	}
	
	public class ContextBackendHandlerWithPango: ContextBackendHandler
	{
		public override void DrawTextLayout (object backend, TextLayout layout, double x, double y)
		{
			var be = (GtkTextLayoutBackendHandler.PangoBackend)Toolkit.GetBackend (layout);
			var pl = be.Layout;
			CairoContextBackend ctx = (CairoContextBackend)backend;
			ctx.Context.MoveTo (x, y);
			if (layout.Height <= 0) {
				Pango.CairoHelper.ShowLayout (ctx.Context, pl);
			} else {
				var lc = pl.LineCount;
				var scale = Pango.Scale.PangoScale;
				double h = 0;
				for (int i=0; i<lc; i++) {
					var line = pl.Lines [i];
					var ext = new Pango.Rectangle ();
					var extl = new Pango.Rectangle ();
					line.GetExtents (ref ext, ref extl);
					h += (extl.Height / scale);
					if (h > layout.Height)
						break;
					ctx.Context.MoveTo (x, y + h);
					Pango.CairoHelper.ShowLayoutLine (ctx.Context, line);
				}
			}
		}
	}
}

