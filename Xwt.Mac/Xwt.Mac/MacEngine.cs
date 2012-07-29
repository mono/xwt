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
	public class MacEngine: Xwt.Backends.EngineBackend
	{
		static AppDelegate appDelegate = new AppDelegate ();
		static NSAutoreleasePool pool;

		public static WidgetRegistry Registry {
			get;
			set;
		}
		
		public static AppDelegate App {
			get { return appDelegate; }
		}

		public override void InitializeApplication ()
		{
			NSApplication.Init ();
			Hijack ();
			pool = new NSAutoreleasePool ();
			NSApplication.SharedApplication.Delegate = appDelegate;
		}
		
		public override void InitializeRegistry (WidgetRegistry registry)
		{
			Registry = registry;
			registry.FromEngine = this;
			
			registry.RegisterBackend (typeof(Xwt.Window), typeof(WindowBackend));
			registry.RegisterBackend (typeof(Xwt.Label), typeof(LabelBackend));
			registry.RegisterBackend (typeof(Xwt.HBox), typeof(BoxBackend));
			registry.RegisterBackend (typeof(Xwt.VBox), typeof(BoxBackend));
			registry.RegisterBackend (typeof(Xwt.Button), typeof(ButtonBackend));
			registry.RegisterBackend (typeof(Xwt.Notebook), typeof(NotebookBackend));
			registry.RegisterBackend (typeof(Xwt.TreeView), typeof(TreeViewBackend));
			registry.RegisterBackend (typeof(Xwt.ListView), typeof(ListViewBackend));
			registry.RegisterBackend (typeof(Xwt.Canvas), typeof(CanvasBackend));
			registry.RegisterBackend (typeof(Xwt.Drawing.Image), typeof(ImageHandler));
			registry.RegisterBackend (typeof(Xwt.Drawing.Context), typeof(ContextBackendHandler));
			registry.RegisterBackend (typeof(Xwt.Drawing.ImageBuilder), typeof(ImageBuilderBackendHandler));
			registry.RegisterBackend (typeof(Xwt.Drawing.ImagePattern), typeof(ImagePatternBackendHandler));
			registry.RegisterBackend (typeof(Xwt.Drawing.Gradient), typeof(GradientBackendHandler));
			registry.RegisterBackend (typeof(Xwt.Drawing.TextLayout), typeof(TextLayoutBackendHandler));
			registry.RegisterBackend (typeof(Xwt.Drawing.Font), typeof(FontBackendHandler));
			registry.RegisterBackend (typeof(Xwt.Menu), typeof(MenuBackend));
			registry.RegisterBackend (typeof(Xwt.MenuItem), typeof(MenuItemBackend));
			registry.RegisterBackend (typeof(Xwt.CheckBoxMenuItem), typeof(CheckBoxMenuItemBackend));
			registry.RegisterBackend (typeof(Xwt.RadioButtonMenuItem), typeof(RadioButtonMenuItemBackend));
			registry.RegisterBackend (typeof(Xwt.SeparatorMenuItem), typeof(SeparatorMenuItemBackend));
			registry.RegisterBackend (typeof(Xwt.ComboBox), typeof(ComboBoxBackend));
			registry.RegisterBackend (typeof(Xwt.ComboBoxEntry), typeof(ComboBoxEntryBackend));
			registry.RegisterBackend (typeof(Xwt.TextEntry), typeof(TextEntryBackend));
			registry.RegisterBackend (typeof(Xwt.ImageView), typeof(ImageViewBackend));
			registry.RegisterBackend (typeof(Xwt.Table), typeof(BoxBackend));
			registry.RegisterBackend (typeof(Xwt.CheckBox), typeof(CheckBoxBackend));
			registry.RegisterBackend (typeof(Xwt.Frame), typeof(FrameBackend));
			registry.RegisterBackend (typeof(Xwt.ScrollView), typeof(ScrollViewBackend));
			registry.RegisterBackend (typeof(Xwt.ToggleButton), typeof(ToggleButtonBackend));
			registry.RegisterBackend (typeof(Xwt.VSeparator), typeof(SeparatorBackend));
			registry.RegisterBackend (typeof(Xwt.HSeparator), typeof(SeparatorBackend));
			registry.RegisterBackend (typeof(Xwt.HPaned), typeof(PanedBackend));
			registry.RegisterBackend (typeof(Xwt.VPaned), typeof(PanedBackend));
			registry.RegisterBackend (typeof(Xwt.Backends.IAlertDialogBackend), typeof(AlertDialogBackend));
			registry.RegisterBackend (typeof(Xwt.StatusIcon), typeof(StatusIconBackend));
			registry.RegisterBackend (typeof(Xwt.ProgressBar), typeof(ProgressBarBackend));
			registry.RegisterBackend (typeof(Xwt.SpinButton), typeof (SpinButtonBackend));
			registry.RegisterBackend (typeof(Xwt.ListStore), typeof (Xwt.DefaultListStoreBackend));
			registry.RegisterBackend (typeof(Xwt.Expander), typeof (ExpanderBackend));
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
			IMacViewBackend wb = (IMacViewBackend)Registry.GetBackend (w);
			return wb.View;
		}
		
		public override Xwt.Backends.IWindowFrameBackend GetBackendForWindow (object nativeWindow)
		{
			throw new NotImplementedException ();
		}
	}
	
	public interface IViewContainer
	{
		void UpdateChildMargins (IMacViewBackend view);
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