//
// CustomScrollViewPort.cs
//
// Author:
//	   Eric Maupin <ermau@xamarin.com>
//     Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SWC = System.Windows.Controls;
using WSize = System.Windows.Size;

namespace Xwt.WPFBackend
{
	internal class CustomScrollViewPort
		: SWC.Panel, IScrollInfo
	{
		internal CustomScrollViewPort (object widget, ScrollAdjustmentBackend verticalBackend, ScrollAdjustmentBackend horizontalBackend)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");

			((FrameworkElement)widget).RenderTransform = this.transform;

			if (verticalBackend != null) {
				usingCustomScrolling = true;
				verticalBackend.TargetViewport = this;
				this.verticalBackend = verticalBackend;
				horizontalBackend.TargetViewport = this;
				this.horizontalBackend = horizontalBackend;
				UpdateCustomExtent ();
			}
			Children.Add ((UIElement) widget);
		}

		public bool CanHorizontallyScroll
		{
			get;
			set;
		}

		public bool CanVerticallyScroll
		{
			get;
			set;
		}

		public double ExtentHeight
		{
			get { return this.extent.Height; }
		}

		public double ExtentWidth
		{
			get { return this.extent.Width; }
		}

		public double ViewportHeight
		{
			get { return this.viewport.Height; }
		}

		public double ViewportWidth
		{
			get { return this.viewport.Width; }
		}

		public double VerticalOffset
		{
			get { return this.contentOffset.Y; }
		}

		public double HorizontalOffset
		{
			get { return this.contentOffset.X; }
		}

		public void LineDown()
		{
			SetVerticalOffset (VerticalOffset + VerticalStepIncrement);
		}

		public void LineLeft()
		{
			SetHorizontalOffset (HorizontalOffset - HorizontalStepIncrement);
		}

		public void LineRight()
		{
			SetHorizontalOffset (HorizontalOffset + HorizontalStepIncrement);
		}

		public void LineUp()
		{
			SetVerticalOffset (VerticalOffset - VerticalStepIncrement);
		}

		public Rect MakeVisible (Visual visual, Rect rectangle)
		{
			// This is the area which is currently visible
			var visibleRect = new Rect (HorizontalOffset, VerticalOffset, ViewportWidth, ViewportHeight);

			// This is the area we wish to be visible
			rectangle = visual.TransformToAncestor (this).TransformBounds (rectangle);

			if (visibleRect == rectangle)
				return rectangle;

			// The co-ordinates are relative to the visible area, so we need to add the visible area offset
			// to convert to values we can use in VerticalOffset/HorizontalOffset
			rectangle.X += visibleRect.X;
			rectangle.Y += visibleRect.Y;

			SetHorizontalOffset (ClampOffset (visibleRect.Left, visibleRect.Right, rectangle.Left, rectangle.Right));
			SetVerticalOffset (ClampOffset (visibleRect.Top, visibleRect.Bottom, rectangle.Top, rectangle.Bottom));

			return rectangle;
		}

		private double ClampOffset (double lowerVisible, double upperVisible, double desiredLower, double desiredUpper)
		{
			// This is not entirely correct. The 'else' statement needs to be improved
			// so we scroll the minimum required distance as opposed to just jumping
			// to 'desiredLower' if our element doesn't already fit perfectly
			if (desiredLower >= lowerVisible && desiredUpper < upperVisible)
				return lowerVisible;
			else
				return desiredLower; 
		}

		public void MouseWheelDown()
		{
			SetVerticalOffset (VerticalOffset + 12);
		}

		public void MouseWheelLeft()
		{
			SetHorizontalOffset (HorizontalOffset - 12);
		}

		public void MouseWheelRight()
		{
			SetHorizontalOffset (HorizontalOffset + 12);
		}

		public void MouseWheelUp()
		{
			SetVerticalOffset (VerticalOffset - 12);
		}

		public void PageDown()
		{
			SetVerticalOffset (VerticalOffset + VerticalPageIncrement);
		}

		public void PageLeft()
		{
			SetHorizontalOffset (HorizontalOffset - HorizontalPageIncrement);
		}

		public void PageRight()
		{
			SetHorizontalOffset (HorizontalOffset + HorizontalPageIncrement);
		}

		public void PageUp()
		{
			SetVerticalOffset (VerticalOffset - VerticalPageIncrement);
		}

		public SWC.ScrollViewer ScrollOwner
		{
			get;
			set;
		}

		public void SetHorizontalOffset (double offset)
		{
			if (offset < 0 || this.viewport.Width >= this.extent.Width)
				offset = 0;
			else if (offset + this.viewport.Width >= this.extent.Width)
				offset = this.extent.Width - this.viewport.Width;

			this.contentOffset.X = offset;
			if (usingCustomScrolling)
				this.horizontalBackend.SetOffset (offset);
			else
				this.transform.X = -offset;
			if (ScrollOwner != null)
				ScrollOwner.InvalidateScrollInfo ();
		}

		public void SetVerticalOffset (double offset)
		{
			if (offset < 0 || this.viewport.Height >= this.extent.Height)
				offset = 0;
			else if (offset + this.viewport.Height >= this.extent.Height)
				offset = this.extent.Height - this.viewport.Height;

			this.contentOffset.Y = offset;
			if (usingCustomScrolling)
				this.verticalBackend.SetOffset (offset);
			else
				this.transform.Y = -offset;
			if (ScrollOwner != null)
				ScrollOwner.InvalidateScrollInfo ();
		}

		public void SetOffset (ScrollAdjustmentBackend scroller, double offset)
		{
			if (scroller == verticalBackend)
				SetVerticalOffset (offset);
			else
				SetHorizontalOffset (offset);
		}

		public void UpdateCustomExtent ()
		{
			// Updates the extent and the viewport, based on the scrollbar properties

			var newExtent = new WSize (horizontalBackend.UpperValue - horizontalBackend.LowerValue, verticalBackend.UpperValue - verticalBackend.LowerValue);
			var newViewport = new WSize (horizontalBackend.PageSize, verticalBackend.PageSize);
			if (newViewport.Width > newExtent.Width)
				newViewport.Width = newExtent.Width;
			if (newViewport.Height > newExtent.Height)
				newViewport.Height = newExtent.Height;

			if (extent != newExtent || viewport != newViewport) {
				extent = newExtent;
				viewport = newViewport;
				if (!viewportAdjustmentQueued) {
					viewportAdjustmentQueued = true;
					Application.MainLoop.QueueExitAction (delegate
					{
						// Adjust the position, if it now falls outside the extents.
						// Doing it in an exit action to make sure the adjustement
						// is made only once for all changes in the scrollbar properties
						viewportAdjustmentQueued = false;
						if (contentOffset.X + viewport.Width > extent.Width)
							SetHorizontalOffset (extent.Width - viewport.Width);
						if (contentOffset.Y + viewport.Height > extent.Height)
							SetVerticalOffset (extent.Height - viewport.Height);
						if (ScrollOwner != null)
							ScrollOwner.InvalidateScrollInfo ();
					});
				}
			}
		}

		private readonly TranslateTransform transform = new TranslateTransform();
		private readonly ScrollAdjustmentBackend verticalBackend;
		private readonly ScrollAdjustmentBackend horizontalBackend;
		private readonly bool usingCustomScrolling;

		private bool viewportAdjustmentQueued;
		private Point contentOffset;
		private WSize extent = new WSize (0, 0);
		private WSize viewport = new WSize (0, 0);

		private static readonly WSize InfiniteSize
			= new System.Windows.Size (Double.PositiveInfinity, Double.PositiveInfinity);

		protected double VerticalPageIncrement
		{
			get { return (this.verticalBackend != null) ? this.verticalBackend.PageIncrement : ViewportHeight; }
		}

		protected double HorizontalPageIncrement
		{
			get { return (this.horizontalBackend != null) ? this.horizontalBackend.PageIncrement : ViewportWidth; }
		}

		protected double VerticalStepIncrement
		{
			get { return (this.verticalBackend != null) ? this.verticalBackend.StepIncrement : VerticalPageIncrement / 10; }
		}

		protected double HorizontalStepIncrement
		{
			get { return (this.horizontalBackend != null) ? this.horizontalBackend.StepIncrement : HorizontalPageIncrement / 10; }
		}

		protected override WSize MeasureOverride (WSize constraint)
		{
			FrameworkElement child = (FrameworkElement) InternalChildren [0];

			if (usingCustomScrolling) {
				// Measure the child using the constraint because when using custom scrolling,
				// the child is not really scrolled (its contents are) and its size is whatever
				// the scroll view decides to assign to the viewport, so the constraint
				// must be satisfied
				child.Measure (constraint);
				return child.DesiredSize;
			}
			else {
				// We don't use the child size here, but WPF requires Measure to
				// be called for all children of a widget in the container's MeasureOverride
				child.Measure (InfiniteSize);
				return new WSize (0, 0);
			}
		}

		protected override System.Windows.Size ArrangeOverride (System.Windows.Size finalSize)
		{
			FrameworkElement child = (FrameworkElement)InternalChildren [0];

			WSize childSize = child.DesiredSize;

			// The child has to fill all the available space in the ScrollView
			// if the ScrollView happens to be bigger than the space required by the child
			if (childSize.Height < finalSize.Height)
				childSize.Height = finalSize.Height;
			if (childSize.Width < finalSize.Width)
				childSize.Width = finalSize.Width;

			if (!usingCustomScrolling) {
				// The viewport and extent doesn't have to be set when using custom scrolling, since they
				// are fully controlled by the child widget through the scroll adjustments
				if (this.extent != childSize) {
					this.extent = childSize;
					ScrollOwner.InvalidateScrollInfo ();
				}

				if (this.viewport != finalSize) {
					this.viewport = finalSize;
					ScrollOwner.InvalidateScrollInfo ();
				}
			}

			child.Arrange (new Rect (0, 0, childSize.Width, childSize.Height));
			child.UpdateLayout ();
			if (child is IWpfWidget)
				((IWidgetSurface)(((IWpfWidget)child).Backend.Frontend)).Reallocate ();

			return finalSize;
		}
	}
}