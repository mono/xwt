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
	public abstract class WidgetTests: XwtTest
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
				f.PackStart (w, true);
				win.Content = box1;

				ShowWindow (win);

				WaitForEvents ();
				var defw = w.Size.Width;
				var defh = w.Size.Height;

				// Min size has priority over the preferred size

				w.MinWidth = 300;
				w.MinHeight = 400;
				WaitForEvents ();
				Assert.AreEqual (300d, w.MinWidth);
				Assert.AreEqual (300d, w.Size.Width);

				Assert.AreEqual (400d, w.MinHeight);
				Assert.AreEqual (400d, w.Size.Height);

				if (defw > 1) {
					w.MinWidth = defw - 1;
					WaitForEvents ();
					Assert.AreEqual (defw - 1, w.MinWidth);
					Assert.AreEqual (defw, w.Size.Width);
				}
				if (defh > 1) {
					w.MinHeight = defh - 1;
					WaitForEvents ();
					Assert.AreEqual (defh - 1, w.MinHeight);
					Assert.AreEqual (defh, w.Size.Height);
				}

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
		public void RequestedSize ()
		{
			using (var win = new Window ()) {
				var w = CreateWidget ();

				VBox box1 = new VBox ();
				HBox box2 = new HBox ();
				HBox f = new HBox ();
				box1.PackStart (box2);
				box2.PackStart (f);
				f.PackStart (w, true);
				win.Content = box1;

				ShowWindow (win);

				WaitForEvents ();
				var defw = w.Size.Width;
				var defh = w.Size.Height;

				w.WidthRequest = 300;
				w.HeightRequest = 400;
				WaitForEvents ();
				Assert.AreEqual (300d, w.WidthRequest);
				Assert.AreEqual (300d, w.Size.Width);

				Assert.AreEqual (400d, w.HeightRequest);
				Assert.AreEqual (400d, w.Size.Height);

				// Size request has priority over min size

				w.MinWidth = 310;
				w.MinHeight = 410;
				WaitForEvents ();
				Assert.AreEqual (300d, w.Size.Width);
				Assert.AreEqual (400d, w.Size.Height);

				w.MinWidth = 290;
				w.MinHeight = 390;
				WaitForEvents ();
				Assert.AreEqual (300d, w.Size.Width);
				Assert.AreEqual (400d, w.Size.Height);

				// Size request has priority over preferred size, so it can make a widget smaller than the default size

				w.MinWidth = -1;
				w.MinHeight = -1;

				if (defw > 1) {
					w.WidthRequest = defw - 1;
					WaitForEvents ();
					Assert.AreEqual (defw - 1, w.WidthRequest, "w1");
					Assert.AreEqual (defw - 1, w.Size.Width, "w2");
				}
				if (defh > 1) {
					w.HeightRequest = defh - 1;
					WaitForEvents ();
					Assert.AreEqual (defh - 1, w.HeightRequest, "h1");
					Assert.AreEqual (defh - 1, w.Size.Height, "h2");
				}

				w.WidthRequest = -1;
				w.HeightRequest = -1;

				WaitForEvents ();
				Assert.AreEqual (-1, w.WidthRequest);
				Assert.AreEqual (defw, w.Size.Width, "fw1");

				Assert.AreEqual (-1, w.HeightRequest);
				Assert.AreEqual (defh, w.Size.Height, "fw2");
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

				Assert.AreEqual (win.ScreenBounds.Inflate (-padding,-padding), w.ScreenBounds);
				Assert.AreEqual (w.ParentBounds.Location, new Point (padding, padding));
			}
		}

		public void VerifyMargin (SquareBox box)
		{
			var r1 = box.ScreenBounds;

			box.Margin = new WidgetSpacing (5, 10, 15, 20);
			WaitForEvents ();
			var r2 = box.ScreenBounds;

			Assert.AreEqual (r1.Left + 5, r2.Left);
			Assert.AreEqual (r1.Top + 10, r2.Top);
			Assert.AreEqual (r1.Width - 20, r2.Width);
			Assert.AreEqual (r1.Height - 30, r2.Height);
			
			box.Margin = 0;
			WaitForEvents ();
			r2 = box.ScreenBounds;
			Assert.AreEqual (r1, r2);
		}
		
		public void VerifyAlignment (SquareBox box)
		{
			var r1 = box.ScreenBounds;
			
			// Horizontal Fill

			box.HorizontalPlacement = WidgetPlacement.Fill;
			box.VerticalPlacement = WidgetPlacement.Fill;
			WaitForEvents ();
			Assert.AreEqual (r1, box.ScreenBounds);

			box.VerticalPlacement = WidgetPlacement.Start;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Left, r1.Top, r1.Width, 10), box.ScreenBounds);
			
			box.VerticalPlacement = WidgetPlacement.Center;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Left, Math.Truncate (r1.Center.Y - 5), r1.Width, 10), box.ScreenBounds);
			
			box.VerticalPlacement = WidgetPlacement.End;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Left, r1.Bottom - 10, r1.Width, 10), box.ScreenBounds);

			// Horizontal Start

			box.HorizontalPlacement = WidgetPlacement.Start;
			box.VerticalPlacement = WidgetPlacement.Fill;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Left, r1.Top, 10, r1.Height), box.ScreenBounds);
			
			box.VerticalPlacement = WidgetPlacement.Start;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Left, r1.Top, 10, 10), box.ScreenBounds);

			box.VerticalPlacement = WidgetPlacement.Center;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Left, Math.Truncate (r1.Center.Y - 5), 10, 10), box.ScreenBounds);

			box.VerticalPlacement = WidgetPlacement.End;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Left, r1.Bottom - 10, 10, 10), box.ScreenBounds);

			// Horizontal Center

			box.HorizontalPlacement = WidgetPlacement.Center;
			box.VerticalPlacement = WidgetPlacement.Fill;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (Math.Truncate (r1.Center.X - 5), r1.Top, 10, r1.Height), box.ScreenBounds);
			
			box.VerticalPlacement = WidgetPlacement.Start;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (Math.Truncate (r1.Center.X - 5), r1.Top, 10, 10), box.ScreenBounds);

			box.VerticalPlacement = WidgetPlacement.Center;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (Math.Truncate (r1.Center.X - 5), Math.Truncate (r1.Center.Y - 5), 10, 10), box.ScreenBounds);

			box.VerticalPlacement = WidgetPlacement.End;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (Math.Truncate (r1.Center.X - 5), r1.Bottom - 10, 10, 10), box.ScreenBounds);

			// Horizontal End

			box.HorizontalPlacement = WidgetPlacement.End;
			box.VerticalPlacement = WidgetPlacement.Fill;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Right - 10, r1.Top, 10, r1.Height), box.ScreenBounds);
			
			box.VerticalPlacement = WidgetPlacement.Start;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Right - 10, r1.Top, 10, 10), box.ScreenBounds);

			box.VerticalPlacement = WidgetPlacement.Center;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Right - 10, Math.Truncate (r1.Center.Y) - 5, 10, 10), box.ScreenBounds);

			box.VerticalPlacement = WidgetPlacement.End;
			WaitForEvents ();
			Assert.AreEqual (new Rectangle (r1.Right - 10, r1.Bottom - 10, 10, 10), box.ScreenBounds);
		}
	}
}

