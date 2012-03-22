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

namespace Xwt.Mac
{
	class MacEngine: Xwt.Backends.EngineBackend
	{
		static AppDelegate appDelegate = new AppDelegate ();
		static NSAutoreleasePool pool;
		
		public static AppDelegate App {
			get { return appDelegate; }
		}
		
		public override void InitializeApplication ()
		{
			NSApplication.Init ();
			Hijack ();
			pool = new NSAutoreleasePool ();
			NSApplication.SharedApplication.Delegate = appDelegate;
			
			WidgetRegistry.RegisterBackend (typeof(Xwt.Window), typeof(WindowBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Label), typeof(LabelBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.HBox), typeof(BoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.VBox), typeof(BoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Button), typeof(ButtonBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Notebook), typeof(NotebookBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.TreeView), typeof(TreeViewBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ListView), typeof(ListViewBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Canvas), typeof(CanvasBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.Image), typeof(ImageHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.Context), typeof(ContextBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.ImageBuilder), typeof(ImageBuilderBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.ImagePattern), typeof(ImagePatternBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.Gradient), typeof(GradientBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.TextLayout), typeof(TextLayoutBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Drawing.Font), typeof(FontBackendHandler));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Menu), typeof(MenuBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.MenuItem), typeof(MenuItemBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.CheckBoxMenuItem), typeof(CheckBoxMenuItemBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.RadioButtonMenuItem), typeof(RadioButtonMenuItemBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.SeparatorMenuItem), typeof(SeparatorMenuItemBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ComboBox), typeof(ComboBoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ComboBoxEntry), typeof(ComboBoxEntryBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.TextEntry), typeof(TextEntryBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ImageView), typeof(ImageViewBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Table), typeof(BoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.CheckBox), typeof(CheckBoxBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Frame), typeof(FrameBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ScrollView), typeof(ScrollViewBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.ToggleButton), typeof(ToggleButtonBackend));
			WidgetRegistry.RegisterBackend (typeof(Xwt.Backends.IAlertDialogBackend), typeof(AlertDialogBackend));
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

		public static void ReplaceChild (NSView cont, NSView oldView, NSView newView)
		{
			if (cont is IViewContainer) {
				((IViewContainer)cont).ReplaceChild (oldView, newView);
			}
			else if (cont is NSView) {
				newView.Frame = oldView.Frame;
				oldView.RemoveFromSuperview ();
				newView.AddSubview (newView);
			}
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
			IMacViewBackend wb = (IMacViewBackend)Xwt.Engine.WidgetRegistry.GetBackend (w);
			return wb.View;
		}
		
		public override Xwt.Backends.IWindowFrameBackend GetBackendForWindow (object nativeWindow)
		{
			throw new NotImplementedException ();
		}
	}
	
	public interface IViewContainer
	{
		void ReplaceChild (NSView oldView, NSView newView);
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