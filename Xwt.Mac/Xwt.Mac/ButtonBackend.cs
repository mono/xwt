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

using MonoMac.ObjCRuntime;
using Xwt.Drawing;

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
			ViewObject = new MacButton (EventSink, ApplicationContext);
		}

		public void EnableEvent (Xwt.Backends.ButtonEvent ev)
		{
			((MacButton)Widget).EnableEvent (ev);
		}

		public void DisableEvent (Xwt.Backends.ButtonEvent ev)
		{
			((MacButton)Widget).DisableEvent (ev);
		}
		
		public void SetContent (string label, ImageDescription image, ContentPosition imagePosition)
		{
			Widget.Title = label ?? "";
			if (string.IsNullOrEmpty (label))
				imagePosition = ContentPosition.Center;
			if (!image.IsNull) {
				var img = image.ToNSImage ();
				Widget.Image = (NSImage)img;
				Widget.Cell.ImageScale = NSImageScale.None;
				switch (imagePosition) {
				case ContentPosition.Bottom: Widget.ImagePosition = NSCellImagePosition.ImageBelow; break;
				case ContentPosition.Left: Widget.ImagePosition = NSCellImagePosition.ImageLeft; break;
				case ContentPosition.Right: Widget.ImagePosition = NSCellImagePosition.ImageRight; break;
				case ContentPosition.Top: Widget.ImagePosition = NSCellImagePosition.ImageAbove; break;
				case ContentPosition.Center: Widget.ImagePosition = string.IsNullOrEmpty (label) ? NSCellImagePosition.ImageOnly : NSCellImagePosition.ImageOverlaps; break;
				}
			}
			ResetFittingSize ();
		}
		
		public void SetButtonStyle (ButtonStyle style)
		{
			switch (style) {
			case ButtonStyle.Normal:
				Widget.BezelStyle = NSBezelStyle.Rounded;
				Widget.SetButtonType (NSButtonType.MomentaryPushIn);
				Messaging.void_objc_msgSend_bool (Widget.Handle, selSetShowsBorderOnlyWhileMouseInside.Handle, false);
				break;
			case ButtonStyle.Flat:
				Widget.BezelStyle = NSBezelStyle.Rounded;
				Messaging.void_objc_msgSend_bool (Widget.Handle, selSetShowsBorderOnlyWhileMouseInside.Handle, true);
				break;
			}
		}
		
		static Selector selSetShowsBorderOnlyWhileMouseInside = new Selector ("setShowsBorderOnlyWhileMouseInside:");
		
		public void SetButtonType (ButtonType type)
		{
			switch (type) {
			case ButtonType.Disclosure: Widget.BezelStyle = NSBezelStyle.Disclosure; break;
			default: Widget.BezelStyle = NSBezelStyle.Rounded; break;
			}
		}
		
		#endregion
	}
	
	class MacButton: NSButton, IViewObject
	{
		//
		// This is necessary since the Activated event for NSControl in AppKit does 
		// not take a list of handlers, instead it supports only one handler.
		//
		// This event is used by the RadioButton backend to implement radio groups
		//
		internal event Action <MacButton> ActivatedInternal;

		public MacButton (IntPtr p): base (p)
		{
		}
		
		public MacButton (IButtonEventSink eventSink, ApplicationContext context)
		{
			BezelStyle = NSBezelStyle.Rounded;
			Activated += delegate {
				context.InvokeUserCode (delegate {
					eventSink.OnClicked ();
				});
				OnActivatedInternal ();
			};
		}
		
		public MacButton (ICheckBoxEventSink eventSink, ApplicationContext context)
		{
			Activated += delegate {
				context.InvokeUserCode (delegate {
					eventSink.OnClicked ();
				});
				OnActivatedInternal ();
			};
		}

		public MacButton (IRadioButtonEventSink eventSink, ApplicationContext context)
		{
			Activated += delegate {
				context.InvokeUserCode (delegate {
					eventSink.OnClicked ();
				});
				OnActivatedInternal ();
			};
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

		void OnActivatedInternal ()
		{
			if (ActivatedInternal == null)
				return;

			ActivatedInternal (this);
		}
	}
}

