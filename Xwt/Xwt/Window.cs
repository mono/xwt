// 
// Window.cs
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

namespace Xwt
{
	public class Window: Widget
	{
		Widget child;
		EventHandler boundsChanged;
		Rectangle bounds;
		Menu mainMenu;
		
		protected new class EventSink: Widget.EventSink, IWindowEventSink
		{
			public void OnBoundsChanged (Rectangle bounds)
			{
				((Window)Parent).OnBoundsChanged (new BoundsChangedEventArgs () { Bounds = bounds });
			}
		}
		
		public Window ()
		{
			Margin.SetAll (6);
		}
		
		public Window (string title): this ()
		{
			Backend.Title = title;
		}
		
		new IWindowBackend Backend {
			get { return (IWindowBackend) base.Backend; }
		}
		
		protected override void OnBackendCreated ()
		{
			base.OnBackendCreated ();
			bounds = Backend.Bounds;
			Backend.EnableEvent (WindowEvent.BoundsChanged);
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		public void Add (Widget child)
		{
			if (child == null)
				throw new ArgumentNullException ("child");
			if (this.child != null)
				throw new InvalidOperationException ("The window already has a child");
			this.child = child;
			RegisterChild (child);
			Backend.SetChild ((IWidgetBackend)GetBackend (child));
			AdjustSize ();
			((IWidgetSurface)this).Reallocate ();
		}
		
		void AdjustSize ()
		{
			IWidgetSurface s = child;
			var w = s.GetPreferredWidth ().MinSize + Margin.Left + Margin.Right;
			if (w > Width)
				Width = w;
			var h = s.GetPreferredHeightForWidth (Width).MinSize + Margin.Top + Margin.Bottom;
			if (h > Height)
				Height = h;
		}
		
		public Rectangle Bounds {
			get {
				LoadBackend();
				return bounds;
			}
			set {
				Backend.Bounds = value;
			}
		}
		
		public double X {
			get { return Bounds.X; }
			set { Bounds = new Xwt.Rectangle (value, Y, Width, Height); }
		}
		
		public double Y {
			get { return Bounds.Y; }
			set { Bounds = new Xwt.Rectangle (X, value, Width, Height); }
		}
		
		public double Width {
			get { return Bounds.Width; }
			set { Bounds = new Xwt.Rectangle (X, Y, value, Height); }
		}
		
		public double Height {
			get { return Bounds.Height; }
			set { Bounds = new Xwt.Rectangle (X, Y, Width, value); }
		}
		
		public Size Size {
			get { return Bounds.Size; }
			set { Bounds = new Rectangle (X, Y, value.Width, value.Height); }
		}
		
		public Point Location {
			get { return Bounds.Location; }
			set { Bounds = new Rectangle (value.X, value.Y, Width, Height); }
		}
		
		
		public void Remove (Widget child)
		{
			if (this.child == child) {
				UnregisterChild (child);
				this.child = null;
				Backend.SetChild ((IWidgetBackend)GetBackend (child));
			}
		}
		
		public Widget Child {
			get { return child; }
		}
		
		public string Title {
			get { return Backend.Title; }
			set { Backend.Title = value; }
		}
		
		public Menu MainMenu {
			get {
				return mainMenu;
			}
			set {
				mainMenu = value;
				Backend.SetMainMenu ((IMenuBackend)GetBackend (mainMenu));
			}
		}
		
		protected override WidgetSize OnGetPreferredWidth ()
		{
			WidgetSize s;
			if (child != null)
				s = ((IWidgetSurface)child).GetPreferredWidth ();
			else
				s = new WidgetSize ();
			s.MinSize += Margin.Left + Margin.Right;
			s.NaturalSize += Margin.Left + Margin.Right;
			return s;
		}
		
		protected override WidgetSize OnGetPreferredHeight ()
		{
			WidgetSize s;
			if (child != null)
				s = ((IWidgetSurface)child).GetPreferredHeight ();
			else
				s = new WidgetSize ();
			s.MinSize += Margin.Top + Margin.Bottom;
			s.NaturalSize += Margin.Top + Margin.Bottom;
			return s;
		}
		
		protected override WidgetSize OnGetPreferredHeightForWidth (double width)
		{
			WidgetSize s;
			if (child != null)
				s = ((IWidgetSurface)child).GetPreferredHeightForWidth (width);
			else
				s = new WidgetSize ();
			s.MinSize += Margin.Top + Margin.Bottom;
			s.NaturalSize += Margin.Top + Margin.Bottom;
			return s;
		}
		
		protected override WidgetSize OnGetPreferredWidthForHeight (double height)
		{
			WidgetSize s;
			if (child != null)
				s = ((IWidgetSurface)child).GetPreferredWidthForHeight (height);
			else
				s = new WidgetSize ();
			s.MinSize += Margin.Left + Margin.Right;
			s.NaturalSize += Margin.Left + Margin.Right;
			return s;
		}
		
		protected virtual void OnBoundsChanged (BoundsChangedEventArgs a)
		{
			if (bounds != a.Bounds) {
				bounds = a.Bounds;
				((IWidgetSurface)this).Reallocate ();
				if (boundsChanged != null)
					boundsChanged (this, a);
			}
		}
		
		public event EventHandler BoundsChanged {
			add {
				boundsChanged += value;
			}
			remove {
				boundsChanged -= value;
			}
		}
	}
	
	public class BoundsChangedEventArgs: EventArgs
	{
		public Rectangle Bounds { get; set; }
	}
}

