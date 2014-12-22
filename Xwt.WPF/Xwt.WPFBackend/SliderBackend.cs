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
		#region ISliderBackend Members

		public void Initialize (Orientation dir)
		{
			Widget = new System.Windows.Controls.Slider () {
				Orientation = dir == Orientation.Horizontal ? SWC.Orientation.Horizontal : SWC.Orientation.Vertical
			};
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
				Slider.ValueChanged += ValueChanged;
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is SliderEvent)
				Slider.ValueChanged -= ValueChanged;
		}

		void ValueChanged (object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
		{
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
			}
		}

		public double StepIncrement { get; set; }

		public bool SnapToTicks { get; set; }

		public double SliderPosition { get { return 0; } }

		#endregion
	}
}
