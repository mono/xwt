// 
// CanvasBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Hywel Thomas <hywel.w.thomas@gmail.com>
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
using AppKit;
using CoreGraphics;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class CanvasBackend: ViewBackend<NSView,ICanvasEventSink>, ICanvasBackend
	{
		CanvasView view;
		
		public CanvasBackend ()
		{
		}

		public override void Initialize ()
		{
			view = new CanvasView (EventSink, ApplicationContext);
			ViewObject = view;
		}

		protected override void OnSizeToFit ()
		{
			var s = EventSink.GetPreferredSize ();
			Widget.SetFrameSize (new CGSize ((nfloat)s.Width, (nfloat)s.Height)); 
		}

		public Rectangle Bounds {
			get {
				return new Rectangle (0, 0, view.Frame.Width, view.Frame.Height);
			}
		}
		
		public void QueueDraw ()
		{
			view.NeedsDisplay = true;
		}
		
		public void QueueDraw (Rectangle rect)
		{
			view.SetNeedsDisplayInRect (new CGRect ((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height));
		}
		
		public void AddChild (IWidgetBackend widget, Rectangle rect)
		{
			var v = GetWidget (widget);
			view.AddSubview (v);
			
			// Not using SetWidgetBounds because the view is flipped
			v.Frame = new CGRect ((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height);;
			v.NeedsDisplay = true;
		}
		
		public void RemoveChild (IWidgetBackend widget)
		{
			var v = GetWidget (widget);
			v.RemoveFromSuperview ();
		}
		
		public void SetChildBounds (IWidgetBackend widget, Rectangle rect)
		{
			var w = GetWidget (widget);
			
			// Not using SetWidgetBounds because the view is flipped
			w.Frame = new CGRect ((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height);;
			w.NeedsDisplay = true;
		}
	}
	
	class CanvasView: WidgetView
	{
		ICanvasEventSink eventSink;
		
		public CanvasView (ICanvasEventSink eventSink, ApplicationContext context): base (eventSink, context)
		{
			this.eventSink = eventSink;
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			context.InvokeUserCode (delegate {
				CGContext ctx = NSGraphicsContext.CurrentContext.GraphicsPort;

				//fill BackgroundColor
				ctx.SetFillColor (Backend.Frontend.BackgroundColor.ToCGColor ());
				ctx.FillRect (Bounds);

				var backend = new CGContextBackend {
					Context = ctx,
					InverseViewTransform = ctx.GetCTM ().Invert ()
				};
				eventSink.OnDraw (backend, new Rectangle (dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height));
			});
		}

		public override Foundation.NSObject AccessibilityProxy {
			get {
				return base.AccessibilityProxy;
			}
			set {
				base.AccessibilityProxy = value;
			}
		}
	}
}

