//
// SliderBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Vsevolod Kukol <sevo@sevo.org>
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
using Xwt.Backends;
using System.Collections.Generic;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using nint = System.Int32;
#else
using AppKit;
using Foundation;
#endif

namespace Xwt.Mac
{
	public class SliderBackend: ViewBackend<NSSlider,ISliderEventSink>, ISliderBackend
	{
		Orientation orientation;

		public SliderBackend ()
		{
		}

		public void Initialize (Orientation dir)
		{
			ViewObject = new MacSlider ();
			//((NSSliderCell)Widget.Cell).SetValueForKey (NSObject.FromObject (false), (NSString)"NSVertical");
			orientation = dir;
			if (dir == Orientation.Horizontal)
				Widget.SetFrameSize (new System.Drawing.SizeF (80, 30));
			else
				Widget.SetFrameSize (new System.Drawing.SizeF (30, 80));
		}

		MacSlider Slider {
			get { return (MacSlider)Widget; }
		}

		protected override void OnSizeToFit ()
		{
			Widget.SizeToFit ();
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is SliderEvent)
				Widget.Activated += HandleActivated;;
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is SliderEvent)
				Widget.Activated -= HandleActivated;
		}

		void HandleActivated (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (EventSink.ValueChanged);
		}

		#region ISliderBackend implementation

		public double Value {
			get {
				return Slider.DoubleValue;
			}
			set {
				Slider.DoubleValue = value;
			}
		}

		public double MinimumValue {
			get {
				return Slider.MinValue;
			}
			set {
				Slider.MinValue = value;
			}
		}

		public double MaximumValue {
			get {
				return Slider.MaxValue;
			}
			set {
				Slider.MaxValue = value;
			}
		}

		public double StepIncrement {
			get {
				return Slider.StepIncrement;
			}
			set {
				Slider.StepIncrement = value;
			}
		}

		public bool SnapToTicks {
			get {
				return Slider.AllowsTickMarkValuesOnly;
			}
			set {
				Slider.AllowsTickMarkValuesOnly = value;
			}
		}

		public double SliderPosition {
			get {
				double prct = 0;
				if (MinimumValue >= 0) {
					prct = (Value / (MaximumValue - MinimumValue));
				} else if (MaximumValue <= 0) {
					prct = (Math.Abs (Value) / Math.Abs (MinimumValue - MaximumValue));
				} else if (MinimumValue < 0) {
					if (Value >= 0)
						prct = 0.5 + ((Value / 2) / MaximumValue);
					else
						prct = 0.5 - Math.Abs ((Value / 2) / MinimumValue);
				}

				double tickStart, tickEnd;

				if (Slider.TickMarksCount > 1) {
					var rectTickStart = Slider.RectOfTick (0);
					var rectTickEnd = Slider.RectOfTick (Widget.TickMarksCount - 1);
					if (orientation == Orientation.Horizontal) {
						tickStart = Math.Min (rectTickStart.X, rectTickEnd.X);
						tickEnd = Math.Max (rectTickStart.X, rectTickEnd.X);
					} else {
						tickStart = Math.Min (rectTickStart.Y, rectTickEnd.Y);
						tickEnd = Math.Max (rectTickStart.Y, rectTickEnd.Y);
					}
				} else {
					double orientationSize = 0;
					if (orientation == Orientation.Horizontal)
						orientationSize = Frontend.Size.Width;
					else 
						orientationSize = Frontend.Size.Height;
					tickStart = (Widget.KnobThickness / 2) + 1;
					tickEnd = orientationSize - tickStart;
				}

				if (orientation == Orientation.Vertical)
					prct = 1 - prct;
				return ((tickEnd - tickStart) * prct) + (tickStart);
			}
		}
		#endregion
	}


	public class MacSlider: NSSlider, IViewObject
	{
		public MacSlider()
		{
			Cell = new MacSliderCell ();
		}

		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }

		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
			if (Backend.Cursor != null)
				AddCursorRect (Bounds, Backend.Cursor);
		}

		public double StepIncrement {
			get {
				return ((MacSliderCell)Cell).StepIncrement;
			}
			set {
				((MacSliderCell)Cell).StepIncrement = value;
			}
		}
	}

	public class MacSliderCell : NSSliderCell
	{
		readonly List<double> Ticks = new List<double>();

		public override double MaxValue {
			get {
				return base.MaxValue;
			}
			set {
				base.MaxValue = value;
				UpdateTicks ();
			}
		}

		public override double MinValue {
			get {
				return base.MinValue;
			}
			set {
				base.MinValue = value;
				UpdateTicks ();
			}
		}

		double stepIncrement;
		public double StepIncrement {
			get {
				return stepIncrement;
			}
			set {
				stepIncrement = value;
				UpdateTicks ();
			}
		}

		public override bool AllowsTickMarkValuesOnly {
			get {
				return base.AllowsTickMarkValuesOnly;
			}
			set {
				base.AllowsTickMarkValuesOnly = value;
				UpdateTicks ();
			}
		}


		void UpdateTicks()
		{
			Ticks.Clear ();
			if (AllowsTickMarkValuesOnly) {
				if (MinValue >= 0) {
					var ticksCount = (int)((MaxValue - MinValue) / StepIncrement) + 1;
					for (int i = 0; i < ticksCount; i++) {
						Ticks.Add (MinValue + (i * StepIncrement));
					}
				} else if (MaxValue <= 0) {
					var ticksCount = (int)((MaxValue - MinValue) / StepIncrement) + 1;
					for (int i = 0; i < ticksCount; i++) {
						Ticks.Add (-(i * StepIncrement));
					}
				} else if (MinValue < 0) {
					var ticksCountN = (int)(Math.Abs(MinValue) / StepIncrement);
					for (int i = ticksCountN; i >= 1; i--) {
						Ticks.Add (-(i * StepIncrement));
					}
					var ticksCount = (int)(MaxValue / StepIncrement) + 1;
					for (int i = 0; i < ticksCount; i++) {
						Ticks.Add (i * StepIncrement);
					}
				}
			}
			TickMarks = Ticks.Count;
		}

		public override double TickMarkValue (nint index)
		{
			return Ticks [(int)index];
		}
	}
}

