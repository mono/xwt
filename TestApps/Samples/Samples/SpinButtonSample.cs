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
using System;
using System.Globalization;
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

			try {
				PackStart (new ExtendedSpinButtonSample()); 
			} catch {
				Console.WriteLine ("Loading extended Spin Buttons failed!");
			}
		}
	}

	public class ExtendedSpinButtonSample : VBox
	{
		SpinButton extspn1 = new SpinButton ();
		SpinButton extspn1hex = new SpinButton();
		SpinButton extspn2 = new SpinButton ();
		SpinButton extspn2hex = new SpinButton();

		public ExtendedSpinButtonSample()
		{
			HBox box1 = new HBox ();
			box1.PackStart (extspn1, true);
			box1.PackStart (extspn1hex, true);
			PackStart (box1); 

			extspn1.MinimumValue = extspn1hex.MinimumValue = 0;
			extspn1.MaximumValue = extspn1hex.MaximumValue = 1;
			extspn1.Digits = 6;
			extspn1hex.Digits = 10;
			extspn1.IncrementValue = extspn1hex.IncrementValue = 0.000001;

			extspn1hex.ValueInput += (sender, e) => {
				try {
					string temp = extspn1hex.Text.StartsWith ("0x") ? extspn1hex.Text.Substring (2) : extspn1hex.Text;
					if (temp.Length % 2 != 0)
						temp = "0" + temp;

					byte[] bytes = new byte[temp.Length / 2];
					for (int index = 0; index < bytes.Length; index++) {
						string byteValue = temp.Substring (index * 2, 2);
						bytes [index] = byte.Parse (byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					}
					e.NewValue = BitConverter.ToSingle (bytes, 0);
				} catch {
					e.NewValue = double.NaN;
				} finally {
					e.Handled = true;
				}
			};

			extspn1hex.ValueOutput += (sender, e) => {
				string res = "0x";
				byte[] data = BitConverter.GetBytes ((float)extspn1hex.Value);
				foreach (byte x in data)
					res += x.ToString ("X2");
				extspn1hex.Text = res;
				e.Handled = true;
			};

			extspn1.Value = extspn1hex.Value = 0;

			extspn1.ValueChanged += (sender, e) => extspn1hex.Value = extspn1.Value;
			extspn1hex.ValueChanged += (sender, e) => extspn1.Value = extspn1hex.Value;

			HBox box2 = new HBox ();
			box2.PackStart (extspn2, true);
			box2.PackStart (extspn2hex, true);
			PackStart (box2); 

			extspn2.MinimumValue = extspn2hex.MinimumValue = -30000;
			extspn2.MaximumValue = extspn2hex.MaximumValue = 30000;
			extspn2.Digits = extspn2hex.Digits = 0;
			extspn2.IncrementValue = extspn2hex.IncrementValue = 1;
			extspn2.Wrap = true;

			extspn2hex.ValueInput += (sender, e) => {
				int new_val;
				if (int.TryParse (extspn2hex.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out new_val)) {
					e.NewValue = new_val;
					if (new_val == 2)
						e.NewValue = double.NaN;
					e.Handled = true;
				}
			};

			extspn2hex.ValueOutput += (sender, e) => {
				extspn2hex.Text = ((int)extspn2hex.Value).ToString ("X");
				e.Handled = true;
			};

			extspn2.Value = extspn2.Value = 0;

			extspn2.ValueChanged += (sender, e) => extspn2hex.Value = extspn2.Value;
			extspn2hex.ValueChanged += (sender, e) => extspn2.Value = extspn2hex.Value;

			var extspn3 = new SpinButton ();
			extspn3.MinimumValue = -0.001;
			extspn3.MaximumValue = 0.001;
			extspn3.Digits = 2;
			extspn3.Value = 0;
			extspn3.IncrementValue = 0.000001;

			extspn3.ValueInput += (sender, e) => {
				double new_val;
				if (double.TryParse (extspn3.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out new_val)) {
					e.NewValue = Math.Round(new_val, 6);
					e.Handled = true;
				}
			};

			extspn3.ValueOutput += (sender, e) => {
				extspn3.Text = Math.Round(extspn3.Value, 6).ToString ("E" + extspn3.Digits);
				e.Handled = true;
			};

			PackStart (extspn3);
		}
	}
}

