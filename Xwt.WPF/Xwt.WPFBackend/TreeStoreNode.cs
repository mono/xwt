// 
// TreeStoreNode.cs
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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Xwt.WPFBackend
{
	internal class TreeStoreNode
		: TreePosition, INotifyPropertyChanged
	{
		public TreeStoreNode (object[] values, TreeStoreNode parent)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			this.values = values;
			Parent = parent;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public object[] Values {
			get { return this.values; }
		}

		public TreeStoreNode Parent {
			get;
			private set;
		}

		public ObservableCollection<TreeStoreNode> Children {
			get { return this.children; }
		}

		private bool isExpanded;

		public bool IsExpanded {
			get { return this.isExpanded; }
			set {
				if (Parent != null && value)
					Parent.IsExpanded = true;

				if (this.isExpanded == value)
					return;

				this.isExpanded = value;
				OnPropertyChanged (new PropertyChangedEventArgs ("IsExpanded"));
			}
		}

		public void SetValue (int column, object value)
		{
			this.values [column] = value;
			OnPropertyChanged (new PropertyChangedEventArgs ("Values"));
		}

		private readonly ObservableCollection<TreeStoreNode> children = new ObservableCollection<TreeStoreNode> ();
		private readonly object[] values;

		private void OnPropertyChanged (PropertyChangedEventArgs e)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
				handler (this, e);
		}
	}
}