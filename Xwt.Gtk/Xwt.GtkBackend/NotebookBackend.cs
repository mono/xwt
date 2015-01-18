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

using System;
using Xwt.Backends;


namespace Xwt.GtkBackend
{
	public class NotebookBackend: WidgetBackend, INotebookBackend
	{
		public NotebookBackend ()
		{
			Widget = new Gtk.Notebook ();
			Widget.Scrollable = true;
			Widget.Show ();
		}
		
		new Gtk.Notebook Widget {
			get { return (Gtk.Notebook)base.Widget; }
			set { base.Widget = value; }
		}

		public override void EnableEvent (object eventId)
		{
			if (eventId is NotebookEvent) {
				NotebookEvent ev = (NotebookEvent) eventId;
				if (ev == NotebookEvent.CurrentTabChanged) {
					Widget.SwitchPage += HandleWidgetSwitchPage;
				}
			}
			base.EnableEvent (eventId);
		}
		
		public override void DisableEvent (object eventId)
		{
			if (eventId is NotebookEvent) {
				NotebookEvent ev = (NotebookEvent) eventId;
				if (ev == NotebookEvent.CurrentTabChanged) {
					Widget.SwitchPage -= HandleWidgetSwitchPage;
				}
			}
			base.DisableEvent (eventId);
		}

		void HandleWidgetSwitchPage (object o, Gtk.SwitchPageArgs args)
		{
			((INotebookEventSink)EventSink).OnCurrentTabChanged ();
		}
		
		public void Add (IWidgetBackend widget, NotebookTab tab)
		{
			var childW = GetWidgetWithPlacement (widget);
			Widget.AppendPage (childW, CreateLabel (tab));
			((Gtk.Notebook.NotebookChild)(Widget [childW])).TabExpand = ExpandTabLabels;
		}

		public void Remove (IWidgetBackend widget)
		{
			Widget.Remove (GetWidgetWithPlacement (widget));
		}
		
		public void UpdateLabel (NotebookTab tab, string hint)
		{
			IWidgetBackend widget = (IWidgetBackend) Toolkit.GetBackend (tab.Child);
			Widget.SetTabLabel (GetWidget (widget), CreateLabel (tab));
		}
		
		public int CurrentTab {
			get {
				return Widget.CurrentPage;
			}
			set {
				Widget.CurrentPage = value;
			}
		}

		public Xwt.NotebookTabOrientation TabOrientation {
			get {
				Xwt.NotebookTabOrientation tabPos = Xwt.NotebookTabOrientation.Top;
				Enum.TryParse (Widget.TabPos.ToString (), out tabPos);
				return tabPos;
			}
			set {
				Gtk.PositionType tabPos = Gtk.PositionType.Top;
				Enum.TryParse (value.ToString (), out tabPos);
				Widget.TabPos = tabPos;
			}
		}

		bool expandTabLabels;
		public bool ExpandTabLabels {
			get {
				return expandTabLabels;
			}
			set {
				expandTabLabels = value;
				foreach (Gtk.Widget child in Widget.AllChildren) {
					Gtk.Notebook.NotebookChild notebook_child = Widget [child] as Gtk.Notebook.NotebookChild;
					if (notebook_child != null) {
						notebook_child.TabExpand = value;
						notebook_child.TabFill = value;
					}
				}
			}
		}
		
		Gtk.Widget CreateLabel (NotebookTab tab)
		{
			Gtk.Label label = new Gtk.Label (tab.Label);
			if (tab.Image.IsNull)
				return label;
			label.Xalign = 0;
			label.Yalign = 0.5f;
			label.Justify = Gtk.Justification.Left;
			var image = new ImageBox (ApplicationContext, tab.Image.WithDefaultSize (Gtk.IconSize.SmallToolbar));
			var image_align = new Gtk.Alignment (1, 0.5f, 0, 1);
			image_align.RightPadding = 6;
			image_align.Add (image);
			var box = new Gtk.HBox ();
			box.Add (image_align);
			box.Add (label);
			box.ShowAll ();
			return box;
		}
	}
}

