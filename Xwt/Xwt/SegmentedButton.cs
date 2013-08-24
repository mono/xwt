// 
// SegmentedButton.cs
//  
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
// 
// Copyright (c) 2013 Xamarin Inc
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
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt
{
	/// <summary>
	/// A segmented button is an horizontal row of button homogeneously sized
	/// </summary>
	[BackendType (typeof(ISegmentedButtonBackend))]
	public class SegmentedButton : Widget
	{
		class ButtonCollection : Collection<Button>
		{
			SegmentedButton parent;

			ISegmentedButtonBackend Backend {
				get {
					return parent.Backend;
				}
			}

			internal ButtonCollection (SegmentedButton parent)
			{
				this.parent = parent;
			}

			protected override void InsertItem (int index, Button item)
			{
				parent.RegisterChild (item);
				Backend.AddChildButton (index, item);
				base.InsertItem (index, item);
			}

			protected override void RemoveItem (int index)
			{
				parent.UnregisterChild (this [index]);
				Backend.RemoveChildButton (index);
				base.RemoveItem (index);
			}

			protected override void SetItem (int index, Button item)
			{
				parent.UnregisterChild (this [index]);
				parent.RegisterChild (item);
				Backend.ReplaceChildButton (index, item);
				base.SetItem (index, item);
			}

			protected override void ClearItems ()
			{
				for (int i = 0; i < Count; i++) {
					parent.UnregisterChild (this [i]);
					Backend.RemoveChildButton (i);
				}
				base.ClearItems ();
			}
		}

		ButtonCollection items;

		public SegmentedButton ()
		{
		}

		public SegmentedButton (IEnumerable<Button> buttons)
		{
			VerifyConstructorCall (this);
			foreach (var button in buttons)
				Items.Add (button);
		}

		ISegmentedButtonBackend Backend {
			get { return (ISegmentedButtonBackend) BackendHost.Backend; }
		}

		public Collection<Button> Items {
			get {
				if (items == null)
					items = new ButtonCollection (this);
				return items;
			}
		}

		public int ButtonDefaultSpacing {
			get {
				return Backend.Spacing;
			}
			set {
				Backend.Spacing = value;
			}
		}
	}
}

