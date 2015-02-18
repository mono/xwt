// 
// MenuButtonBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Alex Corrado <corrado@xamarin.com>
// 
// Copyright (c) 2013 Xamarin Inc
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
using Xwt.Backends;
using Xwt.Drawing;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.Foundation;
using MonoMac.AppKit;
using CGRect = System.Drawing.RectangleF;
#else
using Foundation;
using AppKit;
using CoreGraphics;
#endif

namespace Xwt.Mac
{
	public class MenuButtonBackend: ButtonBackend, IMenuButtonBackend
	{
		public MenuButtonBackend ()
		{
		}

		new IMenuButtonEventSink EventSink {
			get { return (IMenuButtonEventSink)base.EventSink; }
		}

		public override void Initialize ()
		{
			ViewObject = new MacMenuButton (EventSink, ApplicationContext);
		}

		public override Color BackgroundColor {
			get { return ((MacMenuButton)Widget).BackgroundColor; }
			set { ((MacMenuButton)Widget).BackgroundColor = value; }
		}
	}

	class MacMenuButton: NSPopUpButton, IViewObject
	{
		ApplicationContext context;
		IMenuButtonEventSink eventSink;

		public MacMenuButton (IntPtr p): base (p)
		{
		}
		
		public MacMenuButton (IMenuButtonEventSink eventSink, ApplicationContext context)
		{
			this.eventSink = eventSink;
			this.context = context;

			Cell = new ColoredPopUpButtonCell ();

			PullsDown = true;
			Activated += delegate {
				context.InvokeUserCode (delegate {
					eventSink.OnClicked ();
				});
			};

			NSNotificationCenter.DefaultCenter.AddObserver ((NSString)"NSPopUpButtonWillPopUpNotification", CreateMenu, this);
			AddItem ("");
		}

		void CreateMenu (NSNotification notif)
		{
			MenuBackend m = null;
			context.InvokeUserCode (delegate {
				m = (MenuBackend) eventSink.OnCreateMenu ();
			});
			if (m != null) {
				// FIXME: mutating the Menu feels nasty, but NSPopUpButton doesn't give us much choice.. :/
				// see http://www.cocoabuilder.com/archive/cocoa/115220-nspopupbutton-and-actions.html
				if (m.ItemAt (0).Title != Title)
					m.InsertItem (new NSMenuItem (Title), 0);
				Menu = m;
			}
		}
		
		public ViewBackend Backend { get; set; }
		
		public NSView View {
			get { return this; }
		}
		
		public void EnableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}
		
		public void DisableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}

		public Color BackgroundColor {
			get {
				return ((ColoredPopUpButtonCell)Cell).Color.GetValueOrDefault();
			}
			set {
				((ColoredPopUpButtonCell)Cell).Color = value;
			}
		}

		class ColoredPopUpButtonCell : NSPopUpButtonCell
		{
			public Color? Color { get; set; }

			public override void DrawBezelWithFrame (CGRect frame, NSView controlView)
			{
				controlView.DrawWithColorTransform(Color, delegate { base.DrawBezelWithFrame (frame, controlView); });
			}
		}


		protected override void Dispose (bool disposing)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver (this);
			base.Dispose (disposing);
		}
	}
}

