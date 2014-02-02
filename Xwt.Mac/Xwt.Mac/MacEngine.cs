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

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using MonoMac.CoreGraphics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class MacEngine: Xwt.Backends.ToolkitEngineBackend
	{
		static AppDelegate appDelegate;
		static NSAutoreleasePool pool;
		
		public static AppDelegate App {
			get { return appDelegate; }
		}
		
		public override void InitializeApplication ()
		{
			if(!IsGuest)
				NSApplication.Init ();
			//Hijack ();
			if (pool != null)
				pool.Dispose ();
			pool = new NSAutoreleasePool ();
			appDelegate = new AppDelegate (IsGuest);
			NSApplication.SharedApplication.Delegate = appDelegate;

			// If NSPrincipalClass is not set, set it now. This allows running
			// the application without a bundle
			var info = NSBundle.MainBundle.InfoDictionary;
			if (info.ValueForKey ((NSString)"NSPrincipalClass") == null)
				info.SetValueForKey ((NSString)"NSApplication", (NSString)"NSPrincipalClass");
		}

		public override void InitializeBackends ()
		{
			base.InitializeBackends ();
			RegisterBackend <Xwt.Backends.ICustomWidgetBackend, CustomWidgetBackend> ();
			RegisterBackend <Xwt.Backends.IWindowBackend, WindowBackend> ();
			RegisterBackend <Xwt.Backends.ILabelBackend, LabelBackend> ();
			RegisterBackend <Xwt.Backends.IBoxBackend, BoxBackend> ();
			RegisterBackend <Xwt.Backends.IButtonBackend, ButtonBackend> ();
			RegisterBackend <Xwt.Backends.IMenuButtonBackend, MenuButtonBackend> ();
			RegisterBackend <Xwt.Backends.INotebookBackend, NotebookBackend> ();
			RegisterBackend <Xwt.Backends.ITreeViewBackend, TreeViewBackend> ();
			RegisterBackend <Xwt.Backends.IListViewBackend, ListViewBackend> ();
			RegisterBackend <Xwt.Backends.ICanvasBackend, CanvasBackend> ();
			RegisterBackend <Xwt.Backends.ImageBackendHandler, ImageHandler> ();
			RegisterBackend <Xwt.Backends.ContextBackendHandler, MacContextBackendHandler> ();
			RegisterBackend <Xwt.Backends.DrawingPathBackendHandler, MacPathBackendHandler> ();
			RegisterBackend <Xwt.Backends.ImageBuilderBackendHandler, MacImageBuilderBackendHandler> ();
			RegisterBackend <Xwt.Backends.ImagePatternBackendHandler, MacImagePatternBackendHandler> ();
			RegisterBackend <Xwt.Backends.GradientBackendHandler, MacGradientBackendHandler> ();
			RegisterBackend <Xwt.Backends.TextLayoutBackendHandler, MacTextLayoutBackendHandler> ();
			RegisterBackend <Xwt.Backends.FontBackendHandler, MacFontBackendHandler> ();
			RegisterBackend <Xwt.Backends.IMenuBackend, MenuBackend> ();
			RegisterBackend <Xwt.Backends.IMenuItemBackend, MenuItemBackend> ();
			RegisterBackend <Xwt.Backends.ICheckBoxMenuItemBackend, CheckBoxMenuItemBackend> ();
			RegisterBackend <Xwt.Backends.IRadioButtonMenuItemBackend, RadioButtonMenuItemBackend> ();
			RegisterBackend <Xwt.Backends.IRadioButtonBackend, RadioButtonBackend> ();
			RegisterBackend <Xwt.Backends.ISeparatorMenuItemBackend, SeparatorMenuItemBackend> ();
			RegisterBackend <Xwt.Backends.IComboBoxBackend, ComboBoxBackend> ();
			RegisterBackend <Xwt.Backends.IComboBoxEntryBackend, ComboBoxEntryBackend> ();
			RegisterBackend <Xwt.Backends.ITextEntryBackend, TextEntryBackend> ();
			RegisterBackend <Xwt.Backends.IImageViewBackend, ImageViewBackend> ();
			RegisterBackend <Xwt.Backends.ICheckBoxBackend, CheckBoxBackend> ();
			RegisterBackend <Xwt.Backends.IFrameBackend, FrameBackend> ();
			RegisterBackend <Xwt.Backends.IScrollViewBackend, ScrollViewBackend> ();
			RegisterBackend <Xwt.Backends.IToggleButtonBackend, ToggleButtonBackend> ();
			RegisterBackend <Xwt.Backends.ISeparatorBackend, SeparatorBackend> ();
			RegisterBackend <Xwt.Backends.IPanedBackend, PanedBackend> ();
			RegisterBackend <Xwt.Backends.IAlertDialogBackend, AlertDialogBackend> ();
			RegisterBackend <Xwt.Backends.IStatusIconBackend, StatusIconBackend> ();
			RegisterBackend <Xwt.Backends.IProgressBarBackend, ProgressBarBackend> ();
			RegisterBackend <Xwt.Backends.IListStoreBackend, Xwt.DefaultListStoreBackend> ();
			RegisterBackend <Xwt.Backends.ILinkLabelBackend, LinkLabelBackend> ();
			RegisterBackend <Xwt.Backends.ISpinnerBackend, SpinnerBackend> ();
			RegisterBackend <Xwt.Backends.ISpinButtonBackend, SpinButtonBackend> ();
			RegisterBackend <Xwt.Backends.IExpanderBackend, ExpanderBackend> ();
			RegisterBackend <Xwt.Backends.IPopoverBackend, PopoverBackend> ();
			RegisterBackend <Xwt.Backends.ISelectFolderDialogBackend, SelectFolderDialogBackend> ();
			RegisterBackend <Xwt.Backends.IOpenFileDialogBackend, OpenFileDialogBackend> ();
			RegisterBackend <Xwt.Backends.ClipboardBackend, MacClipboardBackend> ();
			RegisterBackend <Xwt.Backends.DesktopBackend, MacDesktopBackend> ();
			RegisterBackend <Xwt.Backends.IMenuButtonBackend, MenuButtonBackend> ();
			RegisterBackend <Xwt.Backends.IListBoxBackend, ListBoxBackend> ();
			RegisterBackend <Xwt.Backends.IDialogBackend, DialogBackend> ();
			RegisterBackend <Xwt.Backends.IRichTextViewBackend, RichTextViewBackend> ();
			RegisterBackend <Xwt.Backends.IScrollbarBackend, ScrollbarBackend> ();
			RegisterBackend <Xwt.Backends.IDatePickerBackend, DatePickerBackend> ();
			RegisterBackend <Xwt.Backends.ISliderBackend, SliderBackend> ();
			RegisterBackend <Xwt.Backends.IEmbeddedWidgetBackend, EmbedNativeWidgetBackend> ();
			RegisterBackend <Xwt.Backends.KeyboardHandler, MacKeyboardHandler> ();
			RegisterBackend <Xwt.Backends.IPasswordEntryBackend, PasswordEntryBackend> ();
			RegisterBackend <Xwt.Backends.IWebViewBackend, WebViewBackend> ();
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

			NSRunLoop.Main.BeginInvokeOnMainThread (delegate {
				action ();
			});
		}
		
		public override object TimerInvoke (Func<bool> action, TimeSpan timeSpan)
		{
			NSTimer timer = null;
			var runLoop = NSRunLoop.Current;
			timer = NSTimer.CreateRepeatingTimer (timeSpan, delegate {
				if (!action ())
					timer.Invalidate ();
			});
			runLoop.AddTimer (timer, NSRunLoop.NSDefaultRunLoopMode);
			runLoop.AddTimer (timer, NSRunLoop.NSRunLoopModalPanelMode);
			return timer;
		}
		
		public override void CancelTimerInvoke (object id)
		{
			((NSTimer)id).Invalidate ();
		}
		
		public override object GetNativeWidget (Widget w)
		{
			ViewBackend wb = (ViewBackend)Toolkit.GetBackend (w);
			wb.SetAutosizeMode (true);
			return wb.Widget;
		}

		public override bool HasNativeParent (Widget w)
		{
			ViewBackend wb = (ViewBackend)Toolkit.GetBackend (w);
			return wb.Widget.Superview != null;
		}
		
		public override Xwt.Backends.IWindowFrameBackend GetBackendForWindow (object nativeWindow)
		{
			throw new NotImplementedException ();
		}

		public override object GetBackendForContext (object nativeContext)
		{
			return new CGContextBackend {
				Context = (CGContext)nativeContext
			};
		}

		public override void DispatchPendingEvents ()
		{
			var until = NSDate.DistantPast;
			var app = NSApplication.SharedApplication;
			var p = new NSAutoreleasePool ();
			while (true) {
				var ev = app.NextEvent (NSEventMask.AnyEvent, until, NSRunLoop.NSDefaultRunLoopMode, true);
				if (ev != null)
					app.SendEvent (ev);
				else
					break;
			}
			p.Dispose ();
		}

		public override object RenderWidget (Widget w)
		{
			var view = ((ViewBackend)w.GetBackend ()).Widget;
			view.LockFocus ();
			var img = new NSImage (view.DataWithPdfInsideRect (view.Bounds));
			var imageData = img.AsTiff ();
			var imageRep = (NSBitmapImageRep)NSBitmapImageRep.ImageRepFromData (imageData);
			var im = new NSImage ();
			im.AddRepresentation (imageRep);
			im.Size = new System.Drawing.SizeF ((float)view.Bounds.Width, (float)view.Bounds.Height);
			return im;
		}
	}

	public class AppDelegate : NSApplicationDelegate
	{
		bool launched;
		List<WindowBackend> pendingWindows = new List<WindowBackend> ();
		
		public AppDelegate (bool launched)
		{
			this.launched = launched;
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

		public override void ScreenParametersChanged (NSNotification notification)
		{
			if (MacDesktopBackend.Instance != null)
				MacDesktopBackend.Instance.NotifyScreensChanged ();
		}
	}
}
