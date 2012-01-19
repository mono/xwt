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
			
			public void OnBoundsChanged ()
			{
				((Canvas)Parent).OnBoundsChanged ();
			}
		}
		
		public Canvas ()
		{
		}
		
		public void AddChild (Widget w)
		{
			AddChild (w, 0, 0);
		}
		
		public void AddChild (Widget w, double x, double y)
		{
			var ws = w as IWidgetSurface;
			var pw = ws.GetPreferredWidth ().NaturalSize;
			AddChild (w, new Rectangle (x, y, pw, ws.GetPreferredHeightForWidth (pw).NaturalSize));
		}
		
		public void AddChild (Widget w, Rectangle rect)
		{
			if (positions != null)
				positions = new Dictionary<Widget,Rectangle> ();
			var bk = (IWidgetBackend)Widget.GetBackend (w);
			Backend.AddChild (bk);
			Backend.SetChildBounds (bk, rect);
			RegisterChild (w);
			OnPreferredSizeChanged ();
		}
		
		public void RemoveChild (Widget w)
		{
			if (positions != null)
				positions.Remove (w);
			Backend.RemoveChild ((IWidgetBackend)Widget.GetBackend (w));
			UnregisterChild (w);
			OnPreferredSizeChanged ();
		}
		
		public void SetChildBounds (Widget w, Rectangle rect)
		{
			Backend.SetChildBounds ((IWidgetBackend)Widget.GetBackend (w), rect);
			OnPreferredSizeChanged ();
		}
		
		public IEnumerable<Widget> Children {
			get { return ((IWidgetSurface)this).Children; }
		}
		
		public Rectangle GetChildBounds (Widget w)
		{
			Rectangle rect;
			if (positions.TryGetValue (w, out rect))
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
		
		public void QueueDraw ()
		{
			Backend.QueueDraw ();
		}
		
		public void QueueDraw (Rectangle rect)
		{
			Backend.QueueDraw (rect);
		}
		
		protected virtual void OnDraw (Context ctx)
		{
		}
		
		protected virtual void OnBoundsChanged ()
		{
		}
		
		public Rectangle Bounds {
			get { return Backend.Bounds; }
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

