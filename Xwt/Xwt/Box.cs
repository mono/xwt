// 
// Box.cs
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
	[BackendType (typeof(IBoxBackend))]
	public class Box: Widget
	{
		ChildrenCollection<BoxPlacement> children;
		Orientation direction;
		double spacing = 6;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, ICollectionEventSink<BoxPlacement>, IContainerEventSink<BoxPlacement>
		{
			public void AddedItem (BoxPlacement item, int index)
			{
				((Box)Parent).OnAdd (item.Child, item);
			}

			public void RemovedItem (BoxPlacement item, int index)
			{
				((Box)Parent).OnRemove (item.Child);
			}

			public void ChildChanged (BoxPlacement child, string hint)
			{
				((Box)Parent).OnChildChanged (child, hint);
			}

			public void ChildReplaced (BoxPlacement child, Widget oldWidget, Widget newWidget)
			{
				((Box)Parent).OnReplaceChild (child, oldWidget, newWidget);
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IBoxBackend Backend {
			get { return (IBoxBackend) BackendHost.Backend; }
		}
		
		internal Box (Orientation dir)
		{
			children = new ChildrenCollection<BoxPlacement> ((WidgetBackendHost)BackendHost);
			direction = dir;
		}
		
		public double Spacing {
			get { return spacing; }
			set {
				spacing = value > 0 ? value : 0;
				OnPreferredSizeChanged ();
			}
		}
		
		public ChildrenCollection<BoxPlacement> Placements {
			get { return children; }
		}
		
		public IEnumerable<Widget> Children {
			get { return children.Select (c => c.Child); }
		}

		public void PackStart (Widget widget)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			Pack (widget, false, WidgetPlacement.Fill, PackOrigin.Start);
		}
		
		public void PackStart (Widget widget, bool expand)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			Pack (widget, expand, WidgetPlacement.Fill, PackOrigin.Start);
		}

		public void PackStart (Widget widget, bool expand, bool fill)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			WidgetPlacement align = fill ? WidgetPlacement.Fill : WidgetPlacement.Center;
			Pack (widget, expand, align, PackOrigin.Start);
		}

		public void PackStart (Widget widget, bool expand = false, WidgetPlacement vpos = WidgetPlacement.Fill, WidgetPlacement hpos = WidgetPlacement.Fill, double marginLeft = -1, double marginTop = -1, double marginRight = -1, double marginBottom = -1, double margin = -1)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			Pack (widget, expand, vpos, hpos, marginLeft, marginTop, marginRight, marginBottom, margin, PackOrigin.Start);
		}

		[Obsolete ("BoxMode is going away")]
		public void PackStart (Widget widget, BoxMode mode)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			bool expand = (mode & BoxMode.Expand) != 0;
			bool fill = (mode & BoxMode.Fill) != 0;
			PackStart (widget, expand, fill);
		}
		
		public void PackEnd (Widget widget)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			Pack (widget, false, WidgetPlacement.Fill, PackOrigin.End);
		}
		
		public void PackEnd (Widget widget, bool expand)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			Pack (widget, expand, WidgetPlacement.Fill, PackOrigin.End);
		}

		public void PackEnd (Widget widget, bool expand, bool fill)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			WidgetPlacement align = fill ? WidgetPlacement.Fill : WidgetPlacement.Center;
			Pack (widget, expand, align, PackOrigin.End);
		}

		public void PackEnd (Widget widget, bool expand = false, WidgetPlacement hpos = WidgetPlacement.Fill, WidgetPlacement vpos = WidgetPlacement.Fill, double marginLeft = -1, double marginTop = -1, double marginRight = -1, double marginBottom = -1, double margin = -1)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			Pack (widget, expand, vpos, hpos, marginLeft, marginTop, marginRight, marginBottom, margin, PackOrigin.End);
		}

		[Obsolete ("BoxMode is going away")]
		public void PackEnd (Widget widget, BoxMode mode)
		{
			bool expand = (mode & BoxMode.Expand) != 0;
			bool fill = (mode & BoxMode.Fill) != 0;
			PackEnd (widget, expand, fill);
		}


		void Pack (Widget widget, bool expand, WidgetPlacement vpos, WidgetPlacement hpos, double marginLeft, double marginTop, double marginRight, double marginBottom, double margin, PackOrigin ptype)
		{
			WidgetPlacement align;

			if (direction == Orientation.Horizontal) {
				align = hpos;
				if (vpos != default (WidgetPlacement))
					widget.VerticalPlacement = vpos;
			} else {
				align = vpos;
				if (hpos != default (WidgetPlacement))
					widget.HorizontalPlacement = hpos;
			}
			if (margin != -1)
				widget.Margin = margin;
			if (marginLeft != -1)
				widget.MarginLeft = marginLeft;
			if (marginTop != -1)
				widget.MarginTop = marginTop;
			if (marginTop != -1)
				widget.MarginRight = marginRight;
			if (marginBottom != -1)
				widget.MarginBottom = marginBottom;
			Pack (widget, expand, align, PackOrigin.Start);
		}

		void Pack (Widget widget, bool? expand, WidgetPlacement align, PackOrigin ptype)
		{
			if (expand.HasValue) {
				if (direction == Orientation.Vertical)
					widget.ExpandVertical = expand.Value;
				else
					widget.ExpandHorizontal = expand.Value;
			}
			if (align != default (WidgetPlacement)) {
				if (direction == Orientation.Vertical)
					widget.VerticalPlacement = align;
				else
					widget.HorizontalPlacement = align;
			}

			if (widget == null)
				throw new ArgumentNullException ("widget");
			var p = new BoxPlacement ((WidgetBackendHost)BackendHost, widget);
			p.PackOrigin = ptype;
			children.Add (p);
		}
		
		public bool Remove (Widget widget)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			for (int n=0; n<children.Count; n++) {
				if (children[n].Child == widget) {
					children.RemoveAt (n);
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Removes all children
		/// </summary>
		public void Clear ()
		{
			children.Clear ();
		}
		
		void OnAdd (Widget child, BoxPlacement placement)
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
		
		void OnChildChanged (BoxPlacement placement, object hint)
		{
			OnPreferredSizeChanged ();
		}
		
		internal protected virtual void OnReplaceChild (BoxPlacement placement, Widget oldWidget, Widget newWidget)
		{
			if (oldWidget != null)
				OnRemove (oldWidget);
			OnAdd (newWidget, placement);
		}

		protected override void OnReallocate ()
		{
			var size = Backend.Size;

			var visibleChildren = children.Where (c => c.Child.Visible).ToArray ();

			IWidgetBackend[] widgets = new IWidgetBackend [visibleChildren.Length];
			Rectangle[] rects = new Rectangle [visibleChildren.Length];

			if (size.Width <= 0 || size.Height <= 0) {
				var ws = visibleChildren.Select (bp => bp.Child.GetBackend ()).ToArray ();
				Backend.SetAllocation (ws, new Rectangle[visibleChildren.Length]);
				return;
			}

			if (direction == Orientation.Horizontal) {
				CalcDefaultSizes (size.Width, size.Height, true);
				double xs = 0;
				double xe = size.Width + spacing;
				for (int n=0; n<visibleChildren.Length; n++) {
					var bp = visibleChildren [n];
					double availableWidth = bp.NextSize >= 0 ? bp.NextSize : 0;
					if (bp.PackOrigin == PackOrigin.End)
						xe -= availableWidth + spacing;

					var slot = new Rectangle (bp.PackOrigin == PackOrigin.Start ? xs : xe, 0, availableWidth, size.Height);
					widgets[n] = (IWidgetBackend)GetBackend (bp.Child);
					rects[n] = bp.Child.Surface.GetPlacementInRect (slot).Round ().WithPositiveSize ();

					if (bp.PackOrigin == PackOrigin.Start)
						xs += availableWidth + spacing;
				}
			} else {
				CalcDefaultSizes (size.Width, size.Height, true);
				double ys = 0;
				double ye = size.Height + spacing;
				for (int n=0; n<visibleChildren.Length; n++) {
					var bp = visibleChildren [n];
					double availableHeight = bp.NextSize >= 0 ? bp.NextSize : 0;
					if (bp.PackOrigin == PackOrigin.End)
						ye -= availableHeight + spacing;

					var slot = new Rectangle (0, bp.PackOrigin == PackOrigin.Start ? ys : ye, size.Width, availableHeight);
					widgets[n] = (IWidgetBackend)GetBackend (bp.Child);
					rects[n] = bp.Child.Surface.GetPlacementInRect (slot).Round ().WithPositiveSize ();

					if (bp.PackOrigin == PackOrigin.Start)
						ys += availableHeight + spacing;
				}
			}
			Backend.SetAllocation (widgets, rects);
		}
		
		void CalcDefaultSizes (SizeConstraint width, SizeConstraint height, bool allowShrink)
		{
			bool vertical = direction == Orientation.Vertical;
			int nexpands = 0;
			double requiredSize = 0;
			double availableSize = vertical ? height.AvailableSize : width.AvailableSize;

			var widthConstraint = vertical ? width : SizeConstraint.Unconstrained;
			var heightConstraint = vertical ? SizeConstraint.Unconstrained : height;

			var visibleChildren = children.Where (b => b.Child.Visible).ToArray ();
			var sizes = new Dictionary<BoxPlacement,double> ();

			// Get the natural size of each child
			foreach (var bp in visibleChildren) {
				Size s;
				s = bp.Child.Surface.GetPreferredSize (widthConstraint, heightConstraint, true);
				bp.NextSize = vertical ? s.Height : s.Width;
				sizes [bp] = bp.NextSize;
				requiredSize += bp.NextSize;
				if (bp.Child.ExpandsForOrientation (direction))
					nexpands++;
			}
			
			double remaining = availableSize - requiredSize - (spacing * (double)(visibleChildren.Length - 1));
			if (remaining > 0) {
				var expandRemaining = new SizeSplitter (remaining, nexpands);
				foreach (var bp in visibleChildren) {
					if (bp.Child.ExpandsForOrientation (direction))
						bp.NextSize += expandRemaining.NextSizePart ();
				}
			}
			else if (allowShrink && remaining < 0) {
				// The box is not big enough to fit the widgets using its natural size.
				// We have to shrink the widgets.

				// The total amount we have to shrink
				double shrinkSize = -remaining;

				var sizePart = new SizeSplitter (shrinkSize, visibleChildren.Length);
				foreach (var bp in visibleChildren)
					bp.NextSize -= sizePart.NextSizePart ();
			}
		}
		
		protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			Size s = new Size ();
			int count = 0;

			var visibleChildren = children.Where (b => b.Child.Visible).ToArray ();

			if (direction == Orientation.Horizontal) {
				// If the width is constrained then we have a total width, and we can calculate the exact width assigned to each child.
				// We can then use that width as a width constraint for the child.

				if (widthConstraint.IsConstrained)
					CalcDefaultSizes (widthConstraint, heightConstraint, false); // Calculates the width assigned to each child

				foreach (var cw in visibleChildren) {
					// Use the calculated width if available
					var wsize = cw.Child.Surface.GetPreferredSize (widthConstraint.IsConstrained ? cw.NextSize : SizeConstraint.Unconstrained, heightConstraint, true);
					s.Width += wsize.Width;
					if (wsize.Height > s.Height)
						s.Height = wsize.Height;
					count++;
				}
				if (count > 0)
					s.Width += spacing * (double)(count - 1);
			} else {
				if (heightConstraint.IsConstrained)
					CalcDefaultSizes (widthConstraint, heightConstraint, false);
				foreach (var cw in visibleChildren) {
					var wsize = cw.Child.Surface.GetPreferredSize (widthConstraint, heightConstraint.IsConstrained ? cw.NextSize : SizeConstraint.Unconstrained, true);
					s.Height += wsize.Height;
					if (wsize.Width > s.Width)
						s.Width = wsize.Width;
					count++;
				}
				if (count > 0)
					s.Height += spacing * (double)(count - 1);
			}
			return s;
		}
	}
	
	[Flags]
	public enum BoxMode
	{
		None = 0,
		Fill = 1,
		Expand = 2,
		FillAndExpand = 3
	}
	
	[ContentProperty("Child")]
	public class BoxPlacement
	{
		IContainerEventSink<BoxPlacement> parent;
		int position;
		PackOrigin packType = PackOrigin.Start;
		Widget child;
		
		internal BoxPlacement (IContainerEventSink<BoxPlacement> parent, Widget child)
		{
			this.parent = parent;
			this.child = child;
		}
		
		internal double NextSize;

		public int Position {
			get {
				return this.position;
			}
			set {
				if (value < 0)
					throw new ArgumentException ("Position can't be negative");
				position = value;
				parent.ChildChanged (this, "Position");
			}
		}

		[DefaultValue (PackOrigin.Start)]
		public PackOrigin PackOrigin {
			get {
				return this.packType;
			}
			set {
				packType = value;
				parent.ChildChanged (this, "PackType");
			}
		}
		
		public Widget Child {
			get { return child; }
			set {
				if (value == null)
					throw new ArgumentNullException ();
				var old = child;
				child = value;
				parent.ChildReplaced (this, old, value);
			}
		}
	}
	
	public enum PackOrigin
	{
		Start,
		End
	}
	
	class SizeSplitter
	{
		int rem;
		int part;
		
		public SizeSplitter (double total, int numParts)
		{
			if (numParts > 0) {
				part = ((int)total) / numParts;
				rem = ((int)total) % numParts;
			}
		}
		
		public double NextSizePart ()
		{
			if (rem > 0) {
				rem--;
				return part + 1;
			}
			else
				return part;
		}
	}
}

