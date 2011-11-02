using System;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class MainWindow: Window
	{
		TreeView samplesTree;
		TreeStore store;
		Image icon;
		VBox sampleBox;
		Label title;
		Widget currentSample;
		
		DataField<string> nameCol = new DataField<string> ();
		DataField<Widget> widgetCol = new DataField<Widget> ();
		DataField<Image> iconCol = new DataField<Image> ();
		
		public MainWindow ()
		{
			Menu menu = new Menu ();
			
			var file = new MenuItem ("File");
			file.SubMenu = new Menu ();
			file.SubMenu.Items.Add (new MenuItem ("Open"));
			file.SubMenu.Items.Add (new MenuItem ("New"));
			file.SubMenu.Items.Add (new MenuItem ("Close"));
			menu.Items.Add (file);
			
			var edit = new MenuItem ("Edit");
			edit.SubMenu = new Menu ();
			edit.SubMenu.Items.Add (new MenuItem ("Copy"));
			edit.SubMenu.Items.Add (new MenuItem ("Cut"));
			edit.SubMenu.Items.Add (new MenuItem ("Paste"));
			menu.Items.Add (edit);
			
			MainMenu = menu;
			
			
			HBox box = new HBox ();
			
			icon = Image.FromResource (typeof(App), "class.png");
			
			store = new TreeStore (nameCol, iconCol, widgetCol);
			samplesTree = new TreeView ();
			samplesTree.Columns.Add ("Name", iconCol, nameCol);
			
			var n = AddSample (null, "Drawing", null);
			AddSample (n, "Chart", new ChartSample ());
			AddSample (null, "Notebook", new NotebookSample ());
			AddSample (null, "Boxes", new Boxes ());
			AddSample (null, "List View", new ListView1 ());
			AddSample (null, "Drag & Drop", new DragDrop ());
			
			samplesTree.DataSource = store;
			
			box.PackStart (samplesTree);
			
			sampleBox = new VBox ();
			title = new Label ("Sample:");
			sampleBox.PackStart (title, BoxMode.None);
			
			box.PackStart (sampleBox, BoxMode.FillAndExpand);
			
			Add (box);
			
			samplesTree.SelectionChanged += HandleSamplesTreeSelectionChanged;
		}

		void HandleSamplesTreeSelectionChanged (object sender, EventArgs e)
		{
			if (samplesTree.SelectedItem != null) {
				if (currentSample != null)
					sampleBox.Remove (currentSample);
				Widget w = store.GetNavigatorAt (samplesTree.SelectedItem).GetValue (widgetCol);
				if (w != null)
					sampleBox.PackStart (w, BoxMode.FillAndExpand);
				string txt = System.Xaml.XamlServices.Save (w);
				currentSample = w;
			}
		}
		
		TreePosition AddSample (TreePosition pos, string name, Widget page)
		{
			//if (page != null)
			//	page.Margin.SetAll (5);
			return store.AddNode (pos).SetValue (nameCol, name).SetValue (iconCol, icon).SetValue (widgetCol, page).CurrentPosition;
		}
	}
}

