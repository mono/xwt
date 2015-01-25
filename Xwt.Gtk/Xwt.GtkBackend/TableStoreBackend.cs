// 
// TableStoreBackend.cs
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
using Xwt.Drawing;

using Xwt.Backends;
using Gtk;
#if XWT_GTK3
using TreeModel = Gtk.ITreeModel;
#endif

namespace Xwt.GtkBackend
{
	public abstract class TableStoreBackend
	{
		public Type[] ColumnTypes {
			get;
			private set;
		}

		public TreeModel Store {
			get;
			protected set;
		}

		public ApplicationContext ApplicationContext {
			get;
			private set;
		}

		public void Initialize (Type[] columnTypes)
		{
			ColumnTypes = new Type[columnTypes.Length];
			for (int n=0; n<ColumnTypes.Length; n++) {
				if (columnTypes [n] == typeof(Gtk.Image))
					ColumnTypes [n] = typeof(ImageDescription);
				else if (columnTypes [n] == typeof(object))
					ColumnTypes [n] = typeof(ObjectWrapper);
				else
					ColumnTypes [n] = columnTypes [n];
			}
			Store = InitializeModel (ColumnTypes);
		}

		public abstract TreeModel InitializeModel (Type[] columnTypes);
		
		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			ApplicationContext = context;
		}

		public void SetValue (Gtk.TreeIter it, int column, object value)
		{
			CellUtil.SetModelValue (Store, it, column, ColumnTypes [column], value);
		}

		public object GetValue (Gtk.TreeIter it, int column)
		{
			return CellUtil.GetModelValue (Store, it, column);
		}
	}
	
	class ObjectWrapper
	{
		public object Object;
		
		public ObjectWrapper (object ob)
		{
			Object = ob;
		}
	}
}

