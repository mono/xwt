// 
// WindowBackend.cs
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

namespace Xwt.GtkBackend
{
	public class WindowBackend: WidgetBackend, IWindowBackend
	{
		Gtk.Alignment alignment;
		Gtk.MenuBar mainMenu;
		Gtk.VBox mainBox;
		
		public WindowBackend ()
		{
			Widget = new Gtk.Window ("");
			mainBox = new Gtk.VBox ();
			Widget.Add (mainBox);
			mainBox.Show ();
			alignment = new Gtk.Alignment (0, 0, 1, 1);
			mainBox.PackStart (alignment, true, true, 0);
			alignment.Show ();
		}
		
		protected new Gtk.Window Widget {
			get { return (Gtk.Window)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IWindowEventSink EventSink {
			get { return (IWindowEventSink) base.EventSink; }
		}
		
		public string Title {
			get { return Widget.Title; }
			set { Widget.Title = value; }
		}
		
		public Gtk.VBox MainBox {
			get { return mainBox; }
		}
		
		public bool Decorated {
			get {
				return Widget.Decorated;
			}
			set {
				Widget.Decorated = value;
			}
		}
		
		public bool ShowInTaskbar {
			get {
				return !Widget.SkipTaskbarHint;
			}
			set {
				Widget.SkipTaskbarHint = !value;
			}
		}
		
		public void SetChild (IWidgetBackend child)
		{
			var w = (IGtkWidgetBackend) child;
			alignment.Child = w.Widget;
		}
		
		public void SetMainMenu (IMenuBackend menu)
		{
			if (mainMenu != null)
				mainBox.Remove (mainMenu);

			MenuBackend m = (MenuBackend) menu;
			mainMenu = m.MenuBar;
			mainBox.PackStart (mainMenu, false, false, 0);
			((Gtk.Box.BoxChild)mainBox[mainMenu]).Position = 0;
		}

		#region IWindowBackend implementation
		
		public void EnableEvent (WindowEvent ev)
		{
			switch (ev) {
			case WindowEvent.BoundsChanged:
				Widget.SizeAllocated += HandleWidgetSizeAllocated; break;
			}
		}

		void HandleWidgetSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			EventSink.OnBoundsChanged (new Rectangle (args.Allocation.X, args.Allocation.Y, args.Allocation.Width, args.Allocation.Height));
		}

		public void DisableEvent (WindowEvent ev)
		{
			switch (ev) {
			case WindowEvent.BoundsChanged:
				Widget.SizeAllocated -= HandleWidgetSizeAllocated; break;
			}
		}
		
		public Rectangle Bounds {
			get {
				int w, h, x, y;
				Widget.GetPosition (out x, out y);
				Widget.GetSize (out w, out h);
				return new Rectangle (x, y, w, h);
			}
			set {
				Widget.Move ((int)value.X, (int)value.Y);
				Widget.Resize ((int)value.Width, (int)value.Height);
				Widget.SetDefaultSize ((int)value.Width, (int)value.Height);
				EventSink.OnBoundsChanged (Bounds);
			}
		}
		
		public override void UpdateLayout ()
		{
			alignment.LeftPadding = (uint) Frontend.Margin.Left;
			alignment.RightPadding = (uint) Frontend.Margin.Right;
			alignment.TopPadding = (uint) Frontend.Margin.Top;
			alignment.BottomPadding = (uint) Frontend.Margin.Bottom;
		}
		
		#endregion
	}
}

