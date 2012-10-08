// 
// IFontBackendHandler.cs
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
using Xwt.Drawing;

namespace Xwt.Backends
{
	public interface IFontBackendHandler: IBackendHandler
	{
		object Create (string fontName, double size, FontSizeUnit sizeUnit, FontStyle style, FontWeight weight, FontStretch stretch);
		object Copy (object handle);
		
		object SetSize (object handle, double size, FontSizeUnit unit);
		object SetFamily (object handle, string family);
		object SetStyle (object handle, FontStyle style);
		object SetWeight (object handle, FontWeight weight);
		object SetStretch (object handle, FontStretch stretch);
		
		double GetSize (object handle);
		string GetFamily (object handle);
		FontStyle GetStyle (object handle);
		FontWeight GetWeight (object handle);
		FontStretch GetStretch (object handle);
		
	}
}

