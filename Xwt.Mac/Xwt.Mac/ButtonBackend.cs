// 
// ButtonBackend.cs
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
using Xwt.Backends;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Xwt.Engine;
using MonoMac.ObjCRuntime;

namespace Xwt.Mac
{
	public class ButtonBackend: ViewBackend<NSButton,IButtonEventSink>, IButtonBackend
	{
		public ButtonBackend ()
		{
		}

		#region IButtonBackend implementation
		public override void Initialize ()
		{
			ViewObject = new MacButton (EventSink);
			Widget.SizeToFit ();
		}

		public void EnableEvent (Xwt.Backends.ButtonEvent ev)
		{
			((MacButton)Widget).EnableEvent (ev);
		}

		public void DisableEvent (Xwt.Backends.ButtonEvent ev)
		{
			((MacButton)Widget).DisableEvent (ev);
		}
		
		public void SetContent (string label, object imageBackend, ContentPosition imagePosition)
		{
			Widget.Title = label ?? "";
			if (string.IsNullOrEmpty (label))
				imagePosition = ContentPosition.Center;
			if (imageBackend != null) {
				Widget.Image = (NSImage)imageBackend;
				switch (imagePosition) {
				case ContentPosition.Bottom: Widget.ImagePosition = NSCellImagePosition.ImageBelow; break;
				case ContentPosition.Left: Widget.ImagePosition = NSCellImagePosition.ImageLeft; break;
				case ContentPosition.Right: Widget.ImagePosition = NSCellImagePosition.ImageRight; break;
				case ContentPosition.Top: Widget.ImagePosition = NSCellImagePosition.ImageAbove; break;
				case ContentPosition.Center: Widget.ImagePosition = NSCellImagePosition.ImageOverlaps; break;
				}
			}
			Widget.SizeToFit ();
		}
		
		public void SetButtonStyle (ButtonStyle style)
		{
			switch (style) {
			case ButtonStyle.Normal:
				Widget.BezelStyle = NSBezelStyle.RoundRect;
				Widget.SetButtonType (NSButtonType.MomentaryPushIn);
				Messaging.void_objc_msgSend_bool (Widget.Handle, selSetShowsBorderOnlyWhileMouseInside.Handle, false);
				break;
			case ButtonStyle.Flat:
				Widget.BezelStyle = NSBezelStyle.RoundRect;
				Messaging.void_objc_msgSend_bool (Widget.Handle, selSetShowsBorderOnlyWhileMouseInside.Handle, true);
				break;
			}
		}
		
		static Selector selSetShowsBorderOnlyWhileMouseInside = new Selector ("setShowsBorderOnlyWhileMouseInside:");
		
		public void SetButtonType (ButtonType type)
		{
			switch (type) {
			case ButtonType.Disclosure: Widget.BezelStyle = NSBezelStyle.Disclosure; break;
			default: Widget.BezelStyle = NSBezelStyle.RoundRect; break;
			}
		}
		
		#endregion
	}
	
	public class MacButton: NSButton, IViewObject
	{
		public MacButton (IntPtr p): base (p)
		{
		}
		
		public MacButton (IButtonEventSink eventSink)
		{
			BezelStyle = NSBezelStyle.Rounded;
			Activated += delegate {
				Toolkit.Invoke (delegate {
					eventSink.OnClicked ();
				});
			};
		}
		
		public MacButton (ICheckBoxEventSink eventSink)
		{
			Activated += delegate {
				Toolkit.Invoke (delegate {
					eventSink.OnClicked ();
				});
			};
		}
		
		public Widget Frontend { get; set; }
		
		public NSView View {
			get { return this; }
		}
		
		public void EnableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}

		public void DisableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}
	}
}

