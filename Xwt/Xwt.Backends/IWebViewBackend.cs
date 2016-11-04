//
// IWebViewBackend.cs
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

namespace Xwt.Backends
{
	public interface IWebViewBackend : IWidgetBackend
	{
		string Url { get; set; }
		string Title { get; }
		double LoadProgress { get; }
		bool CanGoBack { get; }
		void GoBack ();
		bool CanGoForward { get; }
		void GoForward ();
		void Reload ();
		void StopLoading ();
		void LoadHtml (string content, string base_uri);
		bool ContextMenuEnabled { get; set; }
		bool DrawsBackground { get; set; }
		bool ScrollBarsEnabled { get; set; }
		string CustomCss { get; set; }
	}

	public interface IWebViewEventSink : IWidgetEventSink
	{
		void OnLoaded ();
		void OnLoading ();
		bool OnNavigateToUrl (string url);
		void OnTitleChanged ();
	}

	public enum WebViewEvent
	{
		Loaded = 1,
		Loading = 2,
		NavigateToUrl = 3,
		TitleChanged = 4,
	}
}
