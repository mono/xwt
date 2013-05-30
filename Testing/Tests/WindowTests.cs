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

namespace Xwt
{
	public class WindowTests: XwtTest
	{
		[Test]
		public void DefaultSize ()
		{
			using (var win = new Window ()) {
				Label test = new Label ("Testing");
				test.MinWidth = 100;
				test.MinHeight = 100;
				win.Content = test;
				ShowWindow (win);
				Assert.AreEqual (100, win.Size.Width);
				Assert.AreEqual (100, win.Size.Height);
			}
		}

		[Test]
		public void ContentMargin ()
		{
			using (var win = new Window ()) {
				Label test = new Label ("Testing");
				test.MinWidth = 100;
				test.MinHeight = 100;
				win.Content = test;
				ShowWindow (win);
				Assert.AreEqual (100, win.Size.Width);
				Assert.AreEqual (100, win.Size.Height);
			}
		}
	}
}

