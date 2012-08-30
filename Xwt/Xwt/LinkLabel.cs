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

		public void SetHandled ()
		{
			Handled = true;
		}
	}

	public class LinkLabel: Label
	{
		protected new class WidgetBackendHost : Label.WidgetBackendHost, ILinkLabelEventSink
		{
			public void OnNavigateToUrl ()
			{
				((LinkLabel) Parent).OnNavigateToUrl (new NavigateToUrlEventArgs ());
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
			set { Backend.Uri = value; }
		}

		static LinkLabel ()
		{
			MapEvent (LinkLabelEvent.NavigateToUrl, typeof (LinkLabel), "OnNavigateToUrl");
		}

		public LinkLabel ()
			: this ("")
		{
		}

		public LinkLabel (string text) : base (text)
		{
			NavigateToUrl += delegate { };
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		protected virtual void OnNavigateToUrl (NavigateToUrlEventArgs e)
		{
			if (navigateToUrl != null)
				navigateToUrl (this, e);

			if (!e.Handled) {
				System.Diagnostics.Process.Start (Uri.ToString ());
				e.SetHandled ();
			}
		}
	}
}

