// 
// ToggleButton.cs
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
using Xwt.Drawing;

namespace Xwt
{
	public class ToggleButton: Button
	{
		EventHandler toggledEvent;
		
		protected new class EventSink: Button.EventSink, IToggleButtonEventSink
		{
			public void OnToggled ()
			{
				((ToggleButton)Parent).OnToggled (EventArgs.Empty);
			}
		}
		
		static ToggleButton ()
		{
			MapEvent (ToggleButtonEvent.Toggled, typeof(ToggleButton), "OnToggled");
		}
		
		public ToggleButton ()
		{
		}
		
		public ToggleButton (string label): base (label)
		{
		}
		
		public ToggleButton (Image img, string label): base (img, label)
		{
		}
		
		public ToggleButton (Image img): base (img)
		{
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new IToggleButtonBackend Backend {
			get { return (IToggleButtonBackend) base.Backend; }
		}
		
		public bool Active {
			get { return Backend.Active; }
			set { Backend.Active = value; }
		}
		
		protected void OnToggled (EventArgs a)
		{
			if (toggledEvent != null)
				toggledEvent (this, a);
		}
		
		public event EventHandler Toggled {
			add {
				OnBeforeEventAdd (ToggleButtonEvent.Toggled, toggledEvent);
				toggledEvent += value;
			}
			remove {
				toggledEvent -= value;
				OnAfterEventRemove (ToggleButtonEvent.Toggled, toggledEvent);
			}
		}
	}
}

