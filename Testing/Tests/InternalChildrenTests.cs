//
// InternalChildrenTests.cs
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
using System.Linq;
using NUnit.Framework;

namespace Xwt
{
	public class InternalChildrenTests
	{
		[Test]
		public void ChildrenExcludesInternal ()
		{
			MyContainer co = new MyContainer ();
			var c1 = new Label ("hi1");
			var c2 = new Label ("hi2");
			Assert.AreEqual (0, co.Surface.Children.Count ());
			co.Add (c1);
			Assert.AreEqual (1, co.Surface.Children.Count ());
			co.AddInner (c2);
			Assert.AreEqual (2, co.Surface.Children.Count ());
			Assert.IsTrue (co.Surface.Children.Contains (c1));
			Assert.IsTrue (co.Surface.Children.Contains (c2));
		}

		[Test]
		public void ParentIsSet ()
		{
			MyContainer co = new MyContainer ();
			var c1 = new Label ("hi1");
			var c2 = new Label ("hi2");
			co.Add (c1);
			co.AddInner (c2);
			Assert.AreSame (co, c1.Parent);
			Assert.AreSame (co, c2.Parent);
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void InvalidAdd1 ()
		{
			MyContainer co = new MyContainer ();
			var c1 = new Label ("hi1");
			co.InternalAdd (c1);
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void InvalidAdd2 ()
		{
			MyContainer co = new MyContainer ();
			var c1 = new Label ("hi1");
			HBox b = new HBox ();
			b.PackStart (c1);
			co.InternalAdd (c1);
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void InvalidAdd3 ()
		{
			MyContainer co = new MyContainer ();
			var c1 = new Label ("hi1");
			HBox b = new HBox ();
			b.PackStart (c1);
			co.Add (c1);
		}

		[Test]
		public void Remove ()
		{
			MyContainer co = new MyContainer ();
			var c1 = new Label ("hi1");
			var c2 = new Label ("hi2");
			co.Add (c1);
			co.AddInner (c2);
			Assert.AreEqual (2, co.Surface.Children.Count ());
			co.RemoveInner (c2);
			Assert.AreEqual (1, co.Surface.Children.Count ());
			Assert.IsNull (c2.Parent);
			co.Remove (c1);
			Assert.AreEqual (0, co.Surface.Children.Count ());
			Assert.IsNull (c1.Parent);
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void InvalidRemove ()
		{
			MyContainer co = new MyContainer ();
			var c1 = new Label ("hi1");
			co.Add (c1);
			co.WrongRemove (c1);
		}

		[Test]
		public void InternalRemoveAdd ()
		{
			// The a child can be removed from internal containers and still
			// be a child of the container

			MyContainer co = new MyContainer ();
			Assert.AreEqual (0, co.Surface.Children.Count ());

			var c1 = new Label ("hi1");
			co.Add (c1);
			Assert.AreEqual (1, co.Surface.Children.Count ());

			co.InternalRemove (c1);
			Assert.AreEqual (1, co.Surface.Children.Count ());
			Assert.AreSame (co, c1.Parent);

			co.InternalAdd (c1);
			Assert.AreEqual (1, co.Surface.Children.Count ());
			Assert.AreSame (co, c1.Parent);
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void CantSetIternalBeforeAdding ()
		{
			new MyContainerWrong1 ();
		}
	}

	class MyContainer: Widget
	{
		internal HBox box = new HBox ();
		internal VBox inner = new VBox ();

		public MyContainer ()
		{
			Content = SetInternalChild (box);
			box.PackStart (SetInternalChild (inner));
		}

		public void Add (Widget w)
		{
			RegisterChild (w);
			box.PackStart (w);
		}

		public void AddInner (Widget w)
		{
			RegisterChild (w);
			inner.PackStart (w);
		}

		public void Remove (Widget w)
		{
			box.Remove (w);
			UnregisterChild (w);
		}

		public void RemoveInner (Widget w)
		{
			inner.Remove (w);
			UnregisterChild (w);
		}

		public void WrongRemove (Widget w)
		{
			UnregisterChild (w);
			box.Remove (w);
		}

		public void InternalRemove (Widget w)
		{
			box.Remove (w);
		}

		public void InternalAdd (Widget w)
		{
			box.PackStart (w);
		}
	}

	class MyContainerWrong1: Widget
	{
		public MyContainerWrong1 ()
		{
			HBox box = new HBox ();
			Content = box;
			SetInternalChild (box);
		}
	}
}

