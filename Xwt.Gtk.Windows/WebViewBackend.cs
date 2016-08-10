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
using System.Linq;
using System.Runtime.InteropServices;
using SWF = System.Windows.Forms;
using Xwt.GtkBackend;
using Xwt.Backends;
using Gtk;

namespace Xwt.Gtk.Windows
{
	public class WebViewBackend : WidgetBackend, IWebViewBackend
	{
		SWF.WebBrowser view;
		string url;
		Socket socket;
		bool enableNavigatingEvent, enableLoadingEvent, enableLoadedEvent, enableTitleChangedEvent;
		SWF.HtmlElement customCssNode;
		string customCss;

		[DllImportAttribute("user32.dll", EntryPoint = "SetParent")]
		internal static extern System.IntPtr SetParent([InAttribute] System.IntPtr hwndChild, [InAttribute] System.IntPtr hwndNewParent);

		public override void Initialize ()
		{
			base.Initialize ();

			socket = new Socket ();
			Widget = socket;

			this.Widget.Realized += HandleGtkRealized;
			this.Widget.SizeAllocated += HandleGtkSizeAllocated;

            Widget.Show();
		}

		void HandleGtkRealized (object sender, EventArgs e)
		{
			var size = new System.Drawing.Size (Widget.WidthRequest, Widget.HeightRequest);

			view = new SWF.WebBrowser ();
			view.ScriptErrorsSuppressed = true;
			view.AllowWebBrowserDrop = false;
			view.Size = size;
			var browser_handle = view.Handle;
			IntPtr window_handle = (IntPtr)socket.Id;
			SetParent (browser_handle, window_handle);

			view.ProgressChanged += HandleProgressChanged;
			view.Navigating += HandleNavigating;
			view.Navigated += HandleNavigated;
			view.DocumentTitleChanged += HandleDocumentTitleChanged;
			view.DocumentCompleted += HandleDocumentCompleted;
			if (url != null)
				view.Navigate (url);
		}

		void HandleGtkSizeAllocated (object sender, SizeAllocatedArgs e)
		{
			var size = new System.Drawing.Size(e.Allocation.Width, e.Allocation.Height);
			view.Size = size;
		}

		public string Url {
			get {
				if (view != null && !String.IsNullOrEmpty(view.Url.AbsoluteUri))
					url = view.Url.AbsoluteUri;
				return url;
			}
			set {
				url = value;
				if (view != null)
					view.Navigate (url);
			}
		}

		double loadProgress;
		public double LoadProgress {
			get {
				return loadProgress;
			}
		}

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

		public bool ContextMenuEnabled { get; set; }

		public string CustomCss {
			get {
				return customCss;
			}
			set {
				if (customCss != value)
				{
					customCss = value;
					SetCustomCss();
				}
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
			view.Stop ();
		}

		public void LoadHtml (string content, string base_uri)
		{
			loadProgress = 0;
			view.DocumentText = content;
		}

		public string Title {
			get {
				return view.Document.Title;
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

		void HandleProgressChanged (object sender, SWF.WebBrowserProgressChangedEventArgs e)
		{
			if (e.CurrentProgress == -1)
				return;
			if (e.MaximumProgress == 0 || e.MaximumProgress < e.CurrentProgress)
				loadProgress = 1;
			else
				loadProgress = (double)e.CurrentProgress / (double)e.MaximumProgress;
		}

		void HandleNavigating (object sender, SWF.WebBrowserNavigatingEventArgs e)
		{
			if (enableNavigatingEvent) {
				var newurl = string.Empty;
				if (e.Url != null)
					newurl = e.Url.AbsoluteUri;
				ApplicationContext.InvokeUserCode (delegate {
					e.Cancel = EventSink.OnNavigateToUrl (newurl);
				});
			}
		}

		void HandleDocumentTitleChanged (object sender, EventArgs e)
		{
			if (enableTitleChangedEvent)
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnTitleChanged ();
				});
		}

		void HandleNavigated (object sender, SWF.WebBrowserNavigatedEventArgs e)
		{
			if (enableLoadingEvent)
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnLoading ();
				});
		}

		SWF.HtmlDocument currentDocument;

		void HandleDocumentCompleted (object sender, SWF.WebBrowserDocumentCompletedEventArgs e)
		{
			if (currentDocument != view.Document) {
				if (currentDocument != null)
					view.Document.ContextMenuShowing -= HandleContextMenu;

				currentDocument = view.Document;
				view.Document.ContextMenuShowing += HandleContextMenu;
			}
			SetCustomCss();

			if (enableLoadedEvent)
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnLoaded ();
				});
		}

		void HandleContextMenu (object sender, SWF.HtmlElementEventArgs e)
		{
			e.ReturnValue = ContextMenuEnabled;
		}

		void SetCustomCss ()
		{
			var mainDocument = view.Document;
			var head = mainDocument?.GetElementsByTagName("head")?[0];

			if (head == null)
			{
				customCssNode = null;
				return;
			}

			// reuse node reference only if the document did not change and still contains the injected node
			if (customCssNode != null && !head.Children.Cast<SWF.HtmlElement>().Contains(customCssNode))
				customCssNode = null;

			if (!string.IsNullOrEmpty(CustomCss))
			{
				if (customCssNode == null)
				{
					customCssNode = mainDocument.CreateElement("style");
					customCssNode.SetAttribute("type", "text/css");
					head.InsertAdjacentElement(SWF.HtmlElementInsertionOrientation.AfterBegin, customCssNode);
				}
				if (customCssNode.InnerHtml != customCss)
					customCssNode.InnerHtml = customCss;
			}
			else if (customCssNode != null)
			{
				if (head.Children.Cast<SWF.HtmlElement>().Contains(customCssNode))
					customCssNode.OuterHtml = string.Empty;
				customCssNode = null;
			}
		}
	}
}

