using Xwt.Backends;

namespace Xwt.Backends
{
	public interface IPasswordEntryBackend : IWidgetBackend
	{
		string Password { get; set; }
	}

	public interface IPasswordEntryEventSink : IWidgetEventSink
	{
		void OnChanged ();
	}

	public enum PasswordEntryEvent
	{
		Changed
	}
}
