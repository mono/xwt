// 
// Util.cs
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
using MonoMac.AppKit;
using Xwt.Drawing;

namespace Xwt.Mac
{
	public static class Util
	{
		public static double WidgetX (this NSView v)
		{
			return (double) v.Frame.X;
		}
		
		public static double WidgetY (this NSView v)
		{
			return (double) v.Frame.Y;
		}
		
		public static double WidgetWidth (this NSView v)
		{
			return (double) (v.Frame.Width);
		}
		
		public static double WidgetHeight (this NSView v)
		{
			return (double) (v.Frame.Height);
		}
		
		public static Rectangle WidgetBounds (this NSView v)
		{
			return new Rectangle (v.WidgetX(), v.WidgetY(), v.WidgetWidth(), v.WidgetHeight());
		}
		
		public static void SetWidgetBounds (this NSView v, Rectangle rect)
		{
			float y = (float)rect.Y;
			if (v.Superview != null)
				y = v.Superview.Frame.Height - y - (float)rect.Height;
			v.Frame = new System.Drawing.RectangleF ((float)rect.X, y, (float)rect.Width, (float)rect.Height);
		}
		
		public static NSColor ToNSColor (this Color col)
		{
			return NSColor.FromDeviceRgba ((float)col.Red, (float)col.Green, (float)col.Blue, (float)col.Alpha);
		}
	}
}

