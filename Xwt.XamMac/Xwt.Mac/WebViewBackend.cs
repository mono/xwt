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
using Xwt.Backends;
using System.Linq;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.WebKit;
using WebKitView = MonoMac.WebKit.WebView;
#else
using Foundation;
using AppKit;
using WebKit;
using WebKitView = WebKit.WebView;
#endif

namespace Xwt.Mac
{
	public class WebViewBackend : ViewBackend<WebKitView, IWebViewEventSink>, IWebViewBackend
	{
		DomElement customCssNode;
		string customCss;

		public WebViewBackend ()
		{
		}

		internal WebViewBackend (MacWebView macweb)
		{
			ViewObject = macweb;
		}

		#region IWebViewBackend implementation
		public override void Initialize()
		{
			base.Initialize ();
			ViewObject = new MacWebView {
				UIDelegate = new XwtWebUIDelegate (this)
			};
		}

		public string Url {
			get { return Widget.MainFrameUrl; }
			set {
				Widget.MainFrameUrl = value;
			}
		}

		public string Title {
			get {
				return Widget.MainFrameTitle;
			}
		}

		public double LoadProgress { 
			get {
				return Widget.EstimatedProgress;
			}
		}

		public bool CanGoBack {
			get {
				return Widget.CanGoBack ();
			}
		}

		public bool CanGoForward {
			get {
				return Widget.CanGoForward ();
			}
		}

		public bool ContextMenuEnabled { get; set; }

		public bool DrawsBackground {
			get {
				return Widget.DrawsBackground;
			}
			set {
				Widget.DrawsBackground = value;
			}
		}

		public bool ScrollBarsEnabled {
			get {
				return Widget.MainFrame.FrameView.AllowsScrolling;
			}
			set {
				Widget.MainFrame.FrameView.AllowsScrolling = value;
			}
		}

		public string CustomCss {
			get {
				return customCss;
			}
			set {
				if (customCss != value) {
					if (string.IsNullOrEmpty (customCss) && !string.IsNullOrEmpty (value))
						Widget.FinishedLoad += HandleFinishedLoadForCss;
					else if (string.IsNullOrEmpty (value))
						Widget.FinishedLoad -= HandleFinishedLoadForCss;
					customCss = value;
					SetCustomCss ();
				}
			}
		}

		void HandleFinishedLoadForCss (object sender, WebFrameEventArgs e)
		{
			SetCustomCss ();
		}

		public void GoBack ()
		{
			Widget.GoBack ();
		}

		public void GoForward ()
		{
			Widget.GoForward ();
		}

		public void Reload ()
		{
			Widget.MainFrame.Reload ();
		}

		public void StopLoading ()
		{
			Widget.MainFrame.StopLoading ();
		}

		public void LoadHtml (string content, string base_uri)
		{
			Widget.MainFrame.LoadHtmlString (content, new NSUrl(base_uri));
		}

		protected new IWebViewEventSink EventSink {
			get { return (IWebViewEventSink)base.EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is WebViewEvent) {
				switch ((WebViewEvent)eventId) {
					case WebViewEvent.NavigateToUrl: Widget.StartedProvisionalLoad += HandleStartedProvisionalLoad; break;
					case WebViewEvent.Loading: Widget.CommitedLoad += HandleLoadStarted; break;
					case WebViewEvent.Loaded: Widget.FinishedLoad += HandleLoadFinished; break;
					case WebViewEvent.TitleChanged: Widget.ReceivedTitle += HandleTitleChanged; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is WebViewEvent) {
				switch ((WebViewEvent)eventId) {
					case WebViewEvent.NavigateToUrl: Widget.StartedProvisionalLoad -= HandleStartedProvisionalLoad; break;
					case WebViewEvent.Loading: Widget.CommitedLoad -= HandleLoadStarted; break;
					case WebViewEvent.Loaded: Widget.FinishedLoad -= HandleLoadFinished; break;
					case WebViewEvent.TitleChanged: Widget.ReceivedTitle -= HandleTitleChanged; break;
				}
			}
		}

		void HandleStartedProvisionalLoad (object sender, WebFrameEventArgs e)
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
			SetCustomCss ();
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnLoaded ();
			});
		}

		void HandleTitleChanged (object sender, WebFrameTitleEventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnTitleChanged ();
			});
		}
		#endregion

		void SetCustomCss ()
		{
			var mainDocument = Widget.MainFrameDocument ?? Widget.MainFrame.DomDocument;
			var head = mainDocument?.DocumentElement?.GetElementsByTagName ("head")? [0];

			if (head == null) {
				customCssNode = null;
				return;
			}

			// reuse node reference only if the document did not change and still contains the injected node
			if (customCssNode != null && !head.ChildNodes.Contains (customCssNode))
				customCssNode = null;

			if (!string.IsNullOrEmpty (CustomCss)) {
				if (customCssNode == null) {
					customCssNode = mainDocument.CreateElement ("style");
					customCssNode.SetAttribute ("type", "text/css");
					if (head.ChildNodes.Count > 0)
						head.InsertBefore (customCssNode, head.FirstChild);
					else
						head.AppendChild (customCssNode);
				}
				if (customCssNode.FirstChild != null)
					customCssNode.ReplaceChild (mainDocument.CreateTextNode (customCss), customCssNode.FirstChild);
				else
					customCssNode.AppendChild (mainDocument.CreateTextNode (customCss));
			} else if (customCssNode != null) {
				if (head.ChildNodes.Contains (customCssNode) == true)
					head.RemoveChild (customCssNode);
				customCssNode.Dispose ();
				customCssNode = null;
			}
		}
	}

	class MacWebView : WebKitView, IViewObject
	{
		public ViewBackend Backend { get; set; }

		public NSView View {
			get { return this; }
		}
	}

	class XwtWebUIDelegate : WebUIDelegate
	{
		readonly WebViewBackend backend;

		public XwtWebUIDelegate (WebViewBackend backend)
		{
			this.backend = backend;
		}

		public override NSMenuItem [] UIGetContextMenuItems (WebKitView sender, NSDictionary forElement, NSMenuItem [] defaultMenuItems)
		{
			if (backend.ContextMenuEnabled)
				return defaultMenuItems;
			return null;
		}
	}
}
