// 
// LinkLabel.cs
//  
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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

namespace Xwt
{
	public class LinkLabelClickedEventArgs : EventArgs
	{
		public bool Handled {
			get; private set;
		}

		public void SetHandled ()
		{
			Handled = true;
		}
	}

	public class LinkLabel: Label
	{
		protected new class WidgetBackendHost : Label.WidgetBackendHost, ILinkLabelEventSink
		{
			public void OnClicked ()
			{
				((LinkLabel) Parent).OnClicked (new LinkLabelClickedEventArgs ());
			}
		}

		EventHandler<LinkLabelClickedEventArgs> clicked;
		public event EventHandler<LinkLabelClickedEventArgs> Clicked {
			add {
				BackendHost.OnBeforeEventAdd (LinkLabelEvent.Clicked, clicked);
				clicked += value;
			}
			remove {
				clicked -= value;
				BackendHost.OnAfterEventRemove (LinkLabelEvent.Clicked, clicked);
			}
		}

		ILinkLabelBackend Backend {
			get { return (ILinkLabelBackend) BackendHost.Backend; }
		}

		public Uri Uri {
			get { return Backend.Uri; }
			set { Backend.Uri = value; }
		}

		static LinkLabel ()
		{
			MapEvent (LinkLabelEvent.Clicked, typeof(LinkLabel), "OnClicked");
		}

		public LinkLabel ()
			: this ("")
		{
		}

		public LinkLabel (string text) : base (text)
		{
			Clicked += HandleClicked;

		}

		void HandleClicked (object sender, LinkLabelClickedEventArgs e)
		{
			if (!e.Handled) {
				System.Diagnostics.Process.Start (Uri.ToString ());
				e.SetHandled ();
			}
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		protected virtual void OnClicked (LinkLabelClickedEventArgs e)
		{
			if (clicked != null)
				clicked (this, e);
		}
	}
}

