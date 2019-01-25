// 
// WidgetBackend.cs
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
using Xwt;
using System.Collections.Generic;
using System.Linq;

using Xwt.Drawing;
using System.Runtime.InteropServices;

namespace Xwt.GtkBackend
{
	public partial class WidgetBackend: IWidgetBackend, IGtkWidgetBackend
	{
		Gtk.Widget widget;
		Widget frontend;
		Gtk.EventBox eventBox;
		IWidgetEventSink eventSink;
		WidgetEvent enabledEvents;
		bool destroyed;

		bool minSizeSet;
		
		class DragDropData
		{
			public TransferDataSource CurrentDragData;
			public Gdk.DragAction DestDragAction;
			public Gdk.DragAction SourceDragAction;
			public int DragDataRequests;
			public TransferDataStore DragData;
			public bool DragDataForMotion;
			public Gtk.TargetEntry[] ValidDropTypes;
			public Point LastDragPosition;
		}
		
		DragDropData dragDropInfo;

		const WidgetEvent dragDropEvents = WidgetEvent.DragDropCheck | WidgetEvent.DragDrop | WidgetEvent.DragOver | WidgetEvent.DragOverCheck;

		void IBackend.InitializeBackend (object frontend, ApplicationContext context)
		{
			this.frontend = (Widget) frontend;
			ApplicationContext = context;
		}
		
		void IWidgetBackend.Initialize (IWidgetEventSink sink)
		{
			eventSink = sink;
			Initialize ();
		}
		
		public virtual void Initialize ()
		{
		}
		
		public IWidgetEventSink EventSink {
			get { return eventSink; }
		}
		
		public Widget Frontend {
			get { return frontend; }
		}

		public ApplicationContext ApplicationContext {
			get;
			private set;
		}

		public object NativeWidget {
			get {
				return RootWidget;
			}
		}
		
		public Gtk.Widget Widget {
			get { return widget; }
			set {
				if (widget != null) {
					value.Visible = widget.Visible;
					GtkEngine.ReplaceChild (widget, value);
				}
				widget = value;
			}
		}
		
		public Gtk.Widget RootWidget {
			get {
				return eventBox ?? (Gtk.Widget) Widget;
			}
		}

		public string Name {
			get { return Widget.Name; }
			set { Widget.Name = value; }
		}
		
		public virtual bool Visible {
			get { return Widget.Visible; }
			set {
				Widget.Visible = value; 
				if (eventBox != null)
					eventBox.Visible = value;
			}
		}

		void RunWhenRealized (Action a)
		{
			if (Widget.IsRealized)
				a ();
			else {
				EventHandler h = null;
				h = delegate {
					a ();
				};
				EventsRootWidget.Realized += h;
			}
		}

		public virtual bool Sensitive {
			get { return Widget.Sensitive; }
			set {
				Widget.Sensitive = value;
				if (eventBox != null)
					eventBox.Sensitive = value;
			}
		}
		
		public bool CanGetFocus {
			get { return Widget.CanFocus; }
			set { Widget.CanFocus = value; }
		}
		
		public bool HasFocus {
			get { return Widget.IsFocus; }
		}
		
		public virtual void SetFocus ()
		{
			if (CanGetFocus)
				Widget.GrabFocus ();
		}

		public string TooltipText {
			get {
				return Widget.TooltipText;
			}
			set {
				Widget.TooltipText = value;
			}
		}
		
		static Dictionary<CursorType,Gdk.Cursor> gtkCursors = new Dictionary<CursorType, Gdk.Cursor> ();
		
		Gdk.Cursor gdkCursor;
		internal CursorType CurrentCursor { get; private set; }

		public void SetCursor (CursorType cursor)
		{
			AllocEventBox ();
			CurrentCursor = cursor;
			Gdk.Cursor gc;
			if (!gtkCursors.TryGetValue (cursor, out gc)) {
				Gdk.CursorType ctype;
				if (cursor == CursorType.Arrow)
					ctype = Gdk.CursorType.LeftPtr;
				else if (cursor == CursorType.Crosshair)
					ctype = Gdk.CursorType.Crosshair;
				else if (cursor == CursorType.Hand)
					ctype = Gdk.CursorType.Hand1;
				else if (cursor == CursorType.Hand2 || cursor == CursorType.DragCopy)
					ctype = Gdk.CursorType.Hand2;
				else if (cursor == CursorType.IBeam)
					ctype = Gdk.CursorType.Xterm;
				else if (cursor == CursorType.ResizeDown)
					ctype = Gdk.CursorType.BottomSide;
				else if (cursor == CursorType.ResizeUp)
					ctype = Gdk.CursorType.TopSide;
				else if (cursor == CursorType.ResizeLeft)
					ctype = Gdk.CursorType.LeftSide;
				else if (cursor == CursorType.ResizeRight)
					ctype = Gdk.CursorType.RightSide;
				else if (cursor == CursorType.ResizeLeftRight)
					ctype = Gdk.CursorType.SbHDoubleArrow;
				else if (cursor == CursorType.ResizeUpDown)
					ctype = Gdk.CursorType.SbVDoubleArrow;
				else if (cursor == CursorType.ResizeNE)
					ctype = Gdk.CursorType.TopRightCorner;
				else if (cursor == CursorType.ResizeNW)
					ctype = Gdk.CursorType.TopLeftCorner;
				else if (cursor == CursorType.ResizeSE)
					ctype = Gdk.CursorType.BottomRightCorner;
				else if (cursor == CursorType.ResizeSW)
					ctype = Gdk.CursorType.BottomLeftCorner;
				else if (cursor == CursorType.NotAllowed)
					ctype = Gdk.CursorType.XCursor;
				else if (cursor == CursorType.Move)
					ctype = Gdk.CursorType.Fleur;
				else if (cursor == CursorType.Wait)
					ctype = Gdk.CursorType.Watch;
				else if (cursor == CursorType.Help)
					ctype = Gdk.CursorType.QuestionArrow;
				else if (cursor == CursorType.Invisible)
					ctype = (Gdk.CursorType)(-2); // Gdk.CursorType.None, since Gtk 2.16
				else
					ctype = Gdk.CursorType.Arrow;
				
				gtkCursors [cursor] = gc = new Gdk.Cursor (ctype);
			}

			gdkCursor = gc;

			// subscribe mouse entered/leaved events, when widget gets/is realized
			RunWhenRealized(SubscribeCursorEnterLeaveEvent);

			if (immediateCursorChange) // if realized and mouse inside set immediatly
				EventsRootWidget.GdkWindow.Cursor = gdkCursor;
		}

		bool cursorEnterLeaveSubscribed, immediateCursorChange;
		void SubscribeCursorEnterLeaveEvent ()
		{
			if (!cursorEnterLeaveSubscribed) {
				cursorEnterLeaveSubscribed = true; // subscribe only once
				EventsRootWidget.AddEvents ((int)Gdk.EventMask.EnterNotifyMask);
				EventsRootWidget.AddEvents ((int)Gdk.EventMask.LeaveNotifyMask);
				EventsRootWidget.EnterNotifyEvent += (o, args) => {
					immediateCursorChange = true;
					if (gdkCursor != null) ((Gtk.Widget)o).GdkWindow.Cursor = gdkCursor;
				};
				EventsRootWidget.LeaveNotifyEvent += (o, args) => {
					immediateCursorChange = false;
					((Gtk.Widget)o).GdkWindow.Cursor = null;
				};
			}
		}
		
		~WidgetBackend ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (Widget != null && disposing && !destroyed) {
				MarkDestroyed (Frontend);
				Widget.Destroy ();
			}
			if (IMContext != null)
				IMContext.Dispose ();
		}

		void MarkDestroyed (Widget w)
		{
			var wbk = Toolkit.GetBackend (w);
			var bk = wbk as WidgetBackend;
			if (bk == null) {
				var ew = wbk as Xwt.Widget;
				if (ew == null)
					return;
				bk = Toolkit.GetBackend (ew) as WidgetBackend;
				if (bk == null)
					return;
			}
			bk.destroyed = true;
			foreach (var c in w.Surface.Children)
				MarkDestroyed (c);
		}

		public Size Size {
			get {
				return new Size (Widget.Allocation.Width, Widget.Allocation.Height);
			}
		}

		public void SetSizeConstraints (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
		}
		
		DragDropData DragDropInfo {
			get {
				if (dragDropInfo == null)
					dragDropInfo = new DragDropData ();
				return dragDropInfo;
			}
		}

		public Point ConvertToParentCoordinates (Point widgetCoordinates)
		{
			int x = 0, y = 0;
			if (RootWidget?.Parent != null)
				Widget.TranslateCoordinates (RootWidget.Parent, x, y, out x, out y);
			return new Point (x + widgetCoordinates.X, y + widgetCoordinates.Y);
		}

		public Point ConvertToWindowCoordinates (Point widgetCoordinates)
		{
			int x = 0, y = 0;
			if (RootWidget?.Toplevel != null)
				Widget.TranslateCoordinates (RootWidget.Toplevel, x, y, out x, out y);
			return new Point (x + widgetCoordinates.X, y + widgetCoordinates.Y);
		}
		
		public Point ConvertToScreenCoordinates (Point widgetCoordinates)
		{
			if (Widget.ParentWindow == null)
				return Point.Zero;
			int x, y;
			Widget.ParentWindow.GetOrigin (out x, out y);
			var a = Widget.Allocation;
			x += a.X;
			y += a.Y;
			return new Point (x + widgetCoordinates.X, y + widgetCoordinates.Y);
		}
		
		Pango.FontDescription customFont;
		
		public virtual object Font {
			get {
				return customFont ?? Widget.Style.FontDescription;
			}
			set {
				var fd = (Pango.FontDescription) value;
				customFont = fd;
				Widget.ModifyFont (fd);
			}
		}
		
		Color? customBackgroundColor;
		
		public virtual Color BackgroundColor {
			get {
				return customBackgroundColor.HasValue ? customBackgroundColor.Value : Widget.Style.Background (Gtk.StateType.Normal).ToXwtValue ();
			}
			set {
				customBackgroundColor = value;
				AllocEventBox (visibleWindow: true);
				OnSetBackgroundColor (value);
			}
		}
		
		public virtual bool UsingCustomBackgroundColor {
			get { return customBackgroundColor.HasValue; }
		}
		
		Gtk.Widget IGtkWidgetBackend.Widget {
			get { return RootWidget; }
		}
		
		protected virtual Gtk.Widget EventsRootWidget {
			get { return eventBox ?? Widget; }
		}

		bool needsEventBox = true; // require event box by default
		protected virtual bool NeedsEventBox {
			get { return needsEventBox; }
			set { needsEventBox = value; }
		}
		
		public static Gtk.Widget GetWidget (IWidgetBackend w)
		{
			return w != null ? ((IGtkWidgetBackend)w).Widget : null;
		}

		public virtual void UpdateChildPlacement (IWidgetBackend childBackend)
		{
			SetChildPlacement (childBackend);
		}

		public static Gtk.Widget GetWidgetWithPlacement (IWidgetBackend childBackend)
		{
			var backend = (WidgetBackend)childBackend;
			var child = backend.RootWidget;
			var wrapper = child.Parent as WidgetPlacementWrapper;
			if (wrapper != null)
				return wrapper;

			if (!NeedsAlignmentWrapper (backend.Frontend))
				return child;

			wrapper = new WidgetPlacementWrapper ();
			wrapper.UpdatePlacement (backend.Frontend);
			wrapper.Show ();
			wrapper.Add (child);
			return wrapper;
		}

		public static void RemoveChildPlacement (Gtk.Widget w)
		{
			if (w == null)
				return;
			if (w is WidgetPlacementWrapper) {
				var wp = (WidgetPlacementWrapper)w;
				wp.Remove (wp.Child);
			}
		}

		static bool NeedsAlignmentWrapper (Widget fw)
		{
			return fw.HorizontalPlacement != WidgetPlacement.Fill || fw.VerticalPlacement != WidgetPlacement.Fill || fw.Margin.VerticalSpacing != 0 || fw.Margin.HorizontalSpacing != 0;
		}

		public static void SetChildPlacement (IWidgetBackend childBackend)
		{
			var backend = (WidgetBackend)childBackend;
			var child = backend.RootWidget;
			var wrapper = child.Parent as WidgetPlacementWrapper;
			var fw = backend.Frontend;

			if (!NeedsAlignmentWrapper (fw)) {
				if (wrapper != null) {
					wrapper.Remove (child);
					GtkEngine.ReplaceChild (wrapper, child);
				}
				return;
			}

			if (wrapper == null) {
				wrapper = new WidgetPlacementWrapper ();
				wrapper.Show ();
				GtkEngine.ReplaceChild (child, wrapper);
				wrapper.Add (child);
			}
			wrapper.UpdatePlacement (fw);
		}
		
		public virtual void UpdateLayout ()
		{
			Widget.QueueResize ();

			if (!Widget.IsRealized) {
				// This is a workaround to a GTK bug. When a widget is inside a ScrolledWindow, sometimes the QueueResize call on
				// the widget is ignored if the widget is not realized.
				var p = Widget.Parent;
				while (p != null && !(p is Gtk.ScrolledWindow))
					p = p.Parent;
				if (p != null)
					p.QueueResize ();
			}
		}
		
		protected void AllocEventBox (bool visibleWindow = false)
		{
			// Wraps the widget with an event box. Required for some
			// widgets such as Label which doesn't have its own gdk window

			if (visibleWindow) {
				if (eventBox != null)
					eventBox.VisibleWindow = true;
				else if (EventsRootWidget is Gtk.EventBox)
					((Gtk.EventBox)EventsRootWidget).VisibleWindow = true;
			}

			if (!NeedsEventBox) return;

			if (eventBox == null && !EventsRootWidget.GetHasWindow()) {
				if (EventsRootWidget is Gtk.EventBox) {
					((Gtk.EventBox)EventsRootWidget).VisibleWindow = visibleWindow;
					return;
				}
				eventBox = new Gtk.EventBox ();
				eventBox.Visible = Widget.Visible;
				eventBox.Sensitive = Widget.Sensitive;
				eventBox.VisibleWindow = visibleWindow;
				GtkEngine.ReplaceChild (Widget, eventBox);
				eventBox.Add (Widget);
			}
		}
		
		public virtual void EnableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				WidgetEvent ev = (WidgetEvent) eventId;
				switch (ev) {
				case WidgetEvent.DragLeave:
					AllocEventBox ();
					EventsRootWidget.DragLeave += HandleWidgetDragLeave;
					break;
				case WidgetEvent.DragStarted:
					AllocEventBox ();
					EventsRootWidget.DragBegin += HandleWidgetDragBegin;
					break;
				case WidgetEvent.KeyPressed:
					Widget.KeyPressEvent += HandleKeyPressEvent;
					break;
				case WidgetEvent.KeyReleased:
					Widget.KeyReleaseEvent += HandleKeyReleaseEvent;
					break;
				case WidgetEvent.GotFocus:
					EventsRootWidget.AddEvents ((int)Gdk.EventMask.FocusChangeMask);
					Widget.FocusGrabbed += HandleWidgetFocusInEvent;
					break;
				case WidgetEvent.LostFocus:
					EventsRootWidget.AddEvents ((int)Gdk.EventMask.FocusChangeMask);
					Widget.FocusOutEvent += HandleWidgetFocusOutEvent;
					break;
				case WidgetEvent.MouseEntered:
					AllocEventBox ();
					EventsRootWidget.AddEvents ((int)Gdk.EventMask.EnterNotifyMask);
					EventsRootWidget.EnterNotifyEvent += HandleEnterNotifyEvent;
					break;
				case WidgetEvent.MouseExited:
					AllocEventBox ();
					EventsRootWidget.AddEvents ((int)Gdk.EventMask.LeaveNotifyMask);
					EventsRootWidget.LeaveNotifyEvent += HandleLeaveNotifyEvent;
					break;
				case WidgetEvent.ButtonPressed:
					AllocEventBox ();
					EventsRootWidget.AddEvents ((int)Gdk.EventMask.ButtonPressMask);
					EventsRootWidget.ButtonPressEvent += HandleButtonPressEvent;
					break;
				case WidgetEvent.ButtonReleased:
					AllocEventBox ();
					EventsRootWidget.AddEvents ((int)Gdk.EventMask.ButtonReleaseMask);
					EventsRootWidget.ButtonReleaseEvent += HandleButtonReleaseEvent;
					break;
				case WidgetEvent.MouseMoved:
					AllocEventBox ();
					EventsRootWidget.AddEvents ((int)Gdk.EventMask.PointerMotionMask);
					EventsRootWidget.MotionNotifyEvent += HandleMotionNotifyEvent;
					break;
				case WidgetEvent.BoundsChanged:
					Widget.SizeAllocated += HandleWidgetBoundsChanged;
					break;
				case WidgetEvent.MouseScrolled:
					AllocEventBox();
					EventsRootWidget.AddEvents ((int)Gdk.EventMask.ScrollMask);
					Widget.ScrollEvent += HandleScrollEvent;
					break;
				case WidgetEvent.TextInput:
					if (EditableWidget != null) {
						EditableWidget.TextInserted += HandleTextInserted;
					} else {
						RunWhenRealized (delegate {
							if (IMContext == null) {
								IMContext = new Gtk.IMMulticontext ();
								IMContext.ClientWindow = EventsRootWidget.GdkWindow;
								IMContext.Commit += HandleImCommitEvent;
							}
						});
						Widget.KeyPressEvent += HandleTextInputKeyPressEvent;
						Widget.KeyReleaseEvent += HandleTextInputKeyReleaseEvent;
					}
					break;
				}
				if ((ev & dragDropEvents) != 0 && (enabledEvents & dragDropEvents) == 0) {
					// Enabling a drag&drop event for the first time
					AllocEventBox ();
					EventsRootWidget.DragDrop += HandleWidgetDragDrop;
					EventsRootWidget.DragMotion += HandleWidgetDragMotion;
					EventsRootWidget.DragDataReceived += HandleWidgetDragDataReceived;
				}
				if ((ev & WidgetEvent.PreferredSizeCheck) != 0) {
					EnableSizeCheckEvents ();
				}
				enabledEvents |= ev;
			}
		}
		
		public virtual void DisableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				WidgetEvent ev = (WidgetEvent) eventId;
				switch (ev) {
				case WidgetEvent.DragLeave:
					EventsRootWidget.DragLeave -= HandleWidgetDragLeave;
					break;
				case WidgetEvent.DragStarted:
					EventsRootWidget.DragBegin -= HandleWidgetDragBegin;
					break;
				case WidgetEvent.KeyPressed:
					Widget.KeyPressEvent -= HandleKeyPressEvent;
					break;
				case WidgetEvent.KeyReleased:
					Widget.KeyReleaseEvent -= HandleKeyReleaseEvent;
					break;
				case WidgetEvent.GotFocus:
					Widget.FocusInEvent -= HandleWidgetFocusInEvent;
					break;
				case WidgetEvent.LostFocus:
					Widget.FocusOutEvent -= HandleWidgetFocusOutEvent;
					break;
				case WidgetEvent.MouseEntered:
					EventsRootWidget.EnterNotifyEvent -= HandleEnterNotifyEvent;
					break;
				case WidgetEvent.MouseExited:
					EventsRootWidget.LeaveNotifyEvent -= HandleLeaveNotifyEvent;
					break;
				case WidgetEvent.ButtonPressed:
					if (!EventsRootWidget.IsRealized)
						EventsRootWidget.Events &= ~Gdk.EventMask.ButtonPressMask;
					EventsRootWidget.ButtonPressEvent -= HandleButtonPressEvent;
					break;
				case WidgetEvent.ButtonReleased:
					if (!EventsRootWidget.IsRealized)
						EventsRootWidget.Events &= Gdk.EventMask.ButtonReleaseMask;
					EventsRootWidget.ButtonReleaseEvent -= HandleButtonReleaseEvent;
					break;
				case WidgetEvent.MouseMoved:
					if (!EventsRootWidget.IsRealized)
						EventsRootWidget.Events &= Gdk.EventMask.PointerMotionMask;
					EventsRootWidget.MotionNotifyEvent -= HandleMotionNotifyEvent;
					break;
				case WidgetEvent.BoundsChanged:
					Widget.SizeAllocated -= HandleWidgetBoundsChanged;
					break;
				case WidgetEvent.MouseScrolled:
					if (!EventsRootWidget.IsRealized)
						EventsRootWidget.Events &= ~Gdk.EventMask.ScrollMask;
					Widget.ScrollEvent -= HandleScrollEvent;
					break;
				case WidgetEvent.TextInput:
					if (EditableWidget != null) {
						EditableWidget.TextInserted -= HandleTextInserted;
					} else {
						if (IMContext != null)
							IMContext.Commit -= HandleImCommitEvent;
						Widget.KeyPressEvent -= HandleTextInputKeyPressEvent;
						Widget.KeyReleaseEvent -= HandleTextInputKeyReleaseEvent;
					}
					break;
				}
				
				enabledEvents &= ~ev;
				
				if ((ev & dragDropEvents) != 0 && (enabledEvents & dragDropEvents) == 0) {
					// All drag&drop events have been disabled
					EventsRootWidget.DragDrop -= HandleWidgetDragDrop;
					EventsRootWidget.DragMotion -= HandleWidgetDragMotion;
					EventsRootWidget.DragDataReceived -= HandleWidgetDragDataReceived;
				}
				if ((ev & WidgetEvent.PreferredSizeCheck) != 0) {
					DisableSizeCheckEvents ();
				}
				if ((ev & WidgetEvent.GotFocus) == 0 && (enabledEvents & WidgetEvent.LostFocus) == 0 && !EventsRootWidget.IsRealized) {
					EventsRootWidget.Events &= ~Gdk.EventMask.FocusChangeMask;
				}
			}
		}

		Gdk.Rectangle lastAllocation;
		void HandleWidgetBoundsChanged (object o, Gtk.SizeAllocatedArgs args)
		{
			if (Widget.Allocation != lastAllocation) {
				lastAllocation = Widget.Allocation;
				ApplicationContext.InvokeUserCode (EventSink.OnBoundsChanged);
			}
		}


		[GLib.ConnectBefore]
		void HandleKeyReleaseEvent (object o, Gtk.KeyReleaseEventArgs args)
		{
			KeyEventArgs kargs = GetKeyReleaseEventArgs (args);
			if (kargs == null)
				return;
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnKeyReleased (kargs);
			});
			if (kargs.Handled)
				args.RetVal = true;
		}

		protected virtual KeyEventArgs GetKeyReleaseEventArgs (Gtk.KeyReleaseEventArgs args)
		{
			Key k = (Key)args.Event.KeyValue;
			ModifierKeys m = ModifierKeys.None;
			if ((args.Event.State & Gdk.ModifierType.ShiftMask) != 0)
				m |= ModifierKeys.Shift;
			if ((args.Event.State & Gdk.ModifierType.ControlMask) != 0)
				m |= ModifierKeys.Control;
			if ((args.Event.State & Gdk.ModifierType.Mod1Mask) != 0)
				m |= ModifierKeys.Alt;

			return new KeyEventArgs (k, (int)args.Event.KeyValue, m, false, (long)args.Event.Time);
		}

		[GLib.ConnectBefore]
		void HandleKeyPressEvent (object o, Gtk.KeyPressEventArgs args)
		{
			KeyEventArgs kargs = GetKeyPressEventArgs (args);
			if (kargs == null)
				return;
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnKeyPressed (kargs);
			});
			if (kargs.Handled)
				args.RetVal = true;
		}

		protected virtual KeyEventArgs GetKeyPressEventArgs (Gtk.KeyPressEventArgs args)
		{
			Key k = (Key)args.Event.KeyValue;
			ModifierKeys m = args.Event.State.ToXwtValue ();

			return new KeyEventArgs (k, (int)args.Event.KeyValue, m, false, (long)args.Event.Time);
		}

		protected Gtk.IMContext IMContext { get; set; }

		[GLib.ConnectBefore]
		void HandleTextInserted (object o, Gtk.TextInsertedArgs args)
		{
			if (String.IsNullOrEmpty (args.GetText ()))
				return;

			var pargs = new TextInputEventArgs (args.GetText ());
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnTextInput (pargs);
			});

			if (pargs.Handled)
				((GLib.Object)o).StopSignal ("insert-text");
		}

		[GLib.ConnectBefore]
		void HandleTextInputKeyReleaseEvent (object o, Gtk.KeyReleaseEventArgs args)
		{
			if (IMContext != null)
				IMContext.FilterKeypress (args.Event);
		}

		[GLib.ConnectBefore]
		void HandleTextInputKeyPressEvent (object o, Gtk.KeyPressEventArgs args)
		{
			if (IMContext != null)
				IMContext.FilterKeypress (args.Event);

			// new lines are not triggered by im, handle them here
			if (args.Event.Key == Gdk.Key.Return ||
			    args.Event.Key == Gdk.Key.ISO_Enter ||
			    args.Event.Key == Gdk.Key.KP_Enter) {
				var pargs = new TextInputEventArgs (Environment.NewLine);
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnTextInput (pargs);
				});
			}
		}

		[GLib.ConnectBefore]
		void HandleImCommitEvent (object o, Gtk.CommitArgs args)
		{
			if (String.IsNullOrEmpty (args.Str))
				return;

			var pargs = new TextInputEventArgs (args.Str);
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnTextInput (pargs);
			});

			if (pargs.Handled)
				args.RetVal = true;
		}

		[GLib.ConnectBefore]
		void HandleScrollEvent(object o, Gtk.ScrollEventArgs args)
		{
			var a = GetScrollEventArgs (args);
			if (a == null)
				return;
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnMouseScrolled(a);
			});
			if (a.Handled)
			args.RetVal = true;
		}

		protected virtual MouseScrolledEventArgs GetScrollEventArgs (Gtk.ScrollEventArgs args)
		{
			var direction = args.Event.Direction.ToXwtValue ();
			var pointer_coords = EventsRootWidget.CheckPointerCoordinates (args.Event.Window, args.Event.X, args.Event.Y);
			return new MouseScrolledEventArgs ((long) args.Event.Time, pointer_coords.X, pointer_coords.Y, direction);
		}

		[GLib.ConnectBefore]
		void HandleWidgetFocusOutEvent (object o, Gtk.FocusOutEventArgs args)
		{
			ApplicationContext.InvokeUserCode (EventSink.OnLostFocus);
		}

		[GLib.ConnectBefore]
		void HandleWidgetFocusInEvent (object o, EventArgs args)
		{
			if (!CanGetFocus)
				return;
			ApplicationContext.InvokeUserCode (EventSink.OnGotFocus);
		}

		[GLib.ConnectBefore]
		void HandleLeaveNotifyEvent (object o, Gtk.LeaveNotifyEventArgs args)
		{
			if (args.Event.Detail == Gdk.NotifyType.Inferior)
				return;
			ApplicationContext.InvokeUserCode (EventSink.OnMouseExited);
		}

		[GLib.ConnectBefore]
		void HandleEnterNotifyEvent (object o, Gtk.EnterNotifyEventArgs args)
		{
			if (args.Event.Detail == Gdk.NotifyType.Inferior)
				return;
			ApplicationContext.InvokeUserCode (EventSink.OnMouseEntered);
		}

		protected virtual void OnEnterNotifyEvent (Gtk.EnterNotifyEventArgs args) {}

		[GLib.ConnectBefore]
		void HandleMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			var a = GetMouseMovedEventArgs (args);
			if (a == null)
				return;
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnMouseMoved (a);
			});
			if (a.Handled)
				args.RetVal = true;
		}

		protected virtual MouseMovedEventArgs GetMouseMovedEventArgs (Gtk.MotionNotifyEventArgs args)
		{
			var pointer_coords = EventsRootWidget.CheckPointerCoordinates (args.Event.Window, args.Event.X, args.Event.Y);
			return new MouseMovedEventArgs ((long) args.Event.Time, pointer_coords.X, pointer_coords.Y);
		}

		[GLib.ConnectBefore]
		void HandleButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			var a = GetButtonReleaseEventArgs (args);
			if (a == null)
				return;
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnButtonReleased (a);
			});
			if (a.Handled)
				args.RetVal = true;
		}

		protected virtual ButtonEventArgs GetButtonReleaseEventArgs (Gtk.ButtonReleaseEventArgs args)
		{
			var a = new ButtonEventArgs ();

			var pointer_coords = EventsRootWidget.CheckPointerCoordinates (args.Event.Window, args.Event.X, args.Event.Y);
			a.X = pointer_coords.X;
			a.Y = pointer_coords.Y;

			a.Button = (PointerButton) args.Event.Button;
			return a;
		}

		[GLib.ConnectBeforeAttribute]
		void HandleButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			var a = GetButtonPressEventArgs (args);
			if (a == null)
				return;
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnButtonPressed (a);
			});
			if (a.Handled)
				args.RetVal = true;
		}

		protected virtual ButtonEventArgs GetButtonPressEventArgs (Gtk.ButtonPressEventArgs args)
		{
			var a = new ButtonEventArgs ();

			var pointer_coords = EventsRootWidget.CheckPointerCoordinates (args.Event.Window, args.Event.X, args.Event.Y);
			a.X = pointer_coords.X;
			a.Y = pointer_coords.Y;

			a.Button = (PointerButton) args.Event.Button;
			if (args.Event.Type == Gdk.EventType.TwoButtonPress)
				a.MultiplePress = 2;
			else if (args.Event.Type == Gdk.EventType.ThreeButtonPress)
				a.MultiplePress = 3;
			else
				a.MultiplePress = 1;

			a.IsContextMenuTrigger = args.Event.TriggersContextMenu ();
			return a;
		}
		
		[GLib.ConnectBefore]
		void HandleWidgetDragMotion (object o, Gtk.DragMotionArgs args)
		{
			args.RetVal = DoDragMotion (args.Context, args.X, args.Y, args.Time);
		}
		
		internal bool DoDragMotion (Gdk.DragContext context, int x, int y, uint time)
		{
			DragDropInfo.LastDragPosition = new Point (x, y);
			
			DragDropAction ac;
			if ((enabledEvents & WidgetEvent.DragOverCheck) == 0) {
				if ((enabledEvents & WidgetEvent.DragOver) != 0)
					ac = DragDropAction.Default;
				else
					ac = ConvertDragAction (DragDropInfo.DestDragAction);
			}
			else {
				// This is a workaround to what seems to be a mac gtk bug.
				// Suggested action is set to all when no control key is pressed
				var cact = ConvertDragAction (context.Actions);
				if (cact == DragDropAction.All)
					cact = DragDropAction.Move;

				var target = Gtk.Drag.DestFindTarget (EventsRootWidget, context, null);
				var targetTypes = Util.GetDragTypes (new Gdk.Atom[] { target });
				DragOverCheckEventArgs da = new DragOverCheckEventArgs (new Point (x, y), targetTypes, cact);
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnDragOverCheck (da);
				});
				ac = da.AllowedAction;
				if ((enabledEvents & WidgetEvent.DragOver) == 0 && ac == DragDropAction.Default)
					ac = DragDropAction.None;
			}
			
			if (ac == DragDropAction.None) {
				OnSetDragStatus (context, x, y, time, (Gdk.DragAction)0);
				return true;
			}
			else if (ac == DragDropAction.Default) {
				// Undefined, we need more data
				QueryDragData (context, time, true);
				return true;
			}
			else {
//				Gtk.Drag.Highlight (Widget);
				OnSetDragStatus (context, x, y, time, ConvertDragAction (ac));
				return true;
			}
		}
		
		[GLib.ConnectBefore]
		void HandleWidgetDragDrop (object o, Gtk.DragDropArgs args)
		{
			args.RetVal = DoDragDrop (args.Context, args.X, args.Y, args.Time);
		}
		
		internal bool DoDragDrop (Gdk.DragContext context, int x, int y, uint time)
		{
			DragDropInfo.LastDragPosition = new Point (x, y);
			var cda = ConvertDragAction (context.GetSelectedAction());

			DragDropResult res;
			if ((enabledEvents & WidgetEvent.DragDropCheck) == 0) {
				if ((enabledEvents & WidgetEvent.DragDrop) != 0)
					res = DragDropResult.None;
				else
					res = DragDropResult.Canceled;
			}
			else {
				DragCheckEventArgs da = new DragCheckEventArgs (new Point (x, y), Util.GetDragTypes (context.ListTargets ()), cda);
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnDragDropCheck (da);
				});
				res = da.Result;
				if ((enabledEvents & WidgetEvent.DragDrop) == 0 && res == DragDropResult.None)
					res = DragDropResult.Canceled;
			}
			if (res == DragDropResult.Canceled) {
				Gtk.Drag.Finish (context, false, false, time);
				return true;
			}
			else if (res == DragDropResult.Success) {
				Gtk.Drag.Finish (context, true, cda == DragDropAction.Move, time);
				return true;
			}
			else {
				// Undefined, we need more data
				QueryDragData (context, time, false);
				return true;
			}
		}

		void HandleWidgetDragLeave (object o, Gtk.DragLeaveArgs args)
		{
			ApplicationContext.InvokeUserCode (delegate {
				eventSink.OnDragLeave (EventArgs.Empty);
			});
		}
		
		void QueryDragData (Gdk.DragContext ctx, uint time, bool isMotionEvent)
		{
			DragDropInfo.DragDataForMotion = isMotionEvent;
			DragDropInfo.DragData = new TransferDataStore ();
			DragDropInfo.DragDataRequests = DragDropInfo.ValidDropTypes.Length;
			foreach (var t in DragDropInfo.ValidDropTypes) {
				var at = Gdk.Atom.Intern (t.Target, true);
				Gtk.Drag.GetData (EventsRootWidget, ctx, at, time);
			}
		}
		
		void HandleWidgetDragDataReceived (object o, Gtk.DragDataReceivedArgs args)
		{
			args.RetVal = DoDragDataReceived (args.Context, args.X, args.Y, args.SelectionData, args.Info, args.Time);
		}
		
		internal bool DoDragDataReceived (Gdk.DragContext context, int x, int y, Gtk.SelectionData selectionData, uint info, uint time)
		{
			if (DragDropInfo.DragDataRequests == 0) {
				// Got the data without requesting it. Create the datastore here
				DragDropInfo.DragData = new TransferDataStore ();
				DragDropInfo.LastDragPosition = new Point (x, y);
				DragDropInfo.DragDataRequests = 1;
			}
			
			DragDropInfo.DragDataRequests--;

			// If multiple drag/drop data types are supported, we need to iterate through them all and
			// append the data. If one of the supported data types is not found, there's no need to
			// bail out. We must raise the event
			Util.GetSelectionData (ApplicationContext, selectionData, DragDropInfo.DragData);

			if (DragDropInfo.DragDataRequests == 0) {
				if (DragDropInfo.DragDataForMotion) {
					// If no specific action is set, it means that no key has been pressed.
					// In that case, use Move or Copy or Link as default (when allowed, in this order).
					var cact = ConvertDragAction (context.Actions);
					if (cact != DragDropAction.Copy && cact != DragDropAction.Move && cact != DragDropAction.Link) {
						if (cact.HasFlag (DragDropAction.Move))
							cact = DragDropAction.Move;
						else if (cact.HasFlag (DragDropAction.Copy))
							cact = DragDropAction.Copy;
						else if (cact.HasFlag (DragDropAction.Link))
							cact = DragDropAction.Link;
						else
							cact = DragDropAction.None;
					}

					DragOverEventArgs da = new DragOverEventArgs (DragDropInfo.LastDragPosition, DragDropInfo.DragData, cact);
					ApplicationContext.InvokeUserCode (delegate {
						EventSink.OnDragOver (da);
					});
					OnSetDragStatus (context, (int)DragDropInfo.LastDragPosition.X, (int)DragDropInfo.LastDragPosition.Y, time, ConvertDragAction (da.AllowedAction));
					return true;
				}
				else {
					// Use Context.Action here since that's the action selected in DragOver
					var cda = ConvertDragAction (context.GetSelectedAction());
					DragEventArgs da = new DragEventArgs (DragDropInfo.LastDragPosition, DragDropInfo.DragData, cda);
					ApplicationContext.InvokeUserCode (delegate {
						EventSink.OnDragDrop (da);
					});
					Gtk.Drag.Finish (context, da.Success, cda == DragDropAction.Move, time);
					return true;
				}
			} else
				return false;
		}
		
		protected virtual void OnSetDragStatus (Gdk.DragContext context, int x, int y, uint time, Gdk.DragAction action)
		{
			Gdk.Drag.Status (context, action, time);
		}
		
		void HandleWidgetDragBegin (object o, Gtk.DragBeginArgs args)
		{
			// If SetDragSource has not been called, ignore the event
			if (DragDropInfo.SourceDragAction == default (Gdk.DragAction))
				return;

			DragStartData sdata = null;
			ApplicationContext.InvokeUserCode (delegate {
				sdata = EventSink.OnDragStarted ();
			});
			
			if (sdata == null)
				return;
			
			DragDropInfo.CurrentDragData = sdata.Data;
			
			if (sdata.ImageBackend != null) {
				var gi = (GtkImage)sdata.ImageBackend;
				var img = gi.ToPixbuf (ApplicationContext, Widget);
				Gtk.Drag.SetIconPixbuf (args.Context, img, (int)sdata.HotX, (int)sdata.HotY);
			}
			
			HandleDragBegin (null, args);
		}
		
		class IconInitializer
		{
			public Gdk.Pixbuf Image;
			public double HotX, HotY;
			public Gtk.Widget Widget;
			
			public static void Init (Gtk.Widget w, Gdk.Pixbuf image, double hotX, double hotY)
			{
				IconInitializer ii = new WidgetBackend.IconInitializer ();
				ii.Image = image;
				ii.HotX = hotX;
				ii.HotY = hotY;
				ii.Widget = w;
				w.DragBegin += ii.Begin;
			}
			
			void Begin (object o, Gtk.DragBeginArgs args)
			{
				Gtk.Drag.SetIconPixbuf (args.Context, Image, (int)HotX, (int)HotY);
				Widget.DragBegin -= Begin;
			}
		}
		
		public void DragStart (DragStartData sdata)
		{
			AllocEventBox ();
			Gdk.DragAction action = ConvertDragAction (sdata.DragAction);
			DragDropInfo.CurrentDragData = sdata.Data;
			EventsRootWidget.DragBegin += HandleDragBegin;
			if (sdata.ImageBackend != null) {
				var img = ((GtkImage)sdata.ImageBackend).ToPixbuf (ApplicationContext, Widget);
				IconInitializer.Init (EventsRootWidget, img, sdata.HotX, sdata.HotY);
			}
			Gtk.Drag.Begin (EventsRootWidget, Util.BuildTargetTable (sdata.Data.DataTypes), action, 1, Gtk.Global.CurrentEvent ?? new Gdk.Event (IntPtr.Zero));
		}

		void HandleDragBegin (object o, Gtk.DragBeginArgs args)
		{
			EventsRootWidget.DragEnd += HandleWidgetDragEnd;
			EventsRootWidget.DragFailed += HandleDragFailed;
			EventsRootWidget.DragDataDelete += HandleDragDataDelete;
			EventsRootWidget.DragDataGet += HandleWidgetDragDataGet;
		}
		
		void HandleWidgetDragDataGet (object o, Gtk.DragDataGetArgs args)
		{
			Util.SetDragData (DragDropInfo.CurrentDragData, args);
		}

		void HandleDragFailed (object o, Gtk.DragFailedArgs args)
		{
			Console.WriteLine ("FAILED");
		}
		
		void HandleDragDataDelete (object o, Gtk.DragDataDeleteArgs args)
		{
			DoDragaDataDelete ();
		}
		
		internal void DoDragaDataDelete ()
		{
			FinishDrag (true);
		}
		
		void HandleWidgetDragEnd (object o, Gtk.DragEndArgs args)
		{
			FinishDrag (false);
		}
		
		void FinishDrag (bool delete)
		{
			EventsRootWidget.DragEnd -= HandleWidgetDragEnd;
			EventsRootWidget.DragDataGet -= HandleWidgetDragDataGet;
			EventsRootWidget.DragFailed -= HandleDragFailed;
			EventsRootWidget.DragDataDelete -= HandleDragDataDelete;
			EventsRootWidget.DragBegin -= HandleDragBegin; // This event is subscribed only when manualy starting a drag
			ApplicationContext.InvokeUserCode (delegate {
				eventSink.OnDragFinished (new DragFinishedEventArgs (delete));
			});
		}
		
		public void SetDragTarget (TransferDataType[] types, DragDropAction dragAction)
		{
			DragDropInfo.DestDragAction = ConvertDragAction (dragAction);
			var table = Util.BuildTargetTable (types);
			DragDropInfo.ValidDropTypes = (Gtk.TargetEntry[]) table;
			OnSetDragTarget (DragDropInfo.ValidDropTypes, DragDropInfo.DestDragAction);
		}
		
		protected virtual void OnSetDragTarget (Gtk.TargetEntry[] table, Gdk.DragAction actions)
		{
			AllocEventBox ();
			Gtk.Drag.DestSet (EventsRootWidget, Gtk.DestDefaults.Highlight, table, actions);
		}
		
		public void SetDragSource (TransferDataType[] types, DragDropAction dragAction)
		{
			AllocEventBox ();
			DragDropInfo.SourceDragAction = ConvertDragAction (dragAction);
			var table = Util.BuildTargetTable (types);
			OnSetDragSource (Gdk.ModifierType.Button1Mask, (Gtk.TargetEntry[]) table, DragDropInfo.SourceDragAction);
		}
		
		protected virtual void OnSetDragSource (Gdk.ModifierType modifierType, Gtk.TargetEntry[] table, Gdk.DragAction actions)
		{
			Gtk.Drag.SourceSet (EventsRootWidget, modifierType, table, actions);
		}
		
		Gdk.DragAction ConvertDragAction (DragDropAction dragAction)
		{
			Gdk.DragAction action = (Gdk.DragAction)0;
			if ((dragAction & DragDropAction.Copy) != 0)
				action |= Gdk.DragAction.Copy;
			if ((dragAction & DragDropAction.Move) != 0)
				action |= Gdk.DragAction.Move;
			if ((dragAction & DragDropAction.Link) != 0)
				action |= Gdk.DragAction.Link;
			return action;
		}
		
		DragDropAction ConvertDragAction (Gdk.DragAction dragAction)
		{
			DragDropAction action = (DragDropAction)0;
			if ((dragAction & Gdk.DragAction.Copy) != 0)
				action |= DragDropAction.Copy;
			if ((dragAction & Gdk.DragAction.Move) != 0)
				action |= DragDropAction.Move;
			if ((dragAction & Gdk.DragAction.Link) != 0)
				action |= DragDropAction.Link;
			return action;
		}
	}
	
	public interface IGtkWidgetBackend
	{
		Gtk.Widget Widget { get; }
	}

	class WidgetPlacementWrapper: Gtk.Alignment, IConstraintProvider
	{
		public WidgetPlacementWrapper (): base (0, 0, 1, 1)
		{
		}

		public void UpdatePlacement (Xwt.Widget widget)
		{
			LeftPadding = (uint)widget.MarginLeft;
			RightPadding = (uint)widget.MarginRight;
			TopPadding = (uint)widget.MarginTop;
			BottomPadding = (uint)widget.MarginBottom;
			Xalign = (float) widget.HorizontalPlacement.GetValue ();
			Yalign = (float) widget.VerticalPlacement.GetValue ();
			Xscale = (widget.HorizontalPlacement == WidgetPlacement.Fill) ? 1 : 0;
			Yscale = (widget.VerticalPlacement == WidgetPlacement.Fill) ? 1 : 0;
		}

		#region IConstraintProvider implementation

		public void GetConstraints (Gtk.Widget target, out SizeConstraint width, out SizeConstraint height)
		{
			if (Parent is IConstraintProvider)
				((IConstraintProvider)Parent).GetConstraints (this, out width, out height);
			else
				width = height = SizeConstraint.Unconstrained;
		}

		#endregion
	}
}

