// 
// WidgetEvents.cs
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
	public class WidgetEvents: VBox
	{
		Label la = new Label ("Move the mouse here");
		Label res = new Label ("");
		bool inside = false;
		bool moved = false;

		public WidgetEvents ()
		{
			PackStart (la);
			PackStart (res);

			la.MouseEntered += delegate {
				inside = true;
				Application.TimeoutInvoke (100, CheckMouse);
			};
			la.MouseExited += delegate {
				inside = false;
			};
			la.MouseMoved += delegate {
				moved = true;
			};
		}

		bool CheckMouse ()
		{
			if (!inside) {
				res.Text = "Mouse has Exited label";
				la.TextColor = Colors.Black;
				la.BackgroundColor = Colors.LightGray;
				la.Text = "Move the mouse here";
			} else {
				res.Text = "Mouse has Entered label";
				la.TextColor = Colors.White;
				if (moved) {
					la.BackgroundColor = Colors.Green;
					la.Text = "Mouse is moving";
					moved = false;	// reset and check next time
				} else {
					la.BackgroundColor = Colors.Red;
					la.Text = "Mouse has stopped";
				}
			}
			return inside;
		}

	}
}

