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
		
		StatusIcon statusIcon;
		
		public MainWindow ()
		{
			Title = "Xwt Demo Application";
			Width = 500;
			Height = 400;

			try {
				statusIcon = Application.CreateStatusIcon ();
				statusIcon.Menu = new Menu ();
				statusIcon.Menu.Items.Add (new MenuItem ("Test"));
				statusIcon.Image = Image.FromResource (GetType (), "package.png");
			} catch {
				Console.WriteLine ("Status icon could not be shown");
			}
			
			Menu menu = new Menu ();
			
			var file = new MenuItem ("File");
			file.SubMenu = new Menu ();
			file.SubMenu.Items.Add (new MenuItem ("Open"));
			file.SubMenu.Items.Add (new MenuItem ("New"));
			MenuItem mi = new MenuItem ("Close");
			mi.Clicked += delegate {
				Application.Exit();
			};
			file.SubMenu.Items.Add (mi);
			menu.Items.Add (file);
			
			var edit = new MenuItem ("Edit");
			edit.SubMenu = new Menu ();
			edit.SubMenu.Items.Add (new MenuItem ("Copy"));
			edit.SubMenu.Items.Add (new MenuItem ("Cut"));
			edit.SubMenu.Items.Add (new MenuItem ("Paste"));
			menu.Items.Add (edit);
			
			MainMenu = menu;
			
			
			HPaned box = new HPaned ();
			
			icon = Image.FromResource (typeof(App), "class.png");
			
			store = new TreeStore (nameCol, iconCol, widgetCol);
			samplesTree = new TreeView ();
			samplesTree.Columns.Add ("Name", iconCol, nameCol);
			
			AddSample (null, "Boxes", typeof(Boxes));
			AddSample (null, "Buttons", typeof(ButtonSample));
			AddSample (null, "CheckBox", typeof(Checkboxes));
			AddSample (null, "Clipboard", typeof(ClipboardSample));
			AddSample (null, "ColorSelector", typeof(ColorSelectorSample));
			AddSample (null, "ComboBox", typeof(ComboBoxes));
//			AddSample (null, "Designer", typeof(Designer));
			AddSample (null, "Drag & Drop", typeof(DragDrop));
			
			var n = AddSample (null, "Drawing", null);
			AddSample (n, "Canvas with Widget (Linear)", typeof (CanvasWithWidget_Linear));
			AddSample (n, "Canvas with Widget (Radial)", typeof (CanvasWithWidget_Radial));
			AddSample (n, "Chart", typeof (ChartSample));
			AddSample (n, "Colors", typeof(ColorsSample));
			AddSample (n, "Figures", typeof(DrawingFigures));
			AddSample (n, "Transformations", typeof(DrawingTransforms));
			AddSample (n, "Images and Patterns", typeof(DrawingPatternsAndImages));
			AddSample (n, "Text", typeof(DrawingText));
			AddSample (n, "Partial Images", typeof (PartialImages));
			AddSample (n, "Custom Drawn Image", typeof (ImageScaling));

			AddSample (null, "Expander", typeof (ExpanderSample));
			AddSample (null, "Progress bars", typeof(ProgressBarSample));
			AddSample (null, "Frames", typeof(Frames));
			AddSample (null, "Images", typeof(Images));
			AddSample (null, "Labels", typeof(Labels));
			AddSample (null, "ListBox", typeof(ListBoxSample));
			AddSample (null, "LinkLabels", typeof(LinkLabels));
			AddSample (null, "ListView", typeof(ListView1));
			AddSample (null, "Markdown", typeof (MarkDownSample));
			AddSample (null, "Menu", typeof(MenuSamples));
			AddSample (null, "Notebook", typeof(NotebookSample));
			AddSample (null, "Paneds", typeof(PanedViews));
			AddSample (null, "Popover", typeof(PopoverSample));
			AddSample (null, "ReliefFrame", typeof (ReliefFrameSample));
			AddSample (null, "Screens", typeof (ScreensSample));
			AddSample (null, "Scroll View", typeof(ScrollWindowSample));
			AddSample (null, "Spinners", typeof (Spinners));
			AddSample (null, "Tables", typeof (Tables));
			AddSample (null, "Text Entry", typeof (TextEntries));
			AddSample (null, "Tooltips", typeof(Tooltips));
			AddSample (null, "TreeView", typeof(TreeViews));
			AddSample (null, "WidgetEvents", typeof(WidgetEvents));
			AddSample (null, "Windows", typeof(Windows));
			
			samplesTree.DataSource = store;
			
			box.Panel1.Content = samplesTree;
			
			sampleBox = new VBox ();
			title = new Label ("Sample:");
			sampleBox.PackStart (title, BoxMode.None);
			
			box.Panel2.Content = sampleBox;
			box.Panel2.Resize = true;
			box.Position = 160;
			
			Content = box;
			
			samplesTree.SelectionChanged += HandleSamplesTreeSelectionChanged;

			CloseRequested += HandleCloseRequested;
		}

		void HandleCloseRequested (object sender, CloseRequestedEventArgs args)
		{
			args.Handled = !MessageDialog.Confirm ("Samples will be closed", Command.Ok);
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			if (statusIcon != null) {
				statusIcon.Dispose ();
			}
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

