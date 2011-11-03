// 
// CanvasBackend.cs
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
using MonoMac.AppKit;

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
			view = new CanvasView (EventSink);
			ViewObject = view;
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
			view.NeedsToDraw (new System.Drawing.RectangleF ((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height));
		}
		
		public void OnPreferredSizeChanged ()
		{
			
		}
	}
	
	class CanvasView: NSView, IViewObject<NSView>
	{
		ICanvasEventSink eventSink;
		
		public CanvasView (ICanvasEventSink eventSink)
		{
			this.eventSink = eventSink;
		}
		
		public Widget Frontend { get; set; }
		
		public NSView View {
			get { return this; }
		}
		
		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public override void DrawRect (System.Drawing.RectangleF dirtyRect)
		{
			eventSink.OnDraw (null);
		}
		
		public override void MouseDown (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = theEvent.ButtonNumber + 1;
			eventSink.OnButtonPressed (args);
		}
		
		public override void MouseUp (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = theEvent.ButtonNumber + 1;
			eventSink.OnButtonReleased (args);
		}
		
		public override void MouseMoved (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			MouseMovedEventArgs args = new MouseMovedEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			eventSink.OnMouseMoved (args);
		}
		
		public override void MouseDragged (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			MouseMovedEventArgs args = new MouseMovedEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			eventSink.OnMouseMoved (args);
		}
		
		public override void SetFrameSize (System.Drawing.SizeF newSize)
		{
			base.SetFrameSize (newSize);
			eventSink.OnBoundsChanged ();
		}
	}
}

