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

			var checkCellView = new CheckBoxCellView { Editable = true, ActiveField = editableActiveField };
			checkCellView.Toggled += (sender, e) => {

				if (list.CurrentEventRow == null) {
					MessageDialog.ShowError("CurrentEventRow is null. This is not supposed to happen");
				}
				else {
					store.SetValue(list.CurrentEventRow, textField, "Toggled");
				}
			};

			list.Columns.Add (new ListViewColumn("Editable", checkCellView));
			list.Columns.Add (new ListViewColumn("Not Editable", new CheckBoxCellView { Editable = false, ActiveField = nonEditableActiveField }));
			list.Columns.Add (new ListViewColumn("Editable", new TextCellView { Editable = true, TextField = textField }));
			list.Columns.Add(new ListViewColumn("Somewhat Editable", new CheckBoxCellView { EditableField = editableField, ActiveField = somewhatEditableData }));
			list.Columns.Add (new ListViewColumn("Somewhat Editable", new TextCellView { EditableField = editableField, TextField = textField2 }));

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
	}
}

