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
	public class CustomCellRendererText: CellViewBackend
	{
		Gtk.CellRendererText cellRenderer;
		bool mixedMarkupText;

		public CustomCellRendererText ()
		{
			CellRenderer = cellRenderer = new Gtk.CellRendererText ();
			cellRenderer.Edited += HandleEdited;
		}

		protected override void OnLoadData ()
		{
			var view = (ITextCellViewFrontend) Frontend;

			if (view.Markup != null) {
				FormattedText tx = FormattedText.FromMarkup (view.Markup);
				cellRenderer.Text = tx.Text;
				var atts = new FastPangoAttrList ();
				atts.AddAttributes (new TextIndexer (tx.Text), tx.Attributes);
				cellRenderer.Attributes = new Pango.AttrList (atts.Handle);
				atts.Dispose ();
				mixedMarkupText = true;
			} else {
				cellRenderer.Text = view.Text;
				if (mixedMarkupText)
					cellRenderer.Attributes = new Pango.AttrList ();
			}
			cellRenderer.Editable = view.Editable;
			cellRenderer.Ellipsize = view.Ellipsize.ToGtkValue ();
		}
		
		void HandleEdited (object o, EditedArgs args)
		{
			SetCurrentEventRow ();
			var view = (ITextCellViewFrontend) Frontend;

			if (!view.RaiseTextChanged (args.NewText) && view.TextField != null) {
				Gtk.TreeIter iter;
				if (TreeModel.GetIterFromString (out iter, args.Path))
					CellUtil.SetModelValue (TreeModel, iter, view.TextField.Index, view.TextField.FieldType, args.NewText);
			}
		}
	}
}

