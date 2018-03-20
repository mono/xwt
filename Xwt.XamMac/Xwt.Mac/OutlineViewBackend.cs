//
// OutlineViewBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Hywel Thomas <hywel.w.thomas@gmail.com>
//       strnadj <jan.strnadek@gmail.com>
//
// Copyright (c) 2014 Xamarin Inc
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
using System.Linq;
using AppKit;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class OutlineViewBackend : NSOutlineView, IViewObject
	{
		NSTrackingArea trackingArea;

		public OutlineViewBackend (TreeViewBackend viewBackend)
		{
			Backend = viewBackend;
			AllowsColumnReordering = false;
		}

		public NSView View {
			get { return this; }
		}

		public ViewBackend Backend { get; set; }

		public override NSObject WeakDataSource {
			get { return base.WeakDataSource; }
			set {
				base.WeakDataSource = value;
				AutosizeColumns ();
			}
		}

		bool animationsEnabled = true;
		public bool AnimationsEnabled {
			get { return animationsEnabled; }
			set { animationsEnabled = value; }
		}

		public override void AddColumn (NSTableColumn tableColumn)
		{
			base.AddColumn (tableColumn);
			AutosizeColumns ();
		}

		internal void AutosizeColumns ()
		{
			if (DataSource == null || RowCount == 0)
				return;
			var columns = TableColumns ();
			if (columns.Length == 1 && columns[0].ResizingMask.HasFlag (NSTableColumnResizing.Autoresizing))
				return;
			var needsSizeToFit = false;
			for (nint i = 0; i < columns.Length; i++) {
				AutosizeColumn (columns[i], i);
				needsSizeToFit |= columns[i].ResizingMask.HasFlag (NSTableColumnResizing.Autoresizing);
			}
			if (needsSizeToFit)
				SizeToFit ();
		}

		void AutosizeColumn (NSTableColumn tableColumn, nint colIndex)
		{
			var contentWidth = tableColumn.HeaderCell.CellSize.Width;
			if (!tableColumn.ResizingMask.HasFlag (NSTableColumnResizing.UserResizingMask)) {
				contentWidth = Delegate.GetSizeToFitColumnWidth (this, colIndex);
				if (!tableColumn.ResizingMask.HasFlag (NSTableColumnResizing.Autoresizing))
					tableColumn.Width = contentWidth;
			}
			tableColumn.MinWidth = contentWidth;
		}

		public override void ExpandItem (NSObject item)
		{
			BeginExpandCollapseAnimation ();
			base.ExpandItem (item);
			EndExpandCollapseAnimation ();
			QueueColumnResize ();
		}

		public override void ExpandItem (NSObject item, bool expandChildren)
		{
			BeginExpandCollapseAnimation ();
			base.ExpandItem (item, expandChildren);
			EndExpandCollapseAnimation ();
			QueueColumnResize ();
		}

		public override void CollapseItem (NSObject item)
		{
			BeginExpandCollapseAnimation ();
			base.CollapseItem (item);
			EndExpandCollapseAnimation ();
			QueueColumnResize ();
		}

		public override void CollapseItem (NSObject item, bool collapseChildren)
		{
			BeginExpandCollapseAnimation ();
			base.CollapseItem (item, collapseChildren);
			EndExpandCollapseAnimation ();
			QueueColumnResize ();
		}

		public override void NoteHeightOfRowsWithIndexesChanged(NSIndexSet indexSet)
		{
			BeginExpandCollapseAnimation();
			base.NoteHeightOfRowsWithIndexesChanged(indexSet);
			EndExpandCollapseAnimation();
		}

		void BeginExpandCollapseAnimation ()
		{
			if (!AnimationsEnabled) {
				NSAnimationContext.BeginGrouping ();
				NSAnimationContext.CurrentContext.Duration = 0;
			}
		}

		void EndExpandCollapseAnimation ()
		{
			if (!AnimationsEnabled)
				NSAnimationContext.EndGrouping ();
		}

		public override void ReloadData ()
		{
			base.ReloadData ();
			QueueColumnResize ();
		}

		public override void ReloadData (Foundation.NSIndexSet rowIndexes, Foundation.NSIndexSet columnIndexes)
		{
			base.ReloadData (rowIndexes, columnIndexes);
			QueueColumnResize ();
		}

		public override void ReloadItem (Foundation.NSObject item)
		{
			base.ReloadItem (item);
			QueueColumnResize ();
		}

		public override void ReloadItem (Foundation.NSObject item, bool reloadChildren)
		{
			base.ReloadItem (item, reloadChildren);
			QueueColumnResize ();
		}

		bool columnResizeQueued;
		void QueueColumnResize ()
		{
			if (!columnResizeQueued) {
				columnResizeQueued = true;
				(Backend.ApplicationContext.Toolkit.GetSafeBackend (Backend.ApplicationContext.Toolkit) as ToolkitEngineBackend).InvokeBeforeMainLoop (delegate {
					columnResizeQueued = false;
					AutosizeColumns ();
				});
			}
		}

		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
			if (Backend.Cursor != null)
				AddCursorRect (Bounds, Backend.Cursor);
		}

		public override void UpdateTrackingAreas ()
		{
			this.UpdateEventTrackingArea (ref trackingArea);
		}

		public override void RightMouseDown (NSEvent theEvent)
		{
			if (!this.HandleMouseDown (theEvent))
				base.RightMouseDown (theEvent);
		}

		public override void RightMouseUp (NSEvent theEvent)
		{
			if (!this.HandleMouseUp (theEvent))
				base.RightMouseUp (theEvent);
		}

		public override void MouseDown (NSEvent theEvent)
		{
			if (!this.HandleMouseDown (theEvent))
				base.MouseDown (theEvent);
		}

		public override void MouseUp (NSEvent theEvent)
		{
			if (!this.HandleMouseUp (theEvent))
				base.MouseUp (theEvent);
		}

		public override void OtherMouseDown (NSEvent theEvent)
		{
			if (!this.HandleMouseDown (theEvent))
				base.OtherMouseDown (theEvent);
		}

		public override void OtherMouseUp (NSEvent theEvent)
		{
			if (!this.HandleMouseUp (theEvent))
				base.OtherMouseUp (theEvent);
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			this.HandleMouseEntered (theEvent);
		}

		public override void MouseExited (NSEvent theEvent)
		{
			this.HandleMouseExited (theEvent);
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			if (!this.HandleMouseMoved (theEvent))
				base.MouseMoved (theEvent);
		}

		public override void RightMouseDragged (NSEvent theEvent)
		{
			if (!this.HandleMouseMoved (theEvent))
				base.RightMouseDragged (theEvent);
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			if (!this.HandleMouseMoved (theEvent))
				base.MouseDragged (theEvent);
		}

		public override void OtherMouseDragged (NSEvent theEvent)
		{
			if (!this.HandleMouseMoved (theEvent))
				base.OtherMouseDragged (theEvent);
		}

		public override void KeyDown (NSEvent theEvent)
		{
			if (!this.HandleKeyDown (theEvent))
				base.KeyDown (theEvent);
		}

		public override void KeyUp (NSEvent theEvent)
		{
			if (!this.HandleKeyUp (theEvent))
				base.KeyUp (theEvent);
		}
	}
}