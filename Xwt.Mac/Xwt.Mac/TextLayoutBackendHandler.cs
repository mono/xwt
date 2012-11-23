// 
// TextLayoutBackendHandler.cs
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
using Xwt.Backends;
using MonoMac.AppKit;
using MonoMac.Foundation;

using Xwt.Drawing;

namespace Xwt.Mac
{
	public class MacTextLayoutBackendHandler: TextLayoutBackendHandler
	{
		class LayoutInfo
		{
			public NSAttributedString Text;
			public NSFont Font;
			public string PlainText;
			public double Width = -1;
			public double Heigth = -1;
			public TextTrimming TextTrimming;
		}
		
		public override object Create (Xwt.Drawing.Context context)
		{
			return new LayoutInfo ();
		}
		
		public override object Create (ICanvasBackend canvas)
		{
			return new LayoutInfo ();
		}

		public override void SetText (object backend, string text)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.PlainText = text;
			UpdateInfo (li);
		}

		public override void SetFont (object backend, Xwt.Drawing.Font font)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Font = (NSFont)Toolkit.GetBackend (font);
			UpdateInfo (li);
		}
		
		public override void SetWidth (object backend, double value)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Width = value;
		}
		
		public override void SetHeight (object backend, double value)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Heigth = value;
		}
		
		public override void SetTrimming (object backend, TextTrimming value)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.TextTrimming = value;
		}
		
		void UpdateInfo (LayoutInfo li)
		{
			if (li.PlainText == null)
				return;
			if (li.Font != null) {
				NSDictionary dict = NSDictionary.FromObjectsAndKeys (
					new object[] { li.Font },
				    new object[] { NSAttributedString.FontAttributeName }
				);
				li.Text = new NSAttributedString (li.PlainText, dict);
			} else {
				li.Text = new NSAttributedString (li.PlainText);
			}
		}

		public override Size GetSize (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			var s = li.Text.Size;
			return new Xwt.Size (s.Width, s.Height);
		}
		
		public static void Draw (object ctx, object layout, double x, double y)
		{
			LayoutInfo li = (LayoutInfo) layout;
			li.Text.DrawString (new System.Drawing.PointF ((float)x, (float)y));
		}
	}
}

