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
using System.Collections.Generic;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using Xwt.Backends;

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
		ApplicationContext context;

		public List<ICellRenderer> Cells {
			get {
				return cells;
			}
		}

		public CompositeCell (ApplicationContext context, Orientation dir, ICellSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			direction = dir;
			this.context = context;
			this.source = source;
		}
		
		public CompositeCell (IntPtr p): base (p)
		{
		}

		public ApplicationContext Context {
			get { return context; }
		}

		#region ICellDataSource implementation

		object ICellDataSource.GetValue (IDataField field)
		{
			return source.GetValue (tablePosition.Position, field.Index);
		}

		#endregion

		public void SetValue (IDataField field, object value)
		{
			source.SetValue (tablePosition.Position, field.Index, value);
		}

		public void SetCurrentEventRow ()
		{
			source.SetCurrentEventRow (tablePosition.Position);
		}

		public override NSObject Copy (NSZone zone)
		{
			var ob = (ICopiableObject) base.Copy (zone);
			ob.CopyFrom (this);
			return (NSObject) ob;
		}

		void ICopiableObject.CopyFrom (object other)
		{
			var ob = (CompositeCell)other;
			if (ob.source == null)
				throw new ArgumentException ("Cannot copy from a CompositeCell with a null `source`");
			context = ob.context;
			source = ob.source;
			val = ob.val;
			tablePosition = ob.tablePosition;
			direction = ob.direction;
			trackingCell = ob.trackingCell;
			cells = new List<ICellRenderer> ();
			foreach (var c in ob.cells) {
				var copy = (ICellRenderer) Activator.CreateInstance (c.GetType ());
				copy.CopyFrom (c);
				AddCell (copy);
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
			cell.CellContainer = this;
			cells.Add (cell);
		}
		
		public void Fill ()
		{
			foreach (var c in cells) {
				c.Backend.CurrentCell = (NSCell) c;
				c.Backend.CurrentPosition = tablePosition;
				c.Backend.Frontend.Load (this);
				c.Fill ();
			}

			var s = CellSize;
		}

		IEnumerable<ICellRenderer> VisibleCells {
			get { return cells.Where (c => c.Backend.Frontend.Visible); }
		}

		CGSize CalcSize ()
		{
			nfloat w = 0;
			nfloat h = 0;
			foreach (NSCell c in VisibleCells) {
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
			return new CGSize (w, h);
		}

		public override CGSize CellSizeForBounds (CGRect bounds)
		{
			return CalcSize ();
		}

		public override NSBackgroundStyle BackgroundStyle {
			get {
				return base.BackgroundStyle;
			}
			set {
				base.BackgroundStyle = value;
				foreach (NSCell c in cells)
					c.BackgroundStyle = value;
			}
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
		
		public override void DrawInteriorWithFrame (CGRect cellFrame, NSView inView)
		{
			CGContext ctx = NSGraphicsContext.CurrentContext.GraphicsPort;
			ctx.SaveState ();
			ctx.AddRect (cellFrame);
			ctx.Clip ();
			foreach (CellPos cp in GetCells(cellFrame))
				cp.Cell.DrawInteriorWithFrame (cp.Frame, inView);
			ctx.RestoreState ();
		}
		
		public override void Highlight (bool flag, CGRect withFrame, NSView inView)
		{
			foreach (CellPos cp in GetCells(withFrame)) {
				cp.Cell.Highlight (flag, cp.Frame, inView);
			}
		}
		
		public override NSCellHit HitTest (NSEvent forEvent, CGRect inRect, NSView ofView)
		{
			foreach (CellPos cp in GetCells(inRect)) {
				var h = cp.Cell.HitTest (forEvent, cp.Frame, ofView);
				if (h != NSCellHit.None)
					return h;
			}
			return NSCellHit.None;
		}

		public override bool TrackMouse (NSEvent theEvent, CGRect cellFrame, NSView controlView, bool untilMouseUp)
		{
			var c = GetHitCell (theEvent, cellFrame, controlView);
			if (c != null)
				return c.Cell.TrackMouse (theEvent, c.Frame, controlView, untilMouseUp);
			else
				return base.TrackMouse (theEvent, cellFrame, controlView, untilMouseUp);
		}

		public CGRect GetCellRect (CGRect cellFrame, NSCell cell)
		{
			foreach (var c in GetCells (cellFrame)) {
				if (c.Cell == cell)
					return c.Frame;
			}
			return CGRect.Empty;
		}

		CellPos GetHitCell (NSEvent theEvent, CGRect cellFrame, NSView controlView)
		{
			foreach (CellPos cp in GetCells(cellFrame)) {
				var h = cp.Cell.HitTest (theEvent, cp.Frame, controlView);
				if (h != NSCellHit.None)
					return cp;
			}
			return null;
		}
		
		IEnumerable<CellPos> GetCells (CGRect cellFrame)
		{
			if (direction == Orientation.Horizontal) {

				int nexpands = 0;
				double requiredSize = 0;
				double availableSize = cellFrame.Width;

				var sizes = new Dictionary<ICellRenderer, double> ();

				// Get the natural size of each child
				foreach (var bp in VisibleCells) {
					var s = ((NSCell)bp).CellSize;
					sizes [bp] = s.Width;
					requiredSize += s.Width;
					if (bp.Backend.Frontend.Expands)
						nexpands++;
				}

				double remaining = availableSize - requiredSize;
				if (remaining > 0) {
					var expandRemaining = new SizeSplitter (remaining, nexpands);
					foreach (var bp in VisibleCells) {
						if (bp.Backend.Frontend.Expands)
							sizes [bp] += (nfloat)expandRemaining.NextSizePart ();
					}
				}

				double x = cellFrame.X;
				foreach (var s in sizes) {
					yield return new CellPos () { Cell = (NSCell)s.Key, Frame = new CGRect (x, cellFrame.Y, s.Value, cellFrame.Height) };
					x += s.Value;
				}
			} else {
				nfloat y = cellFrame.Y;
				foreach (NSCell c in VisibleCells) {
					var s = c.CellSize;
					var f = new CGRect (cellFrame.X, y, s.Width, cellFrame.Height);
					y += s.Height;
					yield return new CellPos () { Cell = c, Frame = f };
				}
			}
		}
		
		class CellPos
		{
			public NSCell Cell;
			public CGRect Frame;
		}

		class SizeSplitter
		{
			int rem;
			int part;

			public SizeSplitter (double total, int numParts)
			{
				if (numParts > 0)
				{
					part = ((int)total) / numParts;
					rem = ((int)total) % numParts;
				}
			}

			public double NextSizePart ()
			{
				if (rem > 0)
				{
					rem--;
					return part + 1;
				}
				else
					return part;
			}
		}
	}
}
