using System;
namespace Xwt.Backends
{
	public interface ISpinButtonBackend : IWidgetBackend
	{
		double ClimbRate { get; set; }
		int Digits { get; set; }
		double Value { get; set; }
		bool Wrap { get; set; }
		double MinimumValue { get; set; }
		double MaximumValue { get; set; }
		double IncrementValue { get; set; }
		void SetButtonStyle (ButtonStyle style);
	}

	public interface ISpinButtonEventSink: IWidgetEventSink
	{
		void ValueChanged ();
	}
	
	public enum SpinButtonEvent
	{
		ValueChanged
	}
}

