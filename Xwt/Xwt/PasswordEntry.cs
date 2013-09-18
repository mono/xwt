using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xwt.Backends;

namespace Xwt
{
	[BackendType (typeof (IPasswordEntryBackend))]
	public class PasswordEntry : Widget
	{
		EventHandler changed;

		static PasswordEntry ()
		{
			MapEvent (PasswordEntryEvent.Changed, typeof (PasswordEntry), "OnChanged");
		}

		protected new class WidgetBackendHost : Widget.WidgetBackendHost, IPasswordEntryEventSink
		{
			public void OnChanged ()
			{
				((PasswordEntry) Parent).OnChanged (EventArgs.Empty);
			}

			public override Size GetDefaultNaturalSize ()
			{
				return DefaultNaturalSizes.PasswordEntry;
			}
		}

		public PasswordEntry ()
		{
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IPasswordEntryBackend Backend
		{
			get { return (IPasswordEntryBackend) BackendHost.Backend; }
		}

		[DefaultValue ("")]
		public string Password {
			get { return Backend.Password; }
			set { Backend.Password = value; }
		}

		public System.Security.SecureString SecurePassword {
			get { return Backend.SecurePassword; }
		}

		[DefaultValue ("")]
		public string PlaceholderText {
			get { return Backend.PlaceholderText; }
			set { Backend.PlaceholderText = value; }
		}

		protected virtual void OnChanged (EventArgs e)
		{
			if (changed != null)
				changed (this, e);
		}

		public event EventHandler Changed
		{
			add {
				BackendHost.OnBeforeEventAdd (PasswordEntryEvent.Changed, changed);
				changed += value;
			}
			remove {
				changed -= value;
				BackendHost.OnAfterEventRemove (PasswordEntryEvent.Changed, changed);
			}
		}
	}
}
