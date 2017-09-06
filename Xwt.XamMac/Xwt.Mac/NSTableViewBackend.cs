//
// NSTableViewBackend.cs
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
using CoreGraphics;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class NSTableViewBackend : NSTableView
	{
		IWidgetEventSink eventSink;
		protected ApplicationContext context;
		NSTrackingArea trackingArea;	// Captures Mouse Entered, Exited, and Moved events

		class ListDelegate: NSTableViewDelegate
		{
			public override nfloat GetRowHeight (NSTableView tableView, nint row)
			{
				var height = tableView.RowHeight;
				for (int i = 0; i < tableView.ColumnCount; i++) {
					var cell = tableView.GetCell (i, row);
					if (cell != null)
						height = (nfloat) Math.Max (height, cell.CellSize.Height);
				}
				return height;
			}
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
			var columns = TableColumns ();
			foreach (var col in columns)
				AutosizeColumn (col);
			if (columns.Any (c => c.ResizingMask.HasFlag (NSTableColumnResizing.Autoresizing)))
				SizeToFit ();
		}

		void AutosizeColumn (NSTableColumn tableColumn)
		{
			var column = IndexOfColumn (tableColumn);

			var s = tableColumn.HeaderCell.CellSize;
			if (!tableColumn.ResizingMask.HasFlag (NSTableColumnResizing.UserResizingMask)) {
				for (int i = 0; i < base.RowCount; i++)
				{
					var cell = base.GetCell (column, i);
					s.Width = (nfloat)Math.Max (s.Width, cell.CellSize.Width);
				}
				if (!tableColumn.ResizingMask.HasFlag (NSTableColumnResizing.Autoresizing))
					tableColumn.Width = s.Width;
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

		public override void ReloadData ()
		{
			base.ReloadData ();
			QueueColumnResize ();
		}

		public override void ReloadData (NSIndexSet rowIndexes, NSIndexSet columnIndexes)
		{
			base.ReloadData (rowIndexes, columnIndexes);
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

		public NSTableViewBackend(IWidgetEventSink eventSink, ApplicationContext context) {
			this.context = context;
			this.eventSink = eventSink;
			this.Delegate = new ListDelegate ();
			AllowsColumnReordering = false;
		}


		public ViewBackend Backend { get; set; }

		public NSTableView View {
			get { return this; }
		}

		public override void UpdateTrackingAreas ()
		{
			if (trackingArea != null) {
				RemoveTrackingArea (trackingArea);
				trackingArea.Dispose ();
			}
			CGRect viewBounds = this.Bounds;
			var options = NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.MouseEnteredAndExited;
			trackingArea = new NSTrackingArea (viewBounds, options, this, null);
			AddTrackingArea (trackingArea);
		}

		public override void RightMouseDown (NSEvent theEvent)
		{
			base.RightMouseDown (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Right;
			args.IsContextMenuTrigger = theEvent.TriggersContextMenu ();
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
			args.IsContextMenuTrigger = theEvent.TriggersContextMenu ();
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
			args.Button = (PointerButton) (int)theEvent.ButtonNumber + 1;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonReleased (args);
			});
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			context.InvokeUserCode (eventSink.OnMouseEntered);
		}

		public override void MouseExited (NSEvent theEvent)
		{
			context.InvokeUserCode (eventSink.OnMouseExited);
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			MouseMovedEventArgs args = new MouseMovedEventArgs ((long) TimeSpan.FromSeconds (theEvent.Timestamp).TotalMilliseconds, p.X, p.Y);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseMoved (args);
			});
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			MouseMovedEventArgs args = new MouseMovedEventArgs ((long) TimeSpan.FromSeconds (theEvent.Timestamp).TotalMilliseconds, p.X, p.Y);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseMoved (args);
			});
		}

		public override void KeyDown (NSEvent theEvent)
		{
			var keyArgs = theEvent.ToXwtKeyEventArgs ();
			context.InvokeUserCode (delegate {
				eventSink.OnKeyPressed (keyArgs);
			});
			if (keyArgs.Handled)
				return;

			var textArgs = new TextInputEventArgs (theEvent.Characters);
			if (!String.IsNullOrEmpty(theEvent.Characters))
				context.InvokeUserCode (delegate {
					eventSink.OnTextInput (textArgs);
				});
			if (textArgs.Handled)
				return;

			base.KeyDown (theEvent);
		}

		public override void KeyUp (NSEvent theEvent)
		{
			var keyArgs = theEvent.ToXwtKeyEventArgs ();
			context.InvokeUserCode (delegate {
				eventSink.OnKeyReleased (keyArgs);
			});
			if (!keyArgs.Handled)
				base.KeyUp (theEvent);
		}
	}
}