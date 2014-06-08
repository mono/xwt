// 
// PanedViews.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class PanedViews: HPaned
	{
		public PanedViews ()
		{
			Panel1.Content = CreateFrame ("Fixed panel at the left", false);
			
			HPaned centralPaned = new HPaned ();
			centralPaned.Panel1.Content = CreateFrame ("Should expand\nhorizontally and vertically", true);
			centralPaned.Panel1.Resize = true;

			VPaned verticalPaned = new VPaned ();
			verticalPaned.Panel1.Content = CreateFrame ("Fixed panel\nat the top", false);
			
			verticalPaned.Panel2.Content = CreateFrame ("Should expand vertically\n(with green background)", true);
			verticalPaned.Panel2.Content.BackgroundColor = Colors.LightGreen;
			verticalPaned.Panel2.Resize = true;

			centralPaned.Panel2.Content = verticalPaned;
			
			Panel2.Content = centralPaned;
			Panel2.Resize = true;
		}
		
		Frame CreateFrame (string text, bool fixedp)
		{
			var f = new Frame (FrameType.Custom);
			f.BorderColor = fixedp ? new Color (1,0,0) : new Color (0,0,1);
			f.BorderWidth = 1;
			f.Margin = 10;
			f.Content = new Label (text);
			f.Content.Margin = 10;
			return f;
		}
	}
}

