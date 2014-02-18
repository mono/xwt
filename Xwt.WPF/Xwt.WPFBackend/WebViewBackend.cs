//
// WebViewBackend.cs
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
using System.Windows;
using SWC = System.Windows.Controls;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class WebViewBackend : WidgetBackend, IWebViewBackend
	{
		string url;

		public WebViewBackend ()
		{
			Widget = new SWC.WebBrowser ();
		}

		internal WebViewBackend (SWC.WebBrowser browser)
		{
			Widget = browser;
		}

		public string Url {
			get { return url; }
			set {
				url = value;
				((SWC.WebBrowser)Widget).Navigate (url);
			}
		}

		public void LoadHtmlString (string html)
		{
			((SWC.WebBrowser)Widget).NavigateToString (html);
		}

		internal void OnLoadCompleted (object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			var url = e.Uri.AbsoluteUri;
			Context.InvokeUserCode (delegate {
				EventSink.OnNavigatedToUrl (url);
			});
		}

		internal void OnNavigating (object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			var url = e.Uri.AbsoluteUri;
			Context.InvokeUserCode (delegate {
				EventSink.OnNavigatingToUrl (url);
			});
		}

		protected new IWebViewEventSink EventSink {
			get { return (IWebViewEventSink)base.EventSink; }
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);

			if (eventId is WebViewEvent)
			{
				switch ((WebViewEvent)eventId)
				{
				case WebViewEvent.NavigatedToUrl:
					((SWC.WebBrowser)Widget).LoadCompleted -= OnLoadCompleted;
					break;
				case WebViewEvent.NavigatingToUrl:
					((SWC.WebBrowser)Widget).Navigating -= OnNavigating;
					break;
				}
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);

			if (eventId is WebViewEvent)
			{
				switch ((WebViewEvent)eventId)
				{
				case WebViewEvent.NavigatedToUrl:
					((SWC.WebBrowser)Widget).LoadCompleted += OnLoadCompleted;
					break;
				case WebViewEvent.NavigatingToUrl:
					((SWC.WebBrowser)Widget).Navigating += OnNavigating;
					break;
				}
			}
		}
	}
}

