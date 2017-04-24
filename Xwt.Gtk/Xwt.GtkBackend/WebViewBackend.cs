//
// WebViewBackend.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using System.Runtime.InteropServices;
using Xwt.GtkBackend.WebKit;

namespace Xwt.GtkBackend
{
	public class WebViewBackend : WidgetBackend, IWebViewBackend
	{
		WebKit.WebView view;

		public WebViewBackend ()
		{
		}

		public override void Initialize()
		{
			NeedsEventBox = false;
			base.Initialize ();

			view = new WebKit.WebView ();
			view.ContextMenu += HandleContextMenuRequest;
			Widget = view;
			Widget.Show ();
		}

		public string Url {
			get { return view.Uri; }
			set {
				view.LoadUri (value);
			}
		}

		public string Title {
			get {
				return view.Title;
			}
		}

		public double LoadProgress {
			get {
				return view.LoadProgress;
			}
		}

		public bool CanGoBack {
			get {
				return view.CanGoBack ();
			}
		}

		public bool CanGoForward {
			get {
				return view.CanGoForward ();
			}
		}

		public bool ContextMenuEnabled { get; set; }

		public bool DrawsBackground {
			get {
				return !view.Transparent;
			}
			set {
				view.Transparent = !value;
			}
		}

		public bool ScrollBarsEnabled {
			get {
				return view.SelfScrolling;
			}
			set {
				view.SelfScrolling = value;
			}
		}

		public string CustomCss { get; set; }

		public void GoBack ()
		{
			view.GoBack ();
		}

		public void GoForward ()
		{
			view.GoForward ();
		}

		public void Reload ()
		{
			view.Reload ();
		}

		public void StopLoading ()
		{
			view.StopLoading ();
		}

		public void LoadHtml (string content, string base_uri)
		{
			view.LoadHtmlString (content, base_uri);
		}

		protected new IWebViewEventSink EventSink {
			get { return (IWebViewEventSink)base.EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is WebViewEvent) {
				switch ((WebViewEvent)eventId) {
					case WebViewEvent.NavigateToUrl: view.NavigationRequested += HandleNavigationRequested; break;
					case WebViewEvent.Loading: view.LoadStarted += HandleLoadStarted; break;
					case WebViewEvent.Loaded: view.LoadFinished += HandleLoadFinished; break;
					case WebViewEvent.TitleChanged: view.TitleChanged += HandleTitleChanged; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is WebViewEvent) {
				switch ((WebViewEvent)eventId) {
					case WebViewEvent.NavigateToUrl: view.NavigationRequested -= HandleNavigationRequested; break;
					case WebViewEvent.Loading: view.LoadStarted -= HandleLoadStarted; break;
					case WebViewEvent.Loaded: view.LoadFinished -= HandleLoadFinished; break;
					case WebViewEvent.TitleChanged: view.TitleChanged -= HandleTitleChanged; break;
				}
			}
		}

		void HandleNavigationRequested (object sender, WebKit.NavigationRequestedArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				if (EventSink.OnNavigateToUrl (e.Request.Uri))
					e.RetVal = NavigationResponse.Ignore;
			});
		}

		void HandleLoadStarted (object o, EventArgs args)
		{
			ApplicationContext.InvokeUserCode (EventSink.OnLoading);
		}

		void HandleLoadFinished (object o, EventArgs args)
		{
			ApplicationContext.InvokeUserCode (EventSink.OnLoaded);
		}

		void HandleTitleChanged (object sender, WebKit.TitleChangedArgs e)
		{
			ApplicationContext.InvokeUserCode (EventSink.OnTitleChanged);
		}

		void HandleContextMenuRequest (object sender, ContextMenuArgs e)
		{
			e.RetVal = !ContextMenuEnabled;
		}
	}
}

