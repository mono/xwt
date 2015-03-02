// 
// ToggleButtonBackend.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.ObjCRuntime;
using MonoMac.AppKit;
#else
using AppKit;
using ObjCRuntime;
#endif

namespace Xwt.Mac
{
	public class ToggleButtonBackend: ButtonBackend, IToggleButtonBackend
	{
		NSCellStateValue lastState;

		public bool Active {
			get { return Widget.State == NSCellStateValue.On; }
			set {
				Widget.State = value? NSCellStateValue.On : NSCellStateValue.Off;
				NotifyToggle ();
			}
		}

		public new IToggleButtonEventSink EventSink {
			get { return (IToggleButtonEventSink) base.EventSink; }
		}

		public ToggleButtonBackend ()
		{
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
			Widget.SetButtonType (NSButtonType.PushOnPushOff);
			lastState = Widget.State = NSCellStateValue.Off;
			((MacButton)Widget).ActivatedInternal += (obj) => NotifyToggle();
		}

		internal void NotifyToggle ()
		{
			if (lastState != Widget.State) {
				switch (((Button)Frontend).Style) {
				case ButtonStyle.Borderless:
				case ButtonStyle.Flat:
#if MONOMAC
					Messaging.void_objc_msgSend_bool (Widget.Handle, selSetShowsBorderOnlyWhileMouseInside.Handle, !Active);
#else
					Widget.ShowsBorderOnlyWhileMouseInside = !Active;
#endif
					break;
				}
				lastState = Widget.State;
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnToggled ();
				});
			}
		}
	}
}

