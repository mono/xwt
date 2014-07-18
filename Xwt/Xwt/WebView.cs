//
// WebView.cs
//
// Author:
//       Cody Russell <cody@xamarin.com>
//       Vsevolod Kukol <sevo@sevo.org>
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
		EventHandler loading;
		EventHandler loaded;
		EventHandler<NavigateToUrlEventArgs> navigateToUrl;
		EventHandler titleChanged;
		string url = String.Empty;

		protected new class WidgetBackendHost : Widget.WidgetBackendHost, IWebViewEventSink
		{
			public bool OnNavigateToUrl (string url)
			{
				var args = new NavigateToUrlEventArgs (new Uri(url, UriKind.RelativeOrAbsolute));
				((WebView)Parent).OnNavigateToUrl (args);
				return args.Handled;
			}

			public void OnLoading ()
			{
				((WebView)Parent).OnLoading (EventArgs.Empty);
			}

			public void OnLoaded ()
			{
				((WebView)Parent).OnLoaded (EventArgs.Empty);
			}

			public void OnTitleChanged ()
			{
				((WebView)Parent).OnTitleChanged (EventArgs.Empty);
			}
		}

		static WebView ()
		{
			MapEvent (WebViewEvent.Loading, typeof(WebView), "OnLoading");
			MapEvent (WebViewEvent.Loaded, typeof(WebView), "OnLoaded");
			MapEvent (WebViewEvent.NavigateToUrl, typeof(WebView), "OnNavigateToUrl");
			MapEvent (WebViewEvent.TitleChanged, typeof(WebView), "OnTitleChanged");
		}

		public WebView ()
		{
		}

		public WebView (string url)
		{
			Url = url;
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
			get { 
				if (!String.IsNullOrEmpty (Backend.Url))
					url = Backend.Url;
				return url;
			}
			set {
		 		url = value;
				Backend.Url = url;
			}
		}

		[DefaultValue("")]
		public string Title {
			get { return Backend.Title ?? ""; }
		}

		[DefaultValue(0.0)]
		public double LoadProgress {
			get { return Backend.LoadProgress; }
		}

		[DefaultValue(false)]
		public bool CanGoBack {
			get { return Backend.CanGoBack; }
		}

		[DefaultValue(false)]
		public bool CanGoForward {
			get { return Backend.CanGoForward; }
		}

		public void GoBack ()
		{
			Backend.GoBack ();
		}

		public void GoForward ()
		{
			Backend.GoForward ();
		}

		public void Reload ()
		{
			Backend.Reload ();
		}

		public void StopLoading ()
		{
			Backend.StopLoading ();
		}

		public void LoadHtml (string content, string base_uri)
		{
			Backend.LoadHtml (content, base_uri);
		}

		protected virtual void OnLoading (EventArgs e)
		{
			if (loading != null)
				loading (this, e);
		}

		public event EventHandler Loading {
			add {
				BackendHost.OnBeforeEventAdd (WebViewEvent.Loading, loading);
				loading += value;
			}

			remove {
				loading -= value;
				BackendHost.OnAfterEventRemove (WebViewEvent.Loading, loading);
			}
		}

		protected virtual void OnLoaded (EventArgs e)
		{
			if (loaded != null)
				loaded (this, e);
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

		protected virtual void OnNavigateToUrl (NavigateToUrlEventArgs e)
		{
			if (navigateToUrl != null)
				navigateToUrl (this, e);
		}

		public event EventHandler<NavigateToUrlEventArgs> NavigateToUrl {
			add {
				BackendHost.OnBeforeEventAdd (WebViewEvent.NavigateToUrl, navigateToUrl);
				navigateToUrl += value;
			}

			remove {
				navigateToUrl -= value;
				BackendHost.OnAfterEventRemove (WebViewEvent.NavigateToUrl, navigateToUrl);
			}
		}

		protected virtual void OnTitleChanged (EventArgs e)
		{
			if (titleChanged != null)
				titleChanged (this, e);
		}

		public event EventHandler TitleChanged {
			add {
				BackendHost.OnBeforeEventAdd (WebViewEvent.TitleChanged, titleChanged);
				titleChanged += value;
			}

			remove {
				titleChanged -= value;
				BackendHost.OnAfterEventRemove (WebViewEvent.TitleChanged, titleChanged);
			}
		}
	}
}

