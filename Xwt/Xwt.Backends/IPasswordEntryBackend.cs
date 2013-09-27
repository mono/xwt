using Xwt.Backends;

namespace Xwt.Backends
{
	public interface IPasswordEntryBackend : IWidgetBackend
	{
		string Password { get; set; }
		System.Security.SecureString SecurePassword { get; }
		string PlaceholderText { get; set; }
	}

	public interface IPasswordEntryEventSink : IWidgetEventSink
	{
		void OnChanged ();
		void OnActivated ();
	}

	public enum PasswordEntryEvent
	{
		Changed,
		Activated
	}
}
