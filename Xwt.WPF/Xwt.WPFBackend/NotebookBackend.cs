// 
// NotebookBackend.cs
//  
// Author:
//       Thomas Ziegler <ziegler.thomas@web.de>
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2012 Thomas Ziegler
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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SWC = System.Windows.Controls;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class NotebookBackend : WidgetBackend, INotebookBackend
	{
		public NotebookBackend ()
		{
			TabControl = new WpfNotebook ();
		}

		public int CurrentTab {
			get { return TabControl.SelectedIndex; }
			set { TabControl.SelectedIndex = value; }
		}

		public Xwt.NotebookTabOrientation TabOrientation {
			get {
				Xwt.NotebookTabOrientation tabPos = Xwt.NotebookTabOrientation.Top;
				Enum.TryParse (TabControl.TabStripPlacement.ToString (), out tabPos);
				return tabPos;
			}
			set {
				SWC.Dock tabPos = SWC.Dock.Top;
				Enum.TryParse (value.ToString (), out tabPos);
				TabControl.TabStripPlacement = tabPos;
			}
		}

		public void Add (IWidgetBackend widget, NotebookTab tab)
		{
			UIElement element = (UIElement)widget.NativeWidget;
			TabItem ti = new TabItem {
				Content = element,
				Header = tab.Label,
				Tag = tab
			};

			FrameworkElement felement = element as FrameworkElement;
			if (felement != null) {
				felement.Tag = widget;
				felement.Loaded += OnContentLoaded;
			}

			TabControl.Items.Add (ti);
			
			if (TabControl.SelectedIndex == -1)
				TabControl.SelectedIndex = 0;
		}

		public void Remove (IWidgetBackend widget)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException ();

			FrameworkElement felement = element as FrameworkElement;
			if (felement != null)
				felement.Loaded -= OnContentLoaded;
			
			for (int i = 0; i < TabControl.Items.Count; ++i) {
				TabItem tab = (TabItem)TabControl.Items [i];
				if (tab.Content == widget.NativeWidget) {
					TabControl.Items.RemoveAt (i);
					break;
				}
			}
		}

		public void UpdateLabel (NotebookTab tab, string hint)
		{
			TabItem item = TabControl.Items.Cast<TabItem> ().FirstOrDefault (t => t.Tag == tab);
			if (item != null)
				item.Header = tab.Label;
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is NotebookEvent) {
				switch ((NotebookEvent)eventId) {
				case NotebookEvent.CurrentTabChanged:
					TabControl.SelectionChanged += OnCurrentTabChanged;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is NotebookEvent) {
				switch ((NotebookEvent)eventId) {
				case NotebookEvent.CurrentTabChanged:
					TabControl.SelectionChanged -= OnCurrentTabChanged;
					break;
				}
			}
		}

		private void OnCurrentTabChanged (object sender, SelectionChangedEventArgs e)
		{
			Context.InvokeUserCode (NotebookEventSink.OnCurrentTabChanged);
		}

		protected TabControl TabControl {
			get { return (TabControl)Widget; }
			set { Widget = value; }
		}

		protected INotebookEventSink NotebookEventSink {
			get { return (INotebookEventSink) EventSink; }
		}

		private void OnContentLoaded (object sender, RoutedEventArgs routedEventArgs)
		{
			WidgetBackend backend = (WidgetBackend)((FrameworkElement) sender).Tag;

			var surface = backend.Frontend as IWidgetSurface;
			if (surface == null)
				return;

			surface.Reallocate();
		}
	}

	class WpfNotebook : SWC.TabControl, IWpfWidget
	{
		public WidgetBackend Backend { get; set; }

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}
	}
}