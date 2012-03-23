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
using Xwt.Backends;
using SWC = System.Windows.Controls;

namespace Xwt.WPFBackend
{
	public class CustomScrollViewPort
		: SWC.UserControl, IScrollInfo
	{
		private readonly ScrollAdjustmentBackend verticalBackend;
		private readonly ScrollAdjustmentBackend horizontalBackend;
		private double verticalOffset, horizontalOffset;

		internal CustomScrollViewPort (object widget, ScrollAdjustmentBackend verticalBackend, ScrollAdjustmentBackend horizontalBackend)
		{
			if (widget == null)
				throw new ArgumentNullException ("widget");
			if (verticalBackend == null)
				throw new ArgumentNullException ("verticalBackend");
			if (horizontalBackend == null)
				throw new ArgumentNullException ("horizontaBackend");

			this.verticalBackend = verticalBackend;
			this.horizontalBackend = horizontalBackend;
			Content = widget;
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
			get { throw new System.NotImplementedException(); }
		}

		public double ExtentWidth
		{
			get { throw new System.NotImplementedException(); }
		}

		public double ViewportHeight
		{
			get { return ActualHeight; }
		}

		public double ViewportWidth
		{
			get { return ActualWidth; }
		}

		public double VerticalOffset
		{
			get { return this.verticalOffset; }
		}

		public double HorizontalOffset
		{
			get { return this.horizontalOffset; }
		}

		public void LineDown()
		{
			SetVerticalOffset (VerticalOffset + this.verticalBackend.StepIncrement);
		}

		public void LineLeft()
		{
			SetVerticalOffset (HorizontalOffset - this.horizontalBackend.StepIncrement);
		}

		public void LineRight()
		{
			SetVerticalOffset (HorizontalOffset + this.horizontalBackend.StepIncrement);
		}

		public void LineUp()
		{
			SetVerticalOffset (VerticalOffset - this.verticalBackend.StepIncrement);
		}

		public Rect MakeVisible (Visual visual, Rect rectangle)
		{
			throw new System.NotImplementedException();
		}

		public void MouseWheelDown()
		{
			SetVerticalOffset (VerticalOffset + this.verticalBackend.StepIncrement);
		}

		public void MouseWheelLeft()
		{
			SetHorizontalOffset (HorizontalOffset - this.horizontalBackend.StepIncrement);
		}

		public void MouseWheelRight()
		{
			SetHorizontalOffset (HorizontalOffset + this.horizontalBackend.StepIncrement);
		}

		public void MouseWheelUp()
		{
			SetVerticalOffset (VerticalOffset - this.verticalBackend.StepIncrement);
		}

		public void PageDown()
		{
			SetVerticalOffset (VerticalOffset + this.verticalBackend.PageIncrement);
		}

		public void PageLeft()
		{
			SetHorizontalOffset (HorizontalOffset - this.horizontalBackend.PageIncrement);
		}

		public void PageRight()
		{
			SetHorizontalOffset (HorizontalOffset + this.horizontalBackend.PageIncrement);
		}

		public void PageUp()
		{
			SetVerticalOffset (VerticalOffset - this.verticalBackend.PageIncrement);
		}

		public SWC.ScrollViewer ScrollOwner
		{
			get;
			set;
		}

		public void SetHorizontalOffset (double offset)
		{
			if (offset < 0)
				offset = 0;

			this.horizontalOffset = offset;
			Xwt.Engine.Toolkit.Invoke (this.horizontalBackend.EventSink.OnValueChanged);
			ScrollOwner.InvalidateScrollInfo();
		}

		public void SetVerticalOffset (double offset)
		{
			if (offset < 0)
				offset = 0;

			this.verticalOffset = offset;
			Xwt.Engine.Toolkit.Invoke (this.verticalBackend.EventSink.OnValueChanged);
			ScrollOwner.InvalidateScrollInfo();
		}
	}
}