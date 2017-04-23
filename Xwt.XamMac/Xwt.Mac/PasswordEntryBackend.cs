//
// PasswordEntryBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
// 
// Copyright (c) 2013 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using AppKit;
using Foundation;
using Xwt.Backends;


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
			var view = new CustomSecureTextField (EventSink, ApplicationContext);
			ViewObject = new CustomAlignedContainer (EventSink, ApplicationContext, (NSView)view) { DrawsBackground = false };
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

		public override Drawing.Color BackgroundColor {
			get {
				return Widget.BackgroundColor.ToXwtColor ();
			}
			set {
				Widget.BackgroundColor = value.ToNSColor ();
				Widget.Cell.DrawsBackground = true;
				Widget.Cell.BackgroundColor = value.ToNSColor ();
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
			Activated += (sender, e) => context.InvokeUserCode (eventSink.OnActivated);
		}

		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }

		public override void DidChange (NSNotification notification)
		{
			base.DidChange (notification);
            context.InvokeUserCode (eventSink.OnChanged);
		}
	}
}

