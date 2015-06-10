//
// MacWebView.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using Xwt.GtkBackend;
using Xwt.Backends;
using Foundation;

namespace Xwt.Gtk.Mac
{
	public class WebViewBackend : WidgetBackend, IWebViewBackend
	{
		WebKit.WebView view;

		public WebViewBackend ()
		{
		}

		#region IWebViewBackend implementation
		public override void Initialize()
		{
			base.Initialize ();

			view = new WebKit.WebView ();
			Widget = GtkMacInterop.NSViewToGtkWidget (view);
			Widget.Show ();
		}

		public string Url {
			get { return view.MainFrameUrl; }
			set {
				view.MainFrameUrl = value;
			}
		}

		public string Title {
			get {
				return view.MainFrameTitle;
			}
		}

		public double LoadProgress { 
			get {
				return view.EstimatedProgress;
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
			view.MainFrame.Reload ();
		}

		public void StopLoading ()
		{
			view.MainFrame.StopLoading ();
		}

		public void LoadHtml (string content, string base_uri)
		{
			view.MainFrame.LoadHtmlString (content, new NSUrl(base_uri));
		}

		protected new IWebViewEventSink EventSink {
			get { return (IWebViewEventSink)base.EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is WebViewEvent) {
				switch ((WebViewEvent)eventId) {
					case WebViewEvent.NavigateToUrl: view.StartedProvisionalLoad += HandleStartedProvisionalLoad; break;
					case WebViewEvent.Loading: view.CommitedLoad += HandleLoadStarted; break;
					case WebViewEvent.Loaded: view.FinishedLoad += HandleLoadFinished; break;
					case WebViewEvent.TitleChanged: view.ReceivedTitle += HandleTitleChanged; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is WebViewEvent) {
				switch ((WebViewEvent)eventId) {
					case WebViewEvent.NavigateToUrl: view.StartedProvisionalLoad -= HandleStartedProvisionalLoad; break;
					case WebViewEvent.Loading: view.CommitedLoad += HandleLoadStarted; break;
					case WebViewEvent.Loaded: view.FinishedLoad -= HandleLoadFinished; break;
					case WebViewEvent.TitleChanged: view.ReceivedTitle -= HandleTitleChanged; break;
				}
			}
		}

		void HandleStartedProvisionalLoad (object sender, WebKit.WebFrameEventArgs e)
		{
			var url = String.Empty;
			if (e.ForFrame.ProvisionalDataSource.Request.MainDocumentURL != null)
				url = e.ForFrame.ProvisionalDataSource.Request.MainDocumentURL.ToString ();
			if (String.IsNullOrEmpty (url))
				return;

			bool cancel = false;
			ApplicationContext.InvokeUserCode (delegate {
				cancel = EventSink.OnNavigateToUrl(url);
			});
			if (cancel)
				e.ForFrame.StopLoading ();
		}

		void HandleLoadStarted (object o, EventArgs args)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnLoading ();
			});
		}

		void HandleLoadFinished (object o, EventArgs args)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnLoaded ();
			});
		}

		void HandleTitleChanged (object sender, WebKit.WebFrameTitleEventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnTitleChanged ();
			});
		}
		#endregion
	}
}

