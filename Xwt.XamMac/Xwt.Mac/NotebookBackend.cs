// 
// NotebookBackend.cs
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

using AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class NotebookBackend: ViewBackend<NSTabView,IWidgetEventSink>, INotebookBackend
	{
		public NotebookBackend ()
		{
			ViewObject = new TabView ();
			Widget.AutoresizesSubviews = true;
		}
		
		public override void EnableEvent (object eventId)
		{
			if (eventId is NotebookEvent) {
				NotebookEvent ev = (NotebookEvent) eventId;
				if (ev == NotebookEvent.CurrentTabChanged) {
					Widget.WillSelect += HandleWidgetWillSelect;
				}
			}
			base.EnableEvent (eventId);
		}
		
		public override void DisableEvent (object eventId)
		{
			if (eventId is NotebookEvent) {
				NotebookEvent ev = (NotebookEvent) eventId;
				if (ev == NotebookEvent.CurrentTabChanged) {
					Widget.WillSelect -= HandleWidgetWillSelect;
				}
			}
			base.DisableEvent (eventId);
		}

		void HandleWidgetWillSelect (object sender, NSTabViewItemEventArgs e)
		{
			((INotebookEventSink)EventSink).OnCurrentTabChanged ();
		}

		#region INotebookBackend implementation
		public void Add (IWidgetBackend widget, NotebookTab tab)
		{
			NSTabViewItem item = new NSTabViewItem ();
			item.Label = tab.Label;
			item.View = GetWidgetWithPlacement (widget);
			Widget.Add (item);
		}

		public void Remove (IWidgetBackend widget)
		{
			var v = GetWidgetWithPlacement (widget);
			var t = FindTab (v);
			if (t != null) {
				Widget.Remove (t);
				RemoveChildPlacement (t.View);
			}
		}
		
		public void UpdateLabel (NotebookTab tab, string hint)
		{
			IWidgetBackend widget = (IWidgetBackend) Toolkit.GetBackend (tab.Child);
			var v = GetWidget (widget);
			var t = FindTab (v);
			if (t != null)
				t.Label = tab.Label;
		}
		
		public int CurrentTab {
			get {
				return (int) Widget.IndexOf (Widget.Selected);
			}
			set {
				Widget.SelectAt (value);
			}
		}

		public Xwt.NotebookTabOrientation TabOrientation {
			get {
				NotebookTabOrientation tabPos = NotebookTabOrientation.Top;
				switch (Widget.TabViewType) {
				case NSTabViewType.NSBottomTabsBezelBorder:
					tabPos = NotebookTabOrientation.Bottom;
					break;
				case NSTabViewType.NSLeftTabsBezelBorder:
					tabPos = NotebookTabOrientation.Left;
					break;
				case NSTabViewType.NSRightTabsBezelBorder:
					tabPos = NotebookTabOrientation.Right;
					break;
				}
				return tabPos;
			}
			set {
				NSTabViewType type = NSTabViewType.NSTopTabsBezelBorder;
				switch (value) {
				case NotebookTabOrientation.Bottom:
					type = NSTabViewType.NSBottomTabsBezelBorder;
					break;
				case NotebookTabOrientation.Left:
					type = NSTabViewType.NSLeftTabsBezelBorder;
					break;
				case NotebookTabOrientation.Right:
					type = NSTabViewType.NSRightTabsBezelBorder;
					break;
				}
				Widget.TabViewType = type;
			}
		}
		#endregion
		
		NSTabViewItem FindTab (NSView v)
		{
			foreach (var t in Widget.Items) {
				if (t.View == v)
					return t;
			}
			return null;
		}
	}
	
	class TabView: NSTabView, IViewObject
	{
		public ViewBackend Backend { get; set; }
		public NSView View {
			get { return this; }
		}
	}
}

