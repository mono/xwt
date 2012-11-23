//
// LinkLabelBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
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
using System.Security;
using Xwt;
using Xwt.Backends;


namespace Xwt.GtkBackend
{
	class LinkLabelBackend : LabelBackend, ILinkLabelBackend
	{
		Uri uri;

		bool ClickEnabled {
			get; set;
		}

		new ILinkLabelEventSink EventSink {
			get { return (ILinkLabelEventSink)base.EventSink; }
		}

		public Uri Uri {
			get {
				return uri;
			}
			set {
				uri = value;
				SetMarkup ();
			}
		}

		public override string Text {
			get { return base.Text; }
			set {
				base.Text = value;
				SetMarkup ();
			}
		}

		public LinkLabelBackend ()
		{
			Label.UseMarkup = true;
			Label.SetLinkHandler (OpenLink);
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is LinkLabelEvent) {
				switch ((LinkLabelEvent) eventId) {
				case LinkLabelEvent.NavigateToUrl:
					ClickEnabled = true;
					break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is LinkLabelEvent) {
				switch ((LinkLabelEvent) eventId) {
				case LinkLabelEvent.NavigateToUrl:
					ClickEnabled = false;
					break;
				}
			}
		}

		void SetMarkup ()
		{
			var text = Label.Text;
			string url = string.Format ("<a href=\"{1}\">{0}</a>", SecurityElement.Escape (text), uri != null ? SecurityElement.Escape (uri.ToString ()) : "");
			Label.Markup = url;
		}

		void OpenLink (string link)
		{
			if (ClickEnabled) {
				var uri = !string.IsNullOrEmpty (link)? new Uri (link, UriKind.RelativeOrAbsolute) : null;
				ApplicationContext.InvokeUserCode (() => {
					EventSink.OnNavigateToUrl (uri);
				});
			}
		}
	}
}

