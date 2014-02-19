//
// WebView.cs
//
// Author:
//       Cody Russell <cody@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc.
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
using Xwt.Drawing;
using Xwt.Backends;
using System.ComponentModel;

namespace Xwt
{
	[BackendType (typeof(IWebViewBackend))]
	public class WebView : Widget
	{
		EventHandler loaded;
		EventHandler navigatingToUrl;
		EventHandler navigatedToUrl;
		string url;

		protected new class WidgetBackendHost : Widget.WidgetBackendHost, IWebViewEventSink
		{
			public void OnLoaded ()
			{
				((WebView)Parent).OnLoaded (EventArgs.Empty);
			}

			public void OnNavigatingToUrl (string url)
			{
				((WebView)Parent).OnNavigatingToUrl (EventArgs.Empty);
			}

			public void OnNavigatedToUrl (string url)
			{
				((WebView)Parent).OnNavigatedToUrl (EventArgs.Empty);
			}
		}

		static WebView ()
		{
			MapEvent (WebViewEvent.Loaded, typeof(WebView), "OnLoaded");
			MapEvent (WebViewEvent.NavigatingToUrl, typeof(WebView), "OnNavigatingToUrl");
			MapEvent (WebViewEvent.NavigatedToUrl, typeof(WebView), "OnNavigatedToUrl");
		}

		public WebView ()
		{
		}

		public WebView (string url)
		{
			Url = url;
		}

		public void LoadHtmlString (string html)
		{
			Backend.LoadHtmlString (html);
		}

		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IWebViewBackend Backend {
			get { return (IWebViewBackend) BackendHost.Backend; }
		}

		[DefaultValue("")]
		public string Url {
			get { return url ?? ""; }
			set {
		 		url = value;
				Backend.Url = url;
			}
		}

		protected virtual void OnLoaded (EventArgs e)
		{
		}

		protected virtual void OnNavigatingToUrl (EventArgs e)
		{
			if (navigatingToUrl != null)
				navigatingToUrl (this, e);
		}

		protected virtual void OnNavigatedToUrl (EventArgs e)
		{
			if (navigatedToUrl != null)
				navigatedToUrl (this, e);
		}

		public event EventHandler Loaded {
			add {
				BackendHost.OnBeforeEventAdd (WebViewEvent.Loaded, loaded);
				loaded += value;
			}

			remove {
				loaded -= value;
				BackendHost.OnAfterEventRemove (WebViewEvent.Loaded, loaded);
			}
		}

		public event EventHandler NavigatingToUrl {
			add {
				BackendHost.OnBeforeEventAdd (WebViewEvent.NavigatingToUrl, navigatingToUrl);
				navigatingToUrl += value;
			}
			remove {
				navigatingToUrl -= value;
				BackendHost.OnAfterEventRemove (WebViewEvent.NavigatingToUrl, navigatingToUrl);
			}
		}

		public event EventHandler NavigatedToUrl {
			add {
				BackendHost.OnBeforeEventAdd (WebViewEvent.NavigatedToUrl, navigatedToUrl);
				navigatedToUrl += value;
			}
			remove {
				navigatedToUrl -= value;
				BackendHost.OnAfterEventRemove (WebViewEvent.NavigatedToUrl, navigatedToUrl);
			}
		}
	}
}

