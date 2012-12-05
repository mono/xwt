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

namespace Xwt.Backends
{
	public interface IContextBackendHandler: IBackendHandler
	{
		void Save (object backend);

		void Restore (object backend);

		void Arc (object backend, double xc, double yc, double radius, double angle1, double angle2);

		void ArcNegative (object backend, double xc, double yc, double radius, double angle1, double angle2);

		void Clip (object backend);
		
		void ClipPreserve(object backend);
		
		void ResetClip (object backend);
		
		void ClosePath(object backend);
		
		void CurveTo (object backend, double x1, double y1, double x2, double y2, double x3, double y3);
		
		void Fill (object backend);
		
		void FillPreserve (object backend);
		
		void LineTo (object backend, double x, double y);
		
		void MoveTo (object backend, double x, double y);
		
		void NewPath (object backend);
		
		void Rectangle (object backend, double x, double y, double width, double height);
		
		void RelCurveTo (object backend, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3);
		
		void RelLineTo (object backend, double dx, double dy);
		
		void RelMoveTo (object backend, double dx, double dy);
		
		void Stroke (object backend);
		
		void StrokePreserve (object backend);
		
		void SetColor (object backend, Xwt.Drawing.Color color);
		
		void SetLineWidth (object backend, double width);
		
		void SetLineDash (object backend, double offset, params double[] pattern);
		
		void SetPattern (object backend, object p);
		
		void SetFont (object backend, Font font);
		
		void DrawTextLayout (object backend, TextLayout layout, double x, double y);
		
		void DrawImage (object backend, object img, double x, double y, double alpha);
		
		void DrawImage (object backend, object img, double x, double y, double width, double height, double alpha);

		void DrawImage (object backend, object img, Rectangle srcRect, Rectangle destRect, double alpha);

		void ResetTransform (object backend);

		void Rotate (object backend, double angle);
		
		void Scale (object backend, double scaleX, double scaleY);
		
		void Translate (object backend, double tx, double ty);

		void TransformPoint (object backend, ref double x, ref double y);

		void TransformDistance (object backend, ref double dx, ref double dy);

		void TransformPoints (object backend, Point[] points);

		void TransformDistances (object backend, Distance[] vectors);

		/// <summary>
		/// Sets a global alpha to be applied to all drawing operations.
		/// It doesn't affect colors that have already been set.
		/// </summary>
		void SetGlobalAlpha (object backend, double globalAlpha);
		
		void Dispose (object backend);
	}
}

