//
// WindowTests.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using Xwt.Drawing;

namespace Xwt
{
	public class WindowTests: XwtTest
	{
		[Test]
		public void DefaultSize ()
		{
			using (var win = new Window ()) {
				win.Padding = 0;
				var test = new VariableSizeBox (200);
				win.Content = test;
				ShowWindow (win);
				Assert.AreEqual (200, win.Size.Width);
				Assert.AreEqual (100, win.Size.Height);
				Assert.AreEqual (200, test.ScreenBounds.Width);
				Assert.AreEqual (100, test.ScreenBounds.Height);
			}
		}

		[Test]
		public void FlexibleContentGrowMakesWindowNotGrow ()
		{
			using (var win = new Window ()) {
				win.Padding = 0;
				var test = new VariableSizeBox (200);
				win.Content = test;
				ShowWindow (win);
				Assert.AreEqual (200, win.Size.Width);
				Assert.AreEqual (100, win.Size.Height);
				test.Size = 300;
				// The preferred size grows, but the widget honors the constraint given
				// by the window (the initial size of the window), so it doesn't make
				// the window grow
				WaitForEvents ();
				Assert.AreEqual (200, win.Size.Width);
				Assert.AreEqual (100, win.Size.Height);
				Assert.AreEqual (200, test.ScreenBounds.Width);
				Assert.AreEqual (100, test.ScreenBounds.Height);
			}
		}
		
		[Test]
		public void FixedContentGrowMakesWindowGrow ()
		{
			using (var win = new Window ()) {
				win.Padding = 0;
				var test = new VariableSizeBox (200);
				test.ForceSize = true;
				win.Content = test;
				ShowWindow (win);
				Assert.AreEqual (200, win.Size.Width);
				Assert.AreEqual (100, win.Size.Height);
				test.Size = 300;
				// The preferred size grows, and it is bigger that the constraint provided
				// by the window (the initial size of the window), so the window grows to adapt
				WaitForEvents ();
				Assert.AreEqual (300, win.Size.Width);
				Assert.AreEqual (150, win.Size.Height);
				Assert.AreEqual (300, test.ScreenBounds.Width);
				Assert.AreEqual (150, test.ScreenBounds.Height);
			}
		}

		[Test]
		public void ContentWidthGrows ()
		{
			using (var win = new Window ()) {
				win.Padding = 0;
				var test = new VariableSizeBox (200);
				win.Content = test;
				ShowWindow (win);
				Assert.AreEqual (200, win.Size.Width);
				Assert.AreEqual (100, win.Size.Height);
				win.Width = 300;
				WaitForEvents ();
				Assert.AreEqual (300, win.Size.Width);
				Assert.AreEqual (150, win.Size.Height);
				Assert.AreEqual (300, test.ScreenBounds.Width);
				Assert.AreEqual (150, test.ScreenBounds.Height);
			}
		}

		[Test]
		public void FixedWidth ()
		{
			using (var win = new Window ()) {
				win.Padding = 0;
				var test = new VariableSizeBox (200);
				win.Content = test;
				win.Width = 300;
				ShowWindow (win);
				WaitForEvents ();
				Assert.AreEqual (300, win.Size.Width);
				Assert.AreEqual (150, win.Size.Height);
				Assert.AreEqual (300, test.ScreenBounds.Width);
				Assert.AreEqual (150, test.ScreenBounds.Height);
			}
		}

		[Test]
		public void DefaultSizeWithMinContentSize ()
		{
			using (var win = new Window ()) {
				win.Padding = 0;
				SquareBox test = new SquareBox ();
				test.MinWidth = 200;
				test.MinHeight = 200;
				win.Content = test;
				ShowWindow (win);
				Assert.AreEqual (200, win.Size.Width);
				Assert.AreEqual (200, win.Size.Height);
				Assert.AreEqual (200, test.ScreenBounds.Width);
				Assert.AreEqual (200, test.ScreenBounds.Height);
			}
		}

		[Test]
		public void ContentMargin ()
		{
			using (var win = new Window ()) {
				win.Padding = 0;
				SquareBox test = new SquareBox ();
				test.MinWidth = 200;
				test.MinHeight = 200;
				test.Margin = 5;
				win.Content = test;
				ShowWindow (win);
				Assert.AreEqual (210, win.Size.Width);
				Assert.AreEqual (210, win.Size.Height);
				Assert.AreEqual (200, test.ScreenBounds.Width);
				Assert.AreEqual (200, test.ScreenBounds.Height);
			}
		}
		
		[Test]
		public void ContentMarginChange ()
		{
			// The size of the window grows if a specific size has not been set
			using (var win = new Window ()) {
				win.Padding = 0;
				SquareBox test = new SquareBox ();
				test.MinWidth = 200;
				test.MinHeight = 200;
				test.Margin = 5;
				win.Content = test;
				ShowWindow (win);
				Assert.AreEqual (210, win.Size.Width);
				Assert.AreEqual (210, win.Size.Height);
				Assert.AreEqual (200, test.ScreenBounds.Width);
				Assert.AreEqual (200, test.ScreenBounds.Height);
				test.Margin = 10;
				WaitForEvents ();
				Assert.AreEqual (220, win.Size.Width);
				Assert.AreEqual (220, win.Size.Height);
				Assert.AreEqual (200, test.ScreenBounds.Width);
				Assert.AreEqual (200, test.ScreenBounds.Height);
			}
		}

		[Test]
		public void Close ()
		{
			using (var win = new Window ()) {
				ShowWindow (win);
				bool closing = false, closed = false;
				win.CloseRequested += delegate(object sender, CloseRequestedEventArgs args) {
					Assert.IsTrue (args.AllowClose);
					closing = true;
				};
				win.Closed += (sender, e) => closed = true;
				win.Close ();
				Assert.IsTrue (closing, "CloseRequested event not fired");
				Assert.IsTrue (closed, "Window not closed");
			}
		}

		[Test]
		public void CloseCancel ()
		{
			bool closed = false;
			using (var win = new Window ()) {
				ShowWindow (win);
				win.CloseRequested += delegate(object sender, CloseRequestedEventArgs args) {
					args.AllowClose = false;
				};
				win.Closed += (sender, e) => closed = true;
				win.Close ();
				Assert.IsFalse (closed, "Window should not be closed");
			}
			Assert.IsFalse (closed, "Window should not be closed");
		}

		[Test]
		public void InitialLocationManualWithoutContent ()
		{
			using (var win = new Window ()) {
				win.InitialLocation = WindowLocation.Manual;
				win.Location = new Point (210, 230);
				win.Size = new Size (100, 100);
				ShowWindow (win);
				Assert.AreEqual (210, win.X);
				Assert.AreEqual (230, win.Y);
			}
		}

		[Test]
		public void InitialLocationManualWithoutContentAndSize ()
		{
			using (var win = new Window ()) {
				win.InitialLocation = WindowLocation.Manual;
				win.Location = new Point (210, 230);
				ShowWindow (win);
				Assert.AreEqual (210, win.X);
				Assert.AreEqual (230, win.Y);
			}
		}

		[Test]
		public void InitialLocationManual ()
		{
			using (var win = new Window ()) {
				win.InitialLocation = WindowLocation.Manual;
				win.Content = new Label ("Hi there!");
				win.Location = new Point (210, 230);
				ShowWindow (win);
				Assert.AreEqual (210, win.X);
				Assert.AreEqual (230, win.Y);
			}
		}
	}

	public class SquareBox: Canvas
	{
		double size;

		public SquareBox (double size = 10)
		{
			this.size = size;
		}

		protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return new Size (size, size);
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			ctx.Rectangle (dirtyRect);
			ctx.SetColor (Colors.Red);
			ctx.Fill ();
		}
	}
	
	class VariableSizeBox: Canvas
	{
		double size;
		bool forceSize;

		public VariableSizeBox (double size)
		{
			this.size = size;
		}

		public new double Size {
			get { return size; }
			set {
				size = value;
				OnPreferredSizeChanged ();
			}
		}

		public bool ForceSize {
			get { return forceSize; }
			set {
				forceSize = value;
				OnPreferredSizeChanged ();
			}
		}

		// The height of this widget is always half of the width
		protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			if (widthConstraint.IsConstrained) {
				var w = forceSize ? Math.Max (size, widthConstraint.AvailableSize) : widthConstraint.AvailableSize;
				return new Size (w, w / 2);
			} else if (heightConstraint.IsConstrained) {
				var h = forceSize ? Math.Max (size / 2, widthConstraint.AvailableSize) : widthConstraint.AvailableSize;
				return new Size (h * 2, h);
			}
			return new Size (size, size / 2);
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			ctx.Rectangle (dirtyRect);
			ctx.SetColor (Colors.Red);
			ctx.Fill ();
		}
	}
}

