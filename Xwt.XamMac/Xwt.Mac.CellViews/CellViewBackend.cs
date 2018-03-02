//
// CellViewBackend.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class CellViewBackend: ICellViewBackend, ICanvasCellViewBackend
	{
		WidgetEvent enabledEvents;

		public CellViewBackend (NSTableView table, int column)
		{
			Table = table;
			Column = column;
		}

		public virtual void InitializeBackend (object frontend, ApplicationContext context)
		{
			Frontend = (ICellViewFrontend)frontend;
			Context = context;
		}

		public ICellViewFrontend Frontend { get; private set; }

		public ICellViewEventSink EventSink { get; private set; }

		public ApplicationContext Context { get; private set; }

		internal NSView CurrentCellView { get; private set; }

		public int Column { get; private set; }

		public NSTableView Table { get; internal set; }

		internal ITablePosition CurrentPosition { get; private set; }

		internal void Load (ICellRenderer cell)
		{
			CurrentCellView = (NSView)cell;
			CurrentPosition = cell.CellContainer.TablePosition;
			EventSink = Frontend.Load (cell.CellContainer);
		}
		
		public virtual void EnableEvent (object eventId)
		{
			if (eventId is WidgetEvent)
				enabledEvents |= (WidgetEvent)eventId;
		}
		
		public virtual void DisableEvent (object eventId)
		{
			if (eventId is WidgetEvent)
				enabledEvents &= ~(WidgetEvent)eventId;
		}

		public bool GetIsEventEnabled (WidgetEvent eventId)
		{
			return enabledEvents.HasFlag (eventId);
		}

		public void QueueDraw ()
		{
			CurrentCellView.NeedsDisplay = true;
		}

		public void QueueResize ()
		{
			CurrentCellView.NeedsDisplay = true;
			((ICellRenderer)CurrentCellView).CellContainer.InvalidateRowHeight ();
		}

		public Rectangle CellBounds {
			get {
				return CurrentCellView.ConvertRectToView (CurrentCellView.Frame, Table).ToXwtRect ();
			}
		}

		public Rectangle BackgroundBounds {
			get {
				return CurrentCellView.ConvertRectToView (CurrentCellView.Frame, ((ICellRenderer)CurrentCellView).CellContainer.Superview).ToXwtRect ();
			}
		}

		public bool Selected {
			get {
				if (CurrentPosition is TableRow)
					return Table.IsRowSelected (((TableRow)CurrentPosition).Row);
				return Table.IsRowSelected (Table.RowForView (CurrentCellView));
			}
		}

		public bool HasFocus {
			get {
				return CurrentCellView?.Window != null && CurrentCellView.Window.FirstResponder == CurrentCellView;
			}
		}

		public bool IsHighlighted {
			get {
				// TODO
				return false;
			}
		}
	}
}

