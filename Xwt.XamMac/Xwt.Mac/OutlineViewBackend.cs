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
using AppKit;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class OutlineViewBackend : NSOutlineView
	{
		ITreeViewEventSink eventSink;
		protected ApplicationContext context;
		NSTrackingArea trackingArea;

		public OutlineViewBackend (ITreeViewEventSink eventSink, ApplicationContext context)
		{
			this.context = context;
			this.eventSink = eventSink;
			AllowsColumnReordering = false;
		}

		public NSOutlineView View {
			get { return this; }
		}

		public override NSObject WeakDataSource {
			get { return base.WeakDataSource; }
			set {
				base.WeakDataSource = value;
				AutosizeColumns ();
			}
		}

		public override void AddColumn (NSTableColumn tableColumn)
		{
			base.AddColumn (tableColumn);
			AutosizeColumns ();
		}

		internal void AutosizeColumns ()
		{
			foreach (var col in TableColumns ())
				AutosizeColumn (col);
		}

		void AutosizeColumn (NSTableColumn tableColumn)
		{
			var column = IndexOfColumn (tableColumn);

			var s = tableColumn.HeaderCell.CellSize;
			if (!tableColumn.ResizingMask.HasFlag (NSTableColumnResizing.UserResizingMask)) {
				for (int i = 0; i < base.RowCount; i++) {
					var cell = GetCell (column, i);
					if (column == 0)
					{ // first column contains expanders
						var f = GetCellFrame (column, i);
						s.Width = (nfloat)Math.Max (s.Width, f.X + cell.CellSize.Width);
					}
					else
						s.Width = (nfloat)Math.Max (s.Width, cell.CellSize.Width);
				}
			}
			tableColumn.MinWidth = s.Width;
		}

		nint IndexOfColumn (NSTableColumn tableColumn)
		{
			nint icol = -1;
			foreach (var col in TableColumns ()) {
				icol++;
				if (col == tableColumn)
					return icol;
			}
			return icol;
		}

		public override void ExpandItem (NSObject item)
		{
			base.ExpandItem (item);
			QueueColumnResize ();
		}

		public override void ExpandItem (NSObject item, bool expandChildren)
		{
			base.ExpandItem (item, expandChildren);
			QueueColumnResize ();
		}

		public override void CollapseItem (NSObject item)
		{
			base.CollapseItem (item);
			QueueColumnResize ();
		}

		public override void CollapseItem (NSObject item, bool collapseChildren)
		{
			base.CollapseItem (item, collapseChildren);
			QueueColumnResize ();
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
				Application.MainLoop.QueueExitAction (delegate {
					columnResizeQueued = false;
					AutosizeColumns ();
				});
			}
		}

		public override void UpdateTrackingAreas ()
		{
			if (trackingArea != null) {
				RemoveTrackingArea (trackingArea);
				trackingArea.Dispose ();
			}
			var viewBounds = this.Bounds;
			var options = NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.MouseEnteredAndExited;
			trackingArea = new NSTrackingArea (viewBounds, options, this, null);
			AddTrackingArea (trackingArea);
		}

		public override void RightMouseDown (NSEvent theEvent)
		{
			base.RightMouseUp (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Right;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonPressed (args);
			});
		}

		public override void RightMouseUp (NSEvent theEvent)
		{
			base.RightMouseUp (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Right;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonReleased (args);
			});
		}

		public override void MouseDown (NSEvent theEvent)
		{
			base.MouseDown (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Left;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonPressed (args);
			});
		}

		public override void MouseUp (NSEvent theEvent)
		{
			base.MouseUp (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = (PointerButton) (int) theEvent.ButtonNumber + 1;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonReleased (args);
			});
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			base.MouseEntered (theEvent);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseEntered ();
			});
		}

		public override void MouseExited (NSEvent theEvent)
		{
			base.MouseExited (theEvent);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseExited ();
			});
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			base.MouseMoved (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			MouseMovedEventArgs args = new MouseMovedEventArgs ((long) TimeSpan.FromSeconds (theEvent.Timestamp).TotalMilliseconds, p.X, p.Y);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseMoved (args);
			});
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			base.MouseDragged (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			MouseMovedEventArgs args = new MouseMovedEventArgs ((long) TimeSpan.FromSeconds (theEvent.Timestamp).TotalMilliseconds, p.X, p.Y);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseMoved (args);
			});
		}
	}
}