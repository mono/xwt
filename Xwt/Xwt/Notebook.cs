// 
// Notebook.cs
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
	[BackendType (typeof(INotebookBackend))]
	public class Notebook: Widget
	{
		ChildrenCollection<NotebookTab> tabs;
		EventHandler currentTabChanged;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, INotebookEventSink, ICollectionEventSink<NotebookTab>, IContainerEventSink<NotebookTab>
		{
			public void AddedItem (NotebookTab item, int index)
			{
				((Notebook)Parent).OnAdd (item);
			}
			
			public void RemovedItem (NotebookTab item, int index)
			{
				((Notebook)Parent).OnRemove (item.Child);
			}
			
			public void ChildChanged (NotebookTab child, string hint)
			{
				((Notebook)Parent).Backend.UpdateLabel (child, hint);
				((Notebook)Parent).OnPreferredSizeChanged ();
			}
			
			public void ChildReplaced (NotebookTab child, Widget oldWidget, Widget newWidget)
			{
				((Notebook)Parent).OnReplaceChild (child, oldWidget, newWidget);
			}
			
			public void OnCurrentTabChanged ()
			{
				((Notebook)Parent).OnCurrentTabChanged (EventArgs.Empty);
			}
		}
		
		static Notebook ()
		{
			MapEvent (NotebookEvent.CurrentTabChanged, typeof(Notebook), "OnCurrentTabChanged");
		}
		
		public Notebook ()
		{
			tabs = new ChildrenCollection <NotebookTab> ((WidgetBackendHost) BackendHost);
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		INotebookBackend Backend {
			get { return (INotebookBackend) BackendHost.Backend; }
		}
		
		public void Add (Widget w, string label)
		{
			NotebookTab t = new NotebookTab ((WidgetBackendHost)BackendHost, w);
			t.Label = label;
			tabs.Add (t);
		}
		
		void OnRemove (Widget child)
		{
			UnregisterChild (child);
			Backend.Remove ((IWidgetBackend)GetBackend (child));
			OnPreferredSizeChanged ();
		}
		
		void OnAdd (NotebookTab tab)
		{
			RegisterChild (tab.Child);
			Backend.Add ((IWidgetBackend)GetBackend (tab.Child), tab);
			OnPreferredSizeChanged ();
		}
		
		void OnReplaceChild (NotebookTab tab, Widget oldWidget, Widget newWidget)
		{
			if (oldWidget != null)
				OnRemove (oldWidget);
			OnAdd (tab);
		}
		
		public ChildrenCollection<NotebookTab> Tabs {
			get { return tabs; }
		}
		
		public NotebookTab CurrentTab {
			get {
				return tabs [Backend.CurrentTab];
			}
			set {
				for (int n=0; n<tabs.Count; n++) {
					if (tabs[n] == value) {
						Backend.CurrentTab = n;
						return;
					}
				}
				CurrentTabIndex = -1;
			}
		}
		
		public int CurrentTabIndex {
			get { return Backend.CurrentTab; }
			set { Backend.CurrentTab = value; }
		}

		public NotebookTabOrientation TabOrientation {
			get { return Backend.TabOrientation; }
			set { Backend.TabOrientation = value; }
		}
		
		protected virtual void OnCurrentTabChanged (EventArgs e)
		{
			if (currentTabChanged != null)
				currentTabChanged (this, e);
		}
		
		public event EventHandler CurrentTabChanged {
			add {
				BackendHost.OnBeforeEventAdd (NotebookEvent.CurrentTabChanged, currentTabChanged);
				currentTabChanged += value;
			}
			remove {
				currentTabChanged -= value;
				BackendHost.OnAfterEventRemove (NotebookEvent.CurrentTabChanged, currentTabChanged);
			}
		}
	}
	
	[ContentProperty("Child")]
	public class NotebookTab
	{
		IContainerEventSink<NotebookTab> parent;
		string label;
		Widget child;
		
		internal NotebookTab (IContainerEventSink<NotebookTab> parent, Widget child)
		{
			this.child = child;
			this.parent = parent;
		}
		
		public string Label {
			get {
				return label;
			}
			set {
				label = value;
				parent.ChildChanged (this, "Label");
			}
		}
		
		public Widget Child {
			get {
				return child;
			}
			set {
				var oldVal = child;
				child = value;
				parent.ChildReplaced (this, oldVal, value);
			}
		}
	}

	public enum NotebookTabOrientation {
		Top,
		Left,
		Right,
		Bottom
	}
}

