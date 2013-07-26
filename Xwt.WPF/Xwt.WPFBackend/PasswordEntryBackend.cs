using System.Windows;
using System.Windows.Controls;

using Xwt.WPFBackend;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	class PasswordEntryBackend : WidgetBackend, IPasswordEntryBackend
	{
		public PasswordEntryBackend ()
		{
			Widget = new PasswordBox ();
		}

		protected PasswordBox PasswordBox
		{
			get { return (PasswordBox) Widget; }
		}

		public string Password
		{
			get { return PasswordBox.Password; }
			set { PasswordBox.Password = value; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);

			if (eventId is PasswordEntryEvent) 
			{
				switch ((PasswordEntryEvent) eventId) 
				{
					case PasswordEntryEvent.Changed:
						PasswordBox.PasswordChanged += OnPasswordChanged;
						break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);

			if (eventId is PasswordEntryEvent)
			{
				switch ((PasswordEntryEvent)eventId)
				{
					case PasswordEntryEvent.Changed:
						PasswordBox.PasswordChanged -= OnPasswordChanged;
						break;
				}
			}
		}

		protected new IPasswordEntryEventSink EventSink {
			get { return (IPasswordEntryEventSink) base.EventSink; }
		}

		void OnPasswordChanged (object s, RoutedEventArgs e)
		{
			Context.InvokeUserCode (EventSink.OnChanged);
		}
	}
}
