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
			PackStart (widget, BoxMode.None, 0);
		}
		
		public void PackStart (Widget widget, BoxMode mode)
		{
			PackStart (widget, mode, 0);
		}
		
		public void PackStart (Widget widget, BoxMode mode, double padding)
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
		
		public void PackEnd (Widget widget, BoxMode mode, double padding)
		{
			Pack (widget, mode, padding, PackOrigin.End);
		}
		
		void Pack (Widget widget, BoxMode mode, double padding, PackOrigin ptype)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			if (padding < 0)
				throw new ArgumentException ("padding can't be negative");
			var p = new BoxPlacement ((WidgetBackendHost)BackendHost, widget);
			p.BoxMode = mode;
			p.Padding = padding;
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
			if (size.Width == 0 || size.Height == 0)
				return;
			
			var visibleChildren = children.Where (c => c.Child.Visible).ToArray ();
			IWidgetBackend[] widgets = new IWidgetBackend [visibleChildren.Length];
			Rectangle[] rects = new Rectangle [visibleChildren.Length];
			
			if (direction == Orientation.Horizontal) {
				CalcDefaultSizes (size.Width, size.Height);
				double xs = 0;
				double xe = size.Width + spacing;
				for (int n=0; n<visibleChildren.Length; n++) {
					var bp = visibleChildren [n];
					if (bp.PackOrigin == PackOrigin.End)
						xe -= bp.NextSize + spacing;
					double x = bp.PackOrigin == PackOrigin.Start ? xs : xe;
					widgets[n] = (IWidgetBackend)GetBackend (bp.Child);
					rects[n] = new Rectangle (x, 0, bp.NextSize >= 0 ? bp.NextSize : 0, size.Height >= 0 ? size.Height : 0).Round ();
					if (bp.PackOrigin == PackOrigin.Start)
						xs += bp.NextSize + spacing;
				}
			} else {
				CalcDefaultSizes (size.Width, size.Height);
				double ys = 0;
				double ye = size.Height + spacing;
				for (int n=0; n<visibleChildren.Length; n++) {
					var bp = visibleChildren [n];
					if (bp.PackOrigin == PackOrigin.End)
						ye -= bp.NextSize + spacing;
					double y = bp.PackOrigin == PackOrigin.Start ? ys : ye;
					widgets[n] = (IWidgetBackend)GetBackend (bp.Child);
					rects[n] = new Rectangle (0, y, size.Width >= 0 ? size.Width : 0, bp.NextSize >= 0 ? bp.NextSize : 0).Round ();
					if (bp.PackOrigin == PackOrigin.Start)
						ys += bp.NextSize + spacing;
				}
			}
			Backend.SetAllocation (widgets, rects);
			
			if (!BackendHost.EngineBackend.HandlesSizeNegotiation) {
				foreach (var bp in visibleChildren)
					bp.Child.Surface.Reallocate ();
			}
		}
		
		void CalcDefaultSizes (double width, double height)
		{
			bool vertical = direction == Orientation.Vertical;
			int nexpands = 0;
			double requiredSize = 0;
			double availableSize = vertical ? height : width;

			var widthConstraint = vertical ? SizeContraint.RequireSize (width) : SizeContraint.Unconstrained;
			var heightConstraint = vertical ? SizeContraint.Unconstrained : SizeContraint.RequireSize (height);

			var visibleChildren = children.Where (b => b.Child.Visible).ToArray ();
			var sizes = new Dictionary<BoxPlacement,double> ();
			
			// Get the natural size of each child
			foreach (var bp in visibleChildren) {
				Size s;
				s = bp.Child.Surface.GetPreferredSize (widthConstraint, heightConstraint);
				bp.NextSize = vertical ? s.Height : s.Width;
				sizes [bp] = bp.NextSize;
				requiredSize += bp.NextSize;
				if ((bp.BoxMode & BoxMode.Expand) != 0)
					nexpands++;
			}
			
			double remaining = availableSize - requiredSize - (spacing * (double)(visibleChildren.Length - 1));
			if (remaining < 0) {
				// The box is not big enough to fit the widgets using its natural size.
				// We have to shrink the widgets.
				
				// The total amount we have to shrink
				double shrinkSize = -remaining;
				
				var sizePart = new SizeSplitter (shrinkSize, visibleChildren.Length);
				foreach (var bp in visibleChildren)
					bp.NextSize -= sizePart.NextSizePart ();
			}
			else {
				var expandRemaining = new SizeSplitter (remaining, nexpands);
				foreach (var bp in visibleChildren) {
					if ((bp.BoxMode & BoxMode.Expand) != 0)
						bp.NextSize += expandRemaining.NextSizePart ();
				}
			}
		}
		
		protected override Size OnGetPreferredSize (SizeContraint widthConstraint, SizeContraint heightConstraint)
		{
			Size s = new Size ();
			int count = 0;

			if (direction == Orientation.Horizontal) {
				foreach (var cw in Children.Where (b => b.Visible)) {
					var wsize = cw.Surface.GetPreferredSize (SizeContraint.Unconstrained, heightConstraint);
					s.Width += wsize.Width;
					if (wsize.Height > s.Height)
						s.Height = wsize.Height;
					count++;
				}
				if (count > 0)
					s.Width += spacing * (double)(count - 1);
			} else {
				foreach (var cw in Children.Where (b => b.Visible)) {
					var wsize = cw.Surface.GetPreferredSize (widthConstraint, SizeContraint.Unconstrained);
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
		BoxMode boxMode = BoxMode.None;
		double padding;
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

		[DefaultValue (0d)]
		public double Padding {
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

