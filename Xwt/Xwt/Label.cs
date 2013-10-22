// 
// Label.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt.Drawing;
using Xwt.Backends;
using System.ComponentModel;

namespace Xwt
{
	[BackendType (typeof(ILabelBackend))]
	public class Label: Widget
	{
		protected new class WidgetBackendHost : Widget.WidgetBackendHost, ILabelEventSink
		{
			public void OnLinkClicked (Uri target)
			{
				((Label) Parent).OnLinkClicked (new LinkEventArgs (target));
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		ILabelBackend Backend {
			get { return (ILabelBackend) BackendHost.Backend; }
		}

		static Label ()
		{
			MapEvent (LabelEvent.LinkClicked, typeof (Label), "OnLinkClicked");
		}
		
		public Label ()
		{
		}
		
		public Label (string text)
		{
			VerifyConstructorCall (this);
			Backend.Text = text;
		}

		[DefaultValue ("")]
		public string Text {
			get { return Backend.Text; }
			set {
				Backend.Text = value; 
				OnPreferredSizeChanged ();
			}
		}

		string markup;
		public string Markup {
			get { return markup; }
			set {
				markup = value;
				var t = FormattedText.FromMarkup (markup);
				Backend.SetFormattedText (t);
			}
		}

		public Color TextColor {
			get { return Backend.TextColor; }
			set { Backend.TextColor = value; }
		}

		[DefaultValue (Alignment.Start)]
		public Alignment TextAlignment {
			get { return Backend.TextAlignment; }
			set {
				Backend.TextAlignment = value; 
				OnPreferredSizeChanged ();
			}
		}
		
		[DefaultValue (EllipsizeMode.None)]
		public EllipsizeMode Ellipsize {
			get { return Backend.Ellipsize; }
			set {
				Backend.Ellipsize = value;
				OnPreferredSizeChanged ();
			}
		}

		[DefaultValue (WrapMode.None)]
		public WrapMode Wrap {
			get { return Backend.Wrap; }
			set {
				Backend.Wrap = value;
				OnPreferredSizeChanged ();
			}
		}
		
		protected virtual void OnLinkClicked (LinkEventArgs e)
		{
			if (linkClicked != null)
				linkClicked (this, e);

			if (!e.Handled && e.Target != null) {
				Desktop.OpenUrl (e.Target);
				e.SetHandled ();
			}
		}
		
		EventHandler<LinkEventArgs> linkClicked;
		public event EventHandler<LinkEventArgs> LinkClicked {
			add {
				BackendHost.OnBeforeEventAdd (LabelEvent.LinkClicked, linkClicked);
				linkClicked += value;
			}
			remove {
				linkClicked -= value;
				BackendHost.OnAfterEventRemove (LabelEvent.LinkClicked, linkClicked);
			}
		}
	}

	public sealed class LinkEventArgs : EventArgs
	{
		public LinkEventArgs (Uri target)
		{
			Target = target;
		}

		public bool Handled {
			get; private set;
		}

		public Uri Target {
			get; private set;
		}

		public void SetHandled ()
		{
			Handled = true;
		}
	}
}

