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
using System.Linq;

namespace Xwt
{
	[BackendType (typeof(ICanvasBackend))]
	public class Canvas: Widget
	{
		Dictionary<Widget,Rectangle> positions;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, ICanvasEventSink
		{
			public void OnDraw (object context, Rectangle dirtyRect)
			{
				Context ctx = null;
				try {
					ctx = new Context (context, ToolkitEngine);
					ctx.Reset (Parent);
					((Canvas)Parent).OnDraw (ctx, dirtyRect);
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
		/// <param name='w'>
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
			var s = w.Surface.GetPreferredSize (SizeConstraint.Unconstrained, SizeConstraint.Unconstrained);
			AddChild (w, new Rectangle (x, y, s.Width, s.Height));
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
			if (positions == null)
				positions = new Dictionary<Widget,Rectangle> ();
			
			positions [widget] = bounds;
			var bk = (IWidgetBackend)Widget.GetBackend (widget);
			RegisterChild (widget);
			Backend.AddChild (bk, bounds);
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
			UnregisterChild (widget);
			positions.Remove (widget);
			Backend.RemoveChild ((IWidgetBackend)Widget.GetBackend (widget));
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
			if (positions == null || !positions.ContainsKey (widget))
				throw new ArgumentException ("Widget is not a child of the canvas");
			
			positions [widget] = bounds;
			Backend.SetChildBounds ((IWidgetBackend)Widget.GetBackend (widget), bounds);
		}
		
		/// <summary>
		/// Gets the children.
		/// </summary>
		/// <value>
		/// The children.
		/// </value>
		public IEnumerable<Widget> Children {
			get { return Surface.Children; }
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
			if (positions != null && positions.TryGetValue (widget, out rect))
				return rect;
			throw new ArgumentException ("Widget is not a child of the canvas");
		}

		/// <summary>
		/// Removes all children of the Canvas
		/// </summary>
		public void Clear ()
		{
			foreach (var c in Children.ToArray ())
				RemoveChild (c);
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		ICanvasBackend Backend {
			get { return (ICanvasBackend) BackendHost.Backend; }
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
		protected virtual void OnDraw (Context ctx, Rectangle dirtyRect)
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
	}
}

