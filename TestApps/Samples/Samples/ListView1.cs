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
		DataField<CellData> progress = new DataField<CellData> ();

		public ListView1 ()
		{
			PackStart (new Label ("The listview should have a red background"));
			ListView list = new ListView ();
			list.BackgroundColor = Colors.Red;
			list.GridLinesVisible = GridLines.Both;
			ListStore store = new ListStore (name, icon, text, icon2, progress);
			list.DataSource = store;

			var col = new ListViewColumn ("Item");
			col.Views.Add (new TextCellView (text), true); // expand the first CellView
			col.Views.Add (new ImageCellView (icon2));
			col.CanResize = true;

			list.Columns.Add ("Name", icon, name);
			list.Columns.Add (col);
			list.Columns.Add ("Progress", new TextCellView () { TextField = text }, new CustomCell () { ValueField = progress });

			var png = Image.FromResource (typeof(App), "class.png");

			Random rand = new Random ();
			
			for (int n=0; n<100; n++) {
				var r = store.AddRow ();
				store.SetValue (r, icon, png);
				store.SetValue (r, name, "Value " + n);
				store.SetValue (r, icon2, png);
				store.SetValue (r, text, "Text " + n);
				store.SetValue (r, progress, new CellData { Value = rand.Next () % 100 });
			}
			PackStart (list, true);

			list.RowActivated += delegate(object sender, ListViewRowEventArgs e) {
				MessageDialog.ShowMessage ("Row " + e.RowIndex + " activated");
			};

			Menu contextMenu = new Menu ();
			contextMenu.Items.Add (new MenuItem ("Test menu"));
			list.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				int row = list.GetRowAtPosition(new Point(e.X, e.Y));
				if (e.Button == PointerButton.Right && row >= 0) {
					// Set actual row to selected
					list.SelectRow(row);
					contextMenu.Popup(list, e.X, e.Y);
				}
			};

			list.KeyPressed += (sender, e) => {
				if (e.Key == Key.Insert) {
					var r = store.InsertRowAfter(list.SelectedRow < 0 ? 0 : list.SelectedRow);
					store.SetValue (r, icon, png);
					store.SetValue (r, name, "Value " + (store.RowCount + 1));
					store.SetValue (r, icon2, png);
					store.SetValue (r, text, "New Text " + (store.RowCount + 1));
					store.SetValue (r, progress, new CellData { Value = rand.Next () % 100 });
					list.ScrollToRow (r);
					list.SelectRow (r);
					list.FocusedRow = r;
				}
			};

			HBox btnBox = new HBox ();
			Button btnAddItem = new Button ("Add item");
			btnAddItem.Clicked += delegate {
				var r = store.InsertRowAfter(list.SelectedRow < 0 ? 0 : list.SelectedRow);
				store.SetValue (r, icon, png);
				store.SetValue (r, name, "Value " + (store.RowCount + 1));
				store.SetValue (r, icon2, png);
				store.SetValue (r, text, "New Text " + (store.RowCount + 1));
				store.SetValue (r, progress, new CellData { Value = rand.Next () % 100 });
				list.ScrollToRow (r);
				list.SelectRow (r);
			};
			btnBox.PackStart (btnAddItem, true);
			Button btnRemoveItem = new Button ("Remove item");
			btnRemoveItem.Clicked += delegate {
				if (list.SelectedRow >= 0)
					store.RemoveRow(list.SelectedRow);
			};
			btnBox.PackStart (btnRemoveItem, true);
			PackStart (btnBox);

			var but = new Button ("Scroll one line");
			but.Clicked += delegate {
				list.VerticalScrollControl.Value += list.VerticalScrollControl.StepIncrement;
			};
			PackStart (but);

			var spnValue = new SpinButton ();
			spnValue.MinimumValue = 0;
			spnValue.MaximumValue = 99;
			spnValue.IncrementValue = 1;
			spnValue.Digits = 0;
			var btnScroll = new Button ("Go!");
			btnScroll.Clicked += (sender, e) => list.ScrollToRow((int)spnValue.Value);

			HBox scrollActBox = new HBox ();
			scrollActBox.PackStart (new Label("Scroll to Value: "));
			scrollActBox.PackStart (spnValue);
			scrollActBox.PackStart (btnScroll);
			PackStart (scrollActBox);
		}
	}

	class CellData
	{
		public int Value;
		public double YPos = -1;
	}

	class CustomCell: CanvasCellView
	{
		public IDataField<CellData> ValueField { get; set; }

		public Size Size {
			get;
			set;
		}

		public CustomCell ()
		{
			Size = new Size (200, 10);
		}

		protected override Size OnGetRequiredSize ()
		{
			return Size;
		}

		protected override void OnDraw (Context ctx, Rectangle cellArea)
		{
			ctx.Rectangle (BackgroundBounds);
			ctx.SetColor (new Color (0.9, 0.9, 0.9));
			ctx.Fill ();

			ctx.Rectangle (Bounds);
			ctx.SetColor (new Color (0.7, 0.7, 0.7));
			ctx.Fill ();

			var pct = GetValue (ValueField);
			var size = (cellArea.Width * pct.Value) / 100f;
			cellArea.Width = (int) size;

			ctx.SetLineWidth (1);
			ctx.Rectangle (cellArea.Inflate (-0.5, -0.5));
			ctx.SetColor (Selected ? Colors.Blue : Colors.LightBlue);
			ctx.FillPreserve ();
			ctx.SetColor (Colors.Gray);
			ctx.Stroke ();

			if (pct.YPos != -1) {
				ctx.MoveTo (cellArea.Right, Bounds.Y + pct.YPos);
				ctx.Arc (cellArea.Right, Bounds.Y + pct.YPos, 2.5, 0, 360);
				ctx.SetColor (Colors.Red);
				ctx.Fill ();
			}
		}

		protected override void OnMouseMoved (MouseMovedEventArgs args)
		{
			if (Bounds.Contains (args.Position)) {
				var data = GetValue (ValueField);
				data.Value = Math.Min (100, (int)(100 * ((args.X - Bounds.X) / Bounds.Width)));
				data.YPos = args.Y - Bounds.Y;
				QueueDraw ();
			}
			base.OnMouseMoved (args);
		}

		protected override void OnButtonPressed (ButtonEventArgs args)
		{
			Console.WriteLine ("Press: " + args.Position);
			base.OnButtonPressed (args);
		}

		protected override void OnButtonReleased (ButtonEventArgs args)
		{
			Console.WriteLine ("Release: " + args.Position);
			base.OnButtonReleased (args);
		}
	}
}

