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

using MonoMac.AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class RadioButtonBackend : ViewBackend <NSButton, IRadioButtonEventSink>, IRadioButtonBackend
	{
		MacRadioButtonGroup radioGroup;

		public object Group {
			get {
				if (radioGroup == null) {
					radioGroup = new MacRadioButtonGroup ();
					radioGroup.Add (Widget);
				}
				return radioGroup;
			}

			set {
				var g = value as MacRadioButtonGroup;
				if (g == radioGroup)
					return;
				radioGroup = g;
				radioGroup.Add (Widget);
			}
		}
		
		public bool Active {
			get { return Widget.State == NSCellStateValue.On; }
			set { 
 				if (value) {
					Widget.State = NSCellStateValue.On; 
					var g = Group as MacRadioButtonGroup;
					g.Activate (Widget);
				} else
					Widget.State = NSCellStateValue.Off;
			}
		}

		public RadioButtonBackend ()
		{
		}
	
		public override void Initialize ()
		{
			var mb = new MacButton (EventSink, ApplicationContext);
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
		NSButton lastActive;

		public void Add (NSButton button)
		{
			if (button == null)
				return;
			if (button.State == NSCellStateValue.On)
				Activate (button);
		}

		public void Activate (NSButton button)
		{
			if (button == null || button == lastActive)
				return;
			if (lastActive != null)
				lastActive.State = NSCellStateValue.Off;
			lastActive = button;
		}
	}
}

