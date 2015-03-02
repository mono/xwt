// 
// CheckBoxBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
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
using MonoMac.AppKit;
#else
using AppKit;
#endif

namespace Xwt.Mac
{
	public class CheckBoxBackend: ViewBackend<NSButton,ICheckBoxEventSink>, ICheckBoxBackend
	{
		bool realAllowMixed;
		NSCellStateValue currentState = NSCellStateValue.Off;

		public CheckBoxBackend ()
		{
		}

		public override void Initialize ()
		{
			var button = new MacButton ();
			ViewObject = button;
			button.SetButtonType (NSButtonType.Switch);
			button.Title = "";
			button.ActivatedInternal += OnActivated;
		}
		
		#region ICheckBoxBackend implementation
		public void SetContent (IWidgetBackend widget)
		{
		}

		public void SetContent (string label)
		{
			Widget.Title = label;
			ResetFittingSize ();
		}

		void OnActivated (MacButton b)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnClicked ();
				CheckStateChanged ();
			});
		}

		public CheckBoxState State {
			get {
				return Widget.State.ToXwtState ();
			}
			set {
				if (value == CheckBoxState.Mixed && !Widget.AllowsMixedState)
					Widget.AllowsMixedState = true;
				Widget.State = value.ToMacState ();
				CheckStateChanged ();
			}
		}

		public bool AllowMixed {
			get {
				return realAllowMixed;
			}
			set {
				realAllowMixed = value;
				Widget.AllowsMixedState = value || State == CheckBoxState.Mixed;
			}
		}

		void CheckStateChanged ()
		{
			if (!realAllowMixed && Widget.State != NSCellStateValue.Mixed)
				Widget.AllowsMixedState = false;
			if (currentState != Widget.State) {
				currentState = Widget.State;
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnToggled ();
				});
			}
		}
		#endregion
	}
}

