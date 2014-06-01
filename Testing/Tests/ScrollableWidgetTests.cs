//
// ScrollableWidgetTests.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using NUnit.Framework;

namespace Xwt
{
	public abstract class ScrollableWidgetTests: WidgetTests
	{
		public abstract IScrollableWidget CreateScrollableWidget ();

		[Test]
		public void BasicScrolling ()
		{
			var list = CreateScrollableWidget ();

			Window w = new Window ();
			w.Content = (Widget) list;
			w.Width = w.Height = 300;
			ShowWindow (w);

			// Vertical

			Assert.AreEqual (list.VerticalScrollControl.LowerValue, list.VerticalScrollControl.Value);

			int changed = 0;
			list.VerticalScrollControl.ValueChanged += delegate {
				changed++;
			};

			var mid = Math.Truncate ((list.VerticalScrollControl.UpperValue - list.VerticalScrollControl.LowerValue) / 2);
			list.VerticalScrollControl.Value = mid;

			Assert.AreEqual (1, changed);
			Assert.AreEqual (mid, list.VerticalScrollControl.Value);

			Assert.IsTrue (list.VerticalScrollControl.UpperValue > list.VerticalScrollControl.LowerValue);
			Assert.IsTrue (list.VerticalScrollControl.PageSize > 0);
			Assert.IsTrue (list.VerticalScrollControl.PageIncrement > 0);
			Assert.IsTrue (list.VerticalScrollControl.StepIncrement > 0);
			Assert.IsTrue (list.VerticalScrollControl.PageIncrement <= list.VerticalScrollControl.PageSize);

			// Horizontal

			Assert.AreEqual (list.VerticalScrollControl.LowerValue, list.HorizontalScrollControl.Value);

			changed = 0;
			list.HorizontalScrollControl.ValueChanged += delegate {
				changed++;
			};

			mid = Math.Truncate ((list.HorizontalScrollControl.UpperValue - list.HorizontalScrollControl.LowerValue) / 2);
			list.HorizontalScrollControl.Value = mid;

			Assert.AreEqual (1, changed);
			Assert.AreEqual (mid, list.HorizontalScrollControl.Value);

			Assert.IsTrue (list.HorizontalScrollControl.UpperValue > list.HorizontalScrollControl.LowerValue);
			Assert.IsTrue (list.HorizontalScrollControl.PageSize > 0);
			Assert.IsTrue (list.HorizontalScrollControl.PageIncrement > 0);
			Assert.IsTrue (list.HorizontalScrollControl.StepIncrement > 0);
			Assert.IsTrue (list.HorizontalScrollControl.PageIncrement <= list.HorizontalScrollControl.PageSize);
		}
	}
}

