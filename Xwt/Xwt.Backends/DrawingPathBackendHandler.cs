// 
// DrawingPathBackendHandler.cs
//  
// Author:
//       Alex Corrado <corrado@xamarin.com>
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
using Xwt.Drawing;

namespace Xwt.Backends
{
	public abstract class DrawingPathBackendHandler: DisposableResourceBackendHandler
	{
		public abstract void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2);

		public abstract void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2);

		public abstract void ClosePath (object backend);

		public abstract void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3);

		public abstract void LineTo (object backend, double x, double y);

		public abstract void MoveTo (object backend, double x, double y);

		public abstract void Rectangle (object backend, double x, double y, double width, double height);

		public abstract void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3);

		public abstract void RelLineTo (object backend, double dx, double dy);

		public abstract void RelMoveTo (object backend, double dx, double dy);

		public abstract object CreatePath ();

		public abstract object CopyPath (object backend);

		public abstract void AppendPath (object backend, object otherBackend);

		public abstract bool IsPointInFill (object backend, double x, double y);
	}
}

