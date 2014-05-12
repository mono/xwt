using Xwt.Backends;
using System;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public class PasswordEntryBackend : WidgetBackend, IPasswordEntryBackend
	{
		#if !XWT_GTK3
		string placeHolderText;
		#endif

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

		#if XWT_GTK3
		public string PlaceholderText {
			get {
				using (GLib.Value property = Widget.GetProperty ("placeholder-text")) {
					string result = (string)property;
					return result;
				}
			}
			set {
				using (GLib.Value val = new GLib.Value (value)) {
					Widget.SetProperty ("placeholder-text", val);
				}
			}
		}
		#else
		public string PlaceholderText {
			get { return placeHolderText; }
			set {
				if (placeHolderText != value) {
					if (placeHolderText == null)
						Widget.ExposeEvent += HandleWidgetExposeEvent;
					else if (value == null)
						Widget.ExposeEvent -= HandleWidgetExposeEvent;
				}
				placeHolderText = value;
			}
		}
		#endif

		public override Color BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = value;
				Widget.ModifyBase (Gtk.StateType.Normal, value.ToGtkValue ());
			}
		}

		Pango.Layout layout;

		#if !XWT_GTK3
		void HandleWidgetExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			TextEntryBackend.RenderPlaceholderText (Widget, args, placeHolderText, ref layout);
		}
		#endif

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

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				var l = layout;
				if (l != null) {
					l.Dispose ();
					layout = null;
				}
			}
			base.Dispose (disposing);
		}
	}
}
