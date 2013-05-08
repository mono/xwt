// 
// PanedBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
	public class PanedBackend: WidgetBackend, IPanedBackend
	{
		public PanedBackend ()
		{
		}
		
		int formerPosition;
		
		protected new Gtk.Paned Widget {
			get { return (Gtk.Paned)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IPanedEventSink EventSink {
			get { return (IPanedEventSink)base.EventSink; }
		}

		public void Initialize (Orientation dir)
		{
			if (dir == Orientation.Horizontal)
				Widget = new Gtk.HPaned ();
			else
				Widget = new Gtk.VPaned ();
			Widget.Show ();
			Widget.WidgetEvent += HandleWidgetEvent;
		}
		
		public void SetPanel (int panel, IWidgetBackend widget, bool resize, bool shrink)
		{
			if (panel == 1)
				Widget.Pack1 (((WidgetBackend)widget).RootWidget, resize, shrink);
			else
				Widget.Pack2 (((WidgetBackend)widget).RootWidget, resize, shrink);
		}
		
		public void RemovePanel (int panel)
		{
			if (panel == 1)
				Widget.Remove (Widget.Child1);
			else
				Widget.Remove (Widget.Child2);
		}
		
		public void UpdatePanel (int panel, bool resize, bool shrink)
		{
			if (panel == 1) {
				var c = (Gtk.Paned.PanedChild)Widget[Widget.Child1];
				c.Resize = resize;
				c.Shrink = shrink;
			}
			else {
				var c = (Gtk.Paned.PanedChild)Widget[Widget.Child2];
				c.Resize = resize;
				c.Shrink = shrink;
			}
		}
		
		public double Position {
			get { return Widget.Position; }
			set { Widget.Position = (int) value; }
		}

		public Size GetDecorationSize ()
		{
			throw new NotSupportedException ();
		}

		public void GetPanelSizes (double totalSize, out double panel1Size, out double panel2Size)
		{
			throw new NotSupportedException ();
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is PanedEvent) {
				switch ((PanedEvent)eventId) {
				case PanedEvent.PositionChanged: Widget.WidgetEvent += HandleWidgetEvent; break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is PanedEvent) {
				switch ((PanedEvent)eventId) {
				case PanedEvent.PositionChanged: Widget.WidgetEvent -= HandleWidgetEvent; break;
				}
			}
		}
		
		void HandleWidgetEvent (object o, Gtk.WidgetEventArgs args)
		{
			if (formerPosition != (o as Gtk.Paned).Position) {
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnPositionChanged();
					formerPosition = (o as Gtk.Paned).Position;
				}
				);
			}
		}
	}
}

