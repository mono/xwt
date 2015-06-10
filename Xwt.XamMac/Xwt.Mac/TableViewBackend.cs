// 
// TableViewBackend.cs
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
using System.Collections.Generic;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
#endif

namespace Xwt.Mac
{
	public abstract class TableViewBackend<T,S>: ViewBackend<NSScrollView,S>, ITableViewBackend, ICellSource
		where T:NSTableView where S:ITableViewEventSink
	{
		List<NSTableColumn> cols = new List<NSTableColumn> ();
		protected NSTableView Table;
		ScrollView scroll;
		NSObject selChangeObserver;
		NormalClipView clipView;
		
		public TableViewBackend ()
		{
		}
		
		public override void Initialize ()
		{
			Table = CreateView ();
			scroll = new ScrollView ();
			clipView = new NormalClipView ();
			clipView.Scrolled += OnScrolled;
			scroll.ContentView = clipView;
			scroll.DocumentView = Table;
			scroll.BorderType = NSBorderType.BezelBorder;
			ViewObject = scroll;
			Widget.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
			Widget.AutoresizesSubviews = true;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			Util.DrainObjectCopyPool ();
		}
		
		public ScrollPolicy VerticalScrollPolicy {
			get {
				if (scroll.AutohidesScrollers && scroll.HasVerticalScroller)
					return ScrollPolicy.Automatic;
				else if (scroll.HasVerticalScroller)
					return ScrollPolicy.Always;
				else
					return ScrollPolicy.Never;
			}
			set {
				switch (value) {
				case ScrollPolicy.Automatic:
					scroll.AutohidesScrollers = true;
					scroll.HasVerticalScroller = true;
					break;
				case ScrollPolicy.Always:
					scroll.AutohidesScrollers = false;
					scroll.HasVerticalScroller = true;
					break;
				case ScrollPolicy.Never:
					scroll.HasVerticalScroller = false;
					break;
				}
			}
		}

		public ScrollPolicy HorizontalScrollPolicy {
			get {
				if (scroll.AutohidesScrollers && scroll.HasHorizontalScroller)
					return ScrollPolicy.Automatic;
				else if (scroll.HasHorizontalScroller)
					return ScrollPolicy.Always;
				else
					return ScrollPolicy.Never;
			}
			set {
				switch (value) {
				case ScrollPolicy.Automatic:
					scroll.AutohidesScrollers = true;
					scroll.HasHorizontalScroller = true;
					break;
				case ScrollPolicy.Always:
					scroll.AutohidesScrollers = false;
					scroll.HasHorizontalScroller = true;
					break;
				case ScrollPolicy.Never:
					scroll.HasHorizontalScroller = false;
					break;
				}
			}
		}

		ScrollControlBackend vertScroll;
		public IScrollControlBackend CreateVerticalScrollControl ()
		{
			if (vertScroll == null)
				vertScroll = new ScrollControlBackend (ApplicationContext, scroll, true);
			return vertScroll;
		}

		ScrollControlBackend horScroll;
		public IScrollControlBackend CreateHorizontalScrollControl ()
		{
			if (horScroll == null)
				horScroll = new ScrollControlBackend (ApplicationContext, scroll, false);
			return horScroll;
		}

		void OnScrolled (object o, EventArgs e)
		{
			if (vertScroll != null)
				vertScroll.NotifyValueChanged ();
			if (horScroll != null)
				horScroll.NotifyValueChanged ();
		}

		protected override Size GetNaturalSize ()
		{
			return EventSink.GetDefaultNaturalSize ();
		}
		
		protected abstract NSTableView CreateView ();
		protected abstract string SelectionChangeEventName { get; }
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TableViewEvent) {
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					selChangeObserver = NSNotificationCenter.DefaultCenter.AddObserver (new NSString (SelectionChangeEventName), HandleTreeSelectionDidChange, Table);
					break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TableViewEvent) {
				switch ((TableViewEvent)eventId) {
				case TableViewEvent.SelectionChanged:
					if (selChangeObserver != null)
						NSNotificationCenter.DefaultCenter.RemoveObserver (selChangeObserver);
					break;
				}
			}
		}

		void HandleTreeSelectionDidChange (NSNotification notif)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnSelectionChanged ();
			});
		}
		
		public void SetSelectionMode (SelectionMode mode)
		{
			Table.AllowsMultipleSelection = mode == SelectionMode.Multiple;
		}

		public virtual NSTableColumn AddColumn (ListViewColumn col)
		{
			var tcol = new NSTableColumn ();
			tcol.Editable = true;
			cols.Add (tcol);
			var c = CellUtil.CreateCell (ApplicationContext, Table, this, col.Views, cols.Count - 1);
			tcol.DataCell = c;
			Table.AddColumn (tcol);
			var hc = new NSTableHeaderCell ();
			hc.Title = col.Title ?? "";
			tcol.HeaderCell = hc;
			Widget.InvalidateIntrinsicContentSize ();
			return tcol;
		}
		object IColumnContainerBackend.AddColumn (ListViewColumn col)
		{
			return AddColumn (col);
		}
		
		public void RemoveColumn (ListViewColumn col, object handle)
		{
			Table.RemoveColumn ((NSTableColumn)handle);
		}

		public void UpdateColumn (ListViewColumn col, object handle, ListViewColumnChange change)
		{
			NSTableColumn tcol = (NSTableColumn) handle;
			tcol.DataCell = CellUtil.CreateCell (ApplicationContext, Table, this, col.Views, cols.IndexOf (tcol));
		}

		public void SelectAll ()
		{
			Table.SelectAll (null);
		}

		public void UnselectAll ()
		{
			Table.DeselectAll (null);
		}

		public void ScrollToRow (int row)
		{
			Table.ScrollRowToVisible (row);
		}
		
		public abstract object GetValue (object pos, int nField);
		
		public abstract void SetValue (object pos, int nField, object value);

		public abstract void SetCurrentEventRow (object pos);

		nfloat ICellSource.RowHeight {
			get { return Table.RowHeight; }
			set { Table.RowHeight = value; }
		}
		
		public bool BorderVisible {
			get { return scroll.BorderType == NSBorderType.BezelBorder;}
			set {
				scroll.BorderType = value ? NSBorderType.BezelBorder : NSBorderType.NoBorder;
			}
		}

		public bool HeadersVisible {
			get {
				return Table.HeaderView != null;
			}
			set {
				if (value) {
					if (Table.HeaderView == null)
						Table.HeaderView = new NSTableHeaderView ();
				} else {
					Table.HeaderView = null;
				}
			}
		}

		public GridLines GridLinesVisible
		{
			get { return Table.GridStyleMask.ToXwtValue (); }
			set { Table.GridStyleMask = value.ToMacValue (); }
		}
	}
	
	class ScrollView: NSScrollView, IViewObject
	{
		public ViewBackend Backend { get; set; }
		public NSView View {
			get { return this; }
		}
	}
}

