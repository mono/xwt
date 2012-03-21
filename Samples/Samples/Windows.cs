// 
// Windows.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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

namespace Samples
{
	public class Windows: VBox
	{
		public Windows ()
		{
			Button b = new Button ("Show borderless window");
			PackStart (b);
			b.Clicked += delegate {
				Window w = new Window ();
				w.Decorated = false;
				Button c = new Button ("This is a window");
//				c.Margin.SetAll (10);
				w.Content = c;
				c.Clicked += delegate {
					w.Dispose ();
				};
				var bpos = b.ScreenBounds;
				w.ScreenBounds = new Rectangle (bpos.X, bpos.Y + b.Size.Height, w.Width, w.Height);
				w.Show ();
			};
			b = new Button ("Show message dialog");
			PackStart (b);
			b.Clicked += delegate {
				MessageDialog.ShowMessage (ParentWindow, "Hi there!");
			};
			
			Button db = new Button ("Show custom dialog");
			PackStart (db);
			db.Clicked += delegate {
				Dialog d = new Dialog ();
				d.Title = "This is a dialog";
				Table t = new Table ();
				t.Attach (new Label ("Some field:"), 0, 1, 0, 1);
				t.Attach (new TextEntry (), 1, 2, 0, 1);
				t.Attach (new Label ("Another field:"), 0, 1, 1, 2);
				t.Attach (new TextEntry (), 1, 2, 1, 2);
				d.Content = t;
				
				Command custom = new Command ("Custom");
				d.Buttons.Add (new DialogButton (custom));
				d.Buttons.Add (new DialogButton ("Custom OK", Command.Ok));
				d.Buttons.Add (new DialogButton (Command.Cancel));
				d.Buttons.Add (new DialogButton (Command.Ok));
				
				var r = d.Run (this.ParentWindow);
				db.Label = "Result: " + r.Label;
				d.Dispose ();
			};
			
			b = new Button ("Show Open File dialog");
			PackStart (b);
			b.Clicked += delegate {
				OpenFileDialog dlg = new OpenFileDialog ("Select a file");
				dlg.InitialFileName = "Some file";
				dlg.Multiselect = true;
				dlg.Filters.Add (new FileDialogFilter ("Xwt files", "*.xwt"));
				dlg.Filters.Add (new FileDialogFilter ("All files", "*.*"));
				if (dlg.Run ())
					MessageDialog.ShowMessage ("Files have been selected!", string.Join ("\n", dlg.FileNames));
			};
			
			b = new Button ("Show Save File dialog");
			PackStart (b);
			b.Clicked += delegate {
				SaveFileDialog dlg = new SaveFileDialog ("Select a file");
				dlg.InitialFileName = "Some file";
				dlg.Multiselect = true;
				dlg.Filters.Add (new FileDialogFilter ("Xwt files", "*.xwt"));
				dlg.Filters.Add (new FileDialogFilter ("All files", "*.*"));
				if (dlg.Run ())
					MessageDialog.ShowMessage ("Files have been selected!", string.Join ("\n", dlg.FileNames));
			};
			
			b = new Button ("Show Select Folder dialog");
			PackStart (b);
			b.Clicked += delegate {
				SelectFolderDialog dlg = new SelectFolderDialog ("Select a folder");
				dlg.Multiselect = true;
				if (dlg.Run ())
					MessageDialog.ShowMessage ("Folders have been selected!", string.Join ("\n", dlg.Folders));
			};
			
			b = new Button ("Show Select Color dialog");
			PackStart (b);
			b.Clicked += delegate {
				SelectColorDialog dlg = new SelectColorDialog ("Select a color");
				dlg.SupportsAlpha = true;
				dlg.Color = Xwt.Drawing.Colors.AliceBlue;
				if (dlg.Run (ParentWindow))
					MessageDialog.ShowMessage ("A color has been selected!", dlg.Color.ToString ());
			};
		}
	}
}

