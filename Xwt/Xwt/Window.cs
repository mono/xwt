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
	[BackendType (typeof(IWindowBackend))]
	public class Window: WindowFrame
	{
		Widget child;
		WidgetSpacing padding;
		Menu mainMenu;
		bool shown;

		protected new class WindowBackendHost: WindowFrame.WindowBackendHost
		{
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WindowBackendHost ();
		}
		
		public Window ()
		{
			Padding = 12;
		}
		
		IWindowBackend Backend {
			get { return (IWindowBackend) BackendHost.Backend; } 
		}

		public WindowLocation InitialLocation {
			get { return initialLocation; }
			set { initialLocation = value; }
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

		void UpdatePadding ()
		{
			Backend.SetPadding (padding.Left, padding.Top, padding.Right, padding.Bottom);
		}

		public Menu MainMenu {
			get {
				return mainMenu;
			}
			set {
				mainMenu = value;
				Backend.SetMainMenu ((IMenuBackend)BackendHost.ToolkitEngine.GetSafeBackend (mainMenu));
			}
		}
		
		public Widget Content {
			get {
				return child;
			}
			set {
				if (child != null)
					child.SetParentWindow (null);
				this.child = value;
				child.SetParentWindow (this);
				Backend.SetChild ((IWidgetBackend)BackendHost.ToolkitEngine.GetSafeBackend (child));
				if (!BackendHost.EngineBackend.HandlesSizeNegotiation)
					Widget.QueueWindowSizeNegotiation (this);
			}
		}
		
		protected override void OnReallocate ()
		{
			if (child != null && !BackendHost.EngineBackend.HandlesSizeNegotiation) {
				child.Surface.Reallocate ();
			}
		}

		bool widthSet;
		bool heightSet;
		bool locationSet;
		Rectangle initialBounds;
		WindowLocation initialLocation = WindowLocation.CenterParent;

		internal override void SetBackendSize (double width, double height)
		{
			if (shown)
				base.SetBackendSize (width, height);
			else {
				if (width != -1) {
					initialBounds.Width = width;
					widthSet = true;
				}
				if (height != -1) {
					heightSet = true;
					initialBounds.Height = height;
				}
			}
		}

		internal override void SetBackendLocation (double x, double y)
		{
			if (shown || BackendHost.EngineBackend.HandlesSizeNegotiation)
				base.SetBackendLocation (x, y);
			if (!shown) {
				locationSet = true;
				initialBounds.Location = new Point (x, y);
			}
		}

		internal override Rectangle BackendBounds
		{
			get
			{
				return shown || BackendHost.EngineBackend.HandlesSizeNegotiation ? base.BackendBounds : initialBounds;
			}
			set
			{
				if (shown || BackendHost.EngineBackend.HandlesSizeNegotiation)
					base.BackendBounds = value;
				if (!shown) {
					widthSet = heightSet = locationSet = true;
					initialBounds = value;
				}
			}
		}

		internal void OnChildPlacementChanged (Widget child)
		{
			Backend.UpdateChildPlacement (child.GetBackend ());
			if (!BackendHost.EngineBackend.HandlesSizeNegotiation)
				Widget.QueueWindowSizeNegotiation (this);
		}

		internal override void AdjustSize ()
		{
			Size mMinSize, mDecorationsSize;
			Backend.GetMetrics (out mMinSize, out mDecorationsSize);

			var size = shown ? Size : initialBounds.Size;

			var wc = (shown || widthSet) ? SizeConstraint.WithSize (Math.Max (size.Width - padding.HorizontalSpacing - mDecorationsSize.Width, mMinSize.Width)) : SizeConstraint.Unconstrained;
			var hc = (shown || heightSet) ? SizeConstraint.WithSize (Math.Max (size.Height - padding.VerticalSpacing - mDecorationsSize.Height, mMinSize.Height)) : SizeConstraint.Unconstrained;

			var ws = mDecorationsSize;
			if (child != null) {
				IWidgetSurface s = child.Surface;
				ws += s.GetPreferredSize (wc, hc, true);
			}
			ws.Width += padding.HorizontalSpacing;
			ws.Height += padding.VerticalSpacing;

			if (!shown) {
				if (!widthSet)
					size.Width = ws.Width;
				if (!heightSet)
					size.Height = ws.Height;
			}

			if (ws.Width < mMinSize.Width)
				ws.Width = mMinSize.Width;
			if (ws.Height < mMinSize.Height)
				ws.Height = mMinSize.Height;

			if (ws.Width > size.Width)
				size.Width = ws.Width;
			if (ws.Height > size.Height)
				size.Height = ws.Height;

			if (!shown) {
				shown = true;

				if (!locationSet && initialLocation != WindowLocation.Manual) {
					Point center;
					if (initialLocation == WindowLocation.CenterScreen || TransientFor == null)
						center = Desktop.PrimaryScreen.VisibleBounds.Center;
					else
						center = TransientFor.ScreenBounds.Center;
					initialBounds.X = Math.Round (center.X - size.Width / 2);
					initialBounds.Y = Math.Round (center.Y - size.Height / 2);
					locationSet = true;
				}
	
				if (size != Size) {
					if (locationSet)
						Backend.Bounds = new Rectangle (initialBounds.X, initialBounds.Y, size.Width, size.Height);
					else
						Backend.SetSize (size.Width, size.Height);
				} else if (locationSet && !shown)
					Backend.Move (initialBounds.X, initialBounds.Y);
	
			} else {
				if (size != Size)
					Backend.SetSize (size.Width, size.Height);
			}
			Backend.SetMinSize (new Size (ws.Width, ws.Height));
		}
	}
}

