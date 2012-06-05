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

