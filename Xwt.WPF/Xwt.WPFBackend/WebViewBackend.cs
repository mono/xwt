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
using System.Runtime.InteropServices.ComTypes;
using Xwt.Backends;
using Xwt.NativeMSHTML;
using Xwt.WPFBackend.Interop;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class WebViewBackend : WidgetBackend, IWebViewBackend, IDocHostUIHandler
	{
		string url;
		SWC.WebBrowser view;
		bool enableNavigatingEvent, enableLoadingEvent, enableLoadedEvent, enableTitleChangedEvent;
		bool initialized;

		ICustomDoc currentDocument;
		static object mshtmlBrowser;

		static PropertyInfo titleProperty;
		static PropertyInfo silentProperty;
		static MethodInfo stopMethod;
		static FieldInfo mshtmlBrowserField;
		static Type mshtmlDocType;

		public WebViewBackend () : this (new SWC.WebBrowser ())
		{
		}

		internal WebViewBackend (SWC.WebBrowser browser)
		{
			view = browser;
			view.Navigating += HandleNavigating;
			view.Navigated += HandleNavigated;
			view.LoadCompleted += HandleLoadCompleted;
			view.Loaded += HandleViewLoaded;
			Widget = view;
			view.Navigate ("about:blank"); // force Document initialization
			Title = string.Empty;
		}

		void UpdateDocumentRef()
		{
			if (currentDocument != view.Document)
			{
				var doc = view.Document as ICustomDoc;
				if (doc != null)
				{
					doc.SetUIHandler(this);
					if (mshtmlDocType == null)
						mshtmlDocType = view.Document.GetType();
				}
				if (currentDocument != null)
					currentDocument.SetUIHandler(null);
				currentDocument = doc;
			}

			// on initialization we load "about:blank" to initialize the document,
			// in that case we load the requested url
			if (currentDocument != null && !initialized)
			{
				initialized = true;
				if (!string.IsNullOrEmpty (url))
					view.Navigate(url);
			}
		}

		void HandleViewLoaded(object sender, System.Windows.RoutedEventArgs e)
		{
			// get the MSHTML.IWebBrowser2 instance field
			if (mshtmlBrowserField == null)
				mshtmlBrowserField = typeof(SWC.WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);

			if (mshtmlBrowser == null)
				mshtmlBrowser = mshtmlBrowserField.GetValue(view);

			if (silentProperty == null)
				silentProperty = mshtmlBrowserField?.FieldType?.GetProperty("Silent");

			if (stopMethod == null)
				stopMethod = mshtmlBrowserField?.FieldType?.GetMethod("Stop");

			// load requested url if the view is still not initialized
			// otherwise it would already have been loaded
			if (!initialized && !string.IsNullOrEmpty(url))
			{
				initialized = true;
				view.Navigate(url);
			}

			DisableJsErrors();
			UpdateDocumentRef();
		}

		public string Url {
			get {
				return url; }
			set {
				url = value;
				if (initialized && view.IsLoaded)
					view.Navigate(url);
			}
		}

		public string Title
		{
			get; private set;
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

		public bool ContextMenuEnabled { get; set; }

		public bool ScrollBarsEnabled { get; set; }

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
			view.Refresh ();
		}

		public void StopLoading ()
		{
			if (stopMethod != null)
				stopMethod.Invoke(mshtmlBrowser, null);
			else
				view.InvokeScript ("eval", "document.execCommand('Stop');");
		}

		public void LoadHtml (string content, string base_uri)
		{
			view.NavigateToString (content);
			url = string.Empty;
		}

		string GetTitle()
		{
			if (titleProperty == null)
			{
				// Get the property with the document Title,
				// property name depends on .NET/mshtml Version
				titleProperty = mshtmlDocType?.GetProperty("Title") ?? mshtmlDocType?.GetProperty("IHTMLDocument2_title");
			}

			string title = null;
			if (titleProperty != null)
			{
				try
				{
					title = titleProperty.GetValue(view.Document, null) as string;
				}
				catch {
					// try to get the title using a script, if reflection fails
					try
					{
						title = (string)view.InvokeScript("eval", "document.title.toString()");
					}
					#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
					catch { }
					#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
				}
			}

			return title;
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

		void HandleNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			LoadProgress = 0;
			if (e.Uri != null && view.IsLoaded)
				this.url = e.Uri.AbsoluteUri;
			if (enableLoadingEvent)
				Context.InvokeUserCode(delegate
				{
					EventSink.OnLoading();
				});
		}

		void HandleLoadCompleted (object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			UpdateDocumentRef();

			LoadProgress = 1;

			if (enableLoadedEvent)
				Context.InvokeUserCode (EventSink.OnLoaded);
		}

		static void DisableJsErrors()
		{
			if (silentProperty != null)
				silentProperty.SetValue(mshtmlBrowser, true, null);
		}

		#region IDocHostUIHandler implementation

		int IDocHostUIHandler.ShowContextMenu(uint dwID, ref POINT ppt, object pcmdtReserved, object pdispReserved)
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

		void IDocHostUIHandler.ShowUI(uint dwID, ref object pActiveObject, ref object pCommandTarget, ref object pFrame, ref object pDoc)
		{
		}

		void IDocHostUIHandler.HideUI()
		{
		}

		void IDocHostUIHandler.UpdateUI()
		{
			var newTitle = GetTitle();
			if (newTitle != Title)
			{
				Title = newTitle;
				if (enableTitleChangedEvent)
					Context.InvokeUserCode(EventSink.OnTitleChanged);
			}
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

		void IDocHostUIHandler.ResizeBorder(ref RECT prcBorder, object pUIWindow, bool fFrameWindow)
		{
		}

		int IDocHostUIHandler.TranslateAccelerator(ref MSG lpMsg, ref Guid pguidCmdGroup, uint nCmdID)
		{
			return (int)HResult.S_FALSE;
		}

		void IDocHostUIHandler.GetOptionKeyPath(ref string pchKey, uint dw)
		{
		}

		int IDocHostUIHandler.GetDropTarget(object pDropTarget, out object ppDropTarget)
		{
			ppDropTarget = pDropTarget;
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

