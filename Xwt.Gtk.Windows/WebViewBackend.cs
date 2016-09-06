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
using System.Runtime.InteropServices.ComTypes;
using SWF = System.Windows.Forms;
using Xwt.GtkBackend;
using Xwt.Backends;
using Xwt.NativeMSHTML;
using Gtk;

namespace Xwt.Gtk.Windows
{
	public class WebViewBackend : WidgetBackend, IWebViewBackend, IDocHostUIHandler
	{
		SWF.WebBrowser view;
		string url;
		Socket socket;
		bool enableNavigatingEvent, enableLoadingEvent, enableLoadedEvent, enableTitleChangedEvent;
		bool initialized;

		[DllImportAttribute("user32.dll", EntryPoint = "SetParent")]
		internal static extern System.IntPtr SetParent([InAttribute] System.IntPtr hwndChild, [InAttribute] System.IntPtr hwndNewParent);

		public override void Initialize ()
		{
			base.Initialize ();

			view = new SWF.WebBrowser();
			view.ScriptErrorsSuppressed = true;
			view.AllowWebBrowserDrop = false;

			view.ProgressChanged += HandleProgressChanged;
			view.Navigating += HandleNavigating;
			view.Navigated += HandleNavigated;
			view.DocumentTitleChanged += HandleDocumentTitleChanged;
			view.DocumentCompleted += HandleDocumentCompleted;
			view.Navigate("about:blank"); // force Document initialization

			socket = new Socket ();
			Widget = socket;

			this.Widget.Realized += HandleGtkRealized;
			this.Widget.SizeAllocated += HandleGtkSizeAllocated;

			Widget.Show();
		}

		void HandleGtkRealized (object sender, EventArgs e)
		{
			var size = new System.Drawing.Size (Widget.WidthRequest, Widget.HeightRequest);
			view.Size = size;

			var browser_handle = view.Handle;
			IntPtr window_handle = (IntPtr)socket.Id;
			SetParent (browser_handle, window_handle);

			// load requested url if the view is still not initialized
			// otherwise it would already have been loaded
			if (!initialized && url != null)
				view.Navigate(url);
		}

		void HandleGtkSizeAllocated (object sender, SizeAllocatedArgs e)
		{
			var size = new System.Drawing.Size(e.Allocation.Width, e.Allocation.Height);
			view.Size = size;
		}

		public string Url {
			get {
				if (view?.Url != null)
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

		public string CustomCss { get; set; }

		public bool ScrollBarsEnabled { get; set; }

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
			UpdateDocumentRef();
			if (enableLoadingEvent)
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnLoading ();
				});
		}

		SWF.HtmlDocument currentDocument;

		void UpdateDocumentRef()
		{
			if (currentDocument != view.Document)
			{
				var doc = view.Document.DomDocument as ICustomDoc;
				if (doc != null)
					doc.SetUIHandler(this);
				if (currentDocument != null)
				{
					var oldDoc = view.Document.DomDocument as ICustomDoc;
					if (oldDoc != null)
						oldDoc.SetUIHandler(null);
				}
				currentDocument = view.Document;
			}

			// on initialization we load "about:blank" to initialize the document,
			// in that case we load the requested url
			if (currentDocument != null && !initialized)
			{
				initialized = true;
				if (url != null)
					view.Navigate(url);
			}
		}

		void HandleDocumentCompleted (object sender, SWF.WebBrowserDocumentCompletedEventArgs e)
		{
			UpdateDocumentRef();

			if (enableLoadedEvent)
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnLoaded ();
				});
		}

		#region IDocHostUIHandler implementation
		int IDocHostUIHandler.ShowContextMenu(int dwID, ref tagPOINT ppt, object pcmdtReserved, object pdispReserved)
		{
			return (int)(ContextMenuEnabled ? HResult.S_FALSE : HResult.S_OK);
		}

		void IDocHostUIHandler.GetHostInfo(ref DOCHOSTUIINFO pInfo)
		{
			if (!ScrollBarsEnabled)
				pInfo.dwFlags = (int)(DOCHOSTUIFLAG.DOCHOSTUIFLAG_SCROLL_NO | DOCHOSTUIFLAG.DOCHOSTUIFLAG_NO3DOUTERBORDER);
			else
				pInfo.dwFlags = 0;
			if (!string.IsNullOrEmpty(CustomCss))
				pInfo.pchHostCss = CustomCss;
		}

		void IDocHostUIHandler.ShowUI(int dwID, ref object pActiveObject, ref object pCommandTarget, ref object pFrame, ref object pDoc)
		{
		}

		void IDocHostUIHandler.HideUI()
		{
		}

		void IDocHostUIHandler.UpdateUI()
		{
		}

		void IDocHostUIHandler.EnableModeless(bool fEnable)
		{
		}

		void IDocHostUIHandler.OnDocWindowActivate(bool fActivate)
		{
		}

		void IDocHostUIHandler.OnFrameWindowActivate(bool fActivate)
		{
		}

		void IDocHostUIHandler.ResizeBorder(ref LPCRECT prcBorder, object pUIWindow, bool fFrameWindow)
		{
		}

		int IDocHostUIHandler.TranslateAccelerator(ref LPMSG lpMsg, ref Guid pguidCmdGroup, uint nCmdID)
		{
			return (int)HResult.S_FALSE;
		}

		void IDocHostUIHandler.GetOptionKeyPath(ref string pchKey, uint dw)
		{
		}

		int IDocHostUIHandler.GetDropTarget(int pDropTarget, ref int ppDropTarget)
		{
			return (int)HResult.S_FALSE;
		}

		void IDocHostUIHandler.GetExternal(out object ppDispatch)
		{
			throw new NotImplementedException();
		}

		int IDocHostUIHandler.TranslateUrl(uint dwTranslate, string pchURLIn, ref string ppchURLOut)
		{
			return (int)HResult.S_FALSE;
		}

		IDataObject IDocHostUIHandler.FilterDataObject(IDataObject pDO)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}

