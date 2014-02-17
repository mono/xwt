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
		DataField<int> progress = new DataField<int> ();

		public ListView1 ()
		{
			PackStart (new Label ("The listview should have a red background"));
			ListView list = new ListView ();
			ListStore store = new ListStore (name, icon, text, icon2, progress);
			list.DataSource = store;
			list.Columns.Add ("Name", icon, name);
			list.Columns.Add ("Text", icon2, text);
			list.Columns.Add ("Progress", new CustomCell () { ValueField = progress });

			var png = Image.FromResource (typeof(App), "class.png");

			Random rand = new Random ();
			
			for (int n=0; n<100; n++) {
				var r = store.AddRow ();
				store.SetValue (r, icon, png);
				store.SetValue (r, name, "Value " + n);
				store.SetValue (r, icon2, png);
				store.SetValue (r, text, "Text " + n);
				store.SetValue (r, progress, rand.Next () % 100);
			}
			PackStart (list, true);

			list.RowActivated += delegate(object sender, ListViewRowEventArgs e) {
				MessageDialog.ShowMessage ("Row " + e.RowIndex + " activated");
			};
		}
	}

	class CustomCell: CanvasCellView
	{
		public Binding ValueField { get; set; }

		protected override Size OnGetRequiredSize ()
		{
			return new Size (200, 10);
		}

		protected override void OnDraw (Context ctx, Rectangle cellArea)
		{
			var pct = GetValue<int> (ValueField);
			var size = (cellArea.Width * pct) / 100f;
			cellArea.Width = (int) size;

			ctx.SetLineWidth (1);
			ctx.Rectangle (cellArea.Inflate (-0.5, -0.5));
			ctx.SetColor (Colors.LightBlue);
			ctx.FillPreserve ();
			ctx.SetColor (Colors.Gray);
			ctx.Stroke ();
		}
	}
}

