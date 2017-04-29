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
	public class CustomCellRendererToggle: CellViewBackend
	{
		Gtk.CellRendererToggle renderer;

		public CustomCellRendererToggle ()
		{
			CellRenderer = renderer = new Gtk.CellRendererToggle ();
			renderer.Toggled += HandleToggled;
		}

		protected override void OnLoadData ()
		{
			var view = (IToggleCellViewFrontend)Frontend;
			renderer.Activatable = view.Editable;
			renderer.Visible = view.Visible;

			var check = view as ICheckBoxCellViewFrontend;
			if (check != null) {
				renderer.Inconsistent = check.State == CheckBoxState.Mixed;
				renderer.Active = check.State == CheckBoxState.On;
			}
			var radio = view as IRadioButtonCellViewFrontend;
			if (radio != null) {
				renderer.Radio = true;
				renderer.Active = radio.Active;
			}
		}

		void HandleToggled (object o, ToggledArgs args)
		{
			SetCurrentEventRow ();

			var view = (IToggleCellViewFrontend) Frontend;
			IDataField field = (IDataField)(view as ICheckBoxCellViewFrontend)?.StateField ?? view.ActiveField;

			if (!view.RaiseToggled () && (field != null)) {
				Type type = field.FieldType;

				Gtk.TreeIter iter;
				if (TreeModel.GetIterFromString (out iter, args.Path)) {
					CheckBoxState newState;

					if (type == typeof(CheckBoxState) && ((ICheckBoxCellViewFrontend)view).AllowMixed) {
						if (renderer.Inconsistent)
							newState = CheckBoxState.Off;
						else if (renderer.Active)
							newState = CheckBoxState.Mixed;
						else
							newState = CheckBoxState.On;
					} else {
						if (renderer.Active)
							newState = CheckBoxState.Off;
						else
							newState = CheckBoxState.On;
					}

					object newValue = type == typeof(CheckBoxState) ?
						(object) newState : (object) (newState == CheckBoxState.On);

					CellUtil.SetModelValue (TreeModel, iter, field.Index, type, newValue);
				}
			}
		}
	}
}

