// 
// ComboBoxBackend.cs
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

// 
// ComboBoxBackend.cs
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
using Xwt.Backends;
using MonoMac.AppKit;

namespace Xwt.Mac
{
	public class ComboBoxBackend: ViewBackend<NSPopUpButton,IComboBoxEventSink>, IComboBoxBackend
	{
		IListDataSource source;
		
		public ComboBoxBackend ()
		{
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
			ViewObject = new PopUpButton ();
			Widget.Menu = new NSMenu ();
			Widget.SizeToFit ();
			Widget.Activated += delegate {
				EventSink.OnSelectionChanged ();
				Widget.SynchronizeTitleAndSelectedItem ();
				Widget.SizeToFit ();
			};
		}

		#region IComboBoxBackend implementation
		public void SetViews (CellViewCollection views)
		{
		}

		public void SetSource (IListDataSource s, IBackend sourceBackend)
		{
			if (source != null) {
				source.RowInserted -= HandleSourceRowInserted;
				source.RowDeleted -= HandleSourceRowDeleted;
				source.RowChanged -= HandleSourceRowChanged;
				source.RowsReordered -= HandleSourceRowsReordered;
			}
			
			source = s;
			Widget.Menu = new NSMenu ();
			
			if (source != null) {
				source.RowInserted += HandleSourceRowInserted;
				source.RowDeleted += HandleSourceRowDeleted;
				source.RowChanged += HandleSourceRowChanged;
				source.RowsReordered += HandleSourceRowsReordered;
				for (int n=0; n<source.RowCount; n++) {
					NSMenuItem it = new NSMenuItem ();
					UpdateItem (it, n);
					Widget.Menu.AddItem (it);
				}
			}
		}

		void HandleSourceRowsReordered (object sender, ListRowOrderEventArgs e)
		{
		}

		void HandleSourceRowChanged (object sender, ListRowEventArgs e)
		{
			NSMenuItem mi = Widget.ItemAtIndex (e.Row);
			UpdateItem (mi, e.Row);
			Widget.SynchronizeTitleAndSelectedItem ();
			Widget.SizeToFit ();
		}

		void HandleSourceRowDeleted (object sender, ListRowEventArgs e)
		{
			Widget.RemoveItem (e.Row);
			Widget.SynchronizeTitleAndSelectedItem ();
			Widget.SizeToFit ();
		}

		void HandleSourceRowInserted (object sender, ListRowEventArgs e)
		{
			NSMenuItem mi = new NSMenuItem ();
			UpdateItem (mi, e.Row);
			Widget.Menu.InsertItematIndex (mi, e.Row);
			Widget.SynchronizeTitleAndSelectedItem ();
			Widget.SizeToFit ();
		}
		
		void UpdateItem (NSMenuItem mi, int index)
		{
			mi.Title = (string) source.GetValue (index, 0);
		}

		public int SelectedRow {
			get {
				return Widget.IndexOfSelectedItem;
			}
			set {
				Widget.SelectItem (value);
				Widget.SynchronizeTitleAndSelectedItem ();
				Widget.SizeToFit ();
			}
		}
		#endregion
	}
	
	class PopUpButton: NSPopUpButton, IViewObject<NSPopUpButton>
	{
		public NSPopUpButton View {
			get {
				return this;
			}
		}

		public Widget Frontend { get; set; }
		
		
	}
}

