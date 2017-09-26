//
// TreeViewTests.cs
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
	public class TreeViewTests: ScrollableWidgetTests
	{
		public override Widget CreateWidget ()
		{
			return new TreeView ();
		}

		public override IScrollableWidget CreateScrollableWidget ()
		{
			DataField<string> text = new DataField<string> ();
			TreeStore s = new TreeStore (text);
			var list = new TreeView (s);
			list.Columns.Add ("Hi", text);

			for (int n = 0; n < 100; n++) {
				var r = s.AddNode ();
				r.SetValue (text, n + new string ('.',100));
			}
			return list;
		}

		[Test]
		[Ignore("Disabling this test because the fix it relies on breaks the treeview. See https://bugzilla.xamarin.com/show_bug.cgi?id=58917 for more details")]
		public void HiddenTree ()
		{
			var f = new DataField<string> ();
			TreeStore ts = new TreeStore (f);
			var node = ts.AddNode ().SetValue (f, "1").AddChild ().SetValue (f, "2").AddChild ().SetValue (f, "3");
			var tree = new TreeView (ts);

			Window w = new Window ();
			Notebook nb = new Notebook ();
			nb.Add (new Label ("Hi"), "One");
			nb.Add (tree, "Two");
			w.Content = nb;
			ShowWindow (w);

			tree.ScrollToRow (node.CurrentPosition);

			tree.Columns.Add ("Hi", f);

			tree.ScrollToRow (node.CurrentPosition);
		}
	}
}

