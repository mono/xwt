// 
// MacEngine.cs
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
using Xwt.Engine;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class MacEngine: Xwt.Backends.ToolkitEngineBackend
	{
		static AppDelegate appDelegate = new AppDelegate ();
		static NSAutoreleasePool pool;
		
		public static AppDelegate App {
			get { return appDelegate; }
		}
		
		public override void InitializeApplication ()
		{
			NSApplication.Init ();
			//Hijack ();
			pool = new NSAutoreleasePool ();
			NSApplication.SharedApplication.Delegate = appDelegate;
		}

		public override void InitializeBackends ()
		{
			base.InitializeBackends ();
			RegisterBackend (typeof(Xwt.Window), typeof(WindowBackend));
			RegisterBackend (typeof(Xwt.Label), typeof(LabelBackend));
			RegisterBackend (typeof(Xwt.HBox), typeof(BoxBackend));
			RegisterBackend (typeof(Xwt.VBox), typeof(BoxBackend));
			RegisterBackend (typeof(Xwt.Button), typeof(ButtonBackend));
			RegisterBackend (typeof(Xwt.Notebook), typeof(NotebookBackend));
			RegisterBackend (typeof(Xwt.TreeView), typeof(TreeViewBackend));
			RegisterBackend (typeof(Xwt.ListView), typeof(ListViewBackend));
			RegisterBackend (typeof(Xwt.Canvas), typeof(CanvasBackend));
			RegisterBackend (typeof(Xwt.Drawing.Image), typeof(ImageHandler));
			RegisterBackend (typeof(Xwt.Drawing.Context), typeof(MacContextBackendHandler));
			RegisterBackend (typeof(Xwt.Drawing.ImageBuilder), typeof(MacImageBuilderBackendHandler));
			RegisterBackend (typeof(Xwt.Drawing.ImagePattern), typeof(MacImagePatternBackendHandler));
			RegisterBackend (typeof(Xwt.Drawing.Gradient), typeof(MacGradientBackendHandler));
			RegisterBackend (typeof(Xwt.Drawing.TextLayout), typeof(MacTextLayoutBackendHandler));
			RegisterBackend (typeof(Xwt.Drawing.Font), typeof(MacFontBackendHandler));
			RegisterBackend (typeof(Xwt.Menu), typeof(MenuBackend));
			RegisterBackend (typeof(Xwt.MenuItem), typeof(MenuItemBackend));
			RegisterBackend (typeof(Xwt.CheckBoxMenuItem), typeof(CheckBoxMenuItemBackend));
			RegisterBackend (typeof(Xwt.RadioButtonMenuItem), typeof(RadioButtonMenuItemBackend));
			RegisterBackend (typeof(Xwt.SeparatorMenuItem), typeof(SeparatorMenuItemBackend));
			RegisterBackend (typeof(Xwt.ComboBox), typeof(ComboBoxBackend));
			RegisterBackend (typeof(Xwt.ComboBoxEntry), typeof(ComboBoxEntryBackend));
			RegisterBackend (typeof(Xwt.TextEntry), typeof(TextEntryBackend));
			RegisterBackend (typeof(Xwt.ImageView), typeof(ImageViewBackend));
			RegisterBackend (typeof(Xwt.Table), typeof(BoxBackend));
			RegisterBackend (typeof(Xwt.CheckBox), typeof(CheckBoxBackend));
			RegisterBackend (typeof(Xwt.Frame), typeof(FrameBackend));
			RegisterBackend (typeof(Xwt.ScrollView), typeof(ScrollViewBackend));
			RegisterBackend (typeof(Xwt.ToggleButton), typeof(ToggleButtonBackend));
			RegisterBackend (typeof(Xwt.VSeparator), typeof(SeparatorBackend));
			RegisterBackend (typeof(Xwt.HSeparator), typeof(SeparatorBackend));
			RegisterBackend (typeof(Xwt.HPaned), typeof(PanedBackend));
			RegisterBackend (typeof(Xwt.VPaned), typeof(PanedBackend));
			RegisterBackend (typeof(Xwt.Backends.IAlertDialogBackend), typeof(AlertDialogBackend));
			RegisterBackend (typeof(Xwt.StatusIcon), typeof(StatusIconBackend));
			RegisterBackend (typeof(Xwt.ProgressBar), typeof(ProgressBarBackend));
			RegisterBackend (typeof(Xwt.ListStore), typeof(Xwt.DefaultListStoreBackend));
			RegisterBackend (typeof(Xwt.LinkLabel), typeof (LinkLabelBackend));
			RegisterBackend (typeof(Xwt.Placement), typeof(BoxBackend));
			RegisterBackend (typeof(Xwt.Spinner), typeof(SpinnerBackend));
			RegisterBackend (typeof(Xwt.SpinButton), typeof(SpinButtonBackend));
			RegisterBackend (typeof(Xwt.Expander), typeof(ExpanderBackend));
			RegisterBackend (typeof(Xwt.Popover), typeof (PopoverBackend));
			RegisterBackend (typeof(Xwt.SelectFolderDialog), typeof(SelectFolderDialogBackend));
			RegisterBackend (typeof(Xwt.OpenFileDialog), typeof(OpenFileDialogBackend));
		}

		public override void RunApplication ()
		{
			pool.Dispose ();
			NSApplication.Main (new string [0]);
			pool = new NSAutoreleasePool ();
		}

		public override void ExitApplication ()
		{
			NSApplication.SharedApplication.Terminate(appDelegate);
		}

		static Selector hijackedSel = new Selector ("hijacked_loadNibNamed:owner:");
		static Selector originalSel = new Selector ("loadNibNamed:owner:");
		
		static void Hijack ()
		{
			Class c = ObjcHelper.GetMetaClass ("NSBundle");
			if (!c.AddMethod (hijackedSel.Handle, new Func<IntPtr, IntPtr, IntPtr, IntPtr,bool>(HijackedLoadNibNamed), "B@:@@"))
				throw new Exception ("Failed to add method");
			c.MethodExchange (originalSel.Handle, hijackedSel.Handle);
		}
		
		static bool HijackedLoadNibNamed (IntPtr self, IntPtr sel, IntPtr filePath, IntPtr owner)
		{
			var str = new NSString (filePath);
			if (str.Length == 0)
				return true;
			return Messaging.bool_objc_msgSend_IntPtr_IntPtr (self, hijackedSel.Handle, filePath, owner);
		}
		
		public override void InvokeAsync (Action action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");

			NSApplication.SharedApplication.BeginInvokeOnMainThread (delegate {
				action ();
			});
		}
		
		public override object TimerInvoke (Func<bool> action, TimeSpan timeSpan)
		{
			throw new NotImplementedException ();
		}
		
		public override void CancelTimerInvoke (object id)
		{
			throw new NotImplementedException ();
		}
		
		public override object GetNativeWidget (Widget w)
		{
			IMacViewBackend wb = (IMacViewBackend)Toolkit.GetBackend (w);
			return wb.View;
		}
		
		public override Xwt.Backends.IWindowFrameBackend GetBackendForWindow (object nativeWindow)
		{
			throw new NotImplementedException ();
		}

		public override void DispatchPendingEvents ()
		{
		}
	}

	public class AppDelegate : NSApplicationDelegate
	{
		bool launched;
		List<WindowBackend> pendingWindows = new List<WindowBackend> ();
		
		public AppDelegate ()
		{
		}
		
		internal void ShowWindow (WindowBackend w)
		{
			if (!launched) {
				if (!pendingWindows.Contains (w))
					pendingWindows.Add (w);
			}
			else
				w.InternalShow ();
		}

		public override void FinishedLaunching (NSObject notification)
		{
			launched = true;
			foreach (var w in pendingWindows)
				w.InternalShow ();
		}
	}
}
