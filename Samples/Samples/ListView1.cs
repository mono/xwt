using System;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class ListView1: VBox
	{
		DataField<string> name = new DataField<string> ();
		DataField<Image> icon = new DataField<Image> ();
		DataField<string> text = new DataField<string> ();
		DataField<Image> icon2 = new DataField<Image> ();
		
		public ListView1 ()
		{
			PackStart (new Label ("The listview should have a red background"));
			ListView list = new ListView () {
				BackgroundColor = Colors.Red
			};
			ListStore store = new ListStore (name, icon, text, icon2);
			list.DataSource = store;
			list.Columns.Add ("Name", icon, name);
			list.Columns.Add ("Text", icon2, text);
			
			var png = Image.FromResource (typeof(App), "class.png");
			
			for (int n=0; n<100; n++) {
				var r = store.AddRow ();
				store.SetValue (r, icon, png);
				store.SetValue (r, name, "Value " + n);
				store.SetValue (r, icon2, png);
				store.SetValue (r, text, "Text " + n);
			}
			PackStart (list, BoxMode.FillAndExpand);

			list.RowActivated += delegate(object sender, ListViewRowEventArgs e) {
				MessageDialog.ShowMessage ("Row " + e.RowIndex + " activated");
			};
		}
	}
}

