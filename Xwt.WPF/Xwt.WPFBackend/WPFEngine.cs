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

			application.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			RegisterBackend<IWindowBackend, WindowBackend> ();
			RegisterBackend<IDialogBackend, DialogBackend> ();
			RegisterBackend<INotebookBackend, NotebookBackend> ();
			RegisterBackend<IMenuBackend, MenuBackend> ();
			RegisterBackend<IMenuItemBackend, MenuItemBackend> ();
			RegisterBackend<ICheckBoxMenuItemBackend, CheckboxMenuItemBackend> ();
			RegisterBackend<IRadioButtonMenuItemBackend, RadioButtonMenuItemBackend> ();
			RegisterBackend<ISeparatorMenuItemBackend, SeparatorMenuItemBackend> ();
			RegisterBackend<IBoxBackend, BoxBackend> ();
			RegisterBackend<ILabelBackend, LabelBackend> ();
			RegisterBackend<ITextEntryBackend, TextEntryBackend> ();
			RegisterBackend<IButtonBackend, ButtonBackend> ();
			RegisterBackend<IToggleButtonBackend, ToggleButtonBackend> ();
			RegisterBackend<IMenuButtonBackend, MenuButtonBackend> ();
			RegisterBackend<ICheckBoxBackend, CheckBoxBackend> ();
			RegisterBackend<ITreeViewBackend, TreeViewBackend> ();
			RegisterBackend<ITreeStoreBackend, TreeStoreBackend> ();
			RegisterBackend<IImageViewBackend, ImageViewBackend> ();
			RegisterBackend<ISeparatorBackend, SeparatorBackend> ();
			RegisterBackend<ImageBackendHandler, ImageHandler> ();
			RegisterBackend<FontBackendHandler, WpfFontBackendHandler> ();
			RegisterBackend<ClipboardBackend, WpfClipboardBackend> ();
			RegisterBackend<IComboBoxBackend, ComboBoxBackend> ();
			RegisterBackend<IComboBoxEntryBackend, ComboBoxEntryBackend> ();
			RegisterBackend<IScrollViewBackend, ScrollViewBackend> ();
			RegisterBackend<IFrameBackend, FrameBackend> ();
			RegisterBackend<ICanvasBackend, CanvasBackend> ();
			RegisterBackend<ContextBackendHandler, WpfContextBackendHandler> ();
			RegisterBackend<DrawingPathBackendHandler, WpfContextBackendHandler> ();
			RegisterBackend<GradientBackendHandler, WpfGradientBackendHandler> ();
			RegisterBackend<TextLayoutBackendHandler, WpfTextLayoutBackendHandler> ();
			RegisterBackend<ICustomWidgetBackend, CustomWidgetBackend> ();
			RegisterBackend<IPanedBackend, PanedBackend> ();
			RegisterBackend<IScrollAdjustmentBackend, ScrollAdjustmentBackend> ();
			RegisterBackend<IOpenFileDialogBackend, OpenFileDialogBackend> ();
			RegisterBackend<ISaveFileDialogBackend, SaveFileDialogBackend> ();
			RegisterBackend<ISelectFolderDialogBackend, SelectFolderDialogBackend> ();
			RegisterBackend<IAlertDialogBackend, AlertDialogBackend> ();
			RegisterBackend<ImageBuilderBackendHandler, WpfImageBuilderBackendHandler> ();
			RegisterBackend<ImagePatternBackendHandler, WpfImagePatternBackendHandler> ();
			RegisterBackend<IListViewBackend, ListViewBackend> ();
			RegisterBackend<IListStoreBackend, ListDataSource> ();
			RegisterBackend<IListBoxBackend, ListBoxBackend> ();
			RegisterBackend<IPopoverBackend, PopoverBackend> ();
			RegisterBackend<IProgressBarBackend, ProgressBarBackend> ();
			RegisterBackend<IRichTextViewBackend, RichTextViewBackend> ();
			RegisterBackend<ILinkLabelBackend, LinkLabelBackend> ();
			RegisterBackend<ISpinnerBackend, SpinnerBackend> ();
			RegisterBackend<DesktopBackend, WpfDesktopBackend>();
			RegisterBackend<IExpanderBackend, ExpanderBackend>();
			RegisterBackend<IDatePickerBackend, DatePickerBackend>();
			RegisterBackend<ISelectColorDialogBackend, SelectColorDialogBackend>();
			RegisterBackend<IRadioButtonBackend, RadioButtonBackend>();
			RegisterBackend<ISpinButtonBackend, SpinButtonBackend>();
			RegisterBackend<ISliderBackend, SliderBackend> ();
			RegisterBackend<IScrollbarBackend, ScrollbarBackend> ();
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

		public override object GetNativeImage (Image image)
		{
			return DataConverter.AsImageSource (Toolkit.GetBackend (image));
		}
	}
}

