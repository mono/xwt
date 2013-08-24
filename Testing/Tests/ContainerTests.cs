//
// ContainerTests.cs
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
	public abstract class ContainerTests: WidgetTests
	{
		protected abstract void AddChild (Widget parent, Widget child);

		[Test]
		public void ChildMargin ()
		{
			using (var win = new Window ()) {
				var c = CreateWidget ();
				win.Content = c;
				win.Size = new Size (100, 100);
				var box = new SquareBox (10);
				AddChild (c, box);
				ShowWindow (win);
				VerifyMargin (box);
			}
		}
		
		[Test]
		public void ChildAlignment ()
		{
			using (var win = new Window ()) {
				var c = CreateWidget ();
				win.Content = c;
				win.Size = new Size (100, 100);
				var box = new SquareBox (10);
				AddChild (c, box);
				ShowWindow (win);
				VerifyAlignment (box);
			}
		}
	}
}

