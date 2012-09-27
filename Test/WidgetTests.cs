//
// Widget.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc
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
	[TestFixture]
	public abstract class WidgetTests
	{
		public abstract Widget CreateWidget ();

		[TestFixtureSetUp]
		public void Init ()
		{
		}

		[TestFixtureTearDown]
		public void Cleanup ()
		{
		}

		public void Run (Action a)
		{
			Exception ex = null;
			Application.Invoke (delegate {
				try {
					a ();
				} catch (Exception e) {
					ex = e;
				}
				Application.Exit ();
			});
			Application.Run ();
			if (ex != null)
				throw new Exception ("Exception in gui event loop", ex);
		}

		[Test]
		public void Visibility ()
		{
			var w = CreateWidget ();
			Assert.IsTrue (w.Visible, "Not visible by default");
			w.Hide ();
			Assert.IsFalse (w.Visible);
			w.Show ();
			Assert.IsTrue (w.Visible);
			w.Visible = false;
			Assert.IsFalse (w.Visible);
			w.Visible = true;
			Assert.IsTrue (w.Visible);
		}

		[Test]
		public void Sensitivity ()
		{
			var w = CreateWidget ();
			Assert.IsTrue (w.Sensitive, "Not sensitive by default");
			w.Sensitive = false;
			Assert.IsFalse (w.Sensitive);
			w.Sensitive = true;
			Assert.IsTrue (w.Sensitive);
		}
		
		[Test]
		public void ParentWindow ()
		{
			var w = CreateWidget ();
			var win = new Window ();
			win.Content = w;
			Assert.AreSame (win, w.ParentWindow);
			win.Dispose ();
		}

		[Test]
		public void Margin ()
		{
			var w = CreateWidget ();
			Assert.AreEqual (0, w.Margin.Left);
			Assert.AreEqual (0, w.Margin.Top);
			Assert.AreEqual (0, w.Margin.Right);
			Assert.AreEqual (0, w.Margin.Bottom);
			w.Margin.SetAll (4);
			Assert.AreEqual (4, w.Margin.Left);
			Assert.AreEqual (4, w.Margin.Top);
			Assert.AreEqual (4, w.Margin.Right);
			Assert.AreEqual (4, w.Margin.Bottom);
			w.Margin.Set (10, 20, 30, 40);
			Assert.AreEqual (10, w.Margin.Left);
			Assert.AreEqual (20, w.Margin.Top);
			Assert.AreEqual (30, w.Margin.Right);
			Assert.AreEqual (40, w.Margin.Bottom);
			w.Margin.Left = 1;
			Assert.AreEqual (1, w.Margin.Left);
			Assert.AreEqual (20, w.Margin.Top);
			Assert.AreEqual (30, w.Margin.Right);
			Assert.AreEqual (40, w.Margin.Bottom);
			w.Margin.Top = 2;
			Assert.AreEqual (1, w.Margin.Left);
			Assert.AreEqual (2, w.Margin.Top);
			Assert.AreEqual (30, w.Margin.Right);
			Assert.AreEqual (40, w.Margin.Bottom);
			w.Margin.Right = 3;
			Assert.AreEqual (1, w.Margin.Left);
			Assert.AreEqual (2, w.Margin.Top);
			Assert.AreEqual (3, w.Margin.Right);
			Assert.AreEqual (40, w.Margin.Bottom);
			w.Margin.Bottom = 4;
			Assert.AreEqual (1, w.Margin.Left);
			Assert.AreEqual (2, w.Margin.Top);
			Assert.AreEqual (3, w.Margin.Right);
			Assert.AreEqual (4, w.Margin.Bottom);
		}

		[Test]
		public void Focus ()
		{
			var win = new Window ();
			var w = CreateWidget ();

			Run (delegate {
				HBox box = new HBox ();
				TextEntry e = new TextEntry ();
				box.PackStart (e);
				box.PackStart (w);
				win.Content = box;
				win.Show ();
				win.Present ();

//				for (int n=0; n < 500; n++) {
					Application.DispatchPendingEvents ();
//					System.Threading.Thread.Sleep (10);
//				}

				e.SetFocus ();

				Application.DispatchPendingEvents ();

				Assert.IsFalse (w.HasFocus);
		//		Assert.IsTrue (w.CanGetFocus);

				int gotFocus = 0;
				w.GotFocus += delegate {
					gotFocus++;
				};

				w.SetFocus ();
				Assert.IsTrue (w.HasFocus);
				Assert.AreEqual (1, gotFocus);

				int lostFocus = 0;
				w.LostFocus += delegate {
					lostFocus++;
				};

				e.SetFocus ();

				Assert.IsFalse (w.HasFocus);
	//			Assert.AreEqual (1, lostFocus);

				win.Dispose ();
			});
		}

		[Test]
		[Ignore]
		public void MinSize ()
		{
			var win = new Window ();
			var w = CreateWidget ();

			win.Content = w;
			win.Show ();

			Application.DispatchPendingEvents ();

			var defw = w.Size.Width;
			var defh = w.Size.Height;

			w.MinWidth = 300;
			Assert.AreEqual (300, w.MinWidth);
			Assert.AreEqual (300, w.Size.Width);

			w.MinHeight = 400;
			Assert.AreEqual (400, w.MinHeight);
			Assert.AreEqual (400, w.Size.Height);

			w.MinWidth = -1;
			Assert.AreEqual (-1, w.MinWidth);
			Assert.AreEqual (defw, w.Size.Width);

			w.MinHeight = -1;
			Assert.AreEqual (-1, w.MinHeight);
			Assert.AreEqual (defh, w.Size.Height);

			win.Dispose ();
		}

		[Test]
		[Ignore]
		public void Coordinates ()
		{
			var win = new Window ();
			var w = CreateWidget ();
			win.Content = w;
			win.Show ();

			Application.DispatchPendingEvents ();

			Assert.AreEqual (w.ScreenBounds, win.ScreenBounds);

			win.Dispose ();
		}
	}
}

