//
// CellRendererBackend.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using Gtk;
#if XWT_GTK3
using TreeModel = Gtk.ITreeModel;
#endif

namespace Xwt.GtkBackend
{
	public class CellViewBackend: ICellViewBackend, ICellDataSource
	{
		ICellRendererTarget rendererTarget;
		object target;
		bool buttonReleaseSubscribed;

		public CellViewBackend ()
		{
		}

		public virtual void Initialize (ICellViewFrontend cellView, ICellRendererTarget rendererTarget, object target)
		{
			Frontend = cellView;
			this.rendererTarget = rendererTarget;
			this.target = target;
		}

		public ApplicationContext ApplicationContext { get; private set; }

		public ICellViewFrontend Frontend { get; private set; }

		public Gtk.CellRenderer CellRenderer {
			get;
			protected set;
		}

		public TreeModel TreeModel { get; private set; }

		public Gtk.TreeIter CurrentIter { get; private set; }

		public ICellViewEventSink EventSink { get; private set; }

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			ApplicationContext = context;
		}

		public virtual void EnableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				switch ((WidgetEvent)eventId) {
				case WidgetEvent.MouseMoved:
					rendererTarget.EventRootWidget.AddEvents ((int)Gdk.EventMask.PointerMotionMask);
					rendererTarget.EventRootWidget.MotionNotifyEvent += HandleMotionNotifyEvent;
					break;
				case WidgetEvent.ButtonPressed:
					rendererTarget.EventRootWidget.AddEvents ((int)Gdk.EventMask.ButtonPressMask);
					rendererTarget.EventRootWidget.ButtonPressEvent += HandleButtonPressEvent;
					break;
				case WidgetEvent.ButtonReleased:
					rendererTarget.EventRootWidget.AddEvents ((int)Gdk.EventMask.ButtonReleaseMask);
					rendererTarget.EventRootWidget.ButtonReleaseEvent += HandleButtonReleaseEvent;
					buttonReleaseSubscribed = true;
					break;
				}
			}
		}

		public virtual void DisableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				switch ((WidgetEvent)eventId) {
				case WidgetEvent.MouseMoved:
					rendererTarget.EventRootWidget.MotionNotifyEvent -= HandleMotionNotifyEvent;
					break;
				case WidgetEvent.ButtonPressed:
					rendererTarget.EventRootWidget.ButtonPressEvent -= HandleButtonPressEvent;
					break;
				case WidgetEvent.ButtonReleased:
					rendererTarget.EventRootWidget.ButtonReleaseEvent -= HandleButtonReleaseEvent;
					buttonReleaseSubscribed = false;
					break;
				}
			}
		}

		int pressX, pressY, pressCX, pressCY;

		[GLib.ConnectBefore]
		void HandleButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			int cx, cy;
			Gtk.TreeIter iter;
			bool captured = false;

			if (rendererTarget.PressedCell != null) {
				if (rendererTarget.PressedCell == this) {
					iter = rendererTarget.PressedIter;
					cx = pressCX + (int)args.Event.X - pressX;
					cy = pressCY + (int)args.Event.Y - pressY;
					captured = true;
				}
				else
					return;
			} else if (!rendererTarget.GetCellPosition (CellRenderer, (int)args.Event.X, (int)args.Event.Y, out cx, out cy, out iter))
				return;

			rendererTarget.PressedCell = null;
			rendererTarget.PressedIter = Gtk.TreeIter.Zero;

			if (!buttonReleaseSubscribed)
				rendererTarget.EventRootWidget.ButtonReleaseEvent -= HandleButtonReleaseEvent;

			var rect = rendererTarget.GetCellBounds (target, CellRenderer, iter);
			if (captured || rect.Contains (cx, cy)) {
				ApplicationContext.InvokeUserCode (delegate {
					LoadData (rendererTarget.Model, iter);
					var a = new ButtonEventArgs {
						X = args.Event.X,
						Y = args.Event.Y,
						Button = (PointerButton) args.Event.Button
					};
					EventSink.OnButtonReleased (a);
					if (a.Handled)
						args.RetVal = true;
				});
			}
		}

		[GLib.ConnectBefore]
		void HandleButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			int cx, cy;
			Gtk.TreeIter iter;
			if (rendererTarget.GetCellPosition (CellRenderer, (int)args.Event.X, (int)args.Event.Y, out cx, out cy, out iter)) {
				var rect = rendererTarget.GetCellBounds (target, CellRenderer, iter);
				if (rect.Contains (cx, cy)) {
					rendererTarget.PressedIter = iter;
					rendererTarget.PressedCell = this;
					pressX = (int)args.Event.X;
					pressY = (int)args.Event.Y;
					pressCX = cx;
					pressCY = cy;
					if (!buttonReleaseSubscribed) {
						rendererTarget.EventRootWidget.AddEvents ((int)Gdk.EventMask.ButtonReleaseMask);
						rendererTarget.EventRootWidget.ButtonReleaseEvent += HandleButtonReleaseEvent;
					}
					ApplicationContext.InvokeUserCode (delegate {
						LoadData (rendererTarget.Model, iter);
						var a = new ButtonEventArgs {
							X = args.Event.X,
							Y = args.Event.Y,
							Button = (PointerButton) args.Event.Button
						};
						EventSink.OnButtonPressed (a);
						if (a.Handled)
							args.RetVal = true;
					});
				}
			}
		}

		[GLib.ConnectBefore]
		void HandleMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			int cx, cy;
			Gtk.TreeIter iter;
			if (rendererTarget.GetCellPosition (CellRenderer, (int)args.Event.X, (int)args.Event.Y, out cx, out cy, out iter)) {
				var rect = rendererTarget.GetCellBounds (target, CellRenderer, iter);

				if (rect.Contains (cx, cy)) {
					ApplicationContext.InvokeUserCode (delegate {
						LoadData (rendererTarget.Model, iter);
						EventSink.OnMouseMoved (new MouseMovedEventArgs (args.Event.Time, cx, cy));
					});
				}
			}
		}

		public void LoadData (TreeModel model, Gtk.TreeIter iter)
		{
			TreeModel = model;
			CurrentIter = iter;
			EventSink = Frontend.Load (this);
			CellRenderer.Visible = Frontend.Visible;
			OnLoadData ();
		}

		protected virtual void OnLoadData ()
		{
		}

		public object GetValue (IDataField field)
		{
			return CellUtil.GetModelValue (TreeModel, CurrentIter, field.Index);
		}

		public virtual Rectangle CellBounds {
			get {
				return rendererTarget.GetCellBounds (target, CellRenderer, CurrentIter);
			}
		}

		public virtual Rectangle BackgroundBounds {
			get {
				return rendererTarget.GetCellBackgroundBounds (target, CellRenderer, CurrentIter);
			}
		}

		public virtual bool Selected {
			get {
				return false;
			}
		}

		public virtual bool HasFocus {
			get {
				return false;
			}
		}

		public void SetCurrentEventRow ()
		{
			var path = TreeModel.GetPath (CurrentIter);
			rendererTarget.SetCurrentEventRow (path.ToString ());
		}

		public void QueueDraw ()
		{
			rendererTarget.QueueDraw (target, CurrentIter);
		}
	}
}

