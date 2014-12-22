//
// SliderSample.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
	public class SliderSample: VBox
	{
		public SliderSample ()
		{
			PackStart (new HorizontalSliderSample ());
			PackStart (new VerticalSliderSamples (), true); 
		}
	}

	public class HorizontalSliderSample : Table
	{
		public HorizontalSliderSample ()
		{
			var sl1 = new HSlider {
				MinimumValue = 0,
				MaximumValue = 2,
				StepIncrement = 0.05,
				SnapToTicks = true,
			};

			var lbl1 = new Label (sl1.Value.ToString ("F2"));

			sl1.ValueChanged += (sender, e) => {
				lbl1.Text = (sl1.Value).ToString ("F2");
			};

			var sl2 = new HSlider { 
				MinimumValue = -9, 
				MaximumValue = 0,
				StepIncrement = 2,
				SnapToTicks = true,
			};

			var lbl2 = new Label (sl2.Value.ToString ("F2"));

			sl2.ValueChanged += (sender, e) => {
				lbl2.Text = (sl2.Value).ToString ("F2");
			};

			var sl21 = new HSlider { 
				MinimumValue = 0, 
				MaximumValue = 9,
				StepIncrement = 2,
				SnapToTicks = true,
			};

			var lbl21 = new Label (sl21.Value.ToString ("F2"));

			sl21.ValueChanged += (sender, e) => {
				lbl21.Text = (sl21.Value).ToString ("F2");
			};

			var sl22 = new HSlider { 
				MinimumValue = -9, 
				MaximumValue = 9,
				StepIncrement = 2,
				SnapToTicks = true,
			};

			var lbl22 = new Label (sl22.Value.ToString ("F2"));

			sl22.ValueChanged += (sender, e) => {
				lbl22.Text = (sl22.Value).ToString ("F2");
			};


			var sl23 = new HSlider { 
				MinimumValue = -9, 
				MaximumValue = 9,
				StepIncrement = 1,
				SnapToTicks = true,
			};

			var lbl23 = new Label (sl23.Value.ToString ("F2"));

			sl23.ValueChanged += (sender, e) => {
				lbl23.Text = (sl23.Value).ToString ("F2");
			};

			var sl3 = new HSlider { 
				MinimumValue = -9, 
				MaximumValue = 9,
				StepIncrement = 2,
				SnapToTicks = false,
			};

			var lbl3 = new Label (sl3.Value.ToString ("F2"));

			sl3.ValueChanged += (sender, e) => {
				lbl3.Text = (sl2.Value = sl21.Value = sl22.Value = sl23.Value = sl3.Value).ToString ("F2");
			};

			var sl4 = new HSlider {
				MinimumValue = -1,
				MaximumValue = 1,
				StepIncrement = 0.05
			};

			var lbl4 = new Label (sl4.Value.ToString ("F"));
			lbl4.ExpandHorizontal = false;

			var sl4box = new VBox ();
			sl4box.PackStart (lbl4, false, hpos: WidgetPlacement.Start);
			sl4box.PackStart (sl4);

			sl4.ValueChanged += (sender, e) => {

				var offset = Math.Abs (sl4.Value) % sl4.StepIncrement;
				if (Math.Abs (offset) > double.Epsilon) {
					if (offset > sl4.StepIncrement / 2) {
						if (sl4.Value >= 0)
							sl4.Value += -offset + sl4.StepIncrement;
						else
							sl4.Value += offset - sl4.StepIncrement;
					}
					else
						if (sl4.Value >= 0)
							sl4.Value -= offset;
						else
							sl4.Value += offset;
				}

				lbl4.MarginLeft = sl4.SliderPosition - (lbl4.Size.Width / 2);
				if (lbl4.MarginLeft + lbl4.Size.Width > sl4.Size.Width)
					lbl4.MarginLeft = sl4.Size.Width - lbl4.Size.Width;
				if (lbl4.MarginLeft < 0)
					lbl4.MarginLeft = 0;

				lbl4.Text = (sl4.Value).ToString ("F2");
			};
			sl4.Value = sl4.MinimumValue;


			var sl4Labels = new HBox ();
			sl4Labels.PackStart (new Label ("-1"), true);
			sl4Labels.PackStart (new Label ("0") { TextAlignment = Alignment.Center }, true);
			sl4Labels.PackStart (new Label ("1") { TextAlignment = Alignment.End }, true);
			sl4box.PackStart (sl4Labels);

			Add (sl1, 0, 0, hexpand: true);
			Add (lbl1, 1, 0);
			Add (sl2, 0, 1, hexpand: true);
			Add (lbl2, 1, 1);
			Add (sl21, 0, 2, hexpand: true);
			Add (lbl21, 1, 2);
			Add (sl22, 0, 3, hexpand: true);
			Add (lbl22, 1, 3);
			Add (sl23, 0, 4, hexpand: true);
			Add (lbl23, 1, 4);
			Add (sl3, 0, 5, hexpand: true);
			Add (lbl3, 1, 5);
			Add (sl4box, 0, 6, hexpand: true);


		}
	}

	public class VerticalSliderSamples : Table
	{
		public VerticalSliderSamples()
		{
			var slv1 = new VSlider { MinimumValue = 50, MaximumValue = 100 };


			var lblv1 = new Label (slv1.Value.ToString ("F"));

			slv1.ValueChanged += (sender, e) => {
				lblv1.Text = (slv1.Value).ToString ("F");
			};

			var slv2 = new VSlider { 
				MinimumValue = -9, 
				MaximumValue = 0,
				StepIncrement = 2,
				SnapToTicks = true,
			};

			var lblv2 = new Label (slv2.Value.ToString ("F"));

			slv2.ValueChanged += (sender, e) => {
				lblv2.Text = (slv2.Value).ToString ("F");
			};

			var slv3 = new VSlider { 
				MinimumValue = -9, 
				MaximumValue = 9,
				StepIncrement = 2,
				SnapToTicks = false,
			};

			var lblv3 = new Label (slv3.Value.ToString ("F"));

			slv3.ValueChanged += (sender, e) => {
				lblv3.Text = (slv2.Value = slv3.Value).ToString ("F");
			};

			var slv4 = new VSlider {
				MinimumValue = 0,
				MaximumValue = 20,
				StepIncrement = 0.05
			};

			var lblv4 = new Label (slv4.Value.ToString ("F"));
			lblv4.ExpandVertical = false;

			var slv4box = new HBox ();
			slv4box.PackStart (lblv4, false, vpos: WidgetPlacement.Start, hpos: WidgetPlacement.End);
			slv4box.PackStart (slv4, false, false);

			slv4.ValueChanged += (sender, e) => {

				var offset = Math.Abs (slv4.Value) % slv4.StepIncrement;
				if (Math.Abs (offset) > double.Epsilon) {
					if (offset > slv4.StepIncrement / 2) {
						if (slv4.Value >= 0)
							slv4.Value += -offset + slv4.StepIncrement;
						else
							slv4.Value += offset - slv4.StepIncrement;
					}
					else
						if (slv4.Value >= 0)
							slv4.Value -= offset;
						else
							slv4.Value += offset;
				}

				lblv4.MarginTop = slv4.SliderPosition - (lblv4.Size.Height / 2);
				if (lblv4.MarginTop + lblv4.Size.Height > slv4.Size.Height)
					lblv4.MarginTop = slv4.Size.Height - lblv4.Size.Height;
				if (lblv4.MarginTop < 0)
					lblv4.MarginTop = 0;

				lblv4.Text = (slv4.Value).ToString ("F2");
			};
			slv4.Value = slv4.MaximumValue;

			var slv4Labels = new VBox ();
			slv4Labels.PackStart (new Label ("20"), true, vpos: WidgetPlacement.Start);
			slv4Labels.PackStart (new Label ("10"), true, vpos: WidgetPlacement.Center);
			slv4Labels.PackStart (new Label ("0"), true, vpos: WidgetPlacement.End);
			slv4box.PackStart (slv4Labels, false, hpos: WidgetPlacement.Start);

			Add (slv1, 0, 1, vexpand: true, hexpand: true, hpos: WidgetPlacement.Center);
			Add (lblv1, 0, 0, hpos: WidgetPlacement.Center);
			Add (slv2, 1, 1, vexpand: true, hexpand: true, hpos: WidgetPlacement.Center);
			Add (lblv2, 1, 0, hpos: WidgetPlacement.Center);
			Add (slv3, 2, 1, vexpand: true, hexpand: true, hpos: WidgetPlacement.Center);
			Add (lblv3, 2, 0, hpos: WidgetPlacement.Center);
			Add (slv4box, 3, 1, vexpand: true, hexpand: true, hpos: WidgetPlacement.Center);
		}
	}
}

