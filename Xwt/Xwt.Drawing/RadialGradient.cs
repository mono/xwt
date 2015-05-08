//
// RadialGradient.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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
//

using System;
using Xwt.Backends;

namespace Xwt.Drawing
{
	public sealed class RadialGradient: Gradient
	{
		double cx0, cy0, radius0, cx1, cy1, radius1;

		public RadialGradient (double cx0, double cy0, double radius0, double cx1, double cy1, double radius1)
		{
			this.cx0 = cx0;
			this.cy0 = cy0;
			this.radius0 = radius0;
			this.cx1 = cx1;
			this.cy1 = cy1;
			this.radius1 = radius1;
		}

		protected override object CreateGradientBackend (GradientBackendHandler handler)
		{
			return handler.CreateRadial (cx0, cy0, radius0, cx1, cy1, radius1);
		}
	}
}

