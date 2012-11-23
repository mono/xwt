// 
// ResPool.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Xwt.Backends;

// This is similar in spirit to Mono's System.Windows.Forms.SystemResPool,
// to be able to recycle resources. Guess we will be able to add more elements later.

namespace Xwt.WPFBackend
{
	public static class ResPool
	{
		static Dictionary<Color, SolidColorBrush> solidBrushes = new Dictionary<Color, SolidColorBrush> ();

		public static SolidColorBrush GetSolidBrush (Xwt.Drawing.Color color)
		{
			return GetSolidBrush (DataConverter.ToWpfColor (color));
		}

		public static SolidColorBrush GetSolidBrush (Color color)
		{
			lock (solidBrushes) {
				SolidColorBrush brush;
				if (!solidBrushes.TryGetValue (color, out brush))
					solidBrushes [color] = brush = new SolidColorBrush (color);

				return brush;
			}
		}
	}
}
