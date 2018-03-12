// 
// IViewObject.cs
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

using AppKit;
using CoreGraphics;
using System;

namespace Xwt.Mac
{
	public interface IViewObject
	{
		NSView View { get; }
		ViewBackend Backend { get; set; }
	}

	public static class IViewObjectExtensions
	{
		public static void UpdateEventTrackingArea (this IViewObject view, ref NSTrackingArea replaceArea)
		{
			if (view == null)
				throw new ArgumentNullException (nameof (view));
			if (view.View == null)
				throw new InvalidOperationException ();
			if (replaceArea != null) {
				view.View.RemoveTrackingArea (replaceArea);
				replaceArea.Dispose ();
			}
			CGRect viewBounds = view.View.Bounds;
			var options = NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.MouseEnteredAndExited;
			var trackingArea = new NSTrackingArea (viewBounds, options, view.View, null);
			view.View.AddTrackingArea (trackingArea);
		}

		public static bool HandleMouseDown (this IViewObject view, NSEvent theEvent)
		{
			if (view == null)
				throw new ArgumentNullException (nameof (view));
			if (theEvent == null)
				throw new ArgumentNullException (nameof (theEvent));
			if (view.View == null)
				throw new InvalidOperationException ();
			CGPoint p = view.View.ConvertPointFromEvent (theEvent);
			if (!view.View.Bounds.Contains (p))
				return false;
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = theEvent.GetPointerButton ();
			args.IsContextMenuTrigger = theEvent.TriggersContextMenu ();
			args.MultiplePress = (int)theEvent.ClickCount;
			view.Backend.ApplicationContext.InvokeUserCode (delegate
			{
				view.Backend.EventSink.OnButtonPressed (args);
			});
			return args.Handled;
		}

		public static bool HandleMouseUp (this IViewObject view, NSEvent theEvent)
		{
			if (view == null)
				throw new ArgumentNullException (nameof (view));
			if (theEvent == null)
				throw new ArgumentNullException (nameof (theEvent));
			if (view.View == null)
				throw new InvalidOperationException ();
			CGPoint p = view.View.ConvertPointFromEvent (theEvent);
			if (!view.View.Bounds.Contains (p))
				return false;
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = theEvent.GetPointerButton ();
			args.MultiplePress = (int)theEvent.ClickCount;
			view.Backend.ApplicationContext.InvokeUserCode (delegate {
				view.Backend.EventSink.OnButtonReleased (args);
			});
			return args.Handled;
		}

		public static void HandleMouseEntered (this IViewObject view, NSEvent theEvent)
		{
			if (view == null)
				throw new ArgumentNullException (nameof (view));
			view.Backend.ApplicationContext.InvokeUserCode (view.Backend.EventSink.OnMouseEntered);
		}

		public static void HandleMouseExited (this IViewObject view, NSEvent theEvent)
		{
			if (view == null)
				throw new ArgumentNullException (nameof (view));
			view.Backend.ApplicationContext.InvokeUserCode (view.Backend.EventSink.OnMouseExited);
		}

		public static bool HandleMouseMoved (this IViewObject view, NSEvent theEvent)
		{
			if (view == null)
				throw new ArgumentNullException (nameof (view));
			if (theEvent == null)
				throw new ArgumentNullException (nameof (theEvent));
			if (view.View == null)
				throw new InvalidOperationException ();
			CGPoint p = view.View.ConvertPointFromEvent (theEvent);
			if (!view.View.Bounds.Contains (p))
				return false;
			MouseMovedEventArgs args = new MouseMovedEventArgs ((long)TimeSpan.FromSeconds (theEvent.Timestamp).TotalMilliseconds, p.X, p.Y);
			view.Backend.ApplicationContext.InvokeUserCode (delegate {
				view.Backend.EventSink.OnMouseMoved (args);
			});
			return args.Handled;
		}

		public static bool HandleKeyDown (this IViewObject view, NSEvent theEvent)
		{
			if (view == null)
				throw new ArgumentNullException (nameof (view));
			if (theEvent == null)
				throw new ArgumentNullException (nameof (theEvent));
			if (view.View == null)
				throw new InvalidOperationException ();

			var keyArgs = theEvent.ToXwtKeyEventArgs ();
			view.Backend.ApplicationContext.InvokeUserCode (delegate {
				view.Backend.EventSink.OnKeyPressed (keyArgs);
			});
			if (keyArgs.Handled)
				return true;

			var textArgs = new TextInputEventArgs (theEvent.Characters);
			if (!String.IsNullOrEmpty (theEvent.Characters))
				view.Backend.ApplicationContext.InvokeUserCode (delegate {
					view.Backend.EventSink.OnTextInput (textArgs);
				});
			if (textArgs.Handled)
				return true;

			return false;
		}

		public static bool HandleKeyUp (this IViewObject view, NSEvent theEvent)
		{
			if (view == null)
				throw new ArgumentNullException (nameof (view));
			if (theEvent == null)
				throw new ArgumentNullException (nameof (theEvent));
			if (view.View == null)
				throw new InvalidOperationException ();

			var keyArgs = theEvent.ToXwtKeyEventArgs ();
			view.Backend.ApplicationContext.InvokeUserCode (delegate {
				view.Backend.EventSink.OnKeyReleased (keyArgs);
			});
			return keyArgs.Handled;
		}
	}
}

