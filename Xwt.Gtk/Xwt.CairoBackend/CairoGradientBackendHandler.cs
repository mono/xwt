// 
// GradientBackendHandler.cs
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

namespace Xwt.CairoBackend
{
	public class CairoGradientBackendHandler: GradientBackendHandler
	{
		public override object CreateLinear (double x0, double y0, double x1, double y1)
		{
			return new Cairo.LinearGradient (x0, y0, x1, y1);
		}

		public override void Dispose (object backend)
		{
			((IDisposable)backend).Dispose ();
		}

		public override object CreateRadial (double cx0, double cy0, double radius0, double cx1, double cy1, double radius1)
		{
			return new Cairo.RadialGradient (cx0, cy0, radius0, cx1, cy1, radius1);
		}

		public override void AddColorStop (object backend, double position, Xwt.Drawing.Color color)
		{
			Cairo.Gradient g = (Cairo.Gradient) backend;
			g.AddColorStop (position, color.ToCairoColor ());
		}

		public override bool DisposeHandleOnUiThread {
			get {
				return true;
			}
		}
	}
}

