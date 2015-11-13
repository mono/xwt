// 
// IContextBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Hywel Thomas <hywel.w.thomas@gmail.com>
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
using Xwt.Drawing;
using System.Collections.Generic;

namespace Xwt.Backends
{
	public abstract class ContextBackendHandler: DrawingPathBackendHandler
	{
		public abstract void Save (object backend);

		public abstract void Restore (object backend);

		public abstract void Clip (object backend);
		
		public abstract void ClipPreserve(object backend);
		
		public abstract void Fill (object backend);
		
		public abstract void FillPreserve (object backend);
		
		public abstract void NewPath (object backend);
		
		public abstract void Stroke (object backend);
		
		public abstract void StrokePreserve (object backend);
		
		public abstract void SetColor (object backend, Xwt.Drawing.Color color);
		
		public abstract void SetLineWidth (object backend, double width);
		
		public abstract void SetLineDash (object backend, double offset, params double[] pattern);
		
		public abstract void SetPattern (object backend, object p);

		public abstract void DrawTextLayout (object backend, TextLayout layout, double x, double y);

		public abstract void DrawImage (object backend, ImageDescription img, double x, double y);

		public abstract void DrawImage (object backend, ImageDescription img, Rectangle srcRect, Rectangle destRect);

		public abstract void Rotate (object backend, double angle);
		
		public abstract void Scale (object backend, double scaleX, double scaleY);
		
		public abstract void Translate (object backend, double tx, double ty);

		public abstract void ModifyCTM (object backend, Matrix transform);

		public abstract Matrix GetCTM (object backend);

		public abstract bool IsPointInStroke (object backend, double x, double y);

		/// <summary>
		/// Sets a global alpha to be applied to all drawing operations.
		/// It doesn't affect colors that have already been set.
		/// </summary>
		public abstract void SetGlobalAlpha (object backend, double globalAlpha);

		public abstract double GetScaleFactor (object backend);

		public abstract void SetStyles (object backend, StyleSet styles);
	}
}

