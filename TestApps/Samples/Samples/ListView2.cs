using System;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class ListView2: VBox
	{
		public ListView2 ()
		{
			ListView list = new ListView ();
			var editableActiveField = new DataField<bool> ();
			var nonEditableActiveField = new DataField<bool> ();
			var textField = new DataField<string> ();
			var textField2 = new DataField<string> ();
			var editableField = new DataField<bool> ();
			var somewhatEditableData = new DataField<bool>();

			ListStore store = new ListStore(editableActiveField, nonEditableActiveField, textField, textField2, editableField, somewhatEditableData);
			list.DataSource = store;
			list.GridLinesVisible = GridLines.Horizontal;

			var cellView = new CheckBoxCellView { Editable = true, ActiveField = editableActiveField };
			cellView.Toggled += (sender, e) => {

				if (list.CurrentEventRow == null) {
					MessageDialog.ShowError("CurrentEventRow is null. This is not supposed to happen");
				}
				else {
					store.SetValue(list.CurrentEventRow, textField, "Toggled");
				}
			};
			cellView.EditingFinished += CellView_EditingFinished;

			list.Columns.Add (new ListViewColumn("Editable", cellView));

			list.Columns.Add (new ListViewColumn("Not Editable", new CheckBoxCellView { Editable = false, ActiveField = nonEditableActiveField }));

			Xwt.Backends.IEditableCellViewFrontend<string> stringCellView = new TextCellView { Editable = true, TextField = textField };
			stringCellView.EditingFinished += StringCellView_EditingFinished;
			list.Columns.Add (new ListViewColumn("Editable",(CellView) stringCellView));

			cellView = new CheckBoxCellView { EditableField = editableField, ActiveField = somewhatEditableData };
			cellView.EditingFinished += CellView_EditingFinished;
			list.Columns.Add(new ListViewColumn("Somewhat Editable",(CellView) cellView));

			stringCellView = new TextCellView { EditableField = editableField, TextField = textField2 };
			stringCellView.EditingFinished += StringCellView_EditingFinished;

			list.Columns.Add (new ListViewColumn("Somewhat Editable", (CellView)stringCellView));

			Random rand = new Random ();
			
			for (int n=0; n<100; n++) {
				var r = store.AddRow ();
				store.SetValue (r, editableActiveField, rand.Next(0, 2) == 0);
				store.SetValue (r, nonEditableActiveField, rand.Next(0, 2) == 0);
				store.SetValue(r, somewhatEditableData, rand.Next(0, 2) == 0);
				store.SetValue (r, textField, n.ToString ());
				var edit = (n % 2) == 0;
				store.SetValue (r, editableField, edit);
				store.SetValue (r, textField2, edit ? "editable" : "not editable");
			}
			PackStart (list, true);
		}

		void CellView_EditingFinished(object sender, Xwt.Backends.CellEditingFinishedArgs<Xwt.CheckBoxState> e)
		{
			Console.WriteLine("Your old value was '{0}' and now is '{1}'", e.OldValue, e.NewValue);
		}

		void StringCellView_EditingFinished(object sender, Xwt.Backends.CellEditingFinishedArgs<string> e)
		{
			Console.WriteLine("Your old value was '{0}' and now is '{1}'", e.OldValue, e.NewValue);
		}
	}
}

