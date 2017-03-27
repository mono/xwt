//
// ExListViewItem.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
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
using System.Windows;
using System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class ExListViewItem
		: ListViewItem
	{
		public ExListViewItem ()
		{
			HorizontalContentAlignment = HorizontalAlignment.Stretch;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.Property.Name == "IsVisible" && IsVisible) {
				foreach (var column in ((GridView)ListView.View).Columns) {
					if (!Double.IsNaN (column.Width))
						continue;
					
					column.Width = column.ActualWidth;
					column.Width = Double.NaN;
				}
			}

			base.OnPropertyChanged(e);
		}

		private ExListView view;
		protected ExListView ListView {
			get {
				if (this.view == null)
					this.view = FindListView ((FrameworkElement) VisualParent);

				return this.view;
			}
		}

		private ExListView FindListView (FrameworkElement element)
		{
			if (element == null)
				return null;

			ExListView view = element as ExListView;
			if (view != null)
				return view;

			return FindListView (element.TemplatedParent as FrameworkElement);
		}

		protected override void OnGotFocus (RoutedEventArgs e)
		{
			base.OnGotFocus (e);
			ListView.FocusItem (this);
		}
	}
}