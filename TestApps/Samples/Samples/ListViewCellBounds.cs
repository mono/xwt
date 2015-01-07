//
// ListViewCellBounds.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2015 Vsevolod Kukol
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class ListViewCellBounds: Canvas
	{
		ListTrackingCanvas tracker;
		ListTrackingCanvas drawer;
		Rectangle drawerBounds;
		VBox container;

		Image drawerBg;
		public Image DrawerBg {
			get {
				return drawerBg;
			}
			set {
				drawerBg = value;
				drawer.TrackerBg = drawerBg;
				if (tracker != null && !Toolkit.CurrentEngine.SupportedFeatures.HasFlag (ToolkitFeatures.WidgetOpacity))
					tracker.TrackerBg = drawerBg;
			}
		}

		int currentRow;
		public int CurrentRow {
			get {
				return currentRow;
			}
			private set {
				if (currentRow != value) {
					try {
						DrawerBg = Toolkit.CurrentEngine.RenderWidget (ListView);
					} catch {}
					currentRow = value;
					QueueForReallocate ();
				}
			}
		}

		public ListView ListView { get; private set; }
		public ListStore ListStore { get; private set; }

		DataField<string> name = new DataField<string> ();
		DataField<Image> icon = new DataField<Image> ();
		DataField<bool> check = new DataField<bool> ();
		DataField<string> text = new DataField<string> ();
		DataField<CellData> progress = new DataField<CellData> ();

		public ListViewCellBounds ()
		{
			MinHeight = 120;
			MinWidth = 100;

			container = new VBox ();
			ListView = new ListView();

			ListView.GridLinesVisible = GridLines.Both;
			ListStore = new ListStore (name, icon, text, check, progress);
			ListView.DataSource = ListStore;
			ListView.Columns.Add ("Name", icon, name);
			ListView.Columns.Add ("Check", new TextCellView () { TextField = text }, new CustomCell () { ValueField = progress, Size = new Size(20, 20) }, new CheckBoxCellView() { ActiveField = check });
			//list.Columns.Add ("Progress", new TextCellView () { TextField = text }, new CustomCell () { ValueField = progress }, new TextCellView () { TextField = text } );
			ListView.Columns.Add ("Progress", new CustomCell () { ValueField = progress });

			var png = Image.FromResource (typeof(App), "class.png");

			Random rand = new Random ();

			for (int n=0; n<100; n++) {
				var r = ListStore.AddRow ();
				ListStore.SetValue (r, icon, png);
				if (rand.Next (50) > 25) {
					ListStore.SetValue (r, name, "Value \n" + n);
					ListStore.SetValue (r, check, false);
				} else {
					ListStore.SetValue (r, name, "Value " + n);
					ListStore.SetValue (r, check, true);
				}
				ListStore.SetValue (r, text, "Text " + n);
				ListStore.SetValue (r, progress, new CellData { Value = rand.Next () % 100 });
			}

			ListView.SelectionChanged += (sender, e) => UpdateTracker (ListView.SelectedRow);
			ListView.MouseMoved += (sender, e) => UpdateTracker (ListView.GetRowAtPosition (e.X, e.Y));

			drawer = new ListTrackingCanvas (this);

			container.PackStart (ListView, true);
			container.PackStart (drawer);
			AddChild (container);
		}

		void InitTracker()
		{
			if (tracker == null) {
				tracker = new ListTrackingCanvas (this);
				AddChild (tracker);
				QueueForReallocate ();
			}
		}

		void UpdateTracker(int row)
		{
			if (row < 0)
				return;
			if (Toolkit.CurrentEngine.SupportedFeatures.HasFlag (ToolkitFeatures.WidgetOpacity))
				InitTracker();
			CurrentRow = row;
		}

		protected override void OnReallocate ()
		{
			var rowBounds = ListView.GetRowBounds (CurrentRow, true);
			SetChildBounds (container, Bounds);
			container.Remove (drawer);
			container.PackStart (drawer);
			if (tracker != null) {
				SetChildBounds (tracker, rowBounds);
				tracker.QueueDraw ();
			}
		}
	}

	class ListTrackingCanvas : Canvas
	{
		readonly ListViewCellBounds parent;

		public Image TrackerBg { get; set; }

		public ListTrackingCanvas(ListViewCellBounds parent)
		{
			this.parent = parent;
		}

		protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			var row_bg_bounds = parent.ListView.GetRowBounds (parent.CurrentRow, true);
			return new Size (row_bg_bounds.Width, row_bg_bounds.Height);
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			ctx.Save ();

			if (parent.CurrentRow < 0)
				return;

			var row_bounds = parent.ListView.GetRowBounds (parent.CurrentRow, false);
			var row_bg_bounds = parent.ListView.GetRowBounds (parent.CurrentRow, true);

			if (TrackerBg != null) {
				ctx.DrawImage (TrackerBg, row_bg_bounds, new Rectangle (0, 0, row_bg_bounds.Width, row_bg_bounds.Height));
			}

			foreach (var col in parent.ListView.Columns) {
				foreach (var cell in col.Views) {
					var cell_bg_bounds = parent.ListView.GetCellBounds (parent.CurrentRow, cell, true);
					var cell_bounds = parent.ListView.GetCellBounds (parent.CurrentRow, cell, false);
					cell_bounds.Y -= row_bg_bounds.Y;
					cell_bounds.X += parent.ListView.HorizontalScrollControl.Value;
					cell_bg_bounds.Y -= row_bg_bounds.Y;
					cell_bg_bounds.X += parent.ListView.HorizontalScrollControl.Value;
					ctx.SetColor (Colors.Red);
					ctx.Rectangle (cell_bg_bounds);
					ctx.Stroke ();
					ctx.SetColor (Colors.Green);
					ctx.Rectangle (cell_bounds);
					ctx.Stroke ();
				}
			}

			row_bounds.Y -= row_bg_bounds.Y;
			row_bounds.X += parent.ListView.HorizontalScrollControl.Value;
			row_bg_bounds.Y = 0;
			row_bg_bounds.X += parent.ListView.HorizontalScrollControl.Value;

			ctx.SetColor (Colors.Red);
			ctx.Rectangle (row_bg_bounds);
			ctx.Stroke ();

			ctx.SetColor (Colors.Blue);
			ctx.Rectangle (row_bounds);
			ctx.Stroke ();
			ctx.Restore ();
		}
	}
}

