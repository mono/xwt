using Xwt.Backends;
using System;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public partial class PasswordEntryBackend : WidgetBackend, IPasswordEntryBackend
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

		protected new IPasswordEntryEventSink EventSink {
			get { return (IPasswordEntryEventSink)base.EventSink; }
		}

		public string Password {
			get { return Widget.Text; }
			set { Widget.Text = value ?? ""; } // null value causes GTK error
		}

		public System.Security.SecureString SecurePassword {
			get {
				var text = Widget.Text;
				unsafe {
					fixed (char *ptr = text) {
						return new System.Security.SecureString (ptr, text.Length);
					}
				}
			}
		}

		public override Color BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = value;
				Widget.ModifyBase (Gtk.StateType.Normal, value.ToGtkValue ());
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is PasswordEntryEvent) {
				switch ((PasswordEntryEvent)eventId) {
				case PasswordEntryEvent.Changed: Widget.Changed += HandleChanged; break;
				case PasswordEntryEvent.Activated: Widget.Activated += HandleActivated; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is PasswordEntryEvent) {
				switch ((PasswordEntryEvent)eventId) {
				case PasswordEntryEvent.Changed: Widget.Changed -= HandleChanged; break;
				case PasswordEntryEvent.Activated: Widget.Activated -= HandleActivated; break;
				}
			}
		}

		void HandleChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnChanged ();
			});
		}

		void HandleActivated (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnActivated ();
			});
		}
	}
}
