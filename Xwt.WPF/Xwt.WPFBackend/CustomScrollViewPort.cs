//
// CustomScrollViewPort.cs
//
// Author:
//	   Eric Maupin <ermau@xamarin.com>
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

			RenderTransform = this.transform;

			this.verticalBackend = verticalBackend;
			this.horizontalBackend = horizontalBackend;
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
			SetVerticalOffset (HorizontalOffset - HorizontalStepIncrement);
		}

		public void LineRight()
		{
			SetVerticalOffset (HorizontalOffset + HorizontalStepIncrement);
		}

		public void LineUp()
		{
			SetVerticalOffset (VerticalOffset - VerticalStepIncrement);
		}

		public Rect MakeVisible (Visual visual, Rect rectangle)
		{
			throw new System.NotImplementedException();
		}

		public void MouseWheelDown()
		{
			SetVerticalOffset (VerticalOffset + VerticalStepIncrement * 4);
		}

		public void MouseWheelLeft()
		{
			SetHorizontalOffset (HorizontalOffset - HorizontalStepIncrement * 4);
		}

		public void MouseWheelRight()
		{
			SetHorizontalOffset (HorizontalOffset + HorizontalStepIncrement * 4);
		}

		public void MouseWheelUp()
		{
			SetVerticalOffset (VerticalOffset - VerticalStepIncrement * 4);
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
			ScrollOwner.InvalidateScrollInfo();

			this.transform.X = -offset;
			if (this.verticalBackend != null)
				this.horizontalBackend.EventSink.OnValueChanged();
		}

		public void SetVerticalOffset (double offset)
		{
			if (offset < 0 || this.viewport.Height >= this.extent.Height)
				offset = 0;
			else if (offset + this.viewport.Height >= this.extent.Height)
				offset = this.extent.Height - this.viewport.Height;

			this.contentOffset.Y = offset;
			ScrollOwner.InvalidateScrollInfo();

			this.transform.Y = -offset;
			if (this.verticalBackend != null)
				this.verticalBackend.EventSink.OnValueChanged();
		}

		private readonly TranslateTransform transform = new TranslateTransform();
		private readonly ScrollAdjustmentBackend verticalBackend;
		private readonly ScrollAdjustmentBackend horizontalBackend;
		
		private Point contentOffset;
		private WSize extent = new WSize (0, 0);
		private WSize viewport = new WSize (0, 0);

		private static readonly WSize InfiniteSize
			= new System.Windows.Size (Double.PositiveInfinity, Double.PositiveInfinity);

		protected double VerticalPageIncrement
		{
			get { return (this.verticalBackend != null) ? this.verticalBackend.PageIncrement : 10; }
		}

		protected double HorizontalPageIncrement
		{
			get { return (this.horizontalBackend != null) ? this.horizontalBackend.PageIncrement : 10; }
		}

		protected double VerticalStepIncrement
		{
			get { return (this.verticalBackend != null) ? this.verticalBackend.StepIncrement : 1; }
		}

		protected double HorizontalStepIncrement
		{
			get { return (this.horizontalBackend != null) ? this.horizontalBackend.StepIncrement : 1; }
		}

		protected override WSize MeasureOverride (WSize constraint)
		{
			FrameworkElement child = (FrameworkElement) InternalChildren [0];
			child.Measure (InfiniteSize);
			
			WSize childSize = child.DesiredSize;

			if (Double.IsInfinity (constraint.Width))
				constraint.Width = ActualWidth;
			if (Double.IsInfinity (constraint.Height))
				constraint.Height = ActualHeight;

			if (this.extent != childSize) {
				this.extent = childSize;
				ScrollOwner.InvalidateScrollInfo();
			}

			if (this.viewport != constraint) {
				this.viewport = constraint;
				ScrollOwner.InvalidateScrollInfo();
			}

			return constraint;
		}

		protected override System.Windows.Size ArrangeOverride (System.Windows.Size finalSize)
		{
			FrameworkElement child = (FrameworkElement)InternalChildren [0];

			WSize childSize = child.DesiredSize;

			if (this.extent != childSize) {
				this.extent = childSize;
				ScrollOwner.InvalidateScrollInfo();
			}

			if (this.viewport != finalSize) {
				this.viewport = finalSize;
				ScrollOwner.InvalidateScrollInfo();
			}

			child.Arrange (new Rect (0, 0, child.ActualWidth, child.ActualHeight));

			return finalSize;
		}
	}
}