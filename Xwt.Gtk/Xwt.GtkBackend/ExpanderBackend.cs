using System;

using Xwt;

using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class ExpanderBackend : WidgetBackend, IExpanderBackend
	{
		public ExpanderBackend ()
		{
			Widget = new Gtk.Expander (string.Empty);
			Widget.Show ();
		}

		protected new Gtk.Expander Widget {
			get { return (Gtk.Expander)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IExpandEventSink EventSink {
			get { return (IExpandEventSink)base.EventSink; }
		}

		public string Label {
			get {
				return Widget.Label;
			}
			set {
				Widget.Label = value;
			}
		}

		public bool Expanded {
			get {
				return Widget.Expanded;
			}
			set {
				Widget.Expanded = value;
			}
		}

		public void SetContent (IWidgetBackend child)
		{
			RemoveChildPlacement (Widget.Child);
			Widget.Child = GetWidgetWithPlacement (child);
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ExpandEvent) {
				if ((ExpandEvent)eventId == ExpandEvent.ExpandChanged)
					Widget.Activated += HandleExpandedChanged;
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ExpandEvent) {
				if ((ExpandEvent)eventId == ExpandEvent.ExpandChanged)
					Widget.Activated -= HandleExpandedChanged;
			}
		}

		void HandleExpandedChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (EventSink.ExpandChanged);
		}
	}
}

