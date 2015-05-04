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
using System.ComponentModel;

namespace Xwt
{
	[BackendType (typeof(IRichTextViewBackend))]
	public class RichTextView : Widget
	{
		[DefaultValue (0)]
		public int CursorPosition {
			get { return Backend.CursorPosition; }
			set { Backend.CursorPosition = value; }
		}

		[DefaultValue (0)]
		public int SelectionStart {
			get { return Backend.SelectionStart; }
			set { Backend.SelectionStart = value; }
		}

		[DefaultValue (0)]
		public int SelectionLength {
			get { return Backend.SelectionLength; }
			set { Backend.SelectionLength = value; }
		}

		[DefaultValue ("")]
		public string SelectedText {
			get { return Backend.SelectedText; }
		}
		EventHandler selectionChanged;
		public event EventHandler SelectionChanged {
			add {
				BackendHost.OnBeforeEventAdd (RichTextViewEvent.SelectionChanged, selectionChanged);
				selectionChanged += value;
			}
			remove {
				selectionChanged -= value;
				BackendHost.OnAfterEventRemove (RichTextViewEvent.SelectionChanged, selectionChanged);
			}
		}

		protected new class WidgetBackendHost : Widget.WidgetBackendHost, IRichTextViewEventSink
		{
			public void OnNavigateToUrl (Uri uri)
			{
				((RichTextView) Parent).OnNavigateToUrl (new NavigateToUrlEventArgs (uri));
			}

			public void OnSelectionChanged ()
			{
				((RichTextView)Parent).OnSelectionChanged (EventArgs.Empty);
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
			MapEvent (RichTextViewEvent.SelectionChanged, typeof(RichTextView), "OnSelectionChanged");		
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

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		protected virtual void OnSelectionChanged (EventArgs e)
		{
			if (selectionChanged != null)
				selectionChanged (this, e);
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

