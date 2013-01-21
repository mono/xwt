using System;
using Xwt.Backends;
using System.ComponentModel;
using Xwt.Drawing;

namespace Xwt
{
	[BackendType (typeof(IExpanderBackend))]
	public class Expander: Widget
	{
		EventHandler expandChanged;
		Xwt.Widget child;

		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IExpandEventSink
		{
			public void ExpandChanged ()
			{
				((Expander)Parent).OnExpandChanged (EventArgs.Empty);
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IExpanderBackend Backend {
			get { return (IExpanderBackend)BackendHost.Backend; }
		}

		public Expander ()
		{
		}

		public string Label {
			get {
				return Backend.Label;
			}
			set {
				Backend.Label = value;
			}
		}

		public bool Expanded {
			get {
				return Backend.Expanded;
			}
			set {
				Backend.Expanded = value;
			}
		}

		public new Widget Content {
			get { return child; }
			set {
				if (child != null)
					UnregisterChild (child);
				child = value;
				if (child != null)
					RegisterChild (child);
				Backend.SetContent ((IWidgetBackend)GetBackend (child));
				OnPreferredSizeChanged ();
			}
		}

		protected void OnExpandChanged (EventArgs args)
		{
			if (expandChanged != null)
				expandChanged (this, args);
		}

		public event EventHandler ExpandChanged {
			add {
				BackendHost.OnBeforeEventAdd (ExpandEvent.ExpandChanged, expandChanged);
				expandChanged += value;
			}
			remove {
				expandChanged -= value;
				BackendHost.OnAfterEventRemove (ExpandEvent.ExpandChanged, expandChanged);
			}
		}
	}
}

