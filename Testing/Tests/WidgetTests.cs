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
using System.Threading;

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

		void WaitForEvents (int ms = 1)
		{
			DateTime t = DateTime.Now;
			do {
				Application.MainLoop.DispatchPendingEvents ();
				System.Threading.Thread.Sleep (20);
			} while ((DateTime.Now - t).TotalMilliseconds < ms);
		}

		public void ShowWindow (Window win)
		{
			var ev = new ManualResetEvent (false);
			
			win.Shown += delegate {
				ev.Set ();
			};
			
			win.Show ();
			ev.WaitForEvent ();
			Application.MainLoop.DispatchPendingEvents ();
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
			w.Margin = 4;
			Assert.AreEqual (4, w.Margin.Left);
			Assert.AreEqual (4, w.Margin.Top);
			Assert.AreEqual (4, w.Margin.Right);
			Assert.AreEqual (4, w.Margin.Bottom);
			w.Margin = new WidgetSpacing (10, 20, 30, 40);
			Assert.AreEqual (10, w.Margin.Left);
			Assert.AreEqual (20, w.Margin.Top);
			Assert.AreEqual (30, w.Margin.Right);
			Assert.AreEqual (40, w.Margin.Bottom);
			w.MarginLeft = 1;
			Assert.AreEqual (1, w.Margin.Left);
			Assert.AreEqual (20, w.Margin.Top);
			Assert.AreEqual (30, w.Margin.Right);
			Assert.AreEqual (40, w.Margin.Bottom);
			w.MarginTop = 2;
			Assert.AreEqual (1, w.Margin.Left);
			Assert.AreEqual (2, w.Margin.Top);
			Assert.AreEqual (30, w.Margin.Right);
			Assert.AreEqual (40, w.Margin.Bottom);
			w.MarginRight = 3;
			Assert.AreEqual (1, w.Margin.Left);
			Assert.AreEqual (2, w.Margin.Top);
			Assert.AreEqual (3, w.Margin.Right);
			Assert.AreEqual (40, w.Margin.Bottom);
			w.MarginBottom = 4;
			Assert.AreEqual (1, w.Margin.Left);
			Assert.AreEqual (2, w.Margin.Top);
			Assert.AreEqual (3, w.Margin.Right);
			Assert.AreEqual (4, w.Margin.Bottom);
		}

		[Test]
		public void Focus ()
		{
			using (var win = new Window ()) {
				var w = CreateWidget ();

				HBox box = new HBox ();
				TextEntry e = new TextEntry ();
				box.PackStart (e);
				box.PackStart (w);
				win.Content = box;
				win.Show ();
				win.Present ();

				Application.MainLoop.DispatchPendingEvents ();

				e.SetFocus ();

				Application.MainLoop.DispatchPendingEvents ();

				Assert.IsFalse (w.HasFocus);
		//		Assert.IsTrue (w.CanGetFocus);

				int gotFocus = 0;
				w.GotFocus += delegate {
					gotFocus++;
				};

				w.SetFocus ();

				if (w.CanGetFocus) {
					Assert.IsTrue (w.HasFocus);
					Assert.AreEqual (1, gotFocus);

					int lostFocus = 0;
					w.LostFocus += delegate {
						lostFocus++;
					};
					
					e.SetFocus ();
					
					Assert.IsFalse (w.HasFocus);
					//			Assert.AreEqual (1, lostFocus);
				} else {
					Assert.IsFalse (w.HasFocus);
					Assert.AreEqual (0, gotFocus);
				}
			}
		}

		[Test]
		public void MinSize ()
		{
			using (var win = new Window ()) {
				var w = CreateWidget ();

				VBox box1 = new VBox ();
				HBox box2 = new HBox ();
				HBox f = new HBox ();
				f.MinWidth = 10;
				f.MinHeight = 10;
				box1.PackStart (box2);
				box2.PackStart (f);
				f.PackStart (w, BoxMode.FillAndExpand);
				win.Content = box1;

				ShowWindow (win);

				var defw = w.Size.Width;
				var defh = w.Size.Height;

				w.MinWidth = 300;
				w.MinHeight = 400;
				WaitForEvents ();
				Assert.AreEqual (300d, w.MinWidth);
				Assert.AreEqual (300d, w.Size.Width);

				Assert.AreEqual (400d, w.MinHeight);
				Assert.AreEqual (400d, w.Size.Height);

				w.MinWidth = -1;
				w.MinHeight = -1;

				WaitForEvents ();
				Assert.AreEqual (-1, w.MinWidth);
				Assert.AreEqual (defw, w.Size.Width);

				Assert.AreEqual (-1, w.MinHeight);
				Assert.AreEqual (defh, w.Size.Height);
			}
		}

		[Test]
		public void Coordinates ()
		{
			double padding = 40;
			using (var win = new Window ()) {
				var w = CreateWidget ();
				w.MinWidth = 1;
				w.MinHeight = 1;
				win.Content = w;
				win.Padding = padding;
				win.Location = new Point (300,300);

				ShowWindow (win);

				Assert.AreEqual (w.ScreenBounds, win.ScreenBounds.Inflate (-padding,-padding));
			}
		}
	}

	static class EventHelper
	{
		public static void WaitForEvent (this ManualResetEvent ev)
		{
			DateTime t = DateTime.Now;
			do {
				Application.MainLoop.DispatchPendingEvents ();
				if (ev.WaitOne (100))
					return;
			} while ((DateTime.Now - t).TotalMilliseconds < 1000);

			Assert.Fail ("Event not fired");
		}
	}
}

