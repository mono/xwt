using System;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class ListView2: VBox
	{
		DataField<bool> editable;
		DataField<bool> nonEditable;

		public ListView2 ()
		{
			ListView list = new ListView ();
			editable = new DataField<bool> ();
			nonEditable = new DataField<bool> ();
			ListStore store = new ListStore (editable, nonEditable);
			list.DataSource = store;

			list.Columns.Add (new ListViewColumn("editable", new CheckBoxCellView { Editable = true, ActiveField = editable }));
			list.Columns.Add (new ListViewColumn("nonEditable", new CheckBoxCellView { Editable = false, ActiveField = nonEditable }));

			Random rand = new Random ();
			
			for (int n=0; n<100; n++) {
				var r = store.AddRow ();
				store.SetValue (r, editable, rand.Next(0, 2) == 0);
				store.SetValue (r, nonEditable, rand.Next(0, 2) == 0);
			}
			PackStart (list, true);
		}
	}
}

