//
// SpinButtons.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class SpinButtonSample : VBox
	{
		public SpinButtonSample ()
		{
			var spn1 = new SpinButton();
			spn1.MinimumValue = -100;
			spn1.MaximumValue = 100;
			spn1.IncrementValue = 1;
			spn1.Digits = 0;
			PackStart (spn1);

			var spn2 = new SpinButton();
			spn2.MinimumValue = 0;
			spn2.MaximumValue = 1;
			spn2.Digits = 3;
			spn2.IncrementValue = 0.01;
			spn2.Style = ButtonStyle.Borderless;
			PackStart (spn2);

			var spn3 = new SpinButton();
			spn3.MinimumValue = -10;
			spn3.MaximumValue = 20;
			spn3.Digits = 1;
			spn3.IncrementValue = 0.1;
			spn3.Wrap = true;
			PackStart (spn3);


			var spn4 = new SpinButton();
			spn4.IsIndeterminate = true;
			spn4.IndeterminateMessage = "I have no initial value";
			var defColor = spn4.BackgroundColor;
			spn4.BackgroundColor = Colors.Red;
			spn4.ValueChanged += (sender, e) => spn4.BackgroundColor = defColor;
			PackStart (spn4);

		}
	}
}

