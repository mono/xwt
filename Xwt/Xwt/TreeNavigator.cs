// 
// TreeNavigator.cs
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
using Xwt.Backends;

namespace Xwt
{
	public struct NodePosition
	{
		internal TreePosition ParentPos;
		internal int Index;
	}
	
	public class TreeNavigator: TreeViewNavigator
	{
		protected new ITreeStoreBackend Backend {
			get {
				return (ITreeStoreBackend)base.Backend;
			}
		}

		[Obsolete]
		internal TreeNavigator (ITreeStoreBackend backend, TreePosition pos) : base (backend, pos)
		{
		}

		internal TreeNavigator (ITreeDataSource backend, TreePosition pos, int index) : base (backend, pos, index)
		{
		}

		public new TreeNavigator Clone ()
		{
			return new TreeNavigator (Backend, CurrentPosition, CurrentIndex);
		}

		public TreeNavigator InsertBefore ()
		{
			CommitPos (Backend.InsertBefore (CurrentPosition));
			return this;
		}
		
		public TreeNavigator InsertAfter ()
		{
			CommitPos (Backend.InsertAfter (CurrentPosition));
			return this;
		}
		
		public TreeNavigator AddChild ()
		{
			CommitPos (Backend.AddChild (CurrentPosition));
			return this;
		}
		
		public TreeNavigator SetValue<T> (IDataField<T> field, T data)
		{
			Backend.SetValue (CurrentPosition, field.Index, data);
			return this;
		}
		

		public TreeNavigator SetValues<T1,T2> (int row, IDataField<T1> column1, T1 value1, IDataField<T2> column2, T2 value2)
		{
			SetValue (column1, value1);
			SetValue (column2, value2);
			return this;
		}

		public TreeNavigator SetValues<T1,T2,T3> (int row, IDataField<T1> column1, T1 value1, IDataField<T2> column2, T2 value2, IDataField<T3> column3, T3 value3)
		{
			SetValue (column1, value1);
			SetValue (column2, value2);
			SetValue (column3, value3);
			return this;
		}

		public TreeNavigator SetValues<T1,T2,T3,T4> (
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4
		)
		{
			SetValue (column1, value1);
			SetValue (column2, value2);
			SetValue (column3, value3);
			SetValue (column4, value4);
			return this;
		}

		public TreeNavigator SetValues<T1,T2,T3,T4,T5> (
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5
		)
		{
			SetValue (column1, value1);
			SetValue (column2, value2);
			SetValue (column3, value3);
			SetValue (column4, value4);
			SetValue (column5, value5);
			return this;
		}

		public TreeNavigator SetValues<T1,T2,T3,T4,T5,T6> (
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6
		)
		{
			SetValue (column1, value1);
			SetValue (column2, value2);
			SetValue (column3, value3);
			SetValue (column4, value4);
			SetValue (column5, value5);
			SetValue (column6, value6);
			return this;
		}

		public TreeNavigator SetValues<T1,T2,T3,T4,T5,T6,T7> (
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6,
			IDataField<T7> column7, T7 value7
		)
		{
			SetValue (column1, value1);
			SetValue (column2, value2);
			SetValue (column3, value3);
			SetValue (column4, value4);
			SetValue (column5, value5);
			SetValue (column6, value6);
			SetValue (column7, value7);
			return this;
		}

		public TreeNavigator SetValues<T1,T2,T3,T4,T5,T6,T7,T8> (
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6,
			IDataField<T7> column7, T7 value7,
			IDataField<T8> column8, T8 value8
		)
		{
			SetValue (column1, value1);
			SetValue (column2, value2);
			SetValue (column3, value3);
			SetValue (column4, value4);
			SetValue (column5, value5);
			SetValue (column6, value6);
			SetValue (column7, value7);
			SetValue (column8, value8);
			return this;
		}

		public TreeNavigator SetValues<T1,T2,T3,T4,T5,T6,T7,T8,T9> (
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6,
			IDataField<T7> column7, T7 value7,
			IDataField<T8> column8, T8 value8,
			IDataField<T9> column9, T9 value9
		)
		{
			SetValue (column1, value1);
			SetValue (column2, value2);
			SetValue (column3, value3);
			SetValue (column4, value4);
			SetValue (column5, value5);
			SetValue (column6, value6);
			SetValue (column7, value7);
			SetValue (column8, value8);
			SetValue (column9, value9);
			return this;
		}

		public TreeNavigator SetValues<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> (
			IDataField<T1> column1, T1 value1, 
			IDataField<T2> column2, T2 value2, 
			IDataField<T3> column3, T3 value3,
			IDataField<T4> column4, T4 value4,
			IDataField<T5> column5, T5 value5,
			IDataField<T6> column6, T6 value6,
			IDataField<T7> column7, T7 value7,
			IDataField<T8> column8, T8 value8,
			IDataField<T9> column9, T9 value9,
			IDataField<T10> column10, T10 value10
		)
		{
			SetValue (column1, value1);
			SetValue (column2, value2);
			SetValue (column3, value3);
			SetValue (column4, value4);
			SetValue (column5, value5);
			SetValue (column6, value6);
			SetValue (column7, value7);
			SetValue (column8, value8);
			SetValue (column9, value9);
			SetValue (column10, value10);
			return this;
		}
		
		public void Remove ()
		{
			Backend.Remove (CurrentPosition);
		}
		
		public void RemoveChildren ()
		{
			TreePosition child = Backend.GetChild (CurrentPosition, 0);
			while (child != null) {
				Backend.Remove (child);
				child = Backend.GetChild (CurrentPosition, 0);
			}
		}
	}
}
