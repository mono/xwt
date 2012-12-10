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

			RegisterBackend (typeof (Window), typeof (WindowBackend));
			RegisterBackend (typeof (Dialog), typeof (DialogBackend));
			RegisterBackend (typeof (Notebook), typeof (NotebookBackend));
			RegisterBackend (typeof (Menu), typeof (MenuBackend));
			RegisterBackend (typeof (MenuItem), typeof (MenuItemBackend));
			RegisterBackend (typeof (CheckBoxMenuItem), typeof (CheckboxMenuItemBackend));
			RegisterBackend (typeof (RadioButtonMenuItem), typeof (RadioButtonMenuItemBackend));
			RegisterBackend (typeof (SeparatorMenuItem), typeof (SeparatorMenuItemBackend));
			RegisterBackend (typeof (Table), typeof (BoxBackend));
			RegisterBackend (typeof (Box), typeof (BoxBackend));
			RegisterBackend (typeof (Label), typeof (LabelBackend));
			RegisterBackend (typeof (TextEntry), typeof (TextEntryBackend));
			RegisterBackend (typeof (Button), typeof (ButtonBackend));
			RegisterBackend (typeof (ToggleButton), typeof (ToggleButtonBackend));
			RegisterBackend (typeof (MenuButton), typeof (MenuButtonBackend));
			RegisterBackend (typeof (CheckBox), typeof (CheckBoxBackend));
			RegisterBackend (typeof (TreeView), typeof (TreeViewBackend));
			RegisterBackend (typeof (TreeStore), typeof (TreeStoreBackend));
			RegisterBackend (typeof (ImageView), typeof (ImageViewBackend));
			RegisterBackend (typeof (Separator), typeof (SeparatorBackend));
			RegisterBackend (typeof (Image), typeof (ImageHandler));
			RegisterBackend (typeof (Font), typeof (WpfFontBackendHandler));
			RegisterBackend (typeof (Clipboard), typeof (WpfClipboardBackend));
			RegisterBackend (typeof (ComboBox), typeof (ComboBoxBackend));
			RegisterBackend (typeof (ComboBoxEntry), typeof (ComboBoxEntryBackend));
			RegisterBackend (typeof (ScrollView), typeof (ScrollViewBackend));
			RegisterBackend (typeof (Frame), typeof (FrameBackend));
			RegisterBackend (typeof (Canvas), typeof (CanvasBackend));
			RegisterBackend (typeof (Context), typeof (WpfContextBackendHandler));
			RegisterBackend (typeof (DrawingPath), typeof (WpfContextBackendHandler));
			RegisterBackend (typeof (Gradient), typeof (WpfGradientBackendHandler));
			RegisterBackend (typeof (TextLayout), typeof (WpfTextLayoutBackendHandler));
			RegisterBackend (typeof (Widget), typeof (CustomWidgetBackend));
			RegisterBackend (typeof (Paned), typeof (PanedBackend));
			RegisterBackend (typeof (ScrollAdjustment), typeof (ScrollAdjustmentBackend));
			RegisterBackend (typeof (OpenFileDialog), typeof (OpenFileDialogBackend));
			RegisterBackend (typeof (SelectFolderDialog), typeof (SelectFolderDialogBackend));
			RegisterBackend (typeof (IAlertDialogBackend), typeof (AlertDialogBackend));
			RegisterBackend (typeof (ImageBuilder), typeof (WpfImageBuilderBackendHandler));
			RegisterBackend (typeof (ImagePattern), typeof (WpfImagePatternBackendHandler));
			RegisterBackend (typeof (ListView), typeof (ListViewBackend));
			RegisterBackend (typeof (ListStore), typeof (ListDataSource));
			RegisterBackend (typeof (ListBox), typeof (ListBoxBackend));
			RegisterBackend (typeof (Placement), typeof (BoxBackend));
			RegisterBackend (typeof (Popover), typeof (PopoverBackend));
			RegisterBackend (typeof (ProgressBar), typeof (ProgressBarBackend));
			RegisterBackend (typeof (RichTextView), typeof (RichTextViewBackend));
			RegisterBackend (typeof (LinkLabel), typeof (LinkLabelBackend));
			RegisterBackend (typeof (Spinner), typeof (SpinnerBackend));
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
			var backend = (IWpfWidgetBackend) WidgetRegistry.GetBackend (w);
			return backend.Widget.Parent != null;
		}
	}
}

