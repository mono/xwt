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
using System.Windows;
using System.Windows.Controls;
using Xwt.Engine;
using SWC = System.Windows.Controls;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	public class NotebookBackend : WidgetBackend, INotebookBackend
	{
		public NotebookBackend ()
		{
			TabControl = new WpfNotebook ();
			TabControl.SelectionChanged += OnTabSelectionChanged;
		}

		public int CurrentTab {
			get { return TabControl.SelectedIndex; }
			set { TabControl.SelectedIndex = value; }
		}

		public void Add (IWidgetBackend widget, NotebookTab tab)
		{
			this.widgets.Add (widget);

			UIElement element = (UIElement)widget.NativeWidget;
			TabItem ti = new TabItem  { Header = tab.Label };

			ti.Content = element;
			TabControl.Items.Add (ti);
			
			if (TabControl.SelectedIndex == -1)
				TabControl.SelectedIndex = 0;
		}

		public void Remove (IWidgetBackend widget)
		{
			this.widgets.Remove (widget);

			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException ();
			
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
			foreach (TabItem item in TabControl.Items) {
				if (item.Header.ToString () == tab.Label) {
					item.Header = hint;
				}	
			}
		}

		private readonly List<IWidgetBackend> widgets = new List<IWidgetBackend> ();

		protected TabControl TabControl {
			get { return (TabControl)Widget; }
			set { Widget = value; }
		}

		private void OnTabSelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			var backend = (WidgetBackend)this.widgets [TabControl.SelectedIndex];
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