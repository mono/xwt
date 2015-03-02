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

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.AppKit;
#else
using AppKit;
#endif

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
			Widget.Activated += delegate {
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnSelectionChanged ();
				});
				Widget.SynchronizeTitleAndSelectedItem ();
				ResetFittingSize ();
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
					if (EventSink.RowIsSeparator (n))
						Widget.Menu.AddItem (NSMenuItem.SeparatorItem);
					else {
						NSMenuItem it = new NSMenuItem ();
						UpdateItem (it, n);
						Widget.Menu.AddItem (it);
					}
				}
			}
		}

		void HandleSourceRowsReordered (object sender, ListRowOrderEventArgs e)
		{
		}

		void HandleSourceRowChanged (object sender, ListRowEventArgs e)
		{
			NSMenuItem mi = Widget.ItemAtIndex (e.Row);
			if (EventSink.RowIsSeparator (e.Row)) {
				if (!mi.IsSeparatorItem) {
					Widget.Menu.InsertItem (NSMenuItem.SeparatorItem, e.Row);
					Widget.Menu.RemoveItemAt (e.Row + 1);
				}
			}
			else {
				if (mi.IsSeparatorItem) {
					mi = new NSMenuItem ();
					Widget.Menu.InsertItem (mi, e.Row);
					Widget.Menu.RemoveItemAt (e.Row + 1);
				}
				UpdateItem (mi, e.Row);
				Widget.SynchronizeTitleAndSelectedItem ();
			}
			ResetFittingSize ();
		}

		void HandleSourceRowDeleted (object sender, ListRowEventArgs e)
		{
			Widget.RemoveItem (e.Row);
			Widget.SynchronizeTitleAndSelectedItem ();
			ResetFittingSize ();
		}

		void HandleSourceRowInserted (object sender, ListRowEventArgs e)
		{
			NSMenuItem mi;
			if (EventSink.RowIsSeparator (e.Row))
				mi = NSMenuItem.SeparatorItem;
			else {
				mi = new NSMenuItem ();
				UpdateItem (mi, e.Row);
			}
			Widget.Menu.InsertItem (mi, e.Row);
			Widget.SynchronizeTitleAndSelectedItem ();
			ResetFittingSize ();
		}
		
		void UpdateItem (NSMenuItem mi, int index)
		{
			mi.Title = (string) source.GetValue (index, 0) ?? "";
		}

		public int SelectedRow {
			get {
				return (int) Widget.IndexOfSelectedItem;
			}
			set {
				Widget.SelectItem (value);
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnSelectionChanged ();
				});
				Widget.SynchronizeTitleAndSelectedItem ();
				ResetFittingSize ();
			}
		}

		public override bool Sensitive {
			get {
				return Widget.Enabled;
			}
			set {
				Widget.Enabled = value;
			}
		}
		#endregion
	}
	
	class PopUpButton: NSPopUpButton, IViewObject
	{
		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }

		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
			if (Backend.Cursor != null)
				AddCursorRect (Bounds, Backend.Cursor);
		}
	}
}

