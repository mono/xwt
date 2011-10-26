// 
// Paned.cs
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
using System.Windows.Markup;

namespace Xwt
{
	public class Paned: Widget
	{
		Orientation direction;
		Panel panel1;
		Panel panel2;
		
		protected new class EventSink: Widget.EventSink, IContainerEventSink<Panel>
		{
			public void ChildChanged (Panel child, string hint)
			{
				((Paned)Parent).OnChildChanged (child, hint);
			}
			
			public void ChildReplaced (Panel child, Widget oldWidget, Widget newWidget)
			{
				((Paned)Parent).OnReplaceChild (child, oldWidget, newWidget);
			}
		}
		
		internal Paned (Orientation direction)
		{
			this.direction = direction;
			panel1 = new Panel ((EventSink)WidgetEventSink, 1);
			panel2 = new Panel ((EventSink)WidgetEventSink, 2);
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new IPanedBackend Backend {
			get { return (IPanedBackend) base.Backend; }
		}
		
		public Panel Panel1 {
			get { return panel1; }
		}
		
		public Panel Panel2 {
			get { return panel2; }
		}
		
		protected override void OnBackendCreated ()
		{
			base.OnBackendCreated ();
			Backend.Initialize (direction);
		}
		
		void OnReplaceChild (Panel panel, Widget oldChild, Widget newChild)
		{
			if (oldChild != null)
				UnregisterChild (oldChild);
			if (newChild != null)
				RegisterChild (newChild);
			Backend.SetPanel (panel.NumPanel, (IWidgetBackend)GetBackend (newChild), panel);
		}
		
		public void Remove (Widget child)
		{
			if (panel1.Child == child)
				panel1.Child = null;
			else if (panel2.Child == child)
				panel2.Child = null;
		}
		
		void OnChildChanged (Panel panel, object hint)
		{
			Backend.Update (panel.NumPanel, panel);
		}
	}
	
	[ContentProperty("Child")]
	public class Panel
	{
		IContainerEventSink<Panel> parent;
		bool resize;
		bool shrink;
		int numPanel;
		Widget child;
		
		internal Panel (IContainerEventSink<Panel> parent, int numPanel)
		{
			this.parent = parent;
			this.numPanel = numPanel;
		}
		
		public bool Resize {
			get {
				return this.resize;
			}
			set {
				resize = value;
				parent.ChildChanged (this, "Resize");
			}
		}

		public bool Shrink {
			get {
				return this.shrink;
			}
			set {
				shrink = value;
				parent.ChildChanged (this, "Shrink");
			}
		}
		
		public Widget Child {
			get {
				return child;
			}
			set {
				var old = child;
				child = value;
				parent.ChildReplaced (this, old, value);
			}
		}
		
		internal int NumPanel {
			get {
				return this.numPanel;
			}
			set {
				numPanel = value;
			}
		}
	}
}

