using System;
using Xwt.Backends;

namespace Xwt
{
	public class SpinButton : Widget
	{
		ButtonStyle style;

		protected new class WidgetBackendHost: Widget.WidgetBackendHost, ISpinButtonEventSink
		{
			public void ValueChanged ()
			{
				((SpinButton)Parent).OnValueChanged (EventArgs.Empty);
			}
		}
		
		ISpinButtonBackend Backend {
			get { return (ISpinButtonBackend) BackendHost.Backend; }
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		public double ClimbRate {
			get { return Backend.ClimbRate; }
			set { Backend.ClimbRate = value; }
		}

		public int Digits {
			get { return Backend.Digits; }
			set { Backend.Digits = value; }
		}

		public double Value {
			get { return Backend.Value; }
			set { Backend.Value = value; }
		}

		public bool Wrap {
			get { return Backend.Wrap; }
			set { Backend.Wrap = value; }
		}

		public double MinimumValue {
			get { return Backend.MinimumValue; }
			set { Backend.MinimumValue = value; }
		}

		public double MaximumValue {
			get { return Backend.MaximumValue; }
			set { Backend.MaximumValue = value; }
		}

		public double IncrementValue {
			get { return Backend.IncrementValue; }
			set { Backend.IncrementValue = value; }
		}

		public ButtonStyle Style {
			get { return style; }
			set {
				style = value;
				Backend.SetButtonStyle (style);
				OnPreferredSizeChanged ();
			}
		}
		
		protected virtual void OnValueChanged (EventArgs e)
		{
			if (valueChanged != null)
				valueChanged (this, e);
		}

		EventHandler valueChanged;
		
		public event EventHandler ValueChanged {
			add {
				BackendHost.OnBeforeEventAdd (SpinButtonEvent.ValueChanged, valueChanged);
				valueChanged += value;
			}
			remove {
				valueChanged -= value;
				BackendHost.OnAfterEventRemove (SpinButtonEvent.ValueChanged, valueChanged);
			}
		}
	}
}

