// 
// CompositeCell.cs
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
using MonoMac.AppKit;
using MonoMac.Foundation;
using Xwt.Backends;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Xwt.Mac
{
	class CompositeCell: NSCell, ICopiableObject, ICellDataSource
	{
		ICellSource source;
		NSObject val;
		List<ICellRenderer> cells = new List<ICellRenderer> ();
		Orientation direction;
		NSCell trackingCell;
		ITablePosition tablePosition;

		static CompositeCell ()
		{
			Util.MakeCopiable<CompositeCell> ();
		}

		public CompositeCell (Orientation dir, ICellSource source)
		{
			direction = dir;
			this.source = source;
		}
		
		public CompositeCell (IntPtr p): base (p)
		{
		}

		#region ICellDataSource implementation

		object ICellDataSource.GetValue (IDataField field)
		{
			return source.GetValue (tablePosition.Position, field.Index);
		}

		#endregion

		void ICopiableObject.CopyFrom (object other)
		{
			var ob = (CompositeCell)other;
			source = ob.source;
			val = ob.val;
			tablePosition = ob.tablePosition;
			direction = ob.direction;
			trackingCell = ob.trackingCell;
			cells = new List<ICellRenderer> ();
			foreach (var c in ob.cells) {
				var copy = Activator.CreateInstance (c.GetType ());
				((ICopiableObject)copy).CopyFrom (c);
				cells.Add ((ICellRenderer) copy);
			}
			if (tablePosition != null)
				Fill ();
		}
		
		public override NSObject ObjectValue {
			get {
				return val;
			}
			set {
				val = value;
				if (val is ITablePosition) {
					tablePosition = (ITablePosition) val;
					Fill ();
				}
				else if (val is NSNumber) {
					tablePosition = new TableRow () {
						Row = ((NSNumber)val).Int32Value
					};
					Fill ();
				} else
					tablePosition = null;
			}
		}

		public override bool IsOpaque {
			get {
				var b = base.IsOpaque;
				return true;
			}
		}
		
		public void AddCell (ICellRenderer cell)
		{
			cells.Add (cell);
		}
		
		public void Fill ()
		{
			foreach (var c in cells)
				c.Fill (this);

			var s = CellSize;
			if (s.Height > source.RowHeight)
				source.RowHeight = s.Height;
		}
		
		public override void CalcDrawInfo (RectangleF aRect)
		{
			base.CalcDrawInfo (aRect);
		}

		SizeF CalcSize ()
		{
			float w = 0;
			float h = 0;
			foreach (NSCell c in cells) {
				var s = c.CellSize;
				if (direction == Orientation.Horizontal) {
					w += s.Width;
					if (s.Height > h)
						h = s.Height;
				} else {
					h += s.Height;
					if (s.Width > w)
						w = s.Width;
				}
			}
			return new SizeF (w, h);
		}

		public override SizeF CellSizeForBounds (RectangleF bounds)
		{
			return CalcSize ();
		}
		
		public override NSCellStateValue State {
			get {
				return base.State;
			}
			set {
				base.State = value;
				foreach (NSCell c in cells)
					c.State = value;
			}
		}
		
		public override bool Highlighted {
			get {
				return base.Highlighted;
			}
			set {
				base.Highlighted = value;
				foreach (NSCell c in cells)
					c.Highlighted = value;
			}
		}
		
		public override void DrawInteriorWithFrame (RectangleF cellFrame, NSView inView)
		{
			foreach (CellPos cp in GetCells(cellFrame))
				cp.Cell.DrawInteriorWithFrame (cp.Frame, inView);
		}
		
		public override void Highlight (bool flag, RectangleF withFrame, NSView inView)
		{
			foreach (CellPos cp in GetCells(withFrame)) {
				cp.Cell.Highlight (flag, cp.Frame, inView);
			}
		}
		
		public override NSCellHit HitTest (NSEvent forEvent, RectangleF inRect, NSView ofView)
		{
			foreach (CellPos cp in GetCells(inRect)) {
				var h = cp.Cell.HitTest (forEvent, cp.Frame, ofView);
				if (h != NSCellHit.None)
					return h;
			}
			return NSCellHit.None;
		}
		
		public override bool StartTracking (PointF startPoint, NSView inView)
		{
			foreach (NSCell c in cells) {
				if (c.StartTracking (startPoint, inView)) {
					trackingCell = c;
					return true;
				}
			}
			return false;
		}
		
		public override void StopTracking (PointF lastPoint, PointF stopPoint, NSView inView, bool mouseIsUp)
		{
			if (trackingCell != null) {
				try {
					trackingCell.StopTracking (lastPoint, stopPoint, inView, mouseIsUp);
				} finally {
					trackingCell = null;
				}
			}
		}
		
		public override bool ContinueTracking (PointF lastPoint, PointF currentPoint, NSView inView)
		{
			if (trackingCell != null)
				return trackingCell.ContinueTracking (lastPoint, currentPoint, inView);
			else
				return false;
		}
		
		IEnumerable<CellPos> GetCells (RectangleF cellFrame)
		{
			if (direction == Orientation.Horizontal) {
				foreach (NSCell c in cells) {
					var s = c.CellSize;
					var w = Math.Min (cellFrame.Width, s.Width);
					RectangleF f = new RectangleF (cellFrame.X, cellFrame.Y, w, cellFrame.Height);
					cellFrame.X += w;
					cellFrame.Width -= w;
					yield return new CellPos () { Cell = c, Frame = f };
				}
			} else {
				float y = cellFrame.Y;
				foreach (NSCell c in cells) {
					var s = c.CellSize;
					RectangleF f = new RectangleF (cellFrame.X, y, s.Width, cellFrame.Height);
					y += s.Height;
					yield return new CellPos () { Cell = c, Frame = f };
				}
			}
		}
		
		struct CellPos
		{
			public NSCell Cell;
			public RectangleF Frame;
		}
	}
}
