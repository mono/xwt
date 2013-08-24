// 
// LinkLabel.cs
//  
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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

using System;
using Xwt.Backends;

namespace Xwt
{
	public class NavigateToUrlEventArgs : EventArgs
	{
		public bool Handled {
			get; private set;
		}

		public Uri Uri {
			get; private set;
		}

		public NavigateToUrlEventArgs (Uri uri)
		{
			Uri = uri;
		}

		public void SetHandled ()
		{
			Handled = true;
		}
	}

	[BackendType (typeof(ILinkLabelBackend))]
	public class LinkLabel: Label
	{
		protected new class WidgetBackendHost : Label.WidgetBackendHost, ILinkLabelEventSink
		{
			public void OnNavigateToUrl (Uri uri)
			{
				((LinkLabel) Parent).OnNavigateToUrl (new NavigateToUrlEventArgs (uri));
			}
		}

		EventHandler<NavigateToUrlEventArgs> navigateToUrl;
		public event EventHandler<NavigateToUrlEventArgs> NavigateToUrl {
			add {
				BackendHost.OnBeforeEventAdd (LinkLabelEvent.NavigateToUrl, navigateToUrl);
				navigateToUrl += value;
			}
			remove {
				navigateToUrl -= value;
				BackendHost.OnAfterEventRemove (LinkLabelEvent.NavigateToUrl, navigateToUrl);
			}
		}

		ILinkLabelBackend Backend {
			get { return (ILinkLabelBackend) BackendHost.Backend; }
		}

		public Uri Uri {
			get { return Backend.Uri; }
			set {
				Backend.Uri = value;
				if (value != null) {
					// add a dummy handler so the default action is enabled
					NavigateToUrl += DummyHandleNavigateToUrl;
				} else {
					NavigateToUrl -= DummyHandleNavigateToUrl;
				}
			}
		}

		static LinkLabel ()
		{
			MapEvent (LinkLabelEvent.NavigateToUrl, typeof (LinkLabel), "OnNavigateToUrl");
		}

		public LinkLabel ()
		{
		}

		public LinkLabel (string text)
		{
			VerifyConstructorCall (this);
			Text = text;
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		protected virtual void OnNavigateToUrl (NavigateToUrlEventArgs e)
		{
			if (navigateToUrl != null)
				navigateToUrl (this, e);

			if (!e.Handled && e.Uri != null) {
				Desktop.OpenUrl (e.Uri);
				e.SetHandled ();
			}
		}

		static void DummyHandleNavigateToUrl (object sender, NavigateToUrlEventArgs e)
		{
		}
	}
}

