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
using Xwt.Backends;
using Xwt.Drawing;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGSize = System.Drawing.SizeF;
using CGRect = System.Drawing.RectangleF;
using MonoMac.AppKit;
#else
using CoreGraphics;
using AppKit;
#endif

namespace Xwt.Mac
{
	public class FrameBackend: ViewBackend<NSBox,IFrameEventSink>, IFrameBackend
	{
		Color borderColor;
		NSView currentChild;
		
		public override void Initialize ()
		{
			ViewObject = new MacFrame ();
		}

		#region IFrameBackend implementation
		public void SetFrameType (FrameType type)
		{
			switch (type) {
			case FrameType.WidgetBox:
				Widget.BoxType = NSBoxType.NSBoxPrimary;
				Widget.ContentViewMargins = new CGSize (5,5);
				break;
			case FrameType.Custom:
				Widget.BoxType = NSBoxType.NSBoxCustom;
				Widget.ContentViewMargins = new CGSize (0,0);
				break;
			}
		}

		public void SetContent (IWidgetBackend child)
		{
			if (currentChild != null) {
				currentChild.RemoveFromSuperview ();
				currentChild = null;
			}
			if (child == null)
				return;

			var childBackend = (ViewBackend) child;

			currentChild = GetWidget (childBackend);

			var contentView = (NSView)Widget.ContentView;
			contentView.AddSubview (currentChild);
			ResetFittingSize ();
		}

		protected override void OnSizeToFit ()
		{
			if (currentChild != null) {
				var s = ((IViewObject)currentChild).Backend.Frontend.Surface.GetPreferredSize ();
				var frame = (Frame)Frontend;
				currentChild.Frame = new CGRect (0, 0, (nfloat)s.Width, (nfloat)s.Height);
				Widget.SizeToFit ();
				Widget.SetFrameSize (new CGSize ((nfloat)(Widget.Frame.Width + frame.Padding.HorizontalSpacing), (nfloat)(Widget.Frame.Height + frame.Padding.VerticalSpacing)));
			}
		}

		public void SetBorderSize (double left, double right, double top, double bottom)
		{
		}

		public void SetPadding (double left, double right, double top, double bottom)
		{
			var cv = (CustomContentView)Widget.ContentView;
			cv.UpdatePlacement ();
		}

		public override void UpdateChildPlacement (IWidgetBackend childBackend)
		{
			var cv = (CustomContentView)Widget.ContentView;
			cv.UpdatePlacement ();
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
	
	class MacFrame: NSBox, IViewObject
	{
		public MacFrame ()
		{
			Title = "";
			ContentView = new CustomContentView ();
		}

		#region IViewObject implementation
		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }
		
		#endregion
	}

	class CustomContentView: NSView
	{
		public override bool IsFlipped {
			get {
				return true;
			}
		}
		public override void SetFrameSize (CGSize newSize)
		{
			base.SetFrameSize (newSize);
			UpdatePlacement ();
		}

		public void UpdatePlacement ()
		{
			if (Subviews.Length > 0) {
				var frame = (Frame) ((IViewObject)Superview).Backend.Frontend;
				var v = Subviews [0];
				var r = new Rectangle (frame.PaddingLeft, frame.PaddingTop, Frame.Width - frame.Padding.HorizontalSpacing, Frame.Height - frame.Padding.VerticalSpacing);
				r = ((IViewObject)v).Backend.Frontend.Surface.GetPlacementInRect (r);
				v.Frame = r.ToCGRect ();
			}
		}
	}
}

