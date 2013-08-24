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
		public override bool DisposeHandleOnUiThread {
			get {
				return true;
			}
		}
		#region IImagePatternBackendHandler implementation
		public override object Create (ImageDescription img)
		{
			return new ImagePatternBackend () {
				Image = img
			};
		}

		public override void Dispose (object img)
		{
			var pb = (ImagePatternBackend)img;
			pb.Dispose ();
		}
		#endregion
	}

	class ImagePatternBackend
	{
		public ImageDescription Image;

		Cairo.SurfacePattern pattern;
		double currentScaleFactor;

		public Cairo.Pattern GetPattern (ApplicationContext actx, double scaleFactor)
		{
			if (pattern == null || currentScaleFactor != scaleFactor) {
				if (pattern != null)
					pattern.Dispose ();
				Gdk.Pixbuf pb = ((GtkImage)Image.Backend).GetBestFrame (actx, scaleFactor, Image.Size.Width, Image.Size.Height, false);
				using (var imgs = new Cairo.ImageSurface (Cairo.Format.ARGB32, (int)(Image.Size.Width * scaleFactor), (int)(Image.Size.Height * scaleFactor))) {
					var ic = new Cairo.Context (imgs);
					ic.Scale ((double)imgs.Width / (double)pb.Width, (double)imgs.Height / (double)pb.Height);
					Gdk.CairoHelper.SetSourcePixbuf (ic, pb, 0, 0);
					ic.Paint ();
					imgs.Flush ();
					((IDisposable)ic).Dispose ();
					pattern = new Cairo.SurfacePattern (imgs);
				}
				pattern.Extend = Cairo.Extend.Repeat;
				var cm = new Cairo.Matrix ();
				cm.Scale (scaleFactor, scaleFactor);
				pattern.Matrix = cm;
				currentScaleFactor = scaleFactor;
			}
			return pattern;
		}

		public void Dispose ()
		{
			if (pattern != null)
				pattern.Dispose ();
		}
	}
}

