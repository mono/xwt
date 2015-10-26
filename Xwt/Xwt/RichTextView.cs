//
// RichTextView.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//       Alex Corrado <corrado@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using System.IO;
using System.Linq;
using System.Text;

using Xwt.Backends;
using Xwt.Formats;

namespace Xwt
{
	[BackendType (typeof(IRichTextViewBackend))]
	public class RichTextView : Widget
	{
		protected new class WidgetBackendHost : Widget.WidgetBackendHost, IRichTextViewEventSink
		{
			public void OnNavigateToUrl (Uri uri)
			{
				((RichTextView) Parent).OnNavigateToUrl (new NavigateToUrlEventArgs (uri));
			}
		}

		IRichTextViewBackend Backend {
			get { return (IRichTextViewBackend) BackendHost.Backend; }
		}

		EventHandler<NavigateToUrlEventArgs> navigateToUrl;
		public event EventHandler<NavigateToUrlEventArgs> NavigateToUrl
		{
			add
			{
				BackendHost.OnBeforeEventAdd (RichTextViewEvent.NavigateToUrl, navigateToUrl);
				navigateToUrl += value;
			}
			remove
			{
				navigateToUrl -= value;
				BackendHost.OnAfterEventRemove (RichTextViewEvent.NavigateToUrl, navigateToUrl);
			}
		}

		public RichTextView ()
		{
			NavigateToUrl += delegate { }; // ensure the virtual method is always called
		}

		public void LoadFile (string fileName, TextFormat format)
		{
			using (var stream = new FileStream (fileName, FileMode.Open, FileAccess.Read))
				LoadStream (stream, format);
		}

		public void LoadText (string text, TextFormat format)
		{
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (text), false))
				LoadStream (stream, format);
		}

		public virtual void LoadStream (Stream input, TextFormat format)
		{
			var buffer = Backend.CreateBuffer ();
			format.Parse (input, buffer);
			Backend.SetBuffer (buffer);
			OnPreferredSizeChanged ();
		}

		public string PlainText {
			get {
				IRichTextBuffer currentBuffer = Backend.CurrentBuffer;
				if (currentBuffer == null)
					return null;
				return currentBuffer.PlainText;				
			}
		}

		public bool ReadOnly {
			get {
				return Backend.ReadOnly;
			}
			set {
				Backend.ReadOnly = value;
			}
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		protected virtual void OnNavigateToUrl (NavigateToUrlEventArgs e)
		{
			if (navigateToUrl != null)
				navigateToUrl (this, e);

			if (!e.Handled && e.Uri != null) {
				Desktop.OpenUrl (e.Uri);
				e.SetHandled ();
			}
		}
	}

	public class MarkdownView : RichTextView
	{
		string markdown;
		public string Markdown
		{
			get
			{
				return markdown;
			}
			set
			{
				markdown = value;
				LoadText (value, TextFormat.Markdown);
			}
		}

		public MarkdownView ()
		{
			Markdown = string.Empty;
		}
	}
}

