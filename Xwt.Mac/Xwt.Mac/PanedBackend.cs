// 
// PanedBackend.cs
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
using MonoMac.AppKit;
using Xwt.Engine;

namespace Xwt.Mac
{
	public class PanedBackend: ViewBackend<NSSplitView,IPanedEventSink>, IPanedBackend
	{
		SplitViewDelegate viewDelegate;
		
		class SplitViewDelegate: NSSplitViewDelegate
		{
			public PanedBackend PanedBackend;
			
			public override void DidResizeSubviews (MonoMac.Foundation.NSNotification notification)
			{
				PanedBackend.DidResizeSubviews ();
			}
		}
		
		public PanedBackend ()
		{
		}

		#region IPanedBackend implementation
		public void Initialize (Orientation dir)
		{
			ViewObject = new CustomSplitView ();
			if (dir == Orientation.Horizontal)
				Widget.IsVertical = true;
			viewDelegate = new SplitViewDelegate () { PanedBackend = this };
			Widget.Delegate = viewDelegate;
		}

		public void SetPanel (int panel, IWidgetBackend widget, bool resize, bool shrink)
		{
			IMacViewBackend view = (IMacViewBackend) widget;
			Widget.AddSubview (view.View);
			Widget.AdjustSubviews ();
			view.NotifyPreferredSizeChanged ();
		}
		
		void DidResizeSubviews ()
		{
			EventSink.OnPositionChanged ();
		}

		public void UpdatePanel (int panel, bool resize, bool shrink)
		{
		}

		public void RemovePanel (int panel)
		{
		}

		public double Position {
			get {
				return 0;
			}
			set {
			}
		}
		#endregion
	}
	
	class CustomSplitView: NSSplitView, IViewObject
	{
		public NSView View {
			get {
				return this;
			}
		}

		public Widget Frontend { get; set; }
	}
}

