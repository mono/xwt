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
	class GtkEngine: Xwt.Backends.EngineBackend
	{
		public override void InitializeApplication ()
		{
			Gtk.Application.Init ();
			
			WidgetRegistry.RegisterBackend (typeof(Xwt.Widget), typeof(CustomWidgetBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Window), typeof(WindowBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Label), typeof(LabelBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.HBox), typeof(BoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.VBox), typeof(BoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Button), typeof(ButtonBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Notebook), typeof(NotebookBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.TreeView), typeof(TreeViewBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.TreeStore), typeof(TreeStoreBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ListView), typeof(ListViewBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ListStore), typeof(ListStoreBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Canvas), typeof(CanvasBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.Image), typeof(ImageHandler));
#if USE_PANGO
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.Context), typeof(ContextBackendHandlerWithPango));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.TextLayout), typeof(TextLayoutBackendHandler));
#else
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.Context), typeof(ContextBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.TextLayout), typeof(CairoTextLayoutBackendHandler));
#endif
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.Gradient), typeof(CairoGradientBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.Font), typeof(FontBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Menu), typeof(MenuBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.MenuItem), typeof(MenuItemBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.CheckBoxMenuItem), typeof(CheckBoxMenuItemBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.RadioButtonMenuItem), typeof(RadioButtonMenuItemBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.SeparatorMenuItem), typeof(SeparatorMenuItemBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ScrollView), typeof(ScrollViewBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ComboBox), typeof(ComboBoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Design.DesignerSurface), typeof(DesignerSurfaceBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.MenuButton), typeof(MenuButtonBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.TextEntry), typeof(TextEntryBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ToggleButton), typeof(ToggleButtonBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ImageView), typeof(ImageViewBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Backends.IAlertDialogBackend), typeof(AlertDialogBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Table), typeof(BoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.CheckBox), typeof(CheckBoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Frame), typeof(FrameBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.VSeparator), typeof(SeparatorBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.HSeparator), typeof(SeparatorBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Dialog), typeof(DialogBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ComboBoxEntry), typeof(ComboBoxEntryBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Clipboard), typeof(ClipboardBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.ImagePattern), typeof(ImagePatternBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.ImageBuilder), typeof(ImageBuilderBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ScrollAdjustment), typeof(ScrollAdjustmentBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.OpenFileDialog), typeof(OpenFileDialogBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.SaveFileDialog), typeof(SaveFileDialogBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.SelectFolderDialog), typeof(SelectFolderDialogBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Paned), typeof(PanedBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.SelectColorDialog), typeof(SelectColorDialogBackend));
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
			IGtkWidgetBackend wb = (IGtkWidgetBackend)Xwt.Engine.WidgetRegistry.GetBackend (w);
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
			IGtkWidgetBackend wb = (IGtkWidgetBackend)Xwt.Engine.WidgetRegistry.GetBackend (w);
			return wb.Widget.Toplevel as Gtk.Window;
		}
	}
	
	public interface IGtkContainer
	{
		void ReplaceChild (Gtk.Widget oldWidget, Gtk.Widget newWidget);
	}
}

