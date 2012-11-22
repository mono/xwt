// 
// GtkEngine.cs
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

#define USE_PANGO

using System;
using Xwt.Engine;
using Xwt.Backends;
using Xwt.CairoBackend;

namespace Xwt.GtkBackend
{
	public class GtkEngine: Xwt.Backends.ToolkitEngineBackend
	{
		public override void InitializeApplication ()
		{
			Gtk.Application.Init ();
		}

		public override void InitializeRegistry ()
		{
			RegisterBackend (typeof(Xwt.Widget), typeof(CustomWidgetBackend));
			RegisterBackend (typeof(Xwt.Window), typeof(WindowBackend));
			RegisterBackend (typeof(Xwt.Label), typeof(LabelBackend));
			RegisterBackend (typeof(Xwt.HBox), typeof(BoxBackend));
			RegisterBackend (typeof(Xwt.VBox), typeof(BoxBackend));
			RegisterBackend (typeof(Xwt.Button), typeof(ButtonBackend));
			RegisterBackend (typeof(Xwt.Notebook), typeof(NotebookBackend));
			RegisterBackend (typeof(Xwt.TreeView), typeof(TreeViewBackend));
			RegisterBackend (typeof(Xwt.TreeStore), typeof(TreeStoreBackend));
			RegisterBackend (typeof(Xwt.ListView), typeof(ListViewBackend));
			RegisterBackend (typeof(Xwt.ListStore), typeof(ListStoreBackend));
			RegisterBackend (typeof(Xwt.Canvas), typeof(CanvasBackend));
			RegisterBackend (typeof(Xwt.Drawing.Image), typeof(ImageHandler));
#if USE_PANGO
			RegisterBackend (typeof(Xwt.Drawing.Context), typeof(ContextBackendHandlerWithPango));
			RegisterBackend (typeof(Xwt.Drawing.TextLayout), typeof(TextLayoutBackendHandler));
#else
			registry.RegisterBackend (typeof(Xwt.Drawing.Context), typeof(ContextBackendHandler));
			registry.RegisterBackend (typeof(Xwt.Drawing.TextLayout), typeof(CairoTextLayoutBackendHandler));
#endif
			RegisterBackend (typeof(Xwt.Drawing.Gradient), typeof(CairoGradientBackendHandler));
			RegisterBackend (typeof(Xwt.Drawing.Font), typeof(FontBackendHandler));
			RegisterBackend (typeof(Xwt.Menu), typeof(MenuBackend));
			RegisterBackend (typeof(Xwt.MenuItem), typeof(MenuItemBackend));
			RegisterBackend (typeof(Xwt.CheckBoxMenuItem), typeof(CheckBoxMenuItemBackend));
			RegisterBackend (typeof(Xwt.RadioButtonMenuItem), typeof(RadioButtonMenuItemBackend));
			RegisterBackend (typeof(Xwt.SeparatorMenuItem), typeof(SeparatorMenuItemBackend));
			RegisterBackend (typeof(Xwt.ScrollView), typeof(ScrollViewBackend));
			RegisterBackend (typeof(Xwt.ComboBox), typeof(ComboBoxBackend));
			RegisterBackend (typeof(Xwt.Design.DesignerSurface), typeof(DesignerSurfaceBackend));
			RegisterBackend (typeof(Xwt.MenuButton), typeof(MenuButtonBackend));
			RegisterBackend (typeof(Xwt.TextEntry), typeof(TextEntryBackend));
			RegisterBackend (typeof(Xwt.ToggleButton), typeof(ToggleButtonBackend));
			RegisterBackend (typeof(Xwt.ImageView), typeof(ImageViewBackend));
			RegisterBackend (typeof(Xwt.Backends.IAlertDialogBackend), typeof(AlertDialogBackend));
			RegisterBackend (typeof(Xwt.Table), typeof(BoxBackend));
			RegisterBackend (typeof(Xwt.CheckBox), typeof(CheckBoxBackend));
			RegisterBackend (typeof(Xwt.Frame), typeof(FrameBackend));
			RegisterBackend (typeof(Xwt.VSeparator), typeof(SeparatorBackend));
			RegisterBackend (typeof(Xwt.HSeparator), typeof(SeparatorBackend));
			RegisterBackend (typeof(Xwt.Dialog), typeof(DialogBackend));
			RegisterBackend (typeof(Xwt.ComboBoxEntry), typeof(ComboBoxEntryBackend));
			RegisterBackend (typeof(Xwt.Clipboard), typeof(ClipboardBackend));
			RegisterBackend (typeof(Xwt.Drawing.ImagePattern), typeof(ImagePatternBackendHandler));
			RegisterBackend (typeof(Xwt.Drawing.ImageBuilder), typeof(ImageBuilderBackend));
			RegisterBackend (typeof(Xwt.ScrollAdjustment), typeof(ScrollAdjustmentBackend));
			RegisterBackend (typeof(Xwt.OpenFileDialog), typeof(OpenFileDialogBackend));
			RegisterBackend (typeof(Xwt.SaveFileDialog), typeof(SaveFileDialogBackend));
			RegisterBackend (typeof(Xwt.SelectFolderDialog), typeof(SelectFolderDialogBackend));
			RegisterBackend (typeof(Xwt.Paned), typeof(PanedBackend));
			RegisterBackend (typeof(Xwt.SelectColorDialog), typeof(SelectColorDialogBackend));
			RegisterBackend (typeof(Xwt.ListBox), typeof(ListBoxBackend));
			RegisterBackend (typeof(Xwt.StatusIcon), typeof(StatusIconBackend));
			RegisterBackend (typeof(Xwt.ProgressBar), typeof(ProgressBarBackend));
			RegisterBackend (typeof(Xwt.Popover), typeof (PopoverBackend));
			RegisterBackend (typeof(Xwt.SpinButton), typeof (SpinButtonBackend));
			RegisterBackend (typeof(Xwt.DatePicker), typeof (DatePickerBackend));
			RegisterBackend (typeof(Xwt.Expander), typeof (ExpanderBackend));
		}

		public override void RunApplication ()
		{
			Gtk.Application.Run ();
		}
		
		public override void ExitApplication ()
		{
			Gtk.Application.Quit ();
		}
		
		public override bool HandlesSizeNegotiation {
			get {
				return true;
			}
		}

		public static void ReplaceChild (Gtk.Widget oldWidget, Gtk.Widget newWidget)
		{
			Gtk.Container cont = oldWidget.Parent as Gtk.Container;
			if (cont == null)
				return;
			
			if (cont is IGtkContainer) {
				((IGtkContainer)cont).ReplaceChild (oldWidget, newWidget);
			}
			else if (cont is Gtk.Notebook) {
				Gtk.Notebook notebook = (Gtk.Notebook) cont;
				Gtk.Notebook.NotebookChild nc = (Gtk.Notebook.NotebookChild) notebook[oldWidget];
				var detachable = nc.Detachable;
				var pos = nc.Position;
				var reorderable = nc.Reorderable;
				var tabExpand = nc.TabExpand;
				var tabFill = nc.TabFill;
				var label = notebook.GetTabLabel (oldWidget);
				notebook.Remove (oldWidget);
				notebook.InsertPage (newWidget, label, pos);
				
				nc = (Gtk.Notebook.NotebookChild) notebook[newWidget];
				nc.Detachable = detachable;
				nc.Reorderable = reorderable;
				nc.TabExpand = tabExpand;
				nc.TabFill = tabFill;
			}
			else if (cont is Gtk.Bin) {
				((Gtk.Bin)cont).Remove (oldWidget);
				((Gtk.Bin)cont).Child  = newWidget;
			}
		}
		
		public override void InvokeAsync (Action action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");

			Gtk.Application.Invoke (delegate {
				action ();
			});
		}

		public override object TimerInvoke (Func<bool> action, TimeSpan timeSpan)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			if (timeSpan.TotalMilliseconds < 0)
				throw new ArgumentException ("Timer period must be >=0", "timeSpan");

			return GLib.Timeout.Add ((uint) timeSpan.TotalMilliseconds, delegate {
				return action ();
			});
		}

		public override void CancelTimerInvoke (object id)
		{
			if (id == null)
				throw new ArgumentNullException ("id");

			GLib.Source.Remove ((uint)id);
		}

		public override object GetNativeWidget (Widget w)
		{
			IGtkWidgetBackend wb = (IGtkWidgetBackend)ToolkitEngine.GetBackend (w);
			return wb.Widget;
		}
		
		public override IWindowFrameBackend GetBackendForWindow (object nativeWindow)
		{
			var win = new WindowFrameBackend ();
			win.Window = (Gtk.Window) nativeWindow;
			return win;
		}
		
		public override object GetNativeParentWindow (Widget w)
		{
			IGtkWidgetBackend wb = (IGtkWidgetBackend)ToolkitEngine.GetBackend (w);
			return wb.Widget.Toplevel as Gtk.Window;
		}

		public override void DispatchPendingEvents ()
		{
			// The loop is limited to 1000 iterations as a workaround for an issue that some users
			// have experienced. Sometimes EventsPending starts return 'true' for all iterations,
			// causing the loop to never end.

			int n = 1000;
			Gdk.Threads.Enter();
			
			while (Gtk.Application.EventsPending () && --n > 0) {
				Gtk.Application.RunIteration (false);
			}
			
			Gdk.Threads.Leave();
		}
	}
	
	public interface IGtkContainer
	{
		void ReplaceChild (Gtk.Widget oldWidget, Gtk.Widget newWidget);
	}
}

