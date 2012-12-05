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
using MonoMac.CoreGraphics;
using SizeF = System.Drawing.SizeF;
using RectangleF = System.Drawing.RectangleF;

namespace Xwt.Mac
{
	public static class Util
	{
		public static readonly string DeviceRGBString = NSColorSpace.DeviceRGB.ToString ();
		static CGColorSpace deviceRGB, pattern;

		public static CGColorSpace DeviceRGBColorSpace {
			get {
				if (deviceRGB == null)
					deviceRGB = CGColorSpace.CreateDeviceRGB ();
				return deviceRGB;
			}
		}

		public static CGColorSpace PatternColorSpace {
			get {
				if (pattern == null)
					pattern = CGColorSpace.CreatePattern (null);
				return pattern;
			}
		}

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
		
		public static CGColor ToCGColor (this Color col)
		{
			return new CGColor ((float)col.Red, (float)col.Green, (float)col.Blue, (float)col.Alpha);
		}

		public static Color ToXwtColor (this NSColor col)
		{
			col = col.UsingColorSpace (DeviceRGBString);
			return new Color (col.RedComponent, col.GreenComponent, col.BlueComponent, col.AlphaComponent);
		}
		
		public static Color ToXwtColor (this CGColor col)
		{
			var cs = col.Components;
			return new Color (cs[0], cs[1], cs[2], col.Alpha);
		}

		public static Size ToXwtSize (this SizeF s)
		{
			return new Size (s.Width, s.Height);
		}

		public static RectangleF ToRectangleF (this Rectangle r)
		{
			return new RectangleF ((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
		}

		// /System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Headers/IconsCore.h
		public static int ToIconType (string id)
		{
			switch (id) {
			case StockIcons.Error:       return 1937010544; // 'stop'
			case StockIcons.Warning:     return 1667331444; // 'caut'
			case StockIcons.Information: return 1852798053; // 'note'
			case StockIcons.Question:    return 1903519091; // 'ques'
			case StockIcons.Remove:      return 1952736620; // 'tdel'
			}
			return 0;
		}

		/* To get the above int values, pass the four char code thru this:
		public static int C (string code)
		{
			return ((int)code[0]) << 24
			     | ((int)code[1]) << 16
			     | ((int)code[2]) << 8
			     | ((int)code[3]);
		}
		*/

		public static SizeF ToIconSize (IconSize size)
		{
			switch (size) {
			case IconSize.Small: return new SizeF (16f, 16f);
			case IconSize.Large: return new SizeF (64f, 64f);
			}
			return new SizeF (32f, 32f);
		}
	}
}

