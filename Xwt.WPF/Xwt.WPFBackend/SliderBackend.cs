using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.Backends;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	class SliderBackend: WidgetBackend, ISliderBackend
	{
		bool onValueChangedEnabled;

		#region ISliderBackend Members

		public void Initialize (Orientation dir)
		{
			Widget = new System.Windows.Controls.Slider () {
				Orientation = dir == Orientation.Horizontal ? SWC.Orientation.Horizontal : SWC.Orientation.Vertical,
			};
			Widget.MouseWheel += HandleMouseWheel;
			Slider.ValueChanged += ValueChanged;
		}

		void HandleMouseWheel (object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			if (e.Handled == true)
				return;

			int jumps = e.Delta / 120;

			if (jumps == 0)
				return;
			if (e.Delta < 0)
				Value -= Math.Abs(jumps) * StepIncrement;
			else if (e.Delta > 0)
				Value += jumps * StepIncrement;
		}

		protected SWC.Slider Slider
		{
			get { return (SWC.Slider)Widget; }
		}

		protected ISliderEventSink SliderEventSink
		{
			get { return (ISliderEventSink)EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is SliderEvent)
				onValueChangedEnabled = true;
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is SliderEvent)
				onValueChangedEnabled = false;
		}

		void ValueChanged (object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
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
				Context.InvokeUserCode (SliderEventSink.ValueChanged);
		}

		public double Value
		{
			get
			{
				return Slider.Value;
			}
			set
			{
				Slider.Value = value;
			}
		}

		public double MinimumValue
		{
			get
			{
				return Slider.Minimum;
			}
			set
			{
				Slider.Minimum = value;
				UpdateTicks ();
			}
		}

		public double MaximumValue
		{
			get
			{
				return Slider.Maximum;
			}
			set
			{
				Slider.Maximum = value;
				UpdateTicks ();
			}
		}

		double stepIncrement;
		public double StepIncrement {
			get { return stepIncrement; }
			set { 
				stepIncrement = value;
				UpdateTicks ();
			}
		}

		bool snapToTicks;
		public bool SnapToTicks {
			get { return snapToTicks; }
			set {
				snapToTicks = value;
				UpdateTicks ();
				if (value) {
					Slider.TickPlacement = SWC.Primitives.TickPlacement.BottomRight;
				} else {
					Slider.TickPlacement = SWC.Primitives.TickPlacement.None;
				}
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

				double orientationSize = 0;
				if (Slider.Orientation == SWC.Orientation.Horizontal)
					orientationSize = Frontend.Size.Width;
				else
					orientationSize = Frontend.Size.Height;

				if (Slider.Orientation == SWC.Orientation.Vertical)
					prct = 1 - prct;
				return (int)(((orientationSize - 16) * prct) + 8);
			}
		}

		void UpdateTicks()
		{

			Slider.Ticks.Clear ();
			if (SnapToTicks) {
				if (MinimumValue >= 0) {
					var ticksCount = (int)((MaximumValue - MinimumValue) / StepIncrement) + 1;
					for (int i = 0; i < ticksCount; i++) {
						Slider.Ticks.Add (MinimumValue + (i * StepIncrement));
					}
				} else if (MaximumValue <= 0) {
					var ticksCount = (int)((MaximumValue - MinimumValue) / StepIncrement) + 1;
					for (int i = 0; i < ticksCount; i++) {
						Slider.Ticks.Add (-(i * StepIncrement));
					}
				} else if (MinimumValue < 0) {
					var ticksCount = (int)(MaximumValue / StepIncrement) + 1;
					for (int i = 0; i < ticksCount; i++) {
						Slider.Ticks.Add (i * StepIncrement);
					}
					var ticksCountN = (int)(Math.Abs(MinimumValue) / StepIncrement) + 1;
					for (int i = 1; i < ticksCountN; i++) {
						Slider.Ticks.Add (-(i * StepIncrement));
					}
				}
			}
		}

		#endregion
	}
}
