//
// SliderBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2013 Xamarin, Inc.
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

namespace Xwt.GtkBackend
{
	public class SliderBackend : WidgetBackend, ISliderBackend
	{
		bool onValueChangedEnabled;
		
		public void Initialize (Orientation dir)
		{
			if (dir == Orientation.Horizontal)
				Widget = new Gtk.HScale (0, 1.0, 1);
			else
				Widget = new Gtk.VScale (0, 1.0, 1) { Inverted = true };

			Widget.DrawValue = false;
			Widget.ValueChanged += HandleValueChanged;
			Widget.Show ();
		}
		
		protected new Gtk.Scale Widget {
			get { return (Gtk.Scale)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new ISliderEventSink EventSink {
			get { return (ISliderEventSink)base.EventSink; }
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is SliderEvent) {
				if ((SliderEvent)eventId == SliderEvent.ValueChanged)
					onValueChangedEnabled = true;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is SliderEvent) {
				if ((SliderEvent)eventId == SliderEvent.ValueChanged)
					onValueChangedEnabled = false;
			}
		}
		
		void HandleValueChanged (object sender, EventArgs e)
		{
			if (SnapToTicks && Math.Abs (StepIncrement) > double.Epsilon)
			{
				var offset = Math.Abs (Value) % StepIncrement;
				if (Math.Abs (offset) > double.Epsilon) {
					if (offset > StepIncrement / 2) {
						if (Value >= 0)
							Value += -offset + StepIncrement;
						else
							Value += offset - StepIncrement;
					}
					else
						if (Value >= 0)
							Value -= offset;
						else
							Value += offset;
				}
			}

			if (onValueChangedEnabled)
				ApplicationContext.InvokeUserCode (EventSink.ValueChanged);
		}

		public double Value {
			get { return Widget.Value; }
			set { Widget.Value = value; }
		}

		public double MaximumValue {
			get { return Widget.Adjustment.Upper; }
			set {
				Widget.SetRange (Math.Min (value - 1, MinimumValue), value);
				UpdateMarks ();
			}
		}

		public double MinimumValue {
			get { return Widget.Adjustment.Lower; }
			set {
				Widget.SetRange (value, Math.Max (value + 1, MaximumValue));
				UpdateMarks ();
			}
		}

		public double StepIncrement {
			get { return Widget.Adjustment.StepIncrement; }
			set { 
				Widget.Adjustment.StepIncrement = Widget.Adjustment.PageIncrement = value;

				if (Math.Abs (value) >= 1.0 || Math.Abs (value) < double.Epsilon)
					Widget.Digits = 0;
				else
				{
					Widget.Digits = Math.Abs ((int) Math.Floor (Math.Log10 (Math.Abs (value))));
					if (Widget.Digits > 5)
						Widget.Digits = 5;
				}
				UpdateMarks ();
			}
		}

		bool snapToTicks;
		public bool SnapToTicks {
			get {
				return snapToTicks;
			}
			set {
				snapToTicks = value;
				UpdateMarks ();
			}
		}

		public double SliderPosition {
			get {
				return Widget.GetSliderPosition ();
			}
		}

		void UpdateMarks ()
		{
			#if XWT_GTK3
			Widget.ClearMarks ();
			if (SnapToTicks) {
				if (MinimumValue >= 0) {
					var ticksCount = (int)((MaximumValue - MinimumValue) / StepIncrement) + 1;
					for (int i = 0; i < ticksCount; i++) {
						Widget.AddMark (MinimumValue + (i * StepIncrement), Gtk.PositionType.Bottom, null);
					}
				} else if (MaximumValue <= 0) {
					var ticksCount = (int)((MaximumValue - MinimumValue) / StepIncrement) + 1;
					for (int i = 0; i < ticksCount; i++) {
						Widget.AddMark (-(i * StepIncrement), Gtk.PositionType.Bottom, null);
					}
				} else if (MinimumValue < 0) {
					var ticksCount = (int)(MaximumValue / StepIncrement) + 1;
					for (int i = 0; i < ticksCount; i++) {
						Widget.AddMark (i * StepIncrement, Gtk.PositionType.Bottom, null);
					}
					var ticksCountN = (int)(Math.Abs(MinimumValue) / StepIncrement) + 1;
					for (int i = 1; i < ticksCountN; i++) {
						Widget.AddMark (-(i * StepIncrement), Gtk.PositionType.Bottom, null);
					}
				}
			}
			#endif
		}
	}
}

