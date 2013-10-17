using System;
using Xwt.Backends;
using MonoMac.AppKit;

namespace Xwt.Mac
{
	public class PasswordEntryBackend : ViewBackend<NSView, IPasswordEntryEventSink>, IPasswordEntryBackend
	{
		public PasswordEntryBackend ()
		{
		}

		public override void Initialize ()
		{
			base.Initialize ();
			ViewObject = new CustomAlignedContainer (new CustomSecureTextField (EventSink, ApplicationContext));
		}

		protected override void OnSizeToFit ()
		{
			Container.SizeToFit ();
		}

		CustomAlignedContainer Container {
			get { return (CustomAlignedContainer)base.Widget; }
		}

		public new NSSecureTextField Widget {
			get { return (NSSecureTextField)Container.Child; }
		}

		protected override Size GetNaturalSize ()
		{
			var s = base.GetNaturalSize ();
			return new Size (EventSink.GetDefaultNaturalSize ().Width, s.Height);
		}

		public string Password {
			get {
				return Widget.StringValue;
			}
			set {
				Widget.StringValue = value ?? string.Empty;
			}
		}

		public System.Security.SecureString SecurePassword {
			get {
				return null;
			}
		}

		public string PlaceholderText {
			get {
				return ((NSTextFieldCell)Widget.Cell).PlaceholderString;
			}
			set {
				((NSTextFieldCell)Widget.Cell).PlaceholderString = value ?? string.Empty;
			}
		}
	}

	class CustomSecureTextField : NSSecureTextField, IViewObject
	{
		IPasswordEntryEventSink eventSink;
		ApplicationContext context;

		public CustomSecureTextField (IPasswordEntryEventSink eventSink, ApplicationContext context)
		{
			this.context = context;
			this.eventSink = eventSink;
			Activated += (sender, e) => context.InvokeUserCode (delegate {
				eventSink.OnActivated ();
			});
		}

		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }

		public override void DidChange (MonoMac.Foundation.NSNotification notification)
		{
			base.DidChange (notification);
			context.InvokeUserCode (delegate {
				eventSink.OnChanged ();
			});
		}
	}
}

