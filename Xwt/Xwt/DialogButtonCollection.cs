// 
// CommandCollection.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xwt.Backends;
using System.Linq;

namespace Xwt
{
	public class DialogButtonCollection: Collection<DialogButton>
	{
		ICollectionListener listener;
		
		public DialogButtonCollection (ICollectionListener listener)
		{
			this.listener = listener;
		}
		
		public void Add (params Command[] commands)
		{
			foreach (var c in commands)
				Add (new DialogButton (c));
		}
		
		public DialogButton GetCommandButton (Command cmd)
		{
			return this.FirstOrDefault (b => b.Command == cmd);
		}
		
		protected override void InsertItem (int index, DialogButton item)
		{
			base.InsertItem (index, item);
			if (listener != null)
				listener.ItemAdded (this, item);
		}
		
		protected override void RemoveItem (int index)
		{
			object ob = this [index];
			base.RemoveItem (index);
			if (listener != null)
				listener.ItemRemoved (this, ob);
		}
		
		protected override void SetItem (int index, DialogButton item)
		{
			object ob = this [index];
			base.SetItem (index, item);
			if (listener != null)
				listener.ItemRemoved (this, ob);
			if (listener != null)
				listener.ItemAdded (this, item);
		}
		
		protected override void ClearItems ()
		{
			List<DialogButton> copy = new List<DialogButton> (this);
			base.ClearItems ();
			foreach (var c in copy)
				listener.ItemRemoved (this, c);
		}
	}
}

