// 
// MenuItemCollection.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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

namespace Xwt
{
	public class MenuItemCollection: Collection<MenuItem>
	{
		Menu parent;
		
		internal MenuItemCollection (Menu parent)
		{
			this.parent = parent;
		}
		
		protected override void InsertItem (int index, MenuItem item)
		{
			base.InsertItem (index, item);
			parent.InsertItem (index, item);
		}
		
		protected override void RemoveItem (int index)
		{
			var item = this[index];
			base.RemoveItem (index);
			parent.RemoveItem (item);
		}
		
		protected override void SetItem (int index, MenuItem item)
		{
			var oldItem = this[index];
			base.SetItem (index, item);
			parent.RemoveItem (oldItem);
			parent.InsertItem (index, item);
		}
		
		protected override void ClearItems ()
		{
			foreach (var item in this)
				parent.RemoveItem (item);
			base.ClearItems ();
		}
	}
}

