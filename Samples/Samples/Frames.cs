// 
// Frames.cs
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class Frames: VBox
	{
		public Frames ()
		{
			Frame f = new Frame ();
			f.Label = "Simple widget box";
			f.Content = new SimpleBox (50);
			PackStart (f);
			
			f = new Frame ();
			f.Content = new Label ("No label");
			PackStart (f);
			
			var fb = new FrameBox ();
			fb.BorderWidthLeft = 1;
			fb.BorderWidthTop = 2;
			fb.BorderWidthRight = 3;
			fb.BorderWidthBottom = 4;
			fb.BorderColor = new Color (0, 0, 1);
			fb.Content = new Label ("Custom");
			PackStart (fb);
			
			fb = new FrameBox ();
			fb.BorderWidth = 2;
			fb.PaddingLeft = 10;
			fb.PaddingTop = 20;
			fb.PaddingRight = 30;
			fb.PaddingBottom = 40;
			fb.Content = new SimpleBox (50);
			PackStart (fb);
			
			fb = new FrameBox ();
			fb.BorderWidth = 2;
			fb.Padding = 10;
			fb.Content = new Label ("With red background");
			fb.BackgroundColor = new Color (1,0,0);
			PackStart (fb);
		}
	}
}

