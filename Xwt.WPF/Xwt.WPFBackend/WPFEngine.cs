// 
// WPFEngine.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
//       Luis Reis <luiscubal@gmail.com>
//       Thomas Ziegler <ziegler.thomas@web.de>
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
// Copyright (c) 2012 Lu�s Reis
// Copyright (c) 2012 Thomas Ziegler
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Windows;
using System.Windows.Threading;
using Xwt.Backends;
using Xwt.Drawing;


namespace Xwt.WPFBackend
{
	public class WPFEngine : Xwt.Backends.ToolkitEngineBackend
	{
		System.Windows.Application application;

		public override void InitializeApplication ()
		{
			application = System.Windows.Application.Current;

			if (application == null)
				application = new System.Windows.Application ();

			RegisterBackend (typeof (IWindowBackend), typeof (WindowBackend));
			RegisterBackend (typeof (IDialogBackend), typeof (DialogBackend));
			RegisterBackend (typeof (INotebookBackend), typeof (NotebookBackend));
			RegisterBackend (typeof (IMenuBackend), typeof (MenuBackend));
			RegisterBackend (typeof (IMenuItemBackend), typeof (MenuItemBackend));
			RegisterBackend (typeof (ICheckBoxMenuItemBackend), typeof (CheckboxMenuItemBackend));
			RegisterBackend (typeof (IRadioButtonMenuItemBackend), typeof (RadioButtonMenuItemBackend));
			RegisterBackend (typeof (ISeparatorMenuItemBackend), typeof (SeparatorMenuItemBackend));
			RegisterBackend (typeof (IBoxBackend), typeof (BoxBackend));
			RegisterBackend (typeof (ILabelBackend), typeof (LabelBackend));
			RegisterBackend (typeof (ITextEntryBackend), typeof (TextEntryBackend));
			RegisterBackend (typeof (IButtonBackend), typeof (ButtonBackend));
			RegisterBackend (typeof (IToggleButtonBackend), typeof (ToggleButtonBackend));
			RegisterBackend (typeof (IMenuButtonBackend), typeof (MenuButtonBackend));
			RegisterBackend (typeof (ICheckBoxBackend), typeof (CheckBoxBackend));
			RegisterBackend (typeof (ITreeViewBackend), typeof (TreeViewBackend));
			RegisterBackend (typeof (ITreeStoreBackend), typeof (TreeStoreBackend));
			RegisterBackend (typeof (IImageViewBackend), typeof (ImageViewBackend));
			RegisterBackend (typeof (ISeparatorBackend), typeof (SeparatorBackend));
			RegisterBackend (typeof (ImageBackendHandler), typeof (ImageHandler));
			RegisterBackend (typeof (FontBackendHandler), typeof (WpfFontBackendHandler));
			RegisterBackend (typeof (ClipboardBackend), typeof (WpfClipboardBackend));
			RegisterBackend (typeof (IComboBoxBackend), typeof (ComboBoxBackend));
			RegisterBackend (typeof (IComboBoxEntryBackend), typeof (ComboBoxEntryBackend));
			RegisterBackend (typeof (IScrollViewBackend), typeof (ScrollViewBackend));
			RegisterBackend (typeof (IFrameBackend), typeof (FrameBackend));
			RegisterBackend (typeof (ICanvasBackend), typeof (CanvasBackend));
			RegisterBackend (typeof (ContextBackendHandler), typeof (WpfContextBackendHandler));
			RegisterBackend (typeof (DrawingPathBackendHandler), typeof (WpfContextBackendHandler));
			RegisterBackend (typeof (GradientBackendHandler), typeof (WpfGradientBackendHandler));
			RegisterBackend (typeof (TextLayoutBackendHandler), typeof (WpfTextLayoutBackendHandler));
			RegisterBackend (typeof (ICustomWidgetBackend), typeof (CustomWidgetBackend));
			RegisterBackend (typeof (IPanedBackend), typeof (PanedBackend));
			RegisterBackend (typeof (IScrollAdjustmentBackend), typeof (ScrollAdjustmentBackend));
			RegisterBackend (typeof (IOpenFileDialogBackend), typeof (OpenFileDialogBackend));
			RegisterBackend (typeof (ISelectFolderDialogBackend), typeof (SelectFolderDialogBackend));
			RegisterBackend (typeof (IAlertDialogBackend), typeof (AlertDialogBackend));
			RegisterBackend (typeof (ImageBuilderBackendHandler), typeof (WpfImageBuilderBackendHandler));
			RegisterBackend (typeof (ImagePatternBackendHandler), typeof (WpfImagePatternBackendHandler));
			RegisterBackend (typeof (IListViewBackend), typeof (ListViewBackend));
			RegisterBackend (typeof (IListStoreBackend), typeof (ListDataSource));
			RegisterBackend (typeof (IListBoxBackend), typeof (ListBoxBackend));
			RegisterBackend (typeof (IPopoverBackend), typeof (PopoverBackend));
			RegisterBackend (typeof (IProgressBarBackend), typeof (ProgressBarBackend));
			RegisterBackend (typeof (IRichTextViewBackend), typeof (RichTextViewBackend));
			RegisterBackend (typeof (ILinkLabelBackend), typeof (LinkLabelBackend));
			RegisterBackend (typeof (ISpinnerBackend), typeof (SpinnerBackend));
			RegisterBackend (typeof (DesktopBackend), typeof (WpfDesktopBackend));
		}

		public override void DispatchPendingEvents()
		{
			application.Dispatcher.Invoke ((Action)(() => { }), DispatcherPriority.Background);
		}

		public override void RunApplication ()
		{
			application.Run ();
		}

		public override void ExitApplication ()
		{
			application.Shutdown();
		}

		public override void InvokeAsync (Action action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");

			application.Dispatcher.BeginInvoke (action, new object [0]);
		}

		public override object TimerInvoke (Func<bool> action, TimeSpan timeSpan)
		{
			if (action == null)
				throw new ArgumentNullException ("action");

			return Timeout.Add (action, timeSpan, application.Dispatcher);
		}

		public override void CancelTimerInvoke (object id)
		{
			if (id == null)
				throw new ArgumentNullException ("id");

			Timeout.CancelTimeout ((uint)id);
		}

		public override IWindowFrameBackend GetBackendForWindow (object nativeWindow)
		{
			return new WindowFrameBackend () {
				Window = (System.Windows.Window) nativeWindow
			};
		}

		public override object GetNativeWidget (Widget w)
		{
			var backend = (IWpfWidgetBackend) Toolkit.GetBackend (w);
			return backend.Widget;
		}

		public override object GetNativeParentWindow (Widget w)
		{
			var backend = (IWpfWidgetBackend)Toolkit.GetBackend (w);

			FrameworkElement e = backend.Widget;
			while ((e = e.Parent as FrameworkElement) != null)
				if (e is System.Windows.Window)
					return e;

			return null;
		}
		
		public override bool HasNativeParent (Widget w)
		{
			var backend = (IWpfWidgetBackend)Toolkit.GetBackend (w);
			return backend.Widget.Parent != null;
		}
	}
}

