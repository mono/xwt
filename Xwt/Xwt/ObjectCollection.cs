// 
// ObjectCollection.cs
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
using System.Collections.ObjectModel;
using Xwt.Backends;
using System.Collections.Generic;

namespace Xwt
{
	public class ObjectCollection<T>: Collection<T>
	{
		ICollectionListener listener;
		Action<T,bool> changeHandler;

		protected ObjectCollection (ICollectionListener listener)
		{
			this.listener = listener;
		}

		protected ObjectCollection (Action<T,bool> changeHandler)
		{
			this.changeHandler = changeHandler;
		}

		protected override void InsertItem (int index, T item)
		{
			base.InsertItem (index, item);
			if (listener != null)
				listener.ItemAdded (this, item);
			if (changeHandler != null)
				changeHandler (item, true);
		}

		protected override void RemoveItem (int index)
		{
			T ob = Items [index];
			base.RemoveItem (index);
			if (listener != null)
				listener.ItemRemoved (this, ob);
			if (changeHandler != null)
				changeHandler (ob, false);
		}

		protected override void SetItem (int index, T item)
		{
			T ob = Items [index];
			base.SetItem (index, item);
			if (listener != null) {
				listener.ItemRemoved (this, ob);
				listener.ItemAdded (this, item);
			}
			if (changeHandler != null) {
				changeHandler (ob, false);
				changeHandler (item, false);
			}
		}

		protected override void ClearItems ()
		{
			List<T> copy = new List<T> (Items);
			if (listener != null) {
				base.ClearItems ();
				foreach (var c in copy)
					listener.ItemRemoved (this, c);
			} else if (changeHandler != null) {
				base.ClearItems ();
				foreach (var c in copy)
					changeHandler (c, false);
			}
			else
				base.ClearItems ();
		}
	}
}

