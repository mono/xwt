// 
// ChildrenCollection.cs
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
using System.Collections.Generic;

namespace Xwt
{
	public class ChildrenCollection<T>: Collection<T>
	{
		ICollectionEventSink<T> eventSink;
		
		public ChildrenCollection (ICollectionEventSink<T> eventSink)
		{
			this.eventSink = eventSink;
		}
		
		protected override void InsertItem (int index, T item)
		{
			base.InsertItem (index, item);
			eventSink.AddedItem (item, index);
		}
		
		protected override void RemoveItem (int index)
		{
			T item = this[index];
			base.RemoveItem (index);
			eventSink.RemovedItem (item, index);
		}
		
		protected override void SetItem (int index, T item)
		{
			T oldItem = this[index];
			base.SetItem (index, item);
			eventSink.RemovedItem (oldItem, index);
			eventSink.AddedItem (item, index);
		}
		
		protected override void ClearItems ()
		{
			List<T> items = new List<T> (this);
			base.ClearItems ();
			for (int n=0; n<items.Count; n++)
				eventSink.RemovedItem (items[n], n);
		}
	}
	
	public interface ICollectionEventSink<T>
	{
		void AddedItem (T item, int index);
		void RemovedItem (T item, int index);
	}
}

