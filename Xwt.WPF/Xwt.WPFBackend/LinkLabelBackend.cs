// 
// LinkLabelBackend.cs
//  
// Author:
//       Alan McGovern <alan@xamarin.com>
// 
// Copyright (c) 2012 Alan McGovern
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SWC = System.Windows.Controls;

using Xwt.Backends;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Xwt.WPFBackend
{
	public class LinkLabelBackend : WidgetBackend, ILinkLabelBackend
	{
		public LinkLabelBackend ()
		{
			Widget = new WpfLinkLabel ();
		}

		new ILinkLabelEventSink EventSink {
			get { return (ILinkLabelEventSink) base.EventSink; }
		}

		new WpfLinkLabel Widget {
			get { return (WpfLinkLabel) base.Widget; }
			set { base.Widget = value; }
		}

		public Uri Uri {
			get { return Widget.Hyperlink.NavigateUri; }
			set { Widget.Hyperlink.NavigateUri = value; }
		}

		public string Text
		{
			get { return (string) Widget.Content; }
			set
			{
				Widget.Text.Text = value;
				Widget.InvalidateMeasure ();
			}
		}

		public void SetFormattedText(FormattedText text)
		{
		}

		public Alignment TextAlignment
		{
			get { return DataConverter.ToXwtAlignment (Widget.HorizontalContentAlignment); }
			set { Widget.HorizontalContentAlignment = DataConverter.ToWpfAlignment (value); }
		}

		// TODO
		public EllipsizeMode Ellipsize
		{
			get;
			set;
		}

		public WrapMode Wrap {
			get;
			set;
		}

		public Xwt.Drawing.Color TextColor
		{
			get
			{
				return Widget.Foreground.ToXwtColor ();
			}
			set
			{
				Widget.Foreground = ResPool.GetSolidBrush (value);
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is LinkLabelEvent) {
				switch ((LinkLabelEvent) eventId) {
				case LinkLabelEvent.NavigateToUrl:
					Widget.Hyperlink.Click += HandleClicked;
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
					Widget.Hyperlink.Click -= HandleClicked;
					break;
				}
			}
		}

		void HandleClicked (object sender, EventArgs e)
		{
			Context.InvokeUserCode (() => {
				EventSink.OnNavigateToUrl (Uri);
			});
		}
	}

	class WpfLinkLabel : ContentControl, IWpfWidget
	{
		public WidgetBackend Backend { get; set; }

		public System.Windows.Documents.Hyperlink Hyperlink {
			get; set;
		}

		public Run Text {
			get; set;
		}

		public WpfLinkLabel ()
		{
			Text = new Run ();
			Hyperlink = new System.Windows.Documents.Hyperlink (Text);

			var content = new TextBlock ();
			content.Inlines.Add (Hyperlink);
			Content = content;
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}
	}
}
