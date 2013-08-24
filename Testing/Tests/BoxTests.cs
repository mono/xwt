//
// BoxTests.cs
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

namespace Xwt
{
	public abstract class BoxTests: ContainerTests
	{
		public override Widget CreateWidget ()
		{
			var box = CreateBox ();
			box.PackStart (new Label ("Hello Worlds"));
			return box;
		}
		
		protected override void AddChild (Widget parent, Widget child)
		{
			((Box)parent).Clear ();
			((Box)parent).PackStart (child, true);
		}

		public abstract Box CreateBox ();

		protected abstract Rectangle AdjustedRect (Rectangle r);

		public Rectangle ToScreenBounds (Window w, Rectangle r)
		{
			r = AdjustedRect (r);
			var wb = w.ScreenBounds;
			return r.Offset (wb.Location);
		}

		Box PrepareBox (Window win)
		{
			win.Padding = 0;
			win.Size = new Size (100, 100);
			var box = CreateBox ();
			win.Content = box;
			return box;
		}

		[Test]
		public void SinglePack ()
		{
			using (Window win = new Window ()) {
				var box = PrepareBox (win);
				SquareBox c = new SquareBox ();
				box.PackStart (c);
				ShowWindow (win);

				Assert.AreEqual (ToScreenBounds (win, new Rectangle (0, 0, 10, 100)), c.ScreenBounds);
			}
		}

		[Test]
		public void SinglePackExpand ()
		{
			using (Window win = new Window ()) {
				var box = PrepareBox (win);
				SquareBox c = new SquareBox ();
				box.PackStart (c, true);
				ShowWindow (win);

				Assert.AreEqual (ToScreenBounds (win, new Rectangle (0, 0, 100, 100)), c.ScreenBounds);
			}
		}
	}
}

