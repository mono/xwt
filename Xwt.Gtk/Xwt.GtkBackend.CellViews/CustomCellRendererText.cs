//
// CustomCellRendererText.cs
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
using Gtk;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class CustomCellRendererText: Gtk.CellRendererText, ICellDataSource
	{
		TreeViewBackend treeBackend;
		ITextCellViewFrontend view;
		TreeModel treeModel;
		TreeIter iter;

		public CustomCellRendererText (ITextCellViewFrontend view)
		{
			this.view = view;
		}

		public void LoadData (TreeViewBackend treeBackend, TreeModel treeModel, TreeIter iter)
		{
			this.treeBackend = treeBackend;
			this.treeModel = treeModel;
			this.iter = iter;
			view.Initialize (this);

			if (view.Markup != null) {
				FormattedText tx = FormattedText.FromMarkup (view.Markup);
				Text = tx.Text;
				var atts = new FastPangoAttrList ();
				atts.AddAttributes (new TextIndexer (tx.Text), tx.Attributes);
				Attributes = new Pango.AttrList (atts.Handle);
				atts.Dispose ();
			} else {
				Text = view.Text;
			}
			Editable = view.Editable;
			Visible = view.Visible;
		}
		
		public object GetValue (IDataField field)
		{
			return CellUtil.GetModelValue (treeModel, iter, field.Index);
		}

		protected override void OnEdited (string path, string new_text)
		{
			CellUtil.SetCurrentEventRow (treeBackend, path);

			if (!view.RaiseTextChanged () && view.TextField != null) {
				Gtk.TreeIter iter;
				if (treeModel.GetIterFromString (out iter, path))
					CellUtil.SetModelValue (treeModel, iter, view.TextField.Index, view.TextField.FieldType, new_text);
			}
			base.OnEdited (path, new_text);
		}
	}
}

