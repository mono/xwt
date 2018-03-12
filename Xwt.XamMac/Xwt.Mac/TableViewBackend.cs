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
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using Xwt.Backends;
using Xwt.Drawing;

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

		NSTableView ICellSource.TableView { get { return Table; } }

		List<NSTableColumn> ICellSource.Columns {
			get { return cols; }
		}

		protected List<NSTableColumn> Columns {
			get { return cols; }
		}

		public override void Initialize ()
		{
			Table = CreateView ();
			Table.ColumnAutoresizingStyle = NSTableViewColumnAutoresizingStyle.Sequential;
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
			ApplicationContext.InvokeUserCode (EventSink.OnSelectionChanged);
		}

		public SelectionMode SelectionMode { get; private set; }
		
		public void SetSelectionMode (SelectionMode mode)
		{
			SelectionMode = mode;
			Table.AllowsMultipleSelection = mode == SelectionMode.Multiple;
			if (mode == SelectionMode.None && Table.SelectedRowCount > 0)
				UnselectAll ();
		}

		public virtual NSTableColumn AddColumn (ListViewColumn col)
		{
			var tcol = new TableColumn (ApplicationContext, this, Table);
			cols.Add (tcol);
			tcol.UpdateColumn (col);
			Table.AddColumn (tcol);
			return tcol;
		}
		object IColumnContainerBackend.AddColumn (ListViewColumn col)
		{
			return AddColumn (col);
		}
		
		public void RemoveColumn (ListViewColumn col, object handle)
		{
			var tcol = (NSTableColumn)handle;
			cols.Remove (tcol);
			Table.RemoveColumn (tcol);
		}

		public void UpdateColumn (ListViewColumn col, object handle, ListViewColumnChange change)
		{
			var tcol = handle as TableColumn;
			if (tcol != null)
				tcol.UpdateColumn (col, change);
		}

		public Rectangle GetCellBounds (int row, CellView cell, bool includeMargin)
		{
			var rect = Rectangle.Zero;
			var cellBackend = cell.GetBackend () as CellViewBackend;
			var container = Table.GetView (cellBackend.Column, row, false) as CompositeCell;
			if (container != null) {
				var cellView = container.GetCellViewForBackend (cellBackend);
				rect = cellView.ConvertRectToView (new CGRect (CGPoint.Empty, cellView.Frame.Size), Table).ToXwtRect ();
				rect.Y -= scroll.DocumentVisibleRect.Y;
				rect.X -= scroll.DocumentVisibleRect.X;
			}
			return rect;
		}

		public Rectangle GetRowBounds (int row, bool includeMargin)
		{
			var rect = Rectangle.Zero;
			var rowView = Table.GetRowView (row, false);
			if (rowView != null) {
				rect = rowView.Frame.ToXwtRect ();
				rect.Y -= scroll.DocumentVisibleRect.Y;
				rect.X -= scroll.DocumentVisibleRect.X;
			}
			return rect;
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

		public void StartEditingCell (int row, CellView cell)
		{
			// TODO
		}

		public abstract object GetValue (object pos, int nField);
		
		public abstract void SetValue (object pos, int nField, object value);

		public abstract void SetCurrentEventRow (object pos);

		public abstract void InvalidateRowHeight (object pos);

		public bool BorderVisible {
			get { return scroll.BorderType == NSBorderType.BezelBorder;}
			set {
				scroll.BorderType = value ? NSBorderType.BezelBorder : NSBorderType.NoBorder;
			}
		}

		public bool UseAlternatingRowColors {
			get { return Table.UsesAlternatingRowBackgroundColors; }
			set { Table.UsesAlternatingRowBackgroundColors = value; }
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

		public override Color BackgroundColor
		{
			get { return Table.BackgroundColor.ToXwtColor (); }
			set { Table.BackgroundColor = value.ToNSColor (); }
		}
	}

	class TableColumn : NSTableColumn
	{
		readonly ICellSource backend;
		readonly ApplicationContext context;

		public CompositeCell DataView { get; private set; }

		List<WeakReference> CachedViews = new List<WeakReference> ();

		public TableColumn (ApplicationContext context, ICellSource backend, NSTableView table)
		{
			this.context = context;
			Identifier = GetHashCode ().ToString (); // this is used to identify cached views
			this.backend = backend;
			TableView = table;
		}

		public CompositeCell CreateNewView ()
		{
			CleanViewCache ();
			var view = DataView.Copy () as CompositeCell;
			view.Identifier = Identifier;
			// Cocoa will manage the native views in the background and eventually dispose
			// them without letting us know. In order to keep track of the active views
			// we store a weak ref for each view to not disturb the internal Cocoa caching login.
			CachedViews.Add (new WeakReference (view));
			return view;
		}

		void UpdateCachedViews (ICollection<CellView> cells)
		{
			if (CachedViews.Count == 0)
				return;
			var col = backend.Columns.IndexOf (this);
			foreach (var cached in CachedViews) {
				// update only the alive and not disposed views
				if (cached.IsAlive) {
					var view = cached.Target as CompositeCell;
					if (view?.IsDisposed == false)
						CellUtil.UpdateCellView (view, backend, cells, col);
				}
			}

			CleanViewCache ();
		}

		void CleanViewCache ()
		{
			// remove any GCd and/or disposed views
			CachedViews.RemoveAll (c => {
				if (!c.IsAlive)
					return true;
				var cell = c.Target as CompositeCell;
				if (cell == null || cell.IsDisposed)
					return true;
				return false;
			});
		}

		public void UpdateColumn (ListViewColumn col)
		{
			Editable = true;
			var hc = new NSTableHeaderCell {
				Title = col.Title ?? string.Empty
			};
			HeaderCell = hc;
			HeaderCell.Alignment = col.Alignment.ToNSTextAlignment ();

			DataView = CellUtil.CreateCellView (context, backend, col.Views, backend.Columns.IndexOf (this));
			DataView.Identifier = Identifier;
			UpdateCachedViews (col.Views);

			if (col.CanResize)
				ResizingMask |= NSTableColumnResizing.UserResizingMask;
			else
				ResizingMask &= ~NSTableColumnResizing.UserResizingMask;
			if (col.Expands)
				ResizingMask |= NSTableColumnResizing.Autoresizing;
			else
				ResizingMask &= ~NSTableColumnResizing.Autoresizing;
			SizeToFit ();
			TableView?.InvalidateIntrinsicContentSize ();
		}

		public void UpdateColumn (ListViewColumn col, ListViewColumnChange change)
		{
			if (TableView == null)
				throw new InvalidOperationException ("Add the column to a table first");
			switch (change) {
			case ListViewColumnChange.CanResize:
				if (col.CanResize)
					ResizingMask |= NSTableColumnResizing.UserResizingMask;
				else
					ResizingMask &= ~NSTableColumnResizing.UserResizingMask;
				break;
			case ListViewColumnChange.Expanding:
				if (col.Expands)
					ResizingMask |= NSTableColumnResizing.Autoresizing;
				else
					ResizingMask &= ~NSTableColumnResizing.Autoresizing;
				break;
			case ListViewColumnChange.Cells:
				DataView = CellUtil.CreateCellView (context, backend, col.Views, backend.Columns.IndexOf (this));
				DataView.Identifier = Identifier;
				UpdateCachedViews (col.Views);
				TableView.ReloadData ();
				break;
			case ListViewColumnChange.Title:
				HeaderCell.Title = col.Title ?? string.Empty;
				if (!col.CanResize)
					SizeToFit ();
				break;
			case ListViewColumnChange.Alignment:
				HeaderCell.Alignment = col.Alignment.ToNSTextAlignment ();
				break;
			}
		}
	}

	class TableRowView : NSTableRowView
	{

		public override bool Selected {
			get {
				return base.Selected;
			}
			set {
				base.Selected = value;
				// the first time NSTableView is presented the background
				// may be drawn already and it will not be redrawn even
				// if Selection has been changed.
				NeedsDisplay = true;
			}
		}

		public override void DrawSelection (CGRect dirtyRect)
		{
			if (EffectiveAppearance.Name == NSAppearance.NameVibrantDark &&
			    SelectionHighlightStyle != NSTableViewSelectionHighlightStyle.None) {
				(Selected ? NSColor.AlternateSelectedControl : BackgroundColor).SetFill ();
				var path = NSBezierPath.FromRect (dirtyRect);
				path.Fill ();
			} else
				base.DrawSelection (dirtyRect);
		}

		public override void DrawBackground (CGRect dirtyRect)
		{
			if (Selected && EffectiveAppearance.Name == NSAppearance.NameVibrantDark &&
				SelectionHighlightStyle != NSTableViewSelectionHighlightStyle.None) {
				(Selected ? NSColor.AlternateSelectedControl : BackgroundColor).SetFill ();
				var path = NSBezierPath.FromRect (dirtyRect);
				path.Fill ();
			} else
				base.DrawBackground (dirtyRect);
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

