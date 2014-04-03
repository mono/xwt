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

namespace Samples
{
	public class WidgetEvents: VBox
	{
		Label res;

		public WidgetEvents ()
		{
			Label la = new Label ("Move the mouse here");
			res =  new Label ();
			PackStart (la);
			PackStart (res);

			la.MouseEntered += delegate {
				la.Text = "Mouse has Entered label";
			};
			la.MouseExited += delegate {
				la.Text = "Mouse has Exited label";
				res.Text = "Mouse has moved out of label";
			};
			la.MouseMoved += delegate {
				res.Text = "Mouse is moving in label";
				Application.TimeoutInvoke (800, MouseStopped);
			};
		}

		bool MouseStopped ()
		{
			res.Text = "Mouse has stopped in label";
			return false;
		}

	}
}

