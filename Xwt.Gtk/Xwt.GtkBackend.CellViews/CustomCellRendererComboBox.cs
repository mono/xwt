//
// CustomCellRendererComboBox.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc (http://www.xamarin.com)
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
using System.Collections.Generic;

namespace Xwt.GtkBackend
{
	public class CustomCellRendererComboBox: CellViewBackend
	{
		Gtk.CellRendererCombo renderer;
		Dictionary<object, CustomListModel> modelCache = new Dictionary<object, CustomListModel> ();

		class DataSourceRef
		{
			readonly WeakReference source;
			readonly int hash;

			public DataSourceRef (IListDataSource dataSource)
			{
				hash = dataSource.GetHashCode ();
				source = new WeakReference (dataSource);
			}

			public bool IsAlive {
				get { return source.IsAlive; }
			}

			public override int GetHashCode ()
			{
				return hash;
			}

			public override bool Equals (object obj)
			{
				var otherDs = obj as IListDataSource;
				if (otherDs == null)
					otherDs = (obj as DataSourceRef)?.source.Target as IListDataSource;
				if (otherDs == null)
					return false;
				var ds = source.Target;
				return ds != null && (ds == otherDs || ds.Equals (otherDs));
			}
		}

		public CustomCellRendererComboBox ()
		{
			CellRenderer = renderer = new Gtk.CellRendererCombo ();
			renderer.HasEntry = false;
			renderer.Edited += HandleEdited;
		}

		protected override void OnLoadData ()
		{
			var view = (IComboBoxCellViewFrontend)Frontend;
			renderer.Text = view.SelectedText;
			var source = view.ItemsSource;
			renderer.Model = GetListModel (source).Store;
			renderer.TextColumn = 0;
			renderer.Editable = view.Editable;
			renderer.Visible = view.Visible;
		}

		CustomListModel GetListModel (IListDataSource source)
		{
			CustomListModel model;
			if (!modelCache.TryGetValue (source, out model))
				modelCache [new DataSourceRef (source)] = model = new CustomListModel (source, CellRendererTarget.EventRootWidget);
			return model;
		}

		void CleanCache ()
		{
			var newModel = new Dictionary<object, CustomListModel> ();
			foreach (var e in modelCache) {
				if (((DataSourceRef)e.Key).IsAlive)
					newModel.Add (e.Key, e.Value);
			}
			modelCache = newModel;
		}

		void HandleEdited (object o, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;
			if (!CellRendererTarget.Model.GetIterFromString (out iter, args.Path))
				return;
			
			LoadData (CellRendererTarget.Model, iter);
			SetCurrentEventRow ();

			var view = (IComboBoxCellViewFrontend)Frontend;
		
			if (!view.RaiseSelectionChanged ()) {
				if (view.SelectedItemField != null || view.SelectedIndexField != null) {
					var source = view.ItemsSource;
					var rowCount = source.RowCount;
					for (int n = 0; n < rowCount; n++) {
						if (args.NewText == source.GetValue (n, 0).ToString ()) {
							if (view.SelectedItemField != null)
								CellUtil.SetModelValue (TreeModel, CurrentIter, view.SelectedItemField.Index, typeof (object), source.GetValue (n, 1));
							if (view.SelectedIndexField != null)
								CellUtil.SetModelValue (TreeModel, CurrentIter, view.SelectedIndexField.Index, typeof (int), n);
							break;
						}
					}
				}
				if (view.SelectedTextField != null)
					CellUtil.SetModelValue (TreeModel, CurrentIter, view.SelectedTextField.Index, typeof (string), args.NewText);
			}
		}
	}
}
