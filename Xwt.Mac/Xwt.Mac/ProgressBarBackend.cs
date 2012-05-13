// 
// ProgressBarBackend.cs
//  
// Author:
//       Andres G. Aragoneses <knocte@gmail.com>
// 
// Copyright (c) 2012 Andres G. Aragoneses
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
	public class ProgressBarBackend: ViewBackend<NSProgressIndicator, IWidgetEventSink>, IProgressBarBackend
	{
		public ProgressBarBackend ()
		{
		}

		public override void Initialize ()
		{
			var widget = new MacProgressBar (EventSink);
			ViewObject = widget;
			Widget.SizeToFit ();
			
			widget.Indeterminate = true;
			widget.MinValue = 0.0;
			widget.MaxValue = 1.0;
			widget.StartAnimation (null);
		}

		public void EnableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}

		public void DisableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}
		
		public void SetFraction (double? fraction)
		{
			var widget = (NSProgressIndicator)ViewObject;
			if (fraction != null) {
				widget.Indeterminate = false;
				widget.DoubleValue = fraction.Value;
			} else {
				widget.Indeterminate = true;
			}
		}
	}
	
	class MacProgressBar: NSProgressIndicator, IViewObject
	{
		public MacProgressBar (IntPtr p): base (p)
		{
		}
		
		public MacProgressBar (IWidgetEventSink eventSink)
		{
		}
		
		public MacProgressBar (ICheckBoxEventSink eventSink)
		{
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

