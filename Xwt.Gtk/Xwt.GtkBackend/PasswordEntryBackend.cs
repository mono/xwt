using Xwt.Backends;
using System;

namespace Xwt.GtkBackend
{
	public class PasswordEntryBackend : WidgetBackend, IPasswordEntryBackend
	{
		public override void Initialize ()
		{
			Widget = new Gtk.Entry ();
			Widget.Show ();
			Widget.Visibility = false;
		}

		protected new Gtk.Entry Widget {
			get { return (Gtk.Entry)base.Widget; }
			set { base.Widget = value; }
		}

		protected new ITextEntryEventSink EventSink {
			get { return (ITextEntryEventSink)base.EventSink; }
		}

		public string Password {
			get {
				return Widget.Text;
			}
			set {
				Widget.Text = value;
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
					case TextEntryEvent.Changed: Widget.Changed += HandleChanged; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
					case TextEntryEvent.Changed: Widget.Changed -= HandleChanged; break;
				}
			}
		}

		void HandleChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnChanged ();
			});
		}
	}
}
