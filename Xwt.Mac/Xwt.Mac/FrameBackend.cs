// 
// FrameBackend.cs
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
using MonoMac.AppKit;
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.Mac
{
	public class FrameBackend: ViewBackend<NSBox,IFrameEventSink>, IFrameBackend
	{
		Color borderColor;
		
		public override void Initialize ()
		{
			ViewObject = new MacFrame ();
			Widget.SizeToFit ();
		}

		#region IFrameBackend implementation
		public void SetFrameType (FrameType type)
		{
			switch (type) {
			case FrameType.WidgetBox: Widget.BoxType = NSBoxType.NSBoxPrimary; break;
			case FrameType.Custom: Widget.BoxType = NSBoxType.NSBoxCustom; break;
			}
		}

		public void SetContent (IWidgetBackend child)
		{
			Widget.ContentView = GetWidget (child);
			Widget.SizeToFit ();
		}

		public void SetBorderSize (double left, double right, double top, double bottom)
		{
		}

		public void SetPadding (double left, double right, double top, double bottom)
		{
		}

		public string Label {
			get {
				return Widget.Title;
			}
			set {
				Widget.Title = value;
			}
		}

		public Xwt.Drawing.Color BorderColor {
			get {
				return borderColor;
			}
			set {
				borderColor = value;
				Widget.BorderColor = value.ToNSColor ();
			}
		}
		
		public override Color BackgroundColor {
			get {
				return Widget.FillColor.ToXwtColor ();
			}
			set {
				Widget.FillColor = value.ToNSColor ();
			}
		}
		
		#endregion
	}
	
	class MacFrame: NSBox, IViewObject<NSBox>
	{
		#region IViewObject implementation
		public NSBox View {
			get {
				return this;
			}
		}

		public Widget Frontend { get; set; }
		
		#endregion
	}
}

