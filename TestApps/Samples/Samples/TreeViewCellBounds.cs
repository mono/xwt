//
// TreeViewCellBounds.cs
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
	public class TreeViewCellBounds: Canvas
	{
		TreeTrackingCanvas tracker;
		TreeTrackingCanvas drawer;
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

		TreeNavigator currentRow;
		public TreeNavigator CurrentRow {
			get {
				return currentRow;
			}
			private set {
				if (currentRow != value) {
					try {
						DrawerBg = Toolkit.CurrentEngine.RenderWidget (TreeView);
					} catch {
					}
					currentRow = value;
					QueueForReallocate ();
				}
			}
		}
		public TreeView TreeView { get; private set; }
		public TreeStore TreeStore { get; private set; }

		DataField<CheckBoxState> triState = new DataField<CheckBoxState>();
		DataField<bool> check = new DataField<bool>();
		DataField<string> text = new DataField<string> ();
		DataField<string> desc = new DataField<string> ();

		public TreeViewCellBounds ()
		{
			MinHeight = 120;
			MinWidth = 100;

			container = new VBox ();
			TreeView = new TreeView ();
			TreeStore = new TreeStore (triState, check, text, desc);
			TreeView.GridLinesVisible = GridLines.Both;

			TreeView.Columns.Add ("TriCheck", triState);
			TreeView.Columns.Add ("Check", check);
			TreeView.Columns.Add ("Item", text);
			TreeView.Columns.Add ("Desc", desc, check, text);

			TreeView.DataSource = TreeStore;

			TreeStore.AddNode ().SetValue (text, "One").SetValue (desc, "First").SetValue (triState, CheckBoxState.Mixed);
			TreeStore.AddNode ().SetValue (text, "Two").SetValue (desc, "Second").AddChild ()
				.SetValue (text, "Sub two").SetValue (desc, "Sub second");
			TreeStore.AddNode ().SetValue (text, "Three").SetValue (desc, "Third").AddChild ()
				.SetValue (text, "Sub three").SetValue (desc, "Sub third");

			TreeView.ExpandAll ();


			TreeView.SelectionChanged += (sender, e) => UpdateTracker (TreeView.SelectedRow);
			TreeView.MouseMoved += (sender, e) => UpdateTracker (TreeView.GetRowAtPosition (e.X, e.Y));

			drawer = new TreeTrackingCanvas (this);

			container.PackStart (TreeView, true);
			container.PackStart (drawer);
			AddChild (container);

			if (currentRow == null)
				currentRow = TreeStore.GetFirstNode ();
		}

		void InitTracker()
		{
			if (tracker == null) {
				tracker = new TreeTrackingCanvas (this);
				AddChild (tracker);
				QueueForReallocate ();
			}
		}

		void UpdateTracker(TreePosition row)
		{
			if (row == null)
				return;
			if (Toolkit.CurrentEngine.SupportedFeatures.HasFlag (ToolkitFeatures.WidgetOpacity))
				InitTracker();
			CurrentRow = TreeStore.GetNavigatorAt (row);
		}

		protected override void OnReallocate ()
		{
			if (CurrentRow == null)
				CurrentRow = TreeStore.GetFirstNode ();

			var rowBounds = TreeView.GetRowBounds (CurrentRow.CurrentPosition, true);

			SetChildBounds (container, Bounds);
			container.Remove (drawer);
			container.PackStart (drawer);
			container.QueueForReallocate ();
			if (tracker != null) {
				SetChildBounds (tracker, rowBounds);
				tracker.QueueDraw ();
			}
		}
	}

	class TreeTrackingCanvas : Canvas
	{
		readonly TreeViewCellBounds parent;

		public Image TrackerBg { get; set; }

		public TreeTrackingCanvas(TreeViewCellBounds parent)
		{
			this.parent = parent;
		}

		protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			if (parent.CurrentRow == null)
				return new Size (20, 100);
			var row_bg_bounds = parent.TreeView.GetRowBounds (parent.CurrentRow.CurrentPosition, true);
			if (row_bg_bounds == Rectangle.Zero)
				return new Size (20, 100);
			return new Size (row_bg_bounds.Width, row_bg_bounds.Height);
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw (ctx, dirtyRect);
			ctx.Save ();
			if (parent.CurrentRow == null)
				return;

			var row_bounds = parent.TreeView.GetRowBounds (parent.CurrentRow.CurrentPosition, false);
			var row_bg_bounds = parent.TreeView.GetRowBounds (parent.CurrentRow.CurrentPosition, true);

			if (TrackerBg != null)
				ctx.DrawImage (TrackerBg, row_bg_bounds, new Rectangle (0, 0, row_bg_bounds.Width, row_bg_bounds.Height));

			foreach (var col in parent.TreeView.Columns) {
				foreach (var cell in col.Views) {
					var cell_bg_bounds = parent.TreeView.GetCellBounds (parent.CurrentRow.CurrentPosition, cell, true);
					var cell_bounds = parent.TreeView.GetCellBounds (parent.CurrentRow.CurrentPosition, cell, false);
					cell_bounds.Y -= row_bg_bounds.Y;
					cell_bounds.X += parent.TreeView.HorizontalScrollControl.Value;
					cell_bg_bounds.Y -= row_bg_bounds.Y;
					cell_bg_bounds.X += parent.TreeView.HorizontalScrollControl.Value;
					ctx.SetColor (Colors.Red);
					ctx.Rectangle (cell_bg_bounds);
					ctx.Stroke ();
					ctx.SetColor (Colors.Green);
					ctx.Rectangle (cell_bounds);
					ctx.Stroke ();
				}
			}

			row_bounds.Y -= row_bg_bounds.Y;
			row_bounds.X += parent.TreeView.HorizontalScrollControl.Value;
			row_bg_bounds.Y = 0;
			row_bg_bounds.X += parent.TreeView.HorizontalScrollControl.Value;

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

