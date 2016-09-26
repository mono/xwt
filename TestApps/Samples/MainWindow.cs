using System;
using Xwt;
using Xwt.Drawing;
using System.Xml;

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
			
			var file = new MenuItem ("_File");
			file.SubMenu = new Menu ();
			file.SubMenu.Items.Add (new MenuItem ("_Open"));
			file.SubMenu.Items.Add (new MenuItem ("_New"));
			MenuItem mi = new MenuItem ("_Close");
			mi.Clicked += delegate {
				Application.Exit();
			};
			file.SubMenu.Items.Add (mi);
			menu.Items.Add (file);
			
			var edit = new MenuItem ("_Edit");
			edit.SubMenu = new Menu ();
			edit.SubMenu.Items.Add (new MenuItem ("_Copy"));
			edit.SubMenu.Items.Add (new MenuItem ("Cu_t"));
			edit.SubMenu.Items.Add (new MenuItem ("_Paste"));
			menu.Items.Add (edit);
			
			MainMenu = menu;
			
			
			HPaned box = new HPaned ();
			
			icon = Image.FromResource (typeof(App), "document-generic.png");
			
			store = new TreeStore (nameCol, iconCol, widgetCol);
			samplesTree = new TreeView ();
			samplesTree.Columns.Add ("Name", iconCol, nameCol);

			var w = AddSample (null, "Widgets", null);
			AddSample (w, "Boxes", typeof(Boxes));
			AddSample (w, "Buttons", typeof(ButtonSample));
			AddSample (w, "Calendar", typeof(CalendarSample));
			AddSample (w, "CheckBox", typeof(Checkboxes));
			AddSample (w, "Clipboard", typeof(ClipboardSample));
			AddSample (w, "ColorSelector", typeof(ColorSelectorSample));
			AddSample (w, "FileSelector", typeof (FileSelectorSample));
			AddSample (w, "FolderSelector", typeof (FolderSelectorSample));
			AddSample (w, "FontSelector", typeof(FontSelectorSample));
			AddSample (w, "ComboBox", typeof(ComboBoxes));
			AddSample (w, "DatePicker", typeof(DatePickerSample));
//			AddSample (null, "Designer", typeof(Designer));
			AddSample (w, "Expander", typeof (ExpanderSample));
			AddSample (w, "Progress bars", typeof(ProgressBarSample));
			AddSample (w, "Frames", typeof(Frames));
			var images = AddSample (w, "Images", typeof(Images));
			AddSample (images, "Themed", typeof(ThemedImages));
			AddSample (w, "Labels", typeof(Labels));
			AddSample (w, "ListBox", typeof(ListBoxSample));
			AddSample (w, "LinkLabels", typeof(LinkLabels));
			var listView = AddSample (w, "ListView", typeof(ListView1));
			AddSample (listView, "Editable Checkboxes", typeof(ListView2));
			AddSample (listView, "Cell Bounds", typeof(ListViewCellBounds));
			AddSample (listView, "Editable Entries", typeof (ListViewEntries));
			AddSample (listView, "ComboBox", typeof (ListViewCombos));
			AddSample (w, "Markdown", typeof (MarkDownSample));
			AddSample (w, "Menu", typeof(MenuSamples));
			AddSample (w, "Mnemonics", typeof (Mnemonics));
			AddSample (w, "Notebook", typeof(NotebookSample));
			AddSample (w, "Paneds", typeof(PanedViews));
			AddSample (w, "Popover", typeof(PopoverSample));
			AddSample (w, "RadioButton", typeof (RadioButtonSample));
			AddSample (w, "SpinButton", typeof (SpinButtonSample));
			AddSample (w, "Scroll View", typeof(ScrollWindowSample));
			AddSample (w, "Scrollbar", typeof(ScrollbarSample));
			AddSample (w, "Slider", typeof (SliderSample));
			AddSample (w, "Spinners", typeof (Spinners));
			AddSample (w, "Tables", typeof (Tables));
			AddSample (w, "Text Entry", typeof (TextEntries));
			AddSample (w, "Password Entry", typeof (PasswordEntries));
			var treeview = AddSample (w, "TreeView", typeof(TreeViews));
			AddSample (treeview, "Cell Bounds", typeof(TreeViewCellBounds));
			AddSample (w, "WebView", typeof(WebViewSample));

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
			AddSample (n, "9-patch Image", typeof (Image9Patch));
			AddSample (n, "Widget Rendering", typeof (WidgetRendering));
			AddSample (n, "Text Input", typeof (TextInput));
			var wf = AddSample (null, "Widget Features", null);
			AddSample (wf, "Drag & Drop", typeof(DragDrop));
			AddSample (wf, "Focus", typeof(WidgetFocus));
			AddSample (wf, "Widget Events", typeof(WidgetEvents));
			AddSample (wf, "Opacity", typeof(OpacitySample));
			AddSample (wf, "Tooltips", typeof(Tooltips));
			AddSample (wf, "Cursors", typeof(MouseCursors));

			var windows = AddSample (null, "Windows", typeof(Windows));
			AddSample (windows, "Message Dialogs", typeof(MessageDialogs));
			
			AddSample (null, "Screens", typeof (ScreensSample));

			AddSample (null, "Multithreading", typeof (MultithreadingSample));

			samplesTree.DataSource = store;
			
			box.Panel1.Content = samplesTree;
			
			sampleBox = new VBox ();
			title = new Label ("Sample:");
			sampleBox.PackStart (title);
			
			box.Panel2.Content = sampleBox;
			box.Panel2.Resize = true;
			box.Position = 160;
			
			Content = box;
			
			samplesTree.SelectionChanged += HandleSamplesTreeSelectionChanged;

			CloseRequested += HandleCloseRequested;
		}

		void HandleCloseRequested (object sender, CloseRequestedEventArgs args)
		{
			args.AllowClose = MessageDialog.Confirm ("Samples will be closed", Command.Ok);
			if (args.AllowClose)
				Application.Exit ();
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
					sampleBox.PackStart (s.Widget, true);
				}

			//	Console.WriteLine (System.Xaml.XamlServices.Save (s.Widget));
				currentSample = s.Widget;
				Dump (currentSample, 0);
			}
		}
		
		void Dump (IWidgetSurface w, int ind)
		{
			if (w == null)
				return;
			var s = w.GetPreferredSize ();
			Console.WriteLine (new string (' ', ind * 2) + " " + w.GetType ().Name + " " + s.Width + " " + s.Height);
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

