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
using ObjCRuntime;
using Xwt.Backends;

namespace Xwt.Mac
{
	class CompositeCell : NSView, ICopiableObject, ICellDataSource, INSCopying
	{
		ICellSource source;
		NSObject val;
		List<ICellRenderer> cells = new List<ICellRenderer> ();
		ITablePosition tablePosition;
		ApplicationContext context;

		public List<ICellRenderer> Cells {
			get {
				return cells;
			}
		}

		public CompositeCell (ApplicationContext context, ICellSource source)
		{
			if (source == null)
				throw new ArgumentNullException (nameof (source));
			this.context = context;
			this.source = source;
		}

		public CompositeCell (IntPtr p) : base (p)
		{
		}

		CompositeCell ()
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

		bool recalculatingHeight = false;
		public void InvalidateRowHeight ()
		{
			if (tablePosition != null && !recalculatingHeight) {
				recalculatingHeight = true;
				source.InvalidateRowHeight (tablePosition.Position);
				recalculatingHeight = false;
			}
		}

		public double GetRequiredHeightForWidth (double width)
		{
			Fill (false);
			double height = 0;
			foreach (var c in GetCells (new CGSize (width, -1)))
				height = Math.Max (height, c.Frame.Height);
			return height;
		}

		public override NSObject Copy ()
		{
			var ob = (ICopiableObject)base.Copy ();
			ob.CopyFrom (this);
			return (NSObject)ob;
		}

		NSObject INSCopying.Copy (NSZone zone)
		{
			var ob = (ICopiableObject)new CompositeCell ();
			ob.CopyFrom (this);
			return (NSObject)ob;
		}

		void ICopiableObject.CopyFrom (object other)
		{
			var ob = (CompositeCell)other;
			if (ob.source == null)
				throw new ArgumentException ("Cannot copy from a CompositeCell with a null `source`");
			Identifier = ob.Identifier;
			context = ob.context;
			source = ob.source;
			cells = new List<ICellRenderer> ();
			foreach (var c in ob.cells) {
				var copy = (ICellRenderer)Activator.CreateInstance (c.GetType ());
				copy.CopyFrom (c);
				AddCell (copy);
			}
			if (tablePosition != null)
				Fill (false);
		}

		public virtual NSObject ObjectValue {
			[Export ("objectValue")]
			get {
				return val;
			}
			[Export ("setObjectValue:")]
			set {
				val = value;
				if (val is ITablePosition) {
					tablePosition = (ITablePosition)val;
					Fill ();
				} else if (val is NSNumber) {
					tablePosition = new TableRow () {
						Row = ((NSNumber)val).Int32Value
					};
					Fill ();
				} else
					tablePosition = null;
			}
		}

		internal ITablePosition TablePosition {
			get { return tablePosition; }
		}

		public void AddCell (ICellRenderer cell)
		{
			cell.CellContainer = this;
			cells.Add (cell);
			AddSubview ((NSView)cell);
		}

		public void ClearCells ()
		{
			foreach (NSView cell in cells) {
				cell.RemoveFromSuperview ();
			}
			cells.Clear ();
		}

		public override CGRect Frame {
			get { return base.Frame; }
			set {
				var oldSize = base.Frame.Size;
				base.Frame = value;
				if (oldSize != value.Size && tablePosition != null) {
					Fill(false);
					double height = 0;
					foreach (var c in GetCells (value.Size)) {
						c.Cell.Frame = c.Frame;
						c.Cell.NeedsDisplay = true;
						height = Math.Max (height, c.Frame.Height);
					}
					if (Math.Abs(value.Height - height) > double.Epsilon)
						InvalidateRowHeight ();
					
				}
			}
		}

		public void Fill (bool reallocateCells = true)
		{
			foreach (var c in cells) {
				c.Backend.Load (c);
				c.Fill ();
			}
			if (!reallocateCells || Frame.IsEmpty)
				return;

			foreach (var c in GetCells (Frame.Size)) {
				c.Cell.Frame = c.Frame;
				c.Cell.NeedsDisplay = true;
			}
		}

		public NSView GetCellViewForBackend (ICellViewBackend backend)
		{
			return cells.FirstOrDefault (c => c.Backend == backend) as NSView;
		}

		CGSize CalcSize ()
		{
			nfloat w = 0;
			nfloat h = 0;
			foreach (var cell in cells) {
				if (!cell.Backend.Frontend.Visible)
					continue;
				var c = (NSView)cell;
				var s = c.FittingSize;
				w += s.Width;
				if (s.Height > h)
					h = s.Height;
			}
			return new CGSize (w, h);
		}

		public override CGSize FittingSize {
			get {
				return CalcSize ();
			}
		}

		static readonly Selector selSetBackgroundStyle = new Selector ("setBackgroundStyle:");

		NSBackgroundStyle backgroundStyle;

		public virtual NSBackgroundStyle BackgroundStyle {
			[Export ("backgroundStyle")]
			get {
				return backgroundStyle;
			}
			[Export ("setBackgroundStyle:")]
			set {
				backgroundStyle = value;
				foreach (NSView cell in cells)
					if (cell.RespondsToSelector (selSetBackgroundStyle)) {
						if (IntPtr.Size == 8)
							Messaging.void_objc_msgSend_Int64 (cell.Handle, selSetBackgroundStyle.Handle, (long)value);
						else
							Messaging.void_objc_msgSend_int (cell.Handle, selSetBackgroundStyle.Handle, (int)value);
					} else
						cell.NeedsDisplay = true;
			}
		}
		
		List <CellPos> GetCells (CGSize cellSize)
		{
			int nexpands = 0;
			double requiredSize = 0;
			double availableSize = cellSize.Width;

			var cellFrames = new List<CellPos> (cells.Count);

			// Get the natural size of each child
			foreach (var cell in cells) {
				if (!cell.Backend.Frontend.Visible)
					continue;
				var cellPos = new CellPos { Cell = (NSView)cell, Frame = CGRect.Empty };
				cellFrames.Add (cellPos);
				var size = cellPos.Cell.FittingSize;
				cellPos.Frame.Width = size.Width;
				requiredSize += size.Width;
				if (cell.Backend.Frontend.Expands)
					nexpands++;
			}

			double remaining = availableSize - requiredSize;
			if (remaining > 0) {
				var expandRemaining = new SizeSplitter (remaining, nexpands);
				foreach (var cellFrame in cellFrames) {
					if (((ICellRenderer)cellFrame.Cell).Backend.Frontend.Expands)
						cellFrame.Frame.Width += (nfloat)expandRemaining.NextSizePart ();
				}
			}

			double x = 0;
			foreach (var cellFrame in cellFrames) {
				var width = cellFrame.Frame.Width;
				var canvas = cellFrame.Cell as ICanvasCellRenderer;
				var height = (canvas != null) ? canvas.GetRequiredSize (SizeConstraint.WithSize (width)).Height : cellFrame.Cell.FittingSize.Height;
				// y-align only if the cell has a valid height, otherwise we're just recalculating the required size
				var y = cellSize.Height > 0 ? (cellSize.Height - height) / 2 : 0;
				cellFrame.Frame = new CGRect (x, y, width, height);
				x += width;
			}
			return cellFrames;
		}

		class CellPos
		{
			public NSView Cell;
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
				} else
					return part;
			}
		}

		bool isDisposed;

		public bool IsDisposed {
			get {
				try {
					// Cocoa may dispose the native view in NSView based table mode
					// in this case Handle and SuperHandle will become Zero.
					return isDisposed || Handle == IntPtr.Zero || SuperHandle == IntPtr.Zero;
				} catch {
					return true;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			isDisposed = true;
			base.Dispose(disposing);
		}
	}
}
