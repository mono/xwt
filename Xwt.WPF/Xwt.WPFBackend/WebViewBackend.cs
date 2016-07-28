//
// WebViewBackend.cs
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
using System.Reflection;
using SWC = System.Windows.Controls;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class WebViewBackend : WidgetBackend, IWebViewBackend
	{
		string url;
		SWC.WebBrowser view;
		bool enableNavigatingEvent, enableLoadingEvent, enableLoadedEvent, enableTitleChangedEvent;

		static PropertyInfo titleProperty;
		static bool canGetDocumentTitle = true;

		static PropertyInfo silentProperty;
		static FieldInfo mshtmlBrowserField;
		static bool canDisableJsErrors = true;

		public WebViewBackend () : this (new SWC.WebBrowser ())
		{
		}

		internal WebViewBackend (SWC.WebBrowser browser)
		{
			view = browser;
			view.Navigating += HandleNavigating;
			view.Navigated += HandleNavigated;
			view.LoadCompleted += HandleLoadCompleted;
			Widget = view;
		}

		public string Url {
			get { return url; }
			set {
				url = value;
				view.Navigate (url);
			}
		}

		public double LoadProgress { get; protected set; }

		public bool CanGoBack {
			get {
				return view.CanGoBack;
			}
		}

		public bool CanGoForward {
			get {
				return view.CanGoForward;
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
			view.Refresh ();
		}

		public void StopLoading ()
		{
			view.InvokeScript ("eval", "document.execCommand('Stop');");
		}

		public void LoadHtml (string content, string base_uri)
		{
			view.NavigateToString (content);
			url = base_uri;
		}

		string prevTitle = String.Empty;

		public string Title
		{
			get
			{
				if (view.Document != null && titleProperty == null && canGetDocumentTitle)
				{
					var mshtmlDocType = view.Document.GetType();
					// Get the property with the document Title,
					// property name depends on .NET Version
					titleProperty = mshtmlDocType?.GetProperty("Title") ?? mshtmlDocType?.GetProperty("IHTMLDocument2_nameProp");
					canGetDocumentTitle = titleProperty == null;
				}

				string title = null;
				if (canGetDocumentTitle)
				{
					try
					{
						title = titleProperty.GetValue(view.Document, null) as string;
					}
					catch
					{
						canGetDocumentTitle = false;
					}
				}
				// try to get the title using a script, if reflection fails
				if (title == null)
				{
					#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
					try
					{
						title = (string)view.InvokeScript("eval", "document.title.toString()");
					}
					catch { }
					#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
				}
				return title;
			}
		}

		protected new IWebViewEventSink EventSink {
			get { return (IWebViewEventSink)base.EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is WebViewEvent) {
				switch ((WebViewEvent)eventId) {
				case WebViewEvent.NavigateToUrl:
					enableNavigatingEvent = true;
					break;
				case WebViewEvent.Loading:
					enableLoadingEvent = true;
					break;
				case WebViewEvent.Loaded:
					enableLoadedEvent = true;
					break;
				case WebViewEvent.TitleChanged:
					enableTitleChangedEvent = true;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is WebViewEvent) {
				switch ((WebViewEvent)eventId) {
				case WebViewEvent.NavigateToUrl:
					enableNavigatingEvent = false;
					break;
				case WebViewEvent.Loading:
					enableLoadingEvent = false;
					break;
				case WebViewEvent.Loaded:
					enableLoadedEvent = false;
					break;
				case WebViewEvent.TitleChanged:
					enableTitleChangedEvent = false;
					break;
				}
			}
		}

		void HandleNavigating (object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			if (enableNavigatingEvent) {
				var newurl = string.Empty;
				if (e.Uri != null)
					newurl = e.Uri.AbsoluteUri;
				Context.InvokeUserCode (delegate {
					e.Cancel = EventSink.OnNavigateToUrl (newurl);
				});
			}
		}

		void HandleLoadCompleted (object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			LoadProgress = 1;
			if (enableLoadedEvent)
				Context.InvokeUserCode (EventSink.OnLoaded);

			if (enableTitleChangedEvent && (prevTitle != Title))
				Context.InvokeUserCode (EventSink.OnTitleChanged);
			prevTitle = Title;
		}

		void HandleNavigated (object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			LoadProgress = 0;
			DisableJsErrors(view);
			if (e.Uri != null)
				this.url = e.Uri.AbsoluteUri;
			if (enableLoadingEvent)
				Context.InvokeUserCode (delegate {
					EventSink.OnLoading ();
				});
		}

		static void DisableJsErrors(SWC.WebBrowser browser)
		{
			try
			{
				if (silentProperty == null && canDisableJsErrors)
				{
					// get the MSHTML.IWebBrowser2 instance field
					mshtmlBrowserField = typeof(SWC.WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
					silentProperty = mshtmlBrowserField.FieldType.GetProperty("Silent");
					canDisableJsErrors = silentProperty != null;
				}
				if (canDisableJsErrors)
					silentProperty.SetValue(mshtmlBrowserField.GetValue(browser), true, null);
			}
			catch
			{
				canDisableJsErrors = false;
			}
		}
	}
}

