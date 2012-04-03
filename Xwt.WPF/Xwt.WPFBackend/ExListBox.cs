//
// ExListBox.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class ExListBox
		: SWC.ListBox, IWpfWidget
	{
		public ExListBox()
		{
			SelectedIndexes = new ObservableCollection<int> ();
		}

		public WidgetBackend Backend {
			get;
			set;
		}

		public static readonly DependencyProperty SelectedIndexesProperty = DependencyProperty.Register (
			"SelectedIndexes",
			typeof (ICollection<int>), typeof (ExListView),
			new UIPropertyMetadata (OnSelectedIndexesPropertyChanged));

		public ICollection<int> SelectedIndexes
		{
			get { return (ICollection<int>) GetValue (SelectedIndexesProperty); }
			set { SetValue (SelectedIndexesProperty, value); }
		}

		protected override void OnSelectionChanged (SelectionChangedEventArgs e)
		{
			if (this.changingSelection) {
				base.OnSelectionChanged (e);
				return;
			}

			this.changingSelection = true;
			if (e.AddedItems != null) {
				foreach (object item in e.AddedItems) {
					SelectedIndexes.Add (Items.IndexOf (item));
				}
			}

			if (e.RemovedItems != null) {
				foreach (object item in e.RemovedItems) {
					SelectedIndexes.Remove (Items.IndexOf (item));
				}
			}

			this.changingSelection = false;
			base.OnSelectionChanged (e);
		}

		protected virtual void OnSelectedIndexesChanged (DependencyPropertyChangedEventArgs e)
		{
			var oldNotifying = e.OldValue as INotifyCollectionChanged;
			if (oldNotifying != null)
				oldNotifying.CollectionChanged -= SelectedIndexesChanged;

			if (SelectionMode == SWC.SelectionMode.Single)
				throw new InvalidOperationException();

			var newNotifying = e.NewValue as INotifyCollectionChanged;
			if (newNotifying != null)
				newNotifying.CollectionChanged += SelectedIndexesChanged;
		}

		private bool changingSelection;
		private void SelectedIndexesChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.changingSelection)
				return;

			this.changingSelection = true;

			if (SelectionMode == SWC.SelectionMode.Single) {
				SelectedItem = null;
				if (e.NewItems != null && e.NewItems.Count > 0)
					SelectedItem = Items [(int) e.NewItems [0]];

				this.changingSelection = false;
				return;
			}

			if (e.Action == NotifyCollectionChangedAction.Reset) {
				SelectedItems.Clear();
				foreach (int index in SelectedIndexes)
				{
					SelectedItems.Add (Items[index]);
					if (SelectionMode == SWC.SelectionMode.Single)
						break;
				}
			} else {
				if (e.NewItems != null) {
					foreach (int index in e.NewItems)
						SelectedItems.Add (Items[index]);
				}

				if (e.OldItems != null) {
					foreach (int index in e.OldItems)
						SelectedItems.Remove (Items [index]);
				}
			}

			this.changingSelection = false;
		}

		private static void OnSelectedIndexesPropertyChanged (DependencyObject dobj, DependencyPropertyChangedEventArgs e)
		{
			var listbox = (ExListBox) dobj;
			listbox.OnSelectedIndexesChanged (e);
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}
	}
}