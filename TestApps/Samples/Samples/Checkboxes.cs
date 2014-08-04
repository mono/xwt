// 
// Checkboxes.cs
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
	public class Checkboxes: VBox
	{
		public Checkboxes ()
		{
			PackStart (new CheckBox ("Normal checkbox"));
			PackStart (new CheckBox ("Mixed to start") { State = CheckBoxState.Mixed });

			var b = new CheckBox ("Allows mixed (with red background)") { AllowMixed = true };
			b.BackgroundColor = Xwt.Drawing.Colors.Red;
			PackStart (b);
			
			int clicks = 0, toggles = 0;
			Label la = new Label ();
			PackStart (la);
			
			b.Clicked += delegate {
				clicks++;
				la.Text = string.Format ("state:{0}, clicks:{1}, toggles:{2}", b.State, clicks, toggles);
			};
			
			b.Toggled += delegate {
				toggles++;
				la.Text = string.Format ("state:{0}, clicks:{1}, toggles:{2}", b.State, clicks, toggles);
			};
		}
	}
}

