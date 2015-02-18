// 
// StockIcons.cs
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
using Xwt.Backends;

namespace Xwt
{
	public static class StockIcons
	{
		static Image GetIcon (string id)
		{
			var img = Toolkit.CurrentEngine.GetStockIcon (id);
			img.SetStockSource (id);
			return img;
		}

		public static Image Error { get { return GetIcon (StockIconId.Error); } }
		public static Image Warning { get { return GetIcon (StockIconId.Warning); } }
		public static Image Information { get { return GetIcon (StockIconId.Information); } }
		public static Image Question { get { return GetIcon (StockIconId.Question); } }
		public static Image OrientationPortrait { get { return GetIcon (StockIconId.OrientationPortrait); } }
		public static Image OrientationLandscape { get { return GetIcon (StockIconId.OrientationLandscape); } }
		public static Image ZoomIn { get { return GetIcon (StockIconId.ZoomIn); } }
		public static Image ZoomOut { get { return GetIcon (StockIconId.ZoomOut); } }
		public static Image ZoomFit { get { return GetIcon (StockIconId.ZoomFit); } }
		public static Image Zoom100 { get { return GetIcon (StockIconId.Zoom100); } }
		public static Image Add { get { return GetIcon (StockIconId.Add); } }
		public static Image Remove { get { return GetIcon (StockIconId.Remove); } }
	}
}

