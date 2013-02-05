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
			RegisterBackend<ICustomWidgetBackend, CustomWidgetBackend> ();
			RegisterBackend<IWindowBackend, WindowBackend> ();
			RegisterBackend<ILabelBackend, LabelBackend> ();
			RegisterBackend<IBoxBackend, BoxBackend> ();
			RegisterBackend<IButtonBackend, ButtonBackend> ();
			RegisterBackend<INotebookBackend, NotebookBackend> ();
			RegisterBackend<ITreeViewBackend, TreeViewBackend> ();
			RegisterBackend<ITreeStoreBackend, TreeStoreBackend> ();
			RegisterBackend<IListViewBackend, ListViewBackend> ();
			RegisterBackend<IListStoreBackend, ListStoreBackend> ();
			RegisterBackend<ICanvasBackend, CanvasBackend> ();
			RegisterBackend<ImageBackendHandler, ImageHandler> ();
#if USE_PANGO
			RegisterBackend<Xwt.Backends.ContextBackendHandler, ContextBackendHandlerWithPango> ();
			RegisterBackend<TextLayoutBackendHandler, GtkTextLayoutBackendHandler> ();
#else
			WidgetRegistry.RegisterBackend<ContextBackendHandler, ContextBackendHandler> ();
			WidgetRegistry.RegisterBackend<TextLayoutBackendHandler, CairoTextLayoutBackendHandler> ();
#endif
			RegisterBackend<DrawingPathBackendHandler, CairoContextBackendHandler> ();
			RegisterBackend<GradientBackendHandler, CairoGradientBackendHandler> ();
			RegisterBackend<FontBackendHandler, GtkFontBackendHandler> ();
			RegisterBackend<IMenuBackend, MenuBackend> ();
			RegisterBackend<IMenuItemBackend, MenuItemBackend> ();
			RegisterBackend<ICheckBoxMenuItemBackend, CheckBoxMenuItemBackend> ();
			RegisterBackend<IRadioButtonMenuItemBackend, RadioButtonMenuItemBackend> ();
			RegisterBackend<ISeparatorMenuItemBackend, SeparatorMenuItemBackend> ();
			RegisterBackend<IScrollViewBackend, ScrollViewBackend> ();
			RegisterBackend<IComboBoxBackend, ComboBoxBackend> ();
			RegisterBackend<IDesignerSurfaceBackend, DesignerSurfaceBackend> ();
			RegisterBackend<IMenuButtonBackend, MenuButtonBackend> ();
			RegisterBackend<ITextEntryBackend, TextEntryBackend> ();
			RegisterBackend<IToggleButtonBackend, ToggleButtonBackend> ();
			RegisterBackend<IImageViewBackend, ImageViewBackend> ();
			RegisterBackend<IAlertDialogBackend, AlertDialogBackend> ();
			RegisterBackend<ICheckBoxBackend, CheckBoxBackend> ();
			RegisterBackend<IFrameBackend, FrameBackend> ();
			RegisterBackend<ISeparatorBackend, SeparatorBackend> ();
			RegisterBackend<IDialogBackend, DialogBackend> ();
			RegisterBackend<IComboBoxEntryBackend, ComboBoxEntryBackend> ();
			RegisterBackend<ClipboardBackend, GtkClipboardBackend> ();
			RegisterBackend<ImagePatternBackendHandler, GtkImagePatternBackendHandler> ();
			RegisterBackend<ImageBuilderBackendHandler, ImageBuilderBackend> ();
			RegisterBackend<IScrollAdjustmentBackend, ScrollAdjustmentBackend> ();
			RegisterBackend<IOpenFileDialogBackend, OpenFileDialogBackend> ();
			RegisterBackend<ISaveFileDialogBackend, SaveFileDialogBackend> ();
			RegisterBackend<ISelectFolderDialogBackend, SelectFolderDialogBackend> ();
			RegisterBackend<IPanedBackend, PanedBackend> ();
			RegisterBackend<ISelectColorDialogBackend, SelectColorDialogBackend> ();
			RegisterBackend<IListBoxBackend, ListBoxBackend> ();
			RegisterBackend<IStatusIconBackend, StatusIconBackend> ();
			RegisterBackend<IProgressBarBackend, ProgressBarBackend> ();
			RegisterBackend<IPopoverBackend, PopoverBackend> ();
			RegisterBackend<ISpinButtonBackend, SpinButtonBackend> ();
			RegisterBackend<IDatePickerBackend, DatePickerBackend> ();
			RegisterBackend<ILinkLabelBackend, LinkLabelBackend> ();
			RegisterBackend<ISpinnerBackend, SpinnerBackend> ();
			RegisterBackend<IRichTextViewBackend, RichTextViewBackend> ();
			RegisterBackend<IExpanderBackend, ExpanderBackend> ();
			RegisterBackend<DesktopBackend, GtkDesktopBackend> ();
			RegisterBackend<IEmbeddedWidgetBackend, EmbeddedWidgetBackend> ();
			RegisterBackend<ISegmentedButtonBackend, SegmentedButtonBackend> ();
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

