// 
// ITextLayoutBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
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
	public abstract class TextLayoutBackendHandler: DisposableResourceBackendHandler
	{
		public abstract object Create ();

		public abstract void SetWidth (object backend, double value);
		public abstract void SetHeight (object backend, double value);
		public abstract void SetText (object backend, string text);
		public abstract void SetFont (object backend, Font font);
		public abstract void SetTrimming (object backend, TextTrimming textTrimming);
		public abstract void SetAlignment (object backend, Alignment alignment);
		public abstract Size GetSize (object backend);
		public abstract int GetIndexFromCoordinates (object backend, double x, double y);
		public abstract Point GetCoordinateFromIndex (object backend, int index);
		public abstract double GetBaseline (object backend);
		public abstract double GetMeanline (object backend);

		public abstract void AddAttribute (object backend, TextAttribute attribute);
		public abstract void ClearAttributes (object backend);
	}
}

