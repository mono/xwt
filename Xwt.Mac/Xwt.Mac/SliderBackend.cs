//
// SliderBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using MonoMac.AppKit;
using Xwt.Backends;
using MonoMac.Foundation;

namespace Xwt.Mac
{
	public class SliderBackend: ViewBackend<NSSlider,ISliderEventSink>, ISliderBackend
	{
		Orientation dir;

		public SliderBackend ()
		{
		}

		public void Initialize (Orientation dir)
		{
			this.dir = dir;
			ViewObject = new MacSlider ();
			//((NSSliderCell)Widget.Cell).SetValueForKey (NSObject.FromObject (false), (NSString)"NSVertical");
			if (dir == Orientation.Horizontal)
				Widget.SetFrameSize (new System.Drawing.SizeF (10, 5));
			else
				Widget.SetFrameSize (new System.Drawing.SizeF (5, 10));
		}

		protected override void OnSizeToFit ()
		{
			Widget.SizeToFit ();
			if (dir == Orientation.Vertical)
				Widget.SetFrameSize (new System.Drawing.SizeF (Widget.Frame.Width, Widget.Frame.Width * 2));
			else
				Widget.SetFrameSize (new System.Drawing.SizeF (Widget.Frame.Width * 2, Widget.Frame.Width));
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is SliderEvent)
				Widget.Activated += HandleActivated;;
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is SliderEvent)
				Widget.Activated -= HandleActivated;
		}

		void HandleActivated (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (EventSink.ValueChanged);
		}

		#region ISliderBackend implementation

		public double Value {
			get {
				return Widget.DoubleValue;
			}
			set {
				Widget.DoubleValue = value;
			}
		}

		public double MinimumValue {
			get {
				return Widget.MinValue;
			}
			set {
				Widget.MinValue = value;
			}
		}

		public double MaximumValue {
			get {
				return Widget.MaxValue;
			}
			set {
				Widget.MaxValue = value;
			}
		}

		#endregion
	}

	class MacSlider: NSSlider, IViewObject
	{
		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }
	}
}

