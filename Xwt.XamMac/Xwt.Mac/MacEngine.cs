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
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
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
			NSApplicationInitializer.Initialize ();

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
			RegisterBackend <Xwt.Backends.ITextAreaBackend, TextEntryBackend> ();
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
			RegisterBackend <Xwt.Backends.ISaveFileDialogBackend, SaveFileDialogBackend> ();
			RegisterBackend <Xwt.Backends.IColorPickerBackend, ColorPickerBackend> ();
			RegisterBackend <Xwt.Backends.ICalendarBackend,CalendarBackend> ();
			RegisterBackend <Xwt.Backends.ISelectFontDialogBackend, SelectFontDialogBackend> ();
			RegisterBackend <Xwt.Backends.IAccessibleBackend, AccessibleBackend> ();
			RegisterBackend <Xwt.Backends.IPopupWindowBackend, PopupWindowBackend> ();
			RegisterBackend <Xwt.Backends.IUtilityWindowBackend, PopupWindowBackend> ();
			RegisterBackend <Xwt.Backends.ISearchTextEntryBackend, SearchTextEntryBackend> ();
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
			var str = (NSString) Runtime.GetNSObject (filePath);
			if (str.Length == 0)
				return true;
			return Messaging.bool_objc_msgSend_IntPtr_IntPtr (self, hijackedSel.Handle, filePath, owner);
		}
		
		public override void InvokeAsync (Action action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");

			NSRunLoop.Main.BeginInvokeOnMainThread (action);
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
			var wb = GetNativeBackend (w);
			wb.SetAutosizeMode (true);
			return wb.Widget;
		}

		public override object GetNativeImage (Xwt.Drawing.Image image)
		{
			if (image == null)
				return null;
			var img = (NSImage)base.GetNativeImage (image);
			if (img is CustomImage) {
				img = ((CustomImage)img).Clone ();
				var idesc = image.ToImageDescription (ApplicationContext);
				((CustomImage)img).Image = idesc;
			}
			img.Size = new CGSize ((nfloat)image.Size.Width, (nfloat)image.Size.Height);
			return img;
		}

		public override bool HasNativeParent (Widget w)
		{
			var wb = GetNativeBackend (w);
			return wb.Widget.Superview != null;
		}

		public ViewBackend GetNativeBackend (Widget w)
		{
			var backend = Toolkit.GetBackend (w);
			if (backend is ViewBackend)
				return (ViewBackend)backend;
			if (backend is XwtWidgetBackend)
				return GetNativeBackend ((Widget)backend);
			return null;
		}

		public override Xwt.Backends.IWindowFrameBackend GetBackendForWindow (object nativeWindow)
		{
			return new WindowFrameBackend ((NSWindow) nativeWindow);
		}

		public override object GetNativeWindow (IWindowFrameBackend backend)
		{
			if (backend == null)
				return null;
			if (backend.Window is NSWindow)
				return backend.Window;
			if (Desktop.DesktopType == DesktopType.Mac && Toolkit.NativeEngine == ApplicationContext.Toolkit)
				return Runtime.GetNSObject (backend.NativeHandle) as NSWindow;
			return null;
		}

		public override object GetBackendForContext (object nativeWidget, object nativeContext)
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
			im.Size = new CGSize ((nfloat)view.Bounds.Width, (nfloat)view.Bounds.Height);
			return im;
		}

		public override Rectangle GetScreenBounds (object nativeWidget)
		{
			var widget = nativeWidget as NSView;
			if (widget == null)
				throw new InvalidOperationException ("Widget belongs to a different toolkit");
			var lo = widget.ConvertPointToView (new CGPoint(0, 0), null);
			lo = widget.Window.ConvertRectToScreen (new CGRect (lo, CGSize.Empty)).Location;
			return MacDesktopBackend.ToDesktopRect (new CGRect (lo.X, lo.Y, widget.Frame.Width, widget.Frame.Height));
		}
	}

	public class AppDelegate : NSApplicationDelegate
	{
		bool launched;
		List<IMacWindowBackend> pendingWindows = new List<IMacWindowBackend> ();

		public event EventHandler<TerminationEventArgs> Terminating;
		public event EventHandler Unhidden;
		public event EventHandler<OpenFilesEventArgs> OpenFilesRequest;
		public event EventHandler<OpenUrlEventArgs> OpenUrl;
		public event EventHandler<ShowDockMenuArgs> ShowDockMenu;
		
		public AppDelegate (bool launched)
		{
			this.launched = launched;
		}
		
		internal void ShowWindow (IMacWindowBackend w)
		{
			if (!launched) {
				if (!pendingWindows.Contains (w))
					pendingWindows.Add (w);
			}
			else
				w.InternalShow ();
		}

		public override void DidFinishLaunching (NSNotification notification)
		{
			launched = true;
			foreach (var w in pendingWindows)
				w.InternalShow ();
		}

		public override void WillFinishLaunching(NSNotification notification)
		{
			NSAppleEventManager eventManager = NSAppleEventManager.SharedAppleEventManager;
			eventManager.SetEventHandler (this, new Selector ("handleGetURLEvent:withReplyEvent:"), AEEventClass.Internet, AEEventID.GetUrl);
		}

		[Export("handleGetURLEvent:withReplyEvent:")]
		void HandleGetUrlEvent(NSAppleEventDescriptor descriptor, NSAppleEventDescriptor reply)
		{
			var openUrlEvent = OpenUrl;
			if (openUrlEvent == null)
				return;
			
			string keyDirectObjectString = "----";
			uint keywordDirectObject = (((uint)keyDirectObjectString [0]) << 24 |
				((uint)keyDirectObjectString [1]) << 16 |
				((uint)keyDirectObjectString [2]) << 8 |
				((uint)keyDirectObjectString [3]));
			
			string urlString = descriptor.ParamDescriptorForKeyword (keywordDirectObject).ToString ();
			openUrlEvent (NSApplication.SharedApplication, new OpenUrlEventArgs (urlString));
		}

		public override void ScreenParametersChanged (NSNotification notification)
		{
			if (MacDesktopBackend.Instance != null)
				MacDesktopBackend.Instance.NotifyScreensChanged ();
		}

		public override NSApplicationTerminateReply ApplicationShouldTerminate (NSApplication sender)
		{
			var terminatingEvent = Terminating;
			if (terminatingEvent != null) {
				var args = new TerminationEventArgs ();
				terminatingEvent (NSApplication.SharedApplication, args);
				return args.Reply;
			}

			return NSApplicationTerminateReply.Now;
		}

		public override void DidUnhide (NSNotification notification)
		{
			var unhiddenEvent = Unhidden;
			if (unhiddenEvent != null)
				unhiddenEvent (NSApplication.SharedApplication, EventArgs.Empty);
		}

		public override void OpenFiles (NSApplication sender, string[] filenames)
		{
			var openFilesEvent = OpenFilesRequest;
			if (openFilesEvent != null) {
				var args = new OpenFilesEventArgs (filenames);
				openFilesEvent (NSApplication.SharedApplication, args);
			}
		}

		public override NSMenu ApplicationDockMenu (NSApplication sender)
		{
			NSMenu retMenu = null;
			var showDockMenuEvent = ShowDockMenu;
			if (showDockMenuEvent != null) {
				var args = new ShowDockMenuArgs ();
				showDockMenuEvent (NSApplication.SharedApplication, args);
				retMenu = args.DockMenu;
			}

			return retMenu;
		}
	}

	public class TerminationEventArgs : EventArgs
	{
		public NSApplicationTerminateReply Reply {get; set;}

		public TerminationEventArgs ()
		{
			Reply = NSApplicationTerminateReply.Now;
		}
	}

	public class OpenFilesEventArgs : EventArgs
	{
		public string[] Filenames { get; set; }
		public OpenFilesEventArgs (string[] filenames)
		{
			Filenames = filenames;
		}
	}

	public class OpenUrlEventArgs : EventArgs
	{
		public string Url { get; set; }
		public OpenUrlEventArgs (string url)
		{
			Url = url;
		}
	}

	public class ShowDockMenuArgs : EventArgs
	{
		public NSMenu DockMenu { get; set; }
	}
}
