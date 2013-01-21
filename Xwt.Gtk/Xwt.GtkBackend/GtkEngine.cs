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

		public override void InitializeBackends ()
		{
			RegisterBackend (typeof(ICustomWidgetBackend), typeof(CustomWidgetBackend));
			RegisterBackend (typeof(IWindowBackend), typeof(WindowBackend));
			RegisterBackend (typeof(ILabelBackend), typeof(LabelBackend));
			RegisterBackend (typeof(IBoxBackend), typeof(BoxBackend));
			RegisterBackend (typeof(IButtonBackend), typeof(ButtonBackend));
			RegisterBackend (typeof(INotebookBackend), typeof(NotebookBackend));
			RegisterBackend (typeof(ITreeViewBackend), typeof(TreeViewBackend));
			RegisterBackend (typeof(ITreeStoreBackend), typeof(TreeStoreBackend));
			RegisterBackend (typeof(IListViewBackend), typeof(ListViewBackend));
			RegisterBackend (typeof(IListStoreBackend), typeof(ListStoreBackend));
			RegisterBackend (typeof(ICanvasBackend), typeof(CanvasBackend));
			RegisterBackend (typeof(ImageBackendHandler), typeof(ImageHandler));
#if USE_PANGO
			RegisterBackend (typeof(Xwt.Backends.ContextBackendHandler), typeof(ContextBackendHandlerWithPango));
			RegisterBackend (typeof(TextLayoutBackendHandler), typeof(GtkTextLayoutBackendHandler));
#else
			WidgetRegistry.RegisterBackend (typeof(ContextBackendHandler), typeof(ContextBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(TextLayoutBackendHandler), typeof(CairoTextLayoutBackendHandler));
#endif
			RegisterBackend (typeof(DrawingPathBackendHandler), typeof(CairoContextBackendHandler));
			RegisterBackend (typeof(GradientBackendHandler), typeof(CairoGradientBackendHandler));
			RegisterBackend (typeof(FontBackendHandler), typeof(GtkFontBackendHandler));
			RegisterBackend (typeof(IMenuBackend), typeof(MenuBackend));
			RegisterBackend (typeof(IMenuItemBackend), typeof(MenuItemBackend));
			RegisterBackend (typeof(ICheckBoxMenuItemBackend), typeof(CheckBoxMenuItemBackend));
			RegisterBackend (typeof(IRadioButtonMenuItemBackend), typeof(RadioButtonMenuItemBackend));
			RegisterBackend (typeof(ISeparatorMenuItemBackend), typeof(SeparatorMenuItemBackend));
			RegisterBackend (typeof(IScrollViewBackend), typeof(ScrollViewBackend));
			RegisterBackend (typeof(IComboBoxBackend), typeof(ComboBoxBackend));
			RegisterBackend (typeof(IDesignerSurfaceBackend), typeof(DesignerSurfaceBackend));
			RegisterBackend (typeof(IMenuButtonBackend), typeof(MenuButtonBackend));
			RegisterBackend (typeof(ITextEntryBackend), typeof(TextEntryBackend));
			RegisterBackend (typeof(IToggleButtonBackend), typeof(ToggleButtonBackend));
			RegisterBackend (typeof(IImageViewBackend), typeof(ImageViewBackend));
			RegisterBackend (typeof(IAlertDialogBackend), typeof(AlertDialogBackend));
			RegisterBackend (typeof(ICheckBoxBackend), typeof(CheckBoxBackend));
			RegisterBackend (typeof(IFrameBackend), typeof(FrameBackend));
			RegisterBackend (typeof(ISeparatorBackend), typeof(SeparatorBackend));
			RegisterBackend (typeof(IDialogBackend), typeof(DialogBackend));
			RegisterBackend (typeof(IComboBoxEntryBackend), typeof(ComboBoxEntryBackend));
			RegisterBackend (typeof(ClipboardBackend), typeof(GtkClipboardBackend));
			RegisterBackend (typeof(ImagePatternBackendHandler), typeof(GtkImagePatternBackendHandler));
			RegisterBackend (typeof(ImageBuilderBackendHandler), typeof(ImageBuilderBackend));
			RegisterBackend (typeof(IScrollAdjustmentBackend), typeof(ScrollAdjustmentBackend));
			RegisterBackend (typeof(IOpenFileDialogBackend), typeof(OpenFileDialogBackend));
			RegisterBackend (typeof(ISaveFileDialogBackend), typeof(SaveFileDialogBackend));
			RegisterBackend (typeof(ISelectFolderDialogBackend), typeof(SelectFolderDialogBackend));
			RegisterBackend (typeof(IPanedBackend), typeof(PanedBackend));
			RegisterBackend (typeof(ISelectColorDialogBackend), typeof(SelectColorDialogBackend));
			RegisterBackend (typeof(IListBoxBackend), typeof(ListBoxBackend));
			RegisterBackend (typeof(IStatusIconBackend), typeof(StatusIconBackend));
			RegisterBackend (typeof(IProgressBarBackend), typeof(ProgressBarBackend));
			RegisterBackend (typeof(IPopoverBackend), typeof (PopoverBackend));
			RegisterBackend (typeof(ISpinButtonBackend), typeof (SpinButtonBackend));
			RegisterBackend (typeof(IDatePickerBackend), typeof (DatePickerBackend));
			RegisterBackend (typeof(ILinkLabelBackend), typeof (LinkLabelBackend));
			RegisterBackend (typeof(ISpinnerBackend), typeof (SpinnerBackend));
			RegisterBackend (typeof(IRichTextViewBackend), typeof (RichTextViewBackend));
			RegisterBackend (typeof(IExpanderBackend), typeof (ExpanderBackend));
			RegisterBackend (typeof(DesktopBackend), typeof(GtkDesktopBackend));
			RegisterBackend (typeof(IEmbeddedWidgetBackend), typeof(EmbeddedWidgetBackend));
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
			IGtkWidgetBackend wb = (IGtkWidgetBackend)Toolkit.GetBackend (w);
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
			IGtkWidgetBackend wb = (IGtkWidgetBackend)Toolkit.GetBackend (w);
			return wb.Widget.Toplevel as Gtk.Window;
		}

		public override bool HasNativeParent (Widget w)
		{
			IGtkWidgetBackend wb = (IGtkWidgetBackend)Toolkit.GetBackend (w);
			return wb.Widget.Parent != null;
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

