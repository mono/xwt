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
using Xwt.Drawing;

namespace Xwt
{
	[BackendType (typeof(IBoxBackend))]
	public partial class Table: Widget
	{
		ChildrenCollection<TablePlacement> placements;
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
			placements = new ChildrenCollection<TablePlacement> ((WidgetBackendHost)BackendHost);
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
			get { return placements; }
		}
		
		public IEnumerable<Widget> Children {
			get { return placements.Select (c => c.Child); }
		}
		
		[Obsolete ("Use the Add method")]
		public void Attach (Widget widget, int left, int top)
		{
			Attach (widget, left, top, null, null);
		}
		
		[Obsolete ("Use the Add method")]
		public void Attach (Widget widget, int left, int top, AttachOptions? xOptions, AttachOptions? yOptions)
		{
			Attach (widget, left, left + 1, top, top + 1, xOptions, yOptions);
		}

		[Obsolete ("Use the Add method")]
		public void Attach (Widget widget, int left, int right, int top, int bottom)
		{
			Attach (widget, left, right, top, bottom, (AttachOptions?)null, (AttachOptions?)null);
		}
		
		public void Add (Widget widget, int left, int top, int rowspan = 1, int colspan = 1, bool hexpand = false, bool vexpand = false, WidgetPlacement hpos = WidgetPlacement.Fill, WidgetPlacement vpos = WidgetPlacement.Fill, double marginLeft = -1, double marginTop = -1, double marginRight = -1, double marginBottom = -1, double margin = -1)
		{
			if (vpos != default (WidgetPlacement))
				widget.VerticalPlacement = vpos;
			if (hpos != default (WidgetPlacement))
				widget.HorizontalPlacement = hpos;

			widget.ExpandHorizontal = hexpand;
			widget.ExpandVertical = vexpand;

			if (margin != -1)
				widget.Margin = margin;
			if (marginLeft != -1)
				widget.MarginLeft = marginLeft;
			if (marginTop != -1)
				widget.MarginTop = marginTop;
			if (marginRight != -1)
				widget.MarginRight = marginRight;
			if (marginBottom != -1)
				widget.MarginBottom = marginBottom;
			
			var p = new TablePlacement ((WidgetBackendHost)BackendHost, widget) {
				Left = left,
				Right = left + colspan,
				Top = top,
				Bottom = top + rowspan
			};
			placements.Add (p);
		}

		public void Attach (Widget widget, int left, int right, int top, int bottom, AttachOptions? xOptions = null, AttachOptions? yOptions = null)
		{
			if (xOptions != null) {
				widget.ExpandHorizontal = (xOptions.Value & AttachOptions.Expand) != 0;
				if ((xOptions.Value & AttachOptions.Fill) != 0)
					widget.HorizontalPlacement = WidgetPlacement.Fill;
				else
					widget.HorizontalPlacement = WidgetPlacement.Center;
			}
			if (yOptions != null) {
				widget.ExpandVertical = (yOptions.Value & AttachOptions.Expand) != 0;
				if ((yOptions.Value & AttachOptions.Fill) != 0)
					widget.VerticalPlacement = WidgetPlacement.Fill;
				else
					widget.VerticalPlacement = WidgetPlacement.Center;
			}

			var p = new TablePlacement ((WidgetBackendHost)BackendHost, widget) {
				Left = left,
				Right = right,
				Top = top,
				Bottom = bottom
			};
			placements.Add (p);
		}
		
		public bool Remove (Widget widget)
		{
			for (int n=0; n<placements.Count; n++) {
				if (placements[n].Child == widget) {
					placements.RemoveAt (n);
					return true;
				}
			}
			return false;
		}

		public void InsertRow (int top, int bottom)
		{
			var potentials = placements.Where (c => c.Top >= top);
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
			placements.Clear ();
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
			Reallocate (Backend.Size);

			TablePlacement[] visibleChildren = VisibleChildren ();
			IWidgetBackend[] widgets = new IWidgetBackend [visibleChildren.Length];
			Rectangle[] rects = new Rectangle [visibleChildren.Length];
			for (int n=0; n<visibleChildren.Length; n++) {
				var bp = visibleChildren [n];
				widgets [n] = (IWidgetBackend)GetBackend (bp.Child);
				var margin = bp.Child.Margin;
				rects [n] = new Rectangle (bp.NextX + margin.Left, bp.NextY + margin.Top, bp.NextWidth - margin.HorizontalSpacing, bp.NextHeight - margin.VerticalSpacing).Round ().WithPositiveSize ();
			}

			Backend.SetAllocation (widgets, rects);
		}

		protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			var wc = widthConstraint.IsConstrained ? widthConstraint.AvailableSize : double.PositiveInfinity;
			var hc = heightConstraint.IsConstrained ? heightConstraint.AvailableSize : double.PositiveInfinity;
			return GetPreferredSize (wc, hc);
		}
	}
	
	[ContentProperty("Child")]
	public class TablePlacement
	{
		IContainerEventSink<TablePlacement> parent;
		int left, right, top, bottom;
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

		internal Size GetPreferredSize (double width, double height)
		{
			var wc = double.IsPositiveInfinity (width) ? SizeConstraint.Unconstrained : SizeConstraint.WithSize (width);
			var hc = double.IsPositiveInfinity (height) ? SizeConstraint.Unconstrained : SizeConstraint.WithSize (height);
			return Child.Surface.GetPreferredSize (wc, hc);
		}
		
		internal float GetAlignmentForOrientation (Orientation o)
		{
			var p = child.AlignmentForOrientation (o);
			if (p == WidgetPlacement.Fill)
				return float.NaN;
			else
				return (float)p.GetValue ();
		}

		internal bool GetExpandsForOrientation (Orientation o)
		{
			return child.ExpandsForOrientation (o);
		}

		internal double LeftMargin {
			get { return child.MarginLeft; }
		}

		internal double TopMargin {
			get { return child.MarginTop; }
		}

		internal double RightMargin {
			get { return child.MarginRight; }
		}

		internal double BottomMargin {
			get { return child.MarginBottom; }
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

