// 
// Canvas.cs
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

// 
// Canvas.cs
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
using System.Collections.Generic;
using Xwt.Backends;
using Xwt.Drawing;
using System.Windows.Markup;
using System.ComponentModel;

namespace Xwt
{
	public class Canvas: Widget
	{
		Size minSize;
		Size naturalSize;
		Dictionary<Widget,Rectangle> positions;
		
		protected new class EventSink: Widget.EventSink, ICanvasEventSink
		{
			public void OnDraw (object context)
			{
				Context ctx = null;
				try {
					if (context == null)
						ctx = new Context (Parent);
					else
						ctx = new Context (context);
					((Canvas)Parent).OnDraw (ctx);
				}
				finally {
					ctx.Dispose ();
				}
			}
		}
		
		public Canvas ()
		{
		}
		
		/// <summary>
		/// Adds a child widget.
		/// </summary>
		/// <param name='widget'>
		/// The widget
		/// </param>
		/// <remarks>
		/// The widget is placed in the canvas area, at the position (0,0)
		/// using the preferred size of the widget
		/// </remarks>
		public void AddChild (Widget w)
		{
			AddChild (w, 0, 0);
		}

		/// <summary>
		/// Adds a child widget.
		/// </summary>
		/// <param name='w'>
		/// The widget
		/// </param>
		/// <param name='x'>
		/// X coordinate
		/// </param>
		/// <param name='y'>
		/// Y coordinate
		/// </param>
		/// <remarks>
		/// The widget is placed in the canvas area, at the specified coordinates,
		/// using the preferred size of the widget
		/// </remarks>
		public void AddChild (Widget w, double x, double y)
		{
			var ws = w as IWidgetSurface;
			var pw = ws.GetPreferredWidth ().NaturalSize;
			AddChild (w, new Rectangle (x, y, pw, ws.GetPreferredHeightForWidth (pw).NaturalSize));
		}
		
		/// <summary>
		/// Adds a child widget.
		/// </summary>
		/// <param name='widget'>
		/// The widget
		/// </param>
		/// <param name='bounds'>
		/// Position and size of the child widget, in container coordinates
		/// </param>
		/// <remarks>
		/// The widget is placed in the canvas area, in the specified coordinates and
		/// using the specified size
		/// </remarks>
		public void AddChild (Widget widget, Rectangle bounds)
		{
			if (positions != null)
				positions = new Dictionary<Widget,Rectangle> ();
			var bk = (IWidgetBackend)Widget.GetBackend (widget);
			Backend.AddChild (bk, bounds);
			RegisterChild (widget);
			OnPreferredSizeChanged ();
		}
		
		/// <summary>
		/// Removes a child widget
		/// </summary>
		/// <param name='widget'>
		/// The widget to remove. The widget must have been previously added using AddChild.
		/// </param>
		/// <exception cref="System.ArgumentException">If the widget is not a child of this canvas</exception>
		public void RemoveChild (Widget widget)
		{
			if (positions != null)
				positions.Remove (widget);
			Backend.RemoveChild ((IWidgetBackend)Widget.GetBackend (widget));
			UnregisterChild (widget);
			OnPreferredSizeChanged ();
		}
		
		/// <summary>
		/// Sets the position and size of a child widget
		/// </summary>
		/// <param name='widget'>
		/// The child widget. It must have been added using the AddChild method.
		/// </param>
		/// <param name='bounds'>
		/// New bounds, in container coordinates
		/// </param>
		/// <exception cref="System.ArgumentException">If the widget is not a child of this canvas</exception>
		public void SetChildBounds (Widget widget, Rectangle bounds)
		{
			Backend.SetChildBounds ((IWidgetBackend)Widget.GetBackend (widget), bounds);
			OnPreferredSizeChanged ();
		}
		
		/// <summary>
		/// Gets the children.
		/// </summary>
		/// <value>
		/// The children.
		/// </value>
		public IEnumerable<Widget> Children {
			get { return ((IWidgetSurface)this).Children; }
		}
		
		/// <summary>
		/// Gets the bounds of a child widget
		/// </summary>
		/// <returns>
		/// The child bounds, in container coordinates
		/// </returns>
		/// <param name='widget'>
		/// The widget
		/// </param>
		/// <exception cref="System.ArgumentException">If the widget is not a child of this canvas</exception>
		public Rectangle GetChildBounds (Widget widget)
		{
			Rectangle rect;
			if (positions.TryGetValue (widget, out rect))
				return rect;
			return Rectangle.Zero;
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new ICanvasBackend Backend {
			get { return (ICanvasBackend) base.Backend; }
		}
		
		/// <summary>
		/// Invalidates and forces the redraw of the whole area of the widget
		/// </summary>
		public void QueueDraw ()
		{
			Backend.QueueDraw ();
		}

		/// <summary>
		/// Invalidates and forces the redraw of a rectangular area of the widget
		/// </summary>
		/// <param name='rect'>
		/// Area to invalidate
		/// </param>
		public void QueueDraw (Rectangle rect)
		{
			Backend.QueueDraw (rect);
		}

		/// <summary>
		/// Called when the widget needs to be redrawn
		/// </summary>
		/// <param name='ctx'>
		/// Drawing context
		/// </param>
		protected virtual void OnDraw (Context ctx)
		{
		}
		
		/// <summary>
		/// Bounds of the area where content can be drawn
		/// </summary>
		/// <value>
		/// The bounds.
		/// </value>
		public Rectangle Bounds {
			get {
				var s = Size;
				return new Rectangle (0, 0, s.Width, s.Height); 
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Size MinSize {
			get { return minSize; }
			set {
				minSize = value;
				if (naturalSize.Width < minSize.Width)
					naturalSize.Width = minSize.Width;
				if (naturalSize.Height < minSize.Height)
					naturalSize.Height = minSize.Height;
				OnPreferredSizeChanged ();
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Size NaturalSize {
			get { return naturalSize; }
			set {
				naturalSize = value;
				if (minSize.Width > naturalSize.Width)
					minSize.Width = naturalSize.Width;
				if (minSize.Height > naturalSize.Height)
					minSize.Height = naturalSize.Height;
				OnPreferredSizeChanged ();
			}
		}
		
		protected override WidgetSize OnGetPreferredWidth ()
		{
			return new WidgetSize (minSize.Width);
		}
		
		protected override WidgetSize OnGetPreferredHeight ()
		{
			return new WidgetSize (minSize.Height);
		}
		
		protected override WidgetSize OnGetPreferredHeightForWidth (double width)
		{
			return OnGetPreferredHeight ();
		}
		
		protected override WidgetSize OnGetPreferredWidthForHeight (double height)
		{
			return OnGetPreferredWidth ();
		}
		
		protected override void OnPreferredSizeChanged ()
		{
			base.OnPreferredSizeChanged ();
			Backend.OnPreferredSizeChanged ();
		}
	}
}

