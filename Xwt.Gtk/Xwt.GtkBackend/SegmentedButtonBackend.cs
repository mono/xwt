// 
// SegmentedButtonBackend.cs
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

using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class SegmentedButtonBackend : WidgetBackend, ISegmentedButtonBackend
	{
		public SegmentedButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = new CustomHButtonBox ();
			Widget.LayoutStyle = Gtk.ButtonBoxStyle.Start;
			base.Widget.Show ();
		}
		
		protected new Gtk.HButtonBox Widget {
			get { return (Gtk.HButtonBox)base.Widget; }
			set { base.Widget = value; }
		}

		public void AddChildButton (int index, Button button)
		{
			// TODO: support index
			Widget.Add ((Gtk.Widget)((WidgetBackend)Toolkit.GetBackend (button)).NativeWidget);
		}

		public void RemoveChildButton (int index)
		{
			Widget.Remove (Widget.Children [index]);
		}

		public void ReplaceChildButton (int index, Button newButton)
		{
			// TODO
		}

		public int Spacing {
			get {
				return Widget.Spacing;
			}
			set {
				Widget.Spacing = value;
			}
		}

		public class CustomHButtonBox : Gtk.HButtonBox
		{
			// The default implementation of HButtonBox don't pay attention
			// to its allocation for its child buttons using instead the size
			// they request. Change that
			protected override void OnSizeAllocated (Gdk.Rectangle allocation)
			{
				base.OnSizeAllocated (allocation);

				var children = Children;
				if (children.Length == 0)
					return;

				int childHeight = (int)(allocation.Height - 2 * BorderWidth);
				int childWidth = (allocation.Width - Spacing * (children.Length - 1)) / children.Length;
				int x = (int)(allocation.X + BorderWidth);
				int y = allocation.Y + (allocation.Height - childHeight) / 2;
				var childAllocation = new Gdk.Rectangle ();
				childAllocation.Width = childWidth;
				childAllocation.Height = childHeight;

				foreach (var child in Children) {
					if (!child.Visible)
						continue;

					childAllocation.Y = y;
					childAllocation.X = x;
					x += childWidth + Spacing;

					child.SizeAllocate (childAllocation);
				}
			}
		}
	}
}

