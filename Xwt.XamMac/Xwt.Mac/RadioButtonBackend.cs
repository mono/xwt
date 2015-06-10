//
// RadioButtonBackend.cs
//
// Author:
//       Marek Habersack <grendel@xamarin.com>
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
using System.Collections.Generic;

using Xwt.Backends;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
#endif

namespace Xwt.Mac
{
	public class RadioButtonBackend : ViewBackend <NSButton, IRadioButtonEventSink>, IRadioButtonBackend
	{
		MacRadioButtonGroup radioGroup;
		NSCellStateValue lastState;

		public object Group {
			get {
				if (radioGroup == null) {
					radioGroup = new MacRadioButtonGroup ();
					radioGroup.Add ((MacButton)Widget);
				}
				return radioGroup;
			}

			set {
				var g = value as MacRadioButtonGroup;
				if (g == radioGroup)
					return;
				radioGroup = g;
				radioGroup.Add ((MacButton)Widget);
			}
		}
		
		public bool Active {
			get { return Widget.State == NSCellStateValue.On; }
			set { 
 				if (value) {
					Widget.State = NSCellStateValue.On; 
					var g = Group as MacRadioButtonGroup;
					g.Activate ((MacButton)Widget);
				} else
					Widget.State = NSCellStateValue.Off;
				NotifyToggle ();
			}
		}

		public RadioButtonBackend ()
		{
		}
	
		public override void Initialize ()
		{
			var mb = new MacButton (EventSink, ApplicationContext);
			lastState = mb.State = NSCellStateValue.On;
			mb.ActivatedInternal += HandleActivatedInternal;
			ViewObject = mb;
			Widget.SetButtonType (NSButtonType.Radio);
			Widget.Title = String.Empty;
		}

		void HandleActivatedInternal (MacButton button)
		{
			if (radioGroup == null)
				return;

			if (button == null || button.State != NSCellStateValue.On)
				return;
			radioGroup.Activate (button);
			NotifyToggle ();
		}

		internal void NotifyToggle ()
		{
			if (lastState != Widget.State) {
				lastState = Widget.State;
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnToggled ();
				});
			}
		}

		public void SetContent (IWidgetBackend widget)
		{
		}

		public void SetContent (string label)
		{
			Widget.Title = label;
			ResetFittingSize ();
		}
	}

	// Radio button groups are implemented using a visual element on Mac - NSMatrix. That limits the ways
	// in which radio buttons can be grouped - they must be children of the same view. Since XWT uses a
	// non-visual radio button group abstraction we need to emulate the job done by NSMatrix here
	class MacRadioButtonGroup
	{
		MacButton lastActive;

		public void Add (MacButton button)
		{
			if (button == null)
				return;
			if (button.State == NSCellStateValue.On) {
				if (lastActive == null)
					Activate (button);
				else {
					button.State = NSCellStateValue.Off;
					((RadioButtonBackend)button.Backend).NotifyToggle ();
				}
			}
		}

		public void Activate (MacButton button)
		{
			if (button == null || button == lastActive)
				return;
			if (lastActive != null) {
				lastActive.State = NSCellStateValue.Off;
				((RadioButtonBackend)lastActive.Backend).NotifyToggle ();
			}
			lastActive = button;
		}
	}
}

