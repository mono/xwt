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
	public class Box: Widget
	{
		ChildrenCollection<BoxPlacement> children;
		Orientation direction;
		double spacing = 6;
		
		protected new class EventSink: Widget.EventSink, ICollectionEventSink<BoxPlacement>, IContainerEventSink<BoxPlacement>
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
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new IBoxBackend Backend {
			get { return (IBoxBackend) base.Backend; }
		}
		
		internal Box (Orientation dir)
		{
			children = new ChildrenCollection<BoxPlacement> ((EventSink)WidgetEventSink);
			direction = dir;
		}
		
		public double Spacing {
			get { return spacing; }
			set { spacing = value; OnPreferredSizeChanged (); }
		}
		
		public ChildrenCollection<BoxPlacement> Placements {
			get { return children; }
		}
		
		public IEnumerable<Widget> Children {
			get { return children.Select (c => c.Child); }
		}
		
		public void PackStart (Widget widget)
		{
			PackStart (widget, BoxMode.None, 0);
		}
		
		public void PackStart (Widget widget, BoxMode mode)
		{
			PackStart (widget, mode, 0);
		}
		
		public void PackStart (Widget widget, BoxMode mode, int padding)
		{
			Pack (widget, mode, padding, PackOrigin.Start);
		}
		
		public void PackEnd (Widget widget)
		{
			PackEnd (widget, BoxMode.None, 0);
		}
		
		public void PackEnd (Widget widget, BoxMode mode)
		{
			PackEnd (widget, mode, 0);
		}
		
		public void PackEnd (Widget widget, BoxMode mode, int padding)
		{
			Pack (widget, mode, padding, PackOrigin.End);
		}
		
		void Pack (Widget widget, BoxMode mode, int padding, PackOrigin ptype)
		{
			var p = new BoxPlacement ((EventSink)WidgetEventSink, widget);
			p.BoxMode = mode;
			p.Padding = padding;
			p.PackOrigin = ptype;
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
			if (direction == Orientation.Horizontal) {
				CalcDefaultSizes (((IWidgetSurface)this).SizeRequestMode, size.Width, size.Height);
				double xs = 0;
				double xe = size.Width + spacing;
				foreach (var bp in children) {
					if (!bp.Child.Visible)
						continue;
					if (bp.PackOrigin == PackOrigin.End)
						xe -= bp.NextSize + spacing;
					double x = bp.PackOrigin == PackOrigin.Start ? xs : xe;
					Backend.SetAllocation ((IWidgetBackend)GetBackend (bp.Child), new Rectangle (x, 0, bp.NextSize, size.Height));
					((IWidgetSurface)bp.Child).Reallocate ();
					if (bp.PackOrigin == PackOrigin.Start)
						xs += bp.NextSize + spacing;
				}
			} else {
				CalcDefaultSizes (((IWidgetSurface)this).SizeRequestMode, size.Height, size.Width);
				double ys = 0;
				double ye = size.Height + spacing;
				foreach (var bp in children) {
					if (!bp.Child.Visible)
						continue;
					if (bp.PackOrigin == PackOrigin.End)
						ye -= bp.NextSize + spacing;
					double y = bp.PackOrigin == PackOrigin.Start ? ys : ye;
					Backend.SetAllocation ((IWidgetBackend)GetBackend (bp.Child), new Rectangle (0, y, size.Width, bp.NextSize));
					((IWidgetSurface)bp.Child).Reallocate ();
					if (bp.PackOrigin == PackOrigin.Start)
						ys += bp.NextSize + spacing;
				}
			}
		}
		
		void CalcDefaultSizes (SizeRequestMode mode, double totalSize, double lengthConstraint)
		{
			bool calcHeights = direction == Orientation.Vertical;
			bool useLengthConstraint = mode == SizeRequestMode.HeightForWidth && calcHeights || mode == SizeRequestMode.WidthForHeight && !calcHeights;
			int nexpands = 0;
			double naturalSize = 0;
			
			var visibleChildren = children.Where (b => b.Child.Visible);
			int childrenCount = 0;
			
			// Get the natural size of each child
			foreach (var bp in visibleChildren) {
				childrenCount++;
				WidgetSize s;
				if (useLengthConstraint)
					s = GetPreferredLengthForSize (mode, bp.Child, lengthConstraint);
				else
					s = GetPreferredSize (calcHeights, bp.Child);
				naturalSize += s.NaturalSize;
				bp.NextSize = s.NaturalSize;
				if ((bp.BoxMode & BoxMode.Expand) != 0)
					nexpands++;
			}
			
			double remaining = totalSize - naturalSize - (spacing * (double)(childrenCount - 1));
			if (remaining < 0) {
				// The box is not big enough to fit the widgets using its natural size.
				// We have to shrink the widgets.
				var sizePart = new SizeSplitter (-remaining, childrenCount);
				var toAdjust = new List<BoxPlacement> ();
				double adjustSize = 0;
				foreach (var bp in visibleChildren) {
					WidgetSize s;
					if (useLengthConstraint)
						s = GetPreferredLengthForSize (mode, bp.Child, lengthConstraint);
					else
						s = GetPreferredSize (calcHeights, bp.Child);
					bp.NextSize = s.NaturalSize - sizePart.NextSizePart ();
					if (bp.NextSize < s.MinSize) {
						adjustSize += (s.MinSize - bp.NextSize);
						bp.NextSize = s.MinSize;
					} else
						toAdjust.Add (bp);
				}
				sizePart = new SizeSplitter (adjustSize, toAdjust.Count);
				foreach (var bp in toAdjust)
					bp.NextSize += sizePart.NextSizePart ();
			}
			else {
				var expandRemaining = new SizeSplitter (remaining, nexpands);
				foreach (var bp in visibleChildren) {
					if ((bp.BoxMode & BoxMode.Expand) != 0)
						bp.NextSize += expandRemaining.NextSizePart ();
				}
			}
		}
		
		protected override WidgetSize OnGetPreferredWidth ()
		{
			WidgetSize s = new WidgetSize ();
			
			if (direction == Orientation.Horizontal) {
				int count = 0;
				foreach (IWidgetSurface cw in Children.Where (b => b.Visible)) {
					s += cw.GetPreferredWidth ();
					count++;
				}
				s += spacing * (double)(count - 1);
			} else {
				foreach (IWidgetSurface cw in Children.Where (b => b.Visible))
					s.UnionWith (cw.GetPreferredWidth ());
			}
			return s;
		}
		
		protected override WidgetSize OnGetPreferredHeight ()
		{
			WidgetSize s = new WidgetSize ();
			
			if (direction == Orientation.Vertical) {
				int count = 0;
				foreach (IWidgetSurface cw in Children.Where (b => b.Visible)) {
					s += cw.GetPreferredHeight ();
					count++;
				}
				s += spacing * (double)(count - 1);
			} else {
				foreach (IWidgetSurface cw in Children.Where (b => b.Visible))
					s.UnionWith (cw.GetPreferredHeight ());
			}
			return s;
		}
		
		protected override WidgetSize OnGetPreferredHeightForWidth (double width)
		{
			return GetPreferredLengthForSize (SizeRequestMode.HeightForWidth, width);
		}
		
		protected override WidgetSize OnGetPreferredWidthForHeight (double height)
		{
			return GetPreferredLengthForSize (SizeRequestMode.WidthForHeight, height);
		}
		
		WidgetSize GetPreferredLengthForSize (SizeRequestMode mode, double width)
		{
			WidgetSize s = new WidgetSize ();
			
			if ((direction == Orientation.Horizontal && mode == SizeRequestMode.HeightForWidth) || (direction == Orientation.Vertical && mode == SizeRequestMode.WidthForHeight)) {
				CalcDefaultSizes (mode, width, -1);
				foreach (var bp in children.Where (b => b.Child.Visible)) {
					s.UnionWith (GetPreferredLengthForSize (mode, bp.Child, bp.NextSize));
				}
			}
			else {
				int count = 0;
				foreach (var bp in children.Where (b => b.Child.Visible)) {
					s += GetPreferredLengthForSize (mode, bp.Child, width);
					count++;
				}
				s += spacing * (double)(count - 1);
			}
			return s;
		}
		
		WidgetSize GetPreferredSize (bool calcHeight, Widget w)
		{
			if (calcHeight)
				return ((IWidgetSurface)w).GetPreferredHeight ();
			else
				return ((IWidgetSurface)w).GetPreferredWidth ();
		}
		
		WidgetSize GetPreferredLengthForSize (SizeRequestMode mode, Widget w, double width)
		{
			IWidgetSurface surface = w;
			if (mode == SizeRequestMode.WidthForHeight)
				return surface.GetPreferredWidthForHeight (width);
			else
				return surface.GetPreferredHeightForWidth (width);
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
		BoxMode boxMode = BoxMode.None;
		int padding;
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
				position = value;
				parent.ChildChanged (this, "Position");
			}
		}
		
		[DefaultValue (BoxMode.None)]
		public BoxMode BoxMode {
			get {
				return this.boxMode;
			}
			set {
				boxMode = value;
				parent.ChildChanged (this, "BoxMode");
			}
		}

		[DefaultValue (0)]
		public int Padding {
			get {
				return this.padding;
			}
			set {
				padding = value;
				parent.ChildChanged (this, "Padding");
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

