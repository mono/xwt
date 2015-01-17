//
// Popover.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using Xwt.Drawing;

using Xwt.Backends;

namespace Xwt
{
	[BackendType (typeof(IPopoverBackend))]
	public class Popover : XwtComponent
	{
		public enum Position {
			Top,
			Bottom,
			/*Left,
				Right*/
		}

		WidgetSpacing padding;
		Widget content;
		bool shown;

		EventHandler closedEvent;

		static Popover ()
		{
			MapEvent (PopoverEvent.Closed, typeof(Popover), "OnClosed");
		}

		protected class PopoverBackendHost: BackendHost<Popover,IPopoverBackend>, IPopoverEventSink
		{
			protected override void OnBackendCreated ()
			{
				base.OnBackendCreated ();
				Backend.Initialize (this);
			}

			public void OnClosed ()
			{
				((Popover)Parent).OnClosed ();
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new PopoverBackendHost ();
		}
		
		IPopoverBackend Backend {
			get { return ((PopoverBackendHost) BackendHost).Backend; } 
		}

		public Popover ()
		{
		}

		public Popover (Widget content)
		{
			VerifyConstructorCall (this);
			Content = content;
		}
		
		public Widget Content {
			get { return content; }
			set {
				if (shown)
					throw new InvalidOperationException ("The content widget can't be changed while the popover is visible");
				content = value;
			}
		}

		public WidgetSpacing Padding {
			get { return padding; }
			set {
				padding = value;
				UpdatePadding ();
			}
		}

		public double PaddingLeft {
			get { return padding.Left; }
			set {
				padding.Left = value;
				UpdatePadding (); 
			}
		}

		public double PaddingRight {
			get { return padding.Right; }
			set {
				padding.Right = value;
				UpdatePadding (); 
			}
		}

		public double PaddingTop {
			get { return padding.Top; }
			set {
				padding.Top = value;
				UpdatePadding (); 
			}
		}

		public double PaddingBottom {
			get { return padding.Bottom; }
			set {
				padding.Bottom = value;
				UpdatePadding (); 
			}
		}

		public Color BackgroundColor {
			get { return Backend.BackgroundColor; }
			set { Backend.BackgroundColor = value; }
		}

		void UpdatePadding ()
		{
		}

		public void Show (Position arrowPosition, Widget referenceWidget)
		{
			Show (arrowPosition, referenceWidget, Xwt.Rectangle.Zero);
		}

		public void Show (Position arrowPosition, Widget referenceWidget, Xwt.Rectangle positionRect)
		{
			if (content == null)
				throw new InvalidOperationException ("A child widget source must be set before running the Popover");
			Backend.Show (arrowPosition, referenceWidget, positionRect, content);
			shown = true;
		}

		public void Hide ()
		{
			Backend.Hide ();
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			// Don't dispose the backend if this object is being finalized
			// The backend has to handle the finalizing on its own
			if (disposing && BackendHost.BackendCreated)
				Backend.Dispose ();
		}

		protected virtual void OnClosed ()
		{
			shown = false;
			if (closedEvent != null)
				closedEvent (this, EventArgs.Empty);
		}
		
		public event EventHandler Closed {
			add {
				BackendHost.OnBeforeEventAdd (PopoverEvent.Closed, closedEvent);
				closedEvent += value;
			}
			remove {
				closedEvent -= value;
				BackendHost.OnAfterEventRemove (PopoverEvent.Closed, closedEvent);
			}
		}
	}
}

