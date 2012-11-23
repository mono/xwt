// 
// ImagePatternBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
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
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class GtkImagePatternBackendHandler: ImagePatternBackendHandler
	{
		#region IImagePatternBackendHandler implementation
		public override object Create (object img)
		{
			Gdk.Pixbuf pb = (Gdk.Pixbuf)img;
			var imgs = new Cairo.ImageSurface (Cairo.Format.ARGB32, pb.Width, pb.Height);
			var ic = new Cairo.Context (imgs);
			Gdk.CairoHelper.SetSourcePixbuf (ic, pb, 0, 0);
			ic.Paint ();
			imgs.Flush ();
			((IDisposable)ic).Dispose ();
			var p = new Cairo.SurfacePattern (imgs);
			p.Extend = Cairo.Extend.Repeat;
			return p;
		}
		#endregion
	}
}

