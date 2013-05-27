using System;

namespace Xwt.Backends
{
	public interface IExpanderBackend: IWidgetBackend, IChildPlacementHandler
	{
		string Label { get; set; }
		bool Expanded { get; set; }
		void SetContent (IWidgetBackend child);
	}

	public interface IExpandEventSink: IWidgetEventSink
	{
		void ExpandChanged ();
	}

	public enum ExpandEvent {
		ExpandChanged
	}
}

