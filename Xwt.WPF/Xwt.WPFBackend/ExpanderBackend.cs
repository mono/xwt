// 
// ExpanderBackend.cs
//  
// Author:
//       Sam Clarke <sam@samclarke.com>
// 
// Copyright (c) 2013 Sam Clarke
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
using SWC = System.Windows.Controls;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	class ExpanderBackend : WidgetBackend, IExpanderBackend
	{
		public ExpanderBackend()
		{
			Widget = new SWC.Expander();

			Widget.Expanded += delegate {
				System.Windows.Application.Current.Dispatcher.BeginInvoke ((System.Action)delegate ()
				{
					EventSink.OnPreferredSizeChanged ();
				});
			};
			Widget.Collapsed += delegate {
				System.Windows.Application.Current.Dispatcher.BeginInvoke ((System.Action)delegate ()
				{
					EventSink.OnPreferredSizeChanged ();
				});
			};
		}

		protected new SWC.Expander Widget
		{
			get { return (SWC.Expander)base.Widget; }
			set { base.Widget = value; }
		}

		protected new IExpandEventSink EventSink
		{
			get { return (IExpandEventSink)base.EventSink; }
		}

		public string Label
		{
			get {
				return Widget.Header.ToString();
			}
			set {
				Widget.Header = value;
			}
		}

		public bool Expanded
		{
			get {
				return Widget.IsExpanded;
			}
			set {
				Widget.IsExpanded = value;
			}
		}

		public void SetContent(IWidgetBackend child)
		{
			if (child == null)
				Widget.Content = null;
			else
				Widget.Content = child.NativeWidget;
		}

		public override void EnableEvent(object eventId)
		{
			base.EnableEvent(eventId);

			if (eventId is ExpandEvent)
			{
				if ((ExpandEvent)eventId == ExpandEvent.ExpandChanged)
				{
					Widget.Expanded += HandleExpandedChanged;
					Widget.Collapsed += HandleExpandedChanged;
				}
			}
		}

		public override void DisableEvent(object eventId)
		{
			base.DisableEvent(eventId);

			if (eventId is ExpandEvent)
			{
				if ((ExpandEvent)eventId == ExpandEvent.ExpandChanged)
				{
					Widget.Expanded -= HandleExpandedChanged;
					Widget.Collapsed -= HandleExpandedChanged;
				}
			}
		}

		void HandleExpandedChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			Context.InvokeUserCode(delegate
			{
				EventSink.ExpandChanged();
			});
		}
	}
}
