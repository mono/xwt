using System;
using Xwt.Backends;

namespace Xwt
{
	public enum DatePickerStyle {
		Time,
		Date,
		DateTime
	}
	
	public class DatePicker : Widget
	{
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IDatePickerEventSink
		{
			public void ValueChanged ()
			{
				((DatePicker)Parent).OnValueChanged (EventArgs.Empty);
			}
		}
		
		IDatePickerBackend Backend {
			get { return (IDatePickerBackend) BackendHost.Backend; }
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		public DateTime DateTime {
			get {
				return Backend.DateTime;
			}
			set {
				Backend.DateTime = value;
			}
		}
		
		public DatePickerStyle Style {
			get;
			set;
		}
		
		protected virtual void OnValueChanged (EventArgs e)
		{
			if (valueChanged != null)
				valueChanged (this, e);
		}

		EventHandler valueChanged;
		
		public event EventHandler ValueChanged {
			add {
				BackendHost.OnBeforeEventAdd (DatePickerEvent.ValueChanged, valueChanged);
				valueChanged += value;
			}
			remove {
				valueChanged -= value;
				BackendHost.OnAfterEventRemove (DatePickerEvent.ValueChanged, valueChanged);
			}
		}
	}
}

