using System;
using Xwt.Backends;
using Xwt.Engine;

using Gtk;

namespace Xwt.GtkBackend
{
	public class SpinButtonBackend : WidgetBackend, ISpinButtonBackend
	{
		public SpinButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = (Gtk.SpinButton) CreateWidget ();
			Widget.Numeric = true;
			Widget.Alignment = 1.0f;
			Widget.Show ();
		}
		
		protected virtual Gtk.Widget CreateWidget ()
		{
			return new Gtk.SpinButton (0, 1, .1);
		}
		
		protected new Gtk.SpinButton Widget {
			get { return (Gtk.SpinButton)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new ISpinButtonEventSink EventSink {
			get { return (ISpinButtonEventSink)base.EventSink; }
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is SpinButtonEvent) {
				if ((SpinButtonEvent)eventId == SpinButtonEvent.ValueChanged)
					Widget.ValueChanged += HandleValueChanged;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is SpinButtonEvent) {
				if ((SpinButtonEvent)eventId == SpinButtonEvent.ValueChanged)
					Widget.ValueChanged -= HandleValueChanged;
			}
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			Toolkit.Invoke (delegate {
				EventSink.ValueChanged ();
			});
		}

		public double ClimbRate {
			get { return Widget.ClimbRate; }
			set { Widget.ClimbRate = value; }
		}

		public int Digits {
			get { return (int)Widget.Digits; }
			set { Widget.Digits = (uint)value; }
		}

		public double Value {
			get { return Widget.Value; }
			set { Widget.Value = value; }
		}

		public bool Wrap {
			get { return Widget.Wrap; }
			set { Widget.Wrap = value; }
		}

		public double MinimumValue {
			get { return Widget.Adjustment.Lower; }
			set { Widget.Adjustment.Lower = value; }
		}

		public double MaximumValue {
			get { return Widget.Adjustment.Upper; }
			set { Widget.Adjustment.Upper = value; }
		}

		public double IncrementValue {
			get { return Widget.Adjustment.StepIncrement; }
			set { Widget.Adjustment.StepIncrement = value; }
		}
	}
}

