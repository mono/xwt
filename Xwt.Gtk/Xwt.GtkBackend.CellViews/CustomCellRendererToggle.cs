//
// CustomCellRendererToggle.cs
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
using System.Linq;
using Gtk;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class CustomCellRendererToggle: Gtk.CellRendererToggle, ICellDataSource
	{
		TreeViewBackend treeBackend;
		ICheckBoxCellViewFrontend view;
		TreeModel treeModel;
		TreeIter iter;

		public CustomCellRendererToggle (ICheckBoxCellViewFrontend view)
		{
			this.view = view;
		}

		public void LoadData (TreeViewBackend treeBackend, TreeModel treeModel, TreeIter iter)
		{
			this.treeBackend = treeBackend;
			this.treeModel = treeModel;
			this.iter = iter;
			view.Initialize (this);

			Inconsistent = view.State == CheckBoxState.Mixed;
			Active = view.State == CheckBoxState.On;
			Activatable = view.Editable;
			Visible = view.Visible;
		}

		public object GetValue (IDataField field)
		{
			return CellUtil.GetModelValue (treeModel, iter, field.Index);
		}

		protected override void OnToggled (string path)
		{
			CellUtil.SetCurrentEventRow (treeBackend, path);

			IDataField field = (IDataField) view.StateField ?? view.ActiveField;

			if (!view.RaiseToggled () && (field != null)) {
				Type type = field.FieldType;

				Gtk.TreeIter iter;
				if (treeModel.GetIterFromString (out iter, path)) {
					CheckBoxState newState;

					if (view.AllowMixed && type == typeof(CheckBoxState)) {
						if (Inconsistent)
							newState = CheckBoxState.Off;
						else if (Active)
							newState = CheckBoxState.Mixed;
						else
							newState = CheckBoxState.On;
					} else {
						newState = (!Active).ToCheckBoxState ();
					}

					object newValue = type == typeof(CheckBoxState) ?
						(object) newState : (object) (newState == CheckBoxState.On);

					CellUtil.SetModelValue (treeModel, iter, field.Index, type, newValue);
				}
			}
			base.OnToggled (path);
		}
	}
}

