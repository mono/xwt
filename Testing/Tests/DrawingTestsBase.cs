//
// DrawingTestsBase.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin, Inc (http://www.xamarin.com)
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
using NUnit.Framework;
using System.Threading;
using Xwt.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Xwt
{
	public class DrawingTestsBase
	{
		protected ImageBuilder builder;
		protected Context context;

		protected void InitBlank (int width = 50, int height = 50)
		{
			if (builder != null)
				builder.Dispose ();

			builder = new ImageBuilder (width, height);
			context = builder.Context;
			context.Rectangle (0, 0, width, height);
			context.SetColor (Colors.White);
			context.Fill ();
			context.SetColor (Colors.Black);
			context.SetLineWidth (1);
		}

		protected void CheckImage (string refImageName, double scaleFactor = 1)
		{
			if (builder == null)
				return;
			var img = builder.ToBitmap (scaleFactor);
			builder.Dispose ();
			builder = null;

			ReferenceImageManager.CheckImage (refImageName, img);
		}

	}
}

