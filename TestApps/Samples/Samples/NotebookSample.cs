using System;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class NotebookSample: VBox
	{
		public NotebookSample ()
		{
			Notebook mainNB = new Notebook ();
			mainNB.TabOrientation = NotebookTabOrientation.Top;
			mainNB.ExpandTabLabels = true;

			Notebook nb1 = new Notebook ();
			nb1.Add (new Label ("First tab content"), "First Tab", StockIcons.Information);
			nb1.Add (new MyWidget (), "Second Tab");
			nb1.Add (new MyTestWidget(), "Third Tab", StockIcons.Warning);
			nb1.TabOrientation = NotebookTabOrientation.Left;


			Notebook nb2 = new Notebook ();
			for (int i = 1; i <= 5; i++) {
				var box = new VBox ();
				box.PackStart (new Label ("tab " + i + " content"), false);
				for (int j = 1; j <= i; j++)
					box.PackStart (new Button ("TEST " + i + " " + j), false, true);
				box.PackStart (new MyWidget (), true);
				nb2.Add (box, "Tab " + i);
			}
			nb2.TabOrientation = NotebookTabOrientation.Bottom;

			mainNB.Add (nb1, "First Tab", StockIcons.Information);
			mainNB.Add (nb2, "Second Tab", StockIcons.Warning);

			var box2 = new VBox ();
			box2.PackStart (new Label ("Third Tab content"), false);
			box2.PackStart (new Button ("TEST"), false, true);
			box2.PackStart (new Button ("TEST"), false, true);
			mainNB.Add (box2, "Third Tab", StockIcons.Information);


			var buttons = new HBox ();
			var btnToggleExpand = new ToggleButton ("Expand Tabs");
			btnToggleExpand.Toggled += (sender, e) => {
				if (btnToggleExpand.Active) {
					mainNB.ExpandTabLabels = true;
					nb1.ExpandTabLabels = true;
					nb2.ExpandTabLabels = true;
				} else {
					mainNB.ExpandTabLabels = false;
					nb1.ExpandTabLabels = false;
					nb2.ExpandTabLabels = false;
				}
			};


			var btnAddTab = new Button ("Add Tab");
			btnAddTab.Clicked += (sender, e) => {
				Notebook nb = new Notebook ();
				nb.Add (new Label ("Tab " + (mainNB.Tabs.Count + 1) + " content"), "Tab " + (mainNB.Tabs.Count + 1) + " 1", StockIcons.Information);
				nb.Add (new MyWidget (), "Tab " + (mainNB.Tabs.Count + 1) + " 2");
				nb.TabOrientation = NotebookTabOrientation.Right;
				nb.ExpandTabLabels = true;
				mainNB.Add (nb, "Another Tab " + (mainNB.Tabs.Count + 1));
			};

			var btnRemoveTab = new Button ("Remove Tab");
			btnRemoveTab.Clicked += (sender, e) => {
				if (mainNB.Tabs.IndexOf (mainNB.CurrentTab) > 2)
					mainNB.Tabs.Remove (mainNB.CurrentTab);
			};
			buttons.PackStart (btnToggleExpand);
			buttons.PackStart (btnAddTab);
			buttons.PackStart (btnRemoveTab);

			PackStart (mainNB, true);
			PackEnd (buttons);
		}
	}

	class MyTestWidget : VBox
	{
		public MyTestWidget()
		{
			PackStart(new TextEntry() { PlaceholderText = "Placeholder Test" });
			PackStart(new Label("Scrollable Test:"));

			VBox ContentData = new VBox()
			{
				ExpandHorizontal = true,
				ExpandVertical = true
			};

			ScrollView ContentScroll = new ScrollView()
			{
				Content = ContentData,
				ExpandHorizontal = true,
				ExpandVertical = true
			};
			PackStart(ContentScroll, true, true);

			ContentData.PackStart(new TextEntry() { PlaceholderText = "Placeholder Test" }, true, true);
			ContentData.PackStart(new TextEntry(), true, true);
			ContentData.PackStart(new TextEntry() { PlaceholderText = "Placeholder Test" }, true, true);
			ContentData.PackStart(new TextEntry(), true, true);
			ContentData.PackStart(new TextEntry() { PlaceholderText = "Placeholder Test" }, true, true);
			ContentData.PackStart(new MyWidget (), true, true);
			ContentData.PackStart(new TextEntry() { PlaceholderText = "Placeholder Test" }, true, true);
			ContentData.PackStart(new TextEntry(), true, true);
			ContentData.PackStart(new TextEntry() { PlaceholderText = "Placeholder Test" }, true, true);
			ContentData.PackStart(new TextEntry(), true, true);
			ContentData.PackStart(new TextEntry() { PlaceholderText = "Placeholder Test" }, true, true);
			ContentData.PackStart(new TextEntry(), true, true);
		}
	}

	class MyWidget: Canvas
	{
		public MyWidget()
		{
			MinWidth = 210;
			MinHeight = 110;
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			ctx.SetLineWidth (5);
			ctx.SetColor (new Color (1.0f, 0f, 0.5f));
			ctx.Rectangle (5, 5, 200, 100);
			ctx.FillPreserve ();
			ctx.SetColor (new Color (0f, 0f, 1f));
			ctx.Stroke ();
		}
	}
}

