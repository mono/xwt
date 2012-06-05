using System;
namespace Xwt.Backends
{
	public interface IDatePickerBackend : IWidgetBackend
	{
		DateTime DateTime { get; set; }
	}

	public interface IDatePickerEventSink: IWidgetEventSink
	{
		void ValueChanged ();
	}
	
	public enum DatePickerEvent
	{
		ValueChanged
	}
}

