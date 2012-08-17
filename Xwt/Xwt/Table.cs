// 
// Table.cs
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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

using Xwt.Backends;
using System.Windows.Markup;

namespace Xwt
{
	public class Table: Widget
	{
		ChildrenCollection<TablePlacement> children;
		double defaultRowSpacing = 6;
		double defaultColSpacing = 6;
		Dictionary<int,double> rowSpacing;
		Dictionary<int,double> colSpacing;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, ICollectionEventSink<TablePlacement>, IContainerEventSink<TablePlacement>
		{
			public void AddedItem (TablePlacement item, int index)
			{
				((Table)Parent).OnAdd (item.Child, item);
			}

			public void RemovedItem (TablePlacement item, int index)
			{
				((Table)Parent).OnRemove (item.Child);
			}

			public void ChildChanged (TablePlacement child, string hint)
			{
				((Table)Parent).OnChildChanged (child, hint);
			}

			public void ChildReplaced (TablePlacement child, Widget oldWidget, Widget newWidget)
			{
				((Table)Parent).OnReplaceChild (child, oldWidget, newWidget);
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IBoxBackend Backend {
			get { return (IBoxBackend) BackendHost.Backend; }
		}
		
		public Table ()
		{
			children = new ChildrenCollection<TablePlacement> ((WidgetBackendHost)BackendHost);
		}
		
		[DefaultValue(6)]
		public double DefaultRowSpacing {
			get { return defaultRowSpacing; }
			set { defaultRowSpacing = value; OnPreferredSizeChanged (); }
		}
		
		[DefaultValue(6)]
		public double DefaultColumnSpacing {
			get { return defaultColSpacing; }
			set { defaultColSpacing = value; OnPreferredSizeChanged (); }
		}
		
		public void SetRowSpacing (int row, double spacing)
		{
			if (rowSpacing == null)
				rowSpacing = new Dictionary<int, double> ();
			rowSpacing [row] = spacing;
			OnPreferredSizeChanged ();
		}
		
		public void SetColumnSpacing (int col, double spacing)
		{
			if (colSpacing == null)
				colSpacing = new Dictionary<int, double> ();
			colSpacing [col] = spacing;
			OnPreferredSizeChanged ();
		}
		
		public ChildrenCollection<TablePlacement> Placements {
			get { return children; }
		}
		
		public IEnumerable<Widget> Children {
			get { return children.Select (c => c.Child); }
		}
		
		public void Attach (Widget widget, int left, int top)
		{
			Attach (widget, left, top, AttachOptions.Fill | AttachOptions.Expand, AttachOptions.Fill | AttachOptions.Expand);
		}
		
		public void Attach (Widget widget, int left, int top, AttachOptions xOptions, AttachOptions yOptions)
		{
			Attach (widget, left, left + 1, top, top + 1, xOptions, yOptions);
		}
		
		public void Attach (Widget widget, int left, int right, int top, int bottom)
		{
			Attach (widget, left, right, top, bottom, AttachOptions.Fill | AttachOptions.Expand, AttachOptions.Fill | AttachOptions.Expand);
		}
		
		public void Attach (Widget widget, int left, int right, int top, int bottom, AttachOptions xOptions, AttachOptions yOptions)
		{
			var p = new TablePlacement ((WidgetBackendHost)BackendHost, widget) {
				Left = left,
				Right = right,
				Top = top,
				Bottom = bottom,
				XOptions = xOptions,
				YOptions = yOptions
			};
			children.Add (p);
		}
		
		public bool Remove (Widget widget)
		{
			for (int n=0; n<children.Count; n++) {
				if (children[n].Child == widget) {
					children.RemoveAt (n);
					return true;
				}
			}
			return false;
		}

		public void InsertRow (int top, int bottom)
		{
			var potentials = children.Where (c => c.Top >= top);
			var shift = bottom - top;
			foreach (var toShift in potentials) {
				toShift.Top += shift;
				toShift.Bottom += shift;
			}
		}
		
		/// <summary>
		/// Removes all children
		/// </summary>
		public void Clear ()
		{
			children.Clear ();
		}
		
		void OnAdd (Widget child, TablePlacement placement)
		{
			RegisterChild (child);
			Backend.Add ((IWidgetBackend)GetBackend (child));
			OnPreferredSizeChanged ();
		}
		
		void OnRemove (Widget child)
		{
			UnregisterChild (child);
			Backend.Remove ((IWidgetBackend)GetBackend (child));
			OnPreferredSizeChanged ();
		}
		
		void OnChildChanged (TablePlacement placement, object hint)
		{
			OnPreferredSizeChanged ();
		}
		
		internal protected virtual void OnReplaceChild (TablePlacement placement, Widget oldWidget, Widget newWidget)
		{
			if (oldWidget != null)
				OnRemove (oldWidget);
			OnAdd (newWidget, placement);
		}
		
		protected override void OnReallocate ()
		{
			var size = Backend.Size;
			var mode = Surface.SizeRequestMode;
			if (mode == SizeRequestMode.HeightForWidth) {
				CalcDefaultSizes (mode, size.Width, false, true);
				CalcDefaultSizes (mode, size.Height, true, true);
			} else {
				CalcDefaultSizes (mode, size.Height, true, true);
				CalcDefaultSizes (mode, size.Width, false, true);
			}
			
			var visibleChildren = children.Where (c => c.Child.Visible).ToArray ();
			IWidgetBackend[] widgets = new IWidgetBackend [visibleChildren.Length];
			Rectangle[] rects = new Rectangle [visibleChildren.Length];
			for (int n=0; n<visibleChildren.Length; n++) {
				var bp = visibleChildren [n];
				widgets [n] = (IWidgetBackend)GetBackend (bp.Child);
				rects [n] = new Rectangle (bp.NextX, bp.NextY, bp.NextWidth, bp.NextHeight);
			}
			
			Backend.SetAllocation (widgets, rects);
			
			if (!Application.EngineBackend.HandlesSizeNegotiation) {
				foreach (var bp in visibleChildren)
					bp.Child.Surface.Reallocate ();
			}
		}
		
		double GetSpacing (int cell, bool isRow)
		{
			double sp;
			if (isRow) {
				if (rowSpacing != null && rowSpacing.TryGetValue (cell, out sp))
					return sp;
				else
					return defaultRowSpacing;
			} else {
				if (colSpacing != null && colSpacing.TryGetValue (cell, out sp))
					return sp;
				else
					return defaultColSpacing;
			}
		}
		
		void CalcDefaultSizes (SizeRequestMode mode, bool calcHeights, out TablePlacement[] visibleChildren, out Dictionary<int,WidgetSize> fixedSizesByCell, out HashSet<int> cellsWithExpand, out WidgetSize[] sizes, out double spacing)
		{
			bool useLengthConstraint = mode == SizeRequestMode.HeightForWidth && calcHeights || mode == SizeRequestMode.WidthForHeight && !calcHeights;
			visibleChildren = children.Where (b => b.Child.Visible).ToArray ();
			int lastCell = 0;

			fixedSizesByCell = new Dictionary<int, WidgetSize> ();
			cellsWithExpand = new HashSet<int> ();
			HashSet<int> cellsWithWidget = new HashSet<int> ();
			sizes = new WidgetSize [visibleChildren.Length];
			
			// Get the size of each widget and store the fixed sizes for widgets which don't span more than one cell
			
			for (int n=0; n<visibleChildren.Length; n++) {
				var bp = visibleChildren[n];
				int start = GetStartAttach (bp, calcHeights);
				int end = GetEndAttach (bp, calcHeights);
				
				if (end > lastCell)
					lastCell = end;
				
				// Check if the cell is expandable and store the value
				AttachOptions ops = calcHeights ? bp.YOptions : bp.XOptions;
				for (int i=start; i < end; i++) {
					cellsWithWidget.Add (i);
					if ((ops & AttachOptions.Expand) != 0)
						cellsWithExpand.Add (i);
				}
					
				WidgetSize s;
				if (useLengthConstraint)
					s = GetPreferredLengthForSize (mode, bp.Child, calcHeights ? bp.NextWidth : bp.NextHeight);
				else
					s = GetPreferredSize (calcHeights, bp.Child);
				sizes [n] = s;
				
				if (end == start + 1) {
					// The widget only takes one cell. Store its size if it is the biggest
					bool changed = false;
					WidgetSize fs;
					fixedSizesByCell.TryGetValue (start, out fs);
					if (s.MinSize > fs.MinSize) { 
						fs.MinSize = s.MinSize;
						changed = true;
					}
					if (s.NaturalSize > fs.NaturalSize) {
						fs.NaturalSize = s.NaturalSize;
						changed = true;
					}
					if (changed)
						fixedSizesByCell [start] = fs;
				}
			}
			
			// For widgets that span more than one cell, calculate the floating size, that is, the size
			// which is not taken by other fixed size widgets
			
			List<TablePlacement> widgetsToAdjust = new List<TablePlacement> ();
			Dictionary<TablePlacement,WidgetSize[]> growSizes = new Dictionary<TablePlacement, WidgetSize[]> ();
			
			for (int n=0; n<visibleChildren.Length; n++) {
				var bp = visibleChildren[n];
				int start = GetStartAttach (bp, calcHeights);
				int end = GetEndAttach (bp, calcHeights);
				if (end == start + 1)
					continue;
				widgetsToAdjust.Add (bp);
				
				WidgetSize fixedSize = new WidgetSize (0);
				
				// We are going to calculate the spacing included in the widget's span of cells
				// (there is spacing between each cell)
				double spanSpacing = 0;
				
				for (int c = start; c < end; c++) {
					WidgetSize fs;
					fixedSizesByCell.TryGetValue (c, out fs);
					fixedSize += fs;
					if (c != start && c != end)
						spanSpacing += GetSpacing (c, calcHeights);
				}
				
				// sizeToGrow is the size that the whole cell span has to grow in order to fit
				// this widget. We substract the spacing between cells because that space will
				// be used by the widget, so we don't need to allocate more size for it
				
				WidgetSize sizeToGrow = sizes [n] - fixedSize - new WidgetSize (spanSpacing);
				
				WidgetSize sizeToGrowPart = new WidgetSize (sizeToGrow.MinSize / (end - start), sizeToGrow.NaturalSize / (end - start));

				// Split the size to grow between the cells of the widget. We need to know how much size the widget
				// requires for each cell it covers.
				
				WidgetSize[] widgetGrowSizes = new WidgetSize [end - start];
				for (int i=0; i<widgetGrowSizes.Length; i++)
					widgetGrowSizes [i] = sizeToGrowPart;
				growSizes[bp] = widgetGrowSizes;
			}
			
			// Now size-to-grow values have to be adjusted. For example, let's say widget A requires 100px for column 1 and 100px for column 2, and widget B requires
			// 60px for column 2 and 60px for column 3. So the widgets are overlapping at column 2. Since A requires at least 100px in column 2, it means that B can assume
			// that it will have 100px available in column 2, which means 40px more than it requested. Those extra 40px can then be substracted from the 60px that
			// it required for column 3.
			
			foreach (var n in cellsWithWidget) {
				// Get a list of all widgets that cover this cell
				var colCells = widgetsToAdjust.Where (bp => GetStartAttach (bp, calcHeights) <= n && GetEndAttach (bp, calcHeights) > n).ToArray ();
				WidgetSize maxv = new WidgetSize (0);
				TablePlacement maxtMin = null;
				TablePlacement maxtNatural = null;
				
				// Find the widget that requires the maximum size for this cell
				foreach (var bp in colCells) {
					WidgetSize cv = growSizes[bp][n - GetStartAttach (bp, calcHeights)];
					if (cv.MinSize > maxv.MinSize) {
						maxv.MinSize = cv.MinSize;
						maxtMin = bp;
					}
					if (cv.NaturalSize > maxv.NaturalSize) {
						maxv.NaturalSize = cv.NaturalSize;
						maxtNatural = bp;
					}
				}
				
				// Adjust the required size of all widgets of the cell (excluding the widget with the max size)
				foreach (var bp in colCells) {
					WidgetSize[] widgetGrows = growSizes[bp];
					int cellIndex = n - GetStartAttach (bp, calcHeights);
					if (bp != maxtMin) {
						double cv = widgetGrows[cellIndex].MinSize;
						// splitExtraSpace is the additional space that the widget can take from this cell (because there is a widget
						// that is requiring more space), split among all other cells of the widget
						double splitExtraSpace = (maxv.MinSize - cv) / (widgetGrows.Length - 1);
						for (int i=0; i<widgetGrows.Length; i++)
							widgetGrows[i].MinSize -= splitExtraSpace;
					}
					if (bp != maxtNatural) {
						double cv = widgetGrows[cellIndex].NaturalSize;
						double splitExtraSpace = (maxv.NaturalSize - cv) / (widgetGrows.Length - 1);
						for (int i=0; i<widgetGrows.Length; i++)
							widgetGrows[i].NaturalSize -= splitExtraSpace;
					}
				}
			}
			
			// Find the maximum size-to-grow for each cell
			
			Dictionary<int,WidgetSize> finalGrowTable = new Dictionary<int, WidgetSize> ();
			
			foreach (var bp in widgetsToAdjust) {
				int start = GetStartAttach (bp, calcHeights);
				int end = GetEndAttach (bp, calcHeights);
				WidgetSize[] widgetGrows = growSizes[bp];
				for (int n=start; n<end; n++) {
					WidgetSize curGrow;
					finalGrowTable.TryGetValue (n, out curGrow);
					var val = widgetGrows [n - start];
					if (val.MinSize > curGrow.MinSize)
						curGrow.MinSize = val.MinSize;
					if (val.NaturalSize > curGrow.NaturalSize)
						curGrow.NaturalSize = val.NaturalSize;
					finalGrowTable [n] = curGrow;
				}
			}
			
			// Add the final size-to-grow to the fixed sizes calculated at the begining
			
			foreach (var it in finalGrowTable) {
				WidgetSize ws;
				fixedSizesByCell.TryGetValue (it.Key, out ws);
				fixedSizesByCell [it.Key] = it.Value + ws;
			}
			
			spacing = 0;
			for (int n=1; n<lastCell; n++) {
				if (cellsWithWidget.Contains (n))
					spacing += GetSpacing (n, calcHeights);
			}
			
		}
		
		void CalcDefaultSizes (SizeRequestMode mode, double totalSize, bool calcHeights, bool calcOffsets)
		{
			TablePlacement[] visibleChildren;
			Dictionary<int,WidgetSize> fixedSizesByCell;
			HashSet<int> cellsWithExpand;
			WidgetSize[] sizes;
			double spacing;
			
			CalcDefaultSizes (mode, calcHeights, out visibleChildren, out fixedSizesByCell, out cellsWithExpand, out sizes, out spacing);
			
			double naturalSize = 0;
			
			// Get the total natural size
			foreach (var ws in fixedSizesByCell.Values)
				naturalSize += ws.NaturalSize;
			
			double remaining = totalSize - naturalSize - spacing;
			
			if (remaining < 0) {
				// The box is not big enough to fit the widgets using its natural size.
				// We have to shrink the cells
				
				// List of cell indexes that we have to shrink
				var toShrink = new List<int> (fixedSizesByCell.Keys);
				
				// The total amount we have to shrink
				double shrinkSize = -remaining;
				
				while (toShrink.Count > 0 && shrinkSize > 0) {
					SizeSplitter sizePart = new SizeSplitter (shrinkSize, toShrink.Count);
					shrinkSize = 0;
					for (int i=0; i < toShrink.Count; i++) {
						int n = toShrink[i];
						double reduction = sizePart.NextSizePart ();
						WidgetSize size;
						fixedSizesByCell.TryGetValue (n, out size);
						size.NaturalSize -= reduction;
						
						if (size.NaturalSize < size.MinSize) {
							// If the widget can't be shrinked anymore, we remove it from the shrink list
							// and increment the remaining shrink size. We'll loop again and this size will be
							// substracted from the cells which can still be reduced
							shrinkSize += (size.MinSize - size.NaturalSize);
							size.NaturalSize = size.MinSize;
							toShrink.RemoveAt (i);
							i--;
						}
						fixedSizesByCell [n] = size;
					}
				}
			}
			else {
				int nexpands = cellsWithExpand.Count;
				var expandRemaining = new SizeSplitter (remaining, nexpands);
				foreach (var c in cellsWithExpand) {
					WidgetSize ws;
					fixedSizesByCell.TryGetValue (c, out ws);
					ws.NaturalSize += expandRemaining.NextSizePart ();
					fixedSizesByCell [c] = ws;
				}
			}
			
			for (int n=0; n<visibleChildren.Length; n++) {
				var bp = visibleChildren[n];
				double allocatedSize = 0;
				double cellOffset = 0;
				AttachOptions ops = calcHeights ? bp.YOptions : bp.XOptions;
				
				int start = GetStartAttach (bp, calcHeights);
				int end = GetEndAttach (bp, calcHeights);
				for (int i=start; i<end; i++) {
					WidgetSize ws;
					fixedSizesByCell.TryGetValue (i, out ws);
					allocatedSize += ws.NaturalSize;
					if (i != start)
						allocatedSize += GetSpacing (i, calcHeights);
				}
				
				if ((ops & AttachOptions.Fill) == 0) {
					double s = sizes[n].NaturalSize;
					if (s < allocatedSize) {
						cellOffset = (allocatedSize - s) / 2;
						allocatedSize = s;
					}
				}
				
				// cellOffset is the offset of the widget inside the cell. We store it in NextX/Y, and
				// will be used below to calculate the total offset of the widget
				
				if (calcHeights) {
					bp.NextHeight = allocatedSize;
					bp.NextY = cellOffset;
				}	
				else {
					bp.NextWidth = allocatedSize;
					bp.NextX = cellOffset;
				}
			}
			
			if (calcOffsets) {
				var sortedChildren = visibleChildren.OrderBy (c => GetStartAttach (c, calcHeights)).ToArray();
				var cells = fixedSizesByCell.OrderBy (c => c.Key);
				double offset = 0;
				int n = 0;
				foreach (var c in cells) {
					if (c.Key > 0)
						offset += GetSpacing (c.Key, calcHeights);
					while (n < sortedChildren.Length && GetStartAttach (sortedChildren[n], calcHeights) == c.Key) {
						// In the loop above we store the offset of the widget inside the cell in the NextX/Y field
						// so now we have to add (not just assign) the offset of the cell to NextX/Y
						if (calcHeights)
							sortedChildren[n].NextY += offset;
						else
							sortedChildren[n].NextX += offset;
						n++;
					}
					offset += c.Value.NaturalSize;
				}
			}
		}
		
		WidgetSize CalcSize (bool calcHeights)
		{
			TablePlacement[] visibleChildren;
			Dictionary<int,WidgetSize> fixedSizesByCell;
			HashSet<int> cellsWithExpand;
			WidgetSize[] sizes;
			double spacing;
			SizeRequestMode mode = calcHeights ? SizeRequestMode.WidthForHeight : SizeRequestMode.HeightForWidth;
			
			CalcDefaultSizes (mode, calcHeights, out visibleChildren, out fixedSizesByCell, out cellsWithExpand, out sizes, out spacing);

			WidgetSize size = new WidgetSize (spacing);
			foreach (var s in fixedSizesByCell.Values)
				size += s;
			return size;
		}
		
		protected override WidgetSize OnGetPreferredWidth ()
		{
			return CalcSize (false);
		}
		
		protected override WidgetSize OnGetPreferredHeight ()
		{
			return CalcSize (true);
		}
		
		protected override WidgetSize OnGetPreferredHeightForWidth (double width)
		{
			CalcDefaultSizes (SizeRequestMode.HeightForWidth, width, false, false);
			return CalcSize (true);
		}
		
		protected override WidgetSize OnGetPreferredWidthForHeight (double height)
		{
			CalcDefaultSizes (SizeRequestMode.WidthForHeight, height, true, false);
			return CalcSize (false);
		}
		
		int GetStartAttach (TablePlacement tp, bool calcHeight)
		{
			if (calcHeight)
				return tp.Top;
			else
				return tp.Left;
		}
		
		int GetEndAttach (TablePlacement tp, bool calcHeight)
		{
			if (calcHeight)
				return tp.Bottom;
			else
				return tp.Right;
		}
		
		WidgetSize GetPreferredSize (bool calcHeight, Widget w)
		{
			if (calcHeight)
				return w.Surface.GetPreferredHeight ();
			else
				return w.Surface.GetPreferredWidth ();
		}
		
		WidgetSize GetPreferredLengthForSize (SizeRequestMode mode, Widget w, double width)
		{
			if (mode == SizeRequestMode.WidthForHeight)
				return w.Surface.GetPreferredWidthForHeight (width);
			else
				return w.Surface.GetPreferredHeightForWidth (width);
		}
	}
	
	[ContentProperty("Child")]
	public class TablePlacement
	{
		IContainerEventSink<TablePlacement> parent;
		int left, right, top, bottom;
		AttachOptions xOptions, yOptions;
		Widget child;
		
		internal TablePlacement (IContainerEventSink<TablePlacement> parent, Widget child)
		{
			this.parent = parent;
			this.child = child;
		}
		
		internal double NextWidth;
		internal double NextHeight;
		internal double NextX;
		internal double NextY;
		
		public int Left {
			get {
				return left;
			}
			set {
				left = value;
				parent.ChildChanged (this, "Left");
			}
		}
		
		public int Right {
			get {
				return right;
			}
			set {
				right = value;
				parent.ChildChanged (this, "Right");
			}
		}
		
		public int Top {
			get {
				return top;
			}
			set {
				top = value;
				parent.ChildChanged (this, "Top");
			}
		}
		
		public int Bottom {
			get {
				return bottom;
			}
			set {
				bottom = value;
				parent.ChildChanged (this, "Bottom");
			}
		}
		
		public Widget Child {
			get { return child; }
			set {
				var old = child;
				child = value;
				parent.ChildReplaced (this, old, value);
			}
		}
		
		public AttachOptions XOptions {
			get { return xOptions; }
			set {
				xOptions = value; 
				parent.ChildChanged (this, "XOptions");
			}
		}
		
		public AttachOptions YOptions {
			get { return yOptions; }
			set {
				yOptions = value; 
				parent.ChildChanged (this, "YOptions");
			}
		}
	}
	
	[Flags]
	public enum AttachOptions
	{
		Expand = 1,
		Fill = 2,
		Shrink = 4
	}
}

