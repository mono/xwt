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
		DataField<Sample> widgetCol = new DataField<Sample> ();
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
			
			AddSample (null, "Boxes", typeof(Boxes));
			AddSample (null, "Buttons", typeof(ButtonSample));
			AddSample (null, "CheckBox", typeof(Checkboxes));
			AddSample (null, "Clipboard", typeof(ClipboardSample));
			AddSample (null, "ComboBox", typeof(ComboBoxes));
//			AddSample (null, "Designer", typeof(Designer));
			AddSample (null, "Drag & Drop", typeof(DragDrop));
			
			var n = AddSample (null, "Drawing", null);
			AddSample (n, "Canvas with Widget", typeof(CanvasWithWidget));
			AddSample (n, "Chart", typeof(ChartSample));
			AddSample (n, "Colors", typeof(Colors));
			AddSample (n, "Transformations", typeof(DrawingTransforms));
			
			AddSample (null, "Frames", typeof(Frames));
			AddSample (null, "Images", typeof(Images));
			AddSample (null, "Labels", typeof(Labels));
			AddSample (null, "List View", typeof(ListView1));
			AddSample (null, "Menu", typeof(MenuSamples));
			AddSample (null, "Notebook", typeof(NotebookSample));
			AddSample (null, "Scroll View", typeof(ScrollWindowSample));
			AddSample (null, "Tables", typeof(Tables));
			AddSample (null, "Text Entry", typeof(TextEntries));
			AddSample (null, "Tooltips", typeof(Tooltips));
			AddSample (null, "WidgetEvents", typeof(WidgetEvents));
			AddSample (null, "Windows", typeof(Windows));
			
			samplesTree.DataSource = store;
			
			box.PackStart (samplesTree);
			
			sampleBox = new VBox ();
			title = new Label ("Sample:");
			sampleBox.PackStart (title, BoxMode.None);
			
			box.PackStart (sampleBox, BoxMode.FillAndExpand);
			
			Content = box;
			
			samplesTree.SelectionChanged += HandleSamplesTreeSelectionChanged;
		}

		void HandleSamplesTreeSelectionChanged (object sender, EventArgs e)
		{
			if (samplesTree.SelectedRow != null) {
				if (currentSample != null)
					sampleBox.Remove (currentSample);
				Sample s = store.GetNavigatorAt (samplesTree.SelectedRow).GetValue (widgetCol);
				if (s.Type != null) {
					if (s.Widget == null)
						s.Widget = (Widget)Activator.CreateInstance (s.Type);
					sampleBox.PackStart (s.Widget, BoxMode.FillAndExpand);
				}
				
//				string txt = System.Xaml.XamlServices.Save (s);
				currentSample = s.Widget;
				Dump (currentSample, 0);
			}
		}
		
		void Dump (IWidgetSurface w, int ind)
		{
			if (w == null)
				return;
			Console.WriteLine (new string (' ', ind * 2) + " " + w.GetType ().Name + " " + w.GetPreferredWidth () + " " + w.GetPreferredHeight ());
			foreach (var c in w.Children)
				Dump (c, ind + 1);
		}
		
		TreePosition AddSample (TreePosition pos, string name, Type sampleType)
		{
			//if (page != null)
			//	page.Margin.SetAll (5);
			return store.AddNode (pos).SetValue (nameCol, name).SetValue (iconCol, icon).SetValue (widgetCol, new Sample (sampleType)).CurrentPosition;
		}
	}
	
	class Sample
	{
		public Sample (Type type)
		{
			Type = type;
		}

		public Type Type;
		public Widget Widget;
	}
}

