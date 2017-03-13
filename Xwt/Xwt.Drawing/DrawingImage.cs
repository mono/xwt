//
// DrawingImage.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
	public class DrawingImage: Image
	{
		public DrawingImage ()
		{
			Backend = Toolkit.CurrentEngine.ImageBackendHandler.CreateCustomDrawn (Draw);
			Init ();
			NativeRef.SetCustomDrawSource (Draw);
		}

		void Draw (object ctx, Rectangle bounds, ImageDescription idesc, Toolkit toolkit)
		{
			var c = new Context (ctx, toolkit);
			c.Reset (null);
			c.Save ();
			if (idesc.Styles != StyleSet.Empty)
				c.SetStyles (idesc.Styles);
			c.GlobalAlpha = idesc.Alpha;
			OnDraw (c, bounds);
			c.Restore ();
		}

		protected virtual void OnDraw (Context ctx, Rectangle bounds)
		{
		}

		protected override Size GetDefaultSize ()
		{
			return Size.Zero;
		}
	}
}

