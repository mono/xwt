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
using Xwt.Engine;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public class WidgetBackend: IWidgetBackend, IGtkWidgetBackend
	{
		Gtk.Widget widget;
		Widget frontend;
		Gtk.Alignment alignment;
		Gtk.EventBox eventBox;
		IWidgetEventSink eventSink;
		WidgetEvent enabledEvents;
		
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
		const WidgetEvent sizeCheckEvents = WidgetEvent.PreferredWidthCheck | WidgetEvent.PreferredHeightCheck | WidgetEvent.PreferredHeightForWidthCheck | WidgetEvent.PreferredWidthForHeightCheck;
		
		void IBackend.Initialize (object frontend)
		{
			this.frontend = (Widget) frontend;
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
		
		public object NativeWidget {
			get {
				return Widget;
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
				return alignment ?? eventBox ?? (Gtk.Widget) Widget;
			}
		}
		
		public virtual bool Visible {
			get { return Widget.Visible; }
			set {
				Widget.Visible = value; 
				if (alignment != null)
					alignment.Visible = value;
				if (eventBox != null)
					eventBox.Visible = value;
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
			get { return Widget.HasFocus; }
		}
		
		public void SetFocus ()
		{
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
		
		public void SetCursor (CursorType cursor)
		{
			AllocEventBox ();
			Gdk.Cursor gc;
			if (!gtkCursors.TryGetValue (cursor, out gc)) {
				Gdk.CursorType ctype;
				if (cursor == CursorType.Arrow)
					ctype = Gdk.CursorType.LeftPtr;
				else if (cursor == CursorType.Crosshair)
					ctype = Gdk.CursorType.Crosshair;
				else if (cursor == CursorType.Hand)
					ctype = Gdk.CursorType.Hand1;
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
				else
					ctype = Gdk.CursorType.Arrow;
				
				gtkCursors [cursor] = gc = new Gdk.Cursor (ctype);
			}
			if (EventsRootWidget.GdkWindow == null) {
				EventHandler h = null;
				h = delegate {
					EventsRootWidget.GdkWindow.Cursor = gc;
					EventsRootWidget.Realized -= h;
				};
				EventsRootWidget.Realized += h;
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
			if (Widget != null && !disposing && Widget.Parent == null)
				Widget.Destroy ();
		}
		
		public Size Size {
			get {
				return new Size (Widget.Allocation.Width, Widget.Allocation.Height);
			}
		}
		
		DragDropData DragDropInfo {
			get {
				if (dragDropInfo == null)
					dragDropInfo = new DragDropData ();
				return dragDropInfo;
			}
		}
		
		public Point ConvertToScreenCoordinates (Point widgetCoordinates)
		{
			if (Widget.GdkWindow == null)
				return Point.Zero;
			int x, y;
			Widget.GdkWindow.GetOrigin (out x, out y);
			var a = Widget.Allocation;
			x += a.X;
			y += a.Y;
			return new Point (x + widgetCoordinates.X, y + widgetCoordinates.Y);
		}
		
		public virtual WidgetSize GetPreferredWidth ()
		{
			try {
				gettingPreferredSize = true;
				var s = new WidgetSize (Widget.SizeRequest ().Width);
				if (minSizeSet && Frontend.MinWidth != -1)
					s.MinSize = Frontend.MinWidth;
				return s;
			} finally {
				gettingPreferredSize = false;
			}
		}
		
		public virtual WidgetSize GetPreferredHeight ()
		{
			try {
				gettingPreferredSize = true;
				var s = new WidgetSize (Widget.SizeRequest ().Height);
				if (minSizeSet && Frontend.MinHeight != -1)
					s.MinSize = Frontend.MinHeight;
				return s;
			} finally {
				gettingPreferredSize = false;
			}
		}
		
		public virtual WidgetSize GetPreferredHeightForWidth (double width)
		{
			try {
				gettingPreferredSize = true;
				var s = new WidgetSize (Widget.SizeRequest ().Height);
				if (minSizeSet && Frontend.MinHeight != -1)
					s.MinSize = Frontend.MinHeight;
				return s;
			} finally {
				gettingPreferredSize = false;
			}
		}
		
		public virtual WidgetSize GetPreferredWidthForHeight (double height)
		{
			try {
				gettingPreferredSize = true;
				var s = new WidgetSize (Widget.SizeRequest ().Width);
				if (minSizeSet && Frontend.MinWidth != -1)
					s.MinSize = Frontend.MinWidth;
				return s;
			} finally {
				gettingPreferredSize = false;
			}
		}
		
		public void SetMinSize (double width, double height)
		{
			if (width != -1 || height != -1) {
				EnableSizeCheckEvents ();
				minSizeSet = true;
			}
			else {
				minSizeSet = false;
				DisableSizeCheckEvents ();
			}
		}
		
		public void SetNaturalSize (double width, double height)
		{
			// Nothing to do
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
				return customBackgroundColor.HasValue ? customBackgroundColor.Value : Util.ToXwtColor (Widget.Style.Background (Gtk.StateType.Normal));
			}
			set {
				customBackgroundColor = value;
				Widget.ModifyBg (Gtk.StateType.Normal, Util.ToGdkColor (value));
			}
		}
		
		public bool UsingCustomBackgroundColor {
			get { return customBackgroundColor.HasValue; }
		}
		
		Gtk.Widget IGtkWidgetBackend.Widget {
			get { return RootWidget; }
		}
		
		Gtk.Widget EventsRootWidget {
			get { return eventBox ?? Widget; }
		}
		
		public static Gtk.Widget GetWidget (IWidgetBackend w)
		{
			return w != null ? ((IGtkWidgetBackend)w).Widget : null;
		}
		
		public virtual void UpdateLayout ()
		{
			if (frontend.Margin.HorizontalSpacing == 0 && frontend.Margin.VerticalSpacing == 0) {
				if (alignment != null) {
					alignment.Remove (alignment.Child);
					GtkEngine.ReplaceChild (alignment, EventsRootWidget);
					alignment.Destroy ();
					alignment = null;
				}
			} else {
				if (alignment == null) {
					alignment = new Gtk.Alignment (0, 0, 1, 1);
					GtkEngine.ReplaceChild (EventsRootWidget, alignment);
					alignment.Add (EventsRootWidget);
					alignment.Visible = Widget.Visible;
				}
				alignment.LeftPadding = (uint) frontend.Margin.Left;
				alignment.RightPadding = (uint) frontend.Margin.Right;
				alignment.TopPadding = (uint) frontend.Margin.Top;
				alignment.BottomPadding = (uint) frontend.Margin.Bottom;
			}
		}
		
		void AllocEventBox ()
		{
			// Wraps the widget with an event box. Required for some
			// widgets such as Label which doesn't have its own gdk window
			
			if (eventBox == null && Widget.IsNoWindow) {
				eventBox = new Gtk.EventBox ();
				eventBox.Visible = Widget.Visible;
				eventBox.Sensitive = Widget.Sensitive;
				if (alignment != null) {
					alignment.Remove (alignment.Child);
					alignment.Add (eventBox);
				} else
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
					Widget.FocusInEvent += HandleWidgetFocusInEvent;
					break;
				case WidgetEvent.LostFocus:
					Widget.FocusOutEvent += HandleWidgetFocusOutEvent;
					break;
				case WidgetEvent.MouseEntered:
					AllocEventBox ();
					EventsRootWidget.Events |= Gdk.EventMask.EnterNotifyMask;
					EventsRootWidget.EnterNotifyEvent += HandleEnterNotifyEvent;
					break;
				case WidgetEvent.MouseExited:
					AllocEventBox ();
					EventsRootWidget.Events |= Gdk.EventMask.LeaveNotifyMask;
					EventsRootWidget.LeaveNotifyEvent += HandleLeaveNotifyEvent;
					break;
				case WidgetEvent.ButtonPressed:
					AllocEventBox ();
					EventsRootWidget.Events |= Gdk.EventMask.ButtonPressMask;
					EventsRootWidget.ButtonPressEvent += HandleButtonPressEvent;
					break;
				case WidgetEvent.ButtonReleased:
					AllocEventBox ();
					EventsRootWidget.Events |= Gdk.EventMask.ButtonReleaseMask;
					EventsRootWidget.ButtonReleaseEvent += HandleButtonReleaseEvent;
					break;
				case WidgetEvent.MouseMoved:
					AllocEventBox ();
					EventsRootWidget.Events |= Gdk.EventMask.PointerMotionMask;
					EventsRootWidget.MotionNotifyEvent += HandleMotionNotifyEvent;
					break;
				case WidgetEvent.BoundsChanged:
					Widget.SizeAllocated += HandleWidgetBoundsChanged;
					break;
				}
				if ((ev & dragDropEvents) != 0 && (enabledEvents & dragDropEvents) == 0) {
					// Enabling a drag&drop event for the first time
					Widget.DragDrop += HandleWidgetDragDrop;
					Widget.DragMotion += HandleWidgetDragMotion;
					Widget.DragDataReceived += HandleWidgetDragDataReceived;
				}
				if ((ev & sizeCheckEvents) != 0) {
					EnableSizeCheckEvents ();
				}
				enabledEvents |= ev;
			}
		}
		
		void EnableSizeCheckEvents ()
		{
			if ((enabledEvents & sizeCheckEvents) == 0 && !minSizeSet) {
				// Enabling a size request event for the first time
				Widget.SizeRequested += HandleWidgetSizeRequested;
				Widget.SizeAllocated += HandleWidgetSizeAllocated;;
			}
		}
		
		public virtual void DisableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				WidgetEvent ev = (WidgetEvent) eventId;
				switch (ev) {
				case WidgetEvent.DragLeave:
					Widget.DragLeave -= HandleWidgetDragLeave;
					break;
				case WidgetEvent.DragStarted:
					Widget.DragBegin -= HandleWidgetDragBegin;
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
					EventsRootWidget.Events &= ~Gdk.EventMask.ButtonPressMask;
					EventsRootWidget.ButtonPressEvent -= HandleButtonPressEvent;
					break;
				case WidgetEvent.ButtonReleased:
					EventsRootWidget.Events &= Gdk.EventMask.ButtonReleaseMask;
					EventsRootWidget.ButtonReleaseEvent -= HandleButtonReleaseEvent;
					break;
				case WidgetEvent.MouseMoved:
					EventsRootWidget.Events &= Gdk.EventMask.PointerMotionMask;
					EventsRootWidget.MotionNotifyEvent -= HandleMotionNotifyEvent;
					break;
				case WidgetEvent.BoundsChanged:
					Widget.SizeAllocated -= HandleWidgetBoundsChanged;
					break;
				}
				
				enabledEvents &= ~ev;
				
				if ((ev & dragDropEvents) != 0 && (enabledEvents & dragDropEvents) == 0) {
					// All drag&drop events have been disabled
					Widget.DragDrop -= HandleWidgetDragDrop;
					Widget.DragMotion -= HandleWidgetDragMotion;
					Widget.DragDataReceived -= HandleWidgetDragDataReceived;
				}
				if ((ev & sizeCheckEvents) != 0) {
					DisableSizeCheckEvents ();
				}
			}
		}
		
		void DisableSizeCheckEvents ()
		{
			if ((enabledEvents & sizeCheckEvents) == 0 && !minSizeSet) {
				// All size request events have been disabled
				Widget.SizeRequested -= HandleWidgetSizeRequested;
				Widget.SizeAllocated -= HandleWidgetSizeAllocated;;
			}
		}
		
		void HandleWidgetBoundsChanged (object o, Gtk.SizeAllocatedArgs args)
		{
			Toolkit.Invoke (delegate {
				EventSink.OnBoundsChanged ();
			});
		}
		
		enum SizeCheckStep
		{
			SizeRequest,
			PreAllocate,
			AdjustSize,
			FinalAllocate
		}
		
		SizeCheckStep sizeCheckStep = SizeCheckStep.SizeRequest;
		int realRequestedWidth;
		int realRequestedHeight;
		bool gettingPreferredSize;

		void HandleWidgetSizeRequested (object o, Gtk.SizeRequestedArgs args)
		{
			if (gettingPreferredSize)
				return;
			
			var req = args.Requisition;
			
			if ((enabledEvents & sizeCheckEvents) == 0) {
				// If no sizing event is set, it means this handler was set because there is a min size.
				if (Frontend.MinWidth != -1)
					req.Width = (int) Frontend.MinWidth;
				if (Frontend.MinHeight != -1)
					req.Height = (int) Frontend.MinHeight;
				return;
			}
			
			if (sizeCheckStep == SizeCheckStep.AdjustSize) {
				req.Width = realRequestedWidth;
				req.Height = realRequestedHeight;
				sizeCheckStep = SizeCheckStep.FinalAllocate;
			}
			else {
				Toolkit.Invoke (delegate {
					if (EventSink.GetSizeRequestMode () == SizeRequestMode.HeightForWidth) {
						if ((enabledEvents & WidgetEvent.PreferredWidthCheck) != 0) {
						    var w = eventSink.OnGetPreferredWidth ();
							req.Width = (int) w.MinSize;
						}
						if ((enabledEvents & WidgetEvent.PreferredHeightForWidthCheck) != 0) {
							req.Height = 1;
							sizeCheckStep = SizeCheckStep.PreAllocate;
						}
						else if ((enabledEvents & WidgetEvent.PreferredHeightCheck) != 0) {
							var h = eventSink.OnGetPreferredHeight ();
							req.Height = (int) h.MinSize;
							sizeCheckStep = SizeCheckStep.FinalAllocate;
						}
					} else {
						if ((enabledEvents & WidgetEvent.PreferredHeightCheck) != 0) {
							var h = eventSink.OnGetPreferredHeight ();
							req.Height = (int) h.MinSize;
						}
						if ((enabledEvents & WidgetEvent.PreferredWidthForHeightCheck) != 0) {
							req.Width = 1;
							sizeCheckStep = SizeCheckStep.PreAllocate;
						}
						else if ((enabledEvents & WidgetEvent.PreferredWidthCheck) != 0) {
						    var w = eventSink.OnGetPreferredWidth ();
							req.Width = (int) w.MinSize;
							sizeCheckStep = SizeCheckStep.FinalAllocate;
						}
					}
				});
			}
			args.Requisition = req;
		}

		void HandleWidgetSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			if ((enabledEvents & sizeCheckEvents) == 0) {
				// If no sizing event is set, it means this handler was set because there is a min size.
				// In that case, there isn't any thing left to do here
				return;
			}
			
			Toolkit.Invoke (delegate {
				if (sizeCheckStep == SizeCheckStep.SizeRequest) {
					Console.WriteLine ("SizeRequest not called. Should not happen");
				}
				else if (sizeCheckStep == SizeCheckStep.PreAllocate || sizeCheckStep == SizeCheckStep.AdjustSize) {
					if (EventSink.GetSizeRequestMode () == SizeRequestMode.HeightForWidth) {
						realRequestedWidth = args.Allocation.Width;
						Toolkit.Invoke (delegate {
							realRequestedHeight = (int) eventSink.OnGetPreferredHeightForWidth (args.Allocation.Width).MinSize;
						});
						sizeCheckStep = SizeCheckStep.AdjustSize;
						Widget.QueueResize ();
					} else {
						realRequestedHeight = args.Allocation.Height;
						Toolkit.Invoke (delegate {
							realRequestedWidth = (int) eventSink.OnGetPreferredWidthForHeight (args.Allocation.Height).MinSize;
						});
						sizeCheckStep = SizeCheckStep.AdjustSize;
						Widget.QueueResize ();
					}
				}
			});
		}

		[GLib.ConnectBefore]
		void HandleKeyReleaseEvent (object o, Gtk.KeyReleaseEventArgs args)
		{
			Key k = (Key)args.Event.KeyValue;
			ModifierKeys m = ModifierKeys.None;
			if ((args.Event.State & Gdk.ModifierType.ShiftMask) != 0)
				m |= ModifierKeys.Shift;
			if ((args.Event.State & Gdk.ModifierType.ControlMask) != 0)
				m |= ModifierKeys.Control;
			if ((args.Event.State & Gdk.ModifierType.Mod1Mask) != 0)
				m |= ModifierKeys.Alt;
			KeyEventArgs kargs = new KeyEventArgs (k, m, false, (long)args.Event.Time);
			Toolkit.Invoke (delegate {
				EventSink.OnKeyReleased (kargs);
			});
			if (kargs.IsEventCanceled)
				args.RetVal = true;
		}

		[GLib.ConnectBefore]
		void HandleKeyPressEvent (object o, Gtk.KeyPressEventArgs args)
		{
			Key k = (Key)args.Event.KeyValue;
			ModifierKeys m = ModifierKeys.None;
			if ((args.Event.State & Gdk.ModifierType.ShiftMask) != 0)
				m |= ModifierKeys.Shift;
			if ((args.Event.State & Gdk.ModifierType.ControlMask) != 0)
				m |= ModifierKeys.Control;
			if ((args.Event.State & Gdk.ModifierType.Mod1Mask) != 0)
				m |= ModifierKeys.Alt;
			KeyEventArgs kargs = new KeyEventArgs (k, m, false, (long)args.Event.Time);
			Toolkit.Invoke (delegate {
				EventSink.OnKeyPressed (kargs);
			});
			if (kargs.IsEventCanceled)
				args.RetVal = true;
		}

		void HandleWidgetFocusOutEvent (object o, Gtk.FocusOutEventArgs args)
		{
			Toolkit.Invoke (delegate {
				EventSink.OnLostFocus ();
			});
		}

		void HandleWidgetFocusInEvent (object o, Gtk.FocusInEventArgs args)
		{
			Toolkit.Invoke (delegate {
				EventSink.OnGotFocus ();
			});
		}

		void HandleLeaveNotifyEvent (object o, Gtk.LeaveNotifyEventArgs args)
		{
			if (args.Event.Detail == Gdk.NotifyType.Inferior)
				return;
			Toolkit.Invoke (delegate {
				EventSink.OnMouseExited ();
			});
		}

		void HandleEnterNotifyEvent (object o, Gtk.EnterNotifyEventArgs args)
		{
			if (args.Event.Detail == Gdk.NotifyType.Inferior)
				return;
			Toolkit.Invoke (delegate {
				EventSink.OnMouseEntered ();
			});
		}

		void HandleMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			var a = new MouseMovedEventArgs ((long) args.Event.Time, args.Event.X, args.Event.Y);
			Toolkit.Invoke (delegate {
				EventSink.OnMouseMoved (a);
			});
		}

		void HandleButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			var a = new ButtonEventArgs ();
			a.X = args.Event.X;
			a.Y = args.Event.Y;
			a.Button = (PointerButton) args.Event.Button;
			Toolkit.Invoke (delegate {
				EventSink.OnButtonReleased (a);
			});
		}

		[GLib.ConnectBeforeAttribute]
		void HandleButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			var a = new ButtonEventArgs ();
			a.X = args.Event.X;
			a.Y = args.Event.Y;
			a.Button = (PointerButton) args.Event.Button;
			if (args.Event.Type == Gdk.EventType.TwoButtonPress)
				a.MultiplePress = 2;
			else if (args.Event.Type == Gdk.EventType.ThreeButtonPress)
				a.MultiplePress = 3;
			else
				a.MultiplePress = 1;
			Toolkit.Invoke (delegate {
				EventSink.OnButtonPressed (a);
			});
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
				
				DragOverCheckEventArgs da = new DragOverCheckEventArgs (new Point (x, y), Util.GetDragTypes (context.Targets), cact);
				Toolkit.Invoke (delegate {
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
			var cda = ConvertDragAction (context.Action);

			DragDropResult res;
			if ((enabledEvents & WidgetEvent.DragDropCheck) == 0) {
				if ((enabledEvents & WidgetEvent.DragDrop) != 0)
					res = DragDropResult.None;
				else
					res = DragDropResult.Canceled;
			}
			else {
				DragCheckEventArgs da = new DragCheckEventArgs (new Point (x, y), Util.GetDragTypes (context.Targets), cda);
				Toolkit.Invoke (delegate {
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
			Toolkit.Invoke (delegate {
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
				Gtk.Drag.GetData (Widget, ctx, at, time);
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
			
			if (!Util.GetSelectionData (selectionData, DragDropInfo.DragData)) {
				return false;
			}

			if (DragDropInfo.DragDataRequests == 0) {
				if (DragDropInfo.DragDataForMotion) {
					// This is a workaround to what seems to be a mac gtk bug.
					// Suggested action is set to all when no control key is pressed
					var cact = ConvertDragAction (context.Actions);
					if (cact == DragDropAction.All)
						cact = DragDropAction.Move;
					
					DragOverEventArgs da = new DragOverEventArgs (DragDropInfo.LastDragPosition, DragDropInfo.DragData, cact);
					Toolkit.Invoke (delegate {
						EventSink.OnDragOver (da);
					});
					OnSetDragStatus (context, (int)DragDropInfo.LastDragPosition.X, (int)DragDropInfo.LastDragPosition.Y, time, ConvertDragAction (da.AllowedAction));
					return true;
				}
				else {
					// Use Context.Action here since that's the action selected in DragOver
					var cda = ConvertDragAction (context.Action);
					DragEventArgs da = new DragEventArgs (DragDropInfo.LastDragPosition, DragDropInfo.DragData, cda);
					Toolkit.Invoke (delegate {
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
			DragStartData sdata = null;
			Toolkit.Invoke (delegate {
				sdata = EventSink.OnDragStarted ();
			});
			
			if (sdata == null)
				return;
			
			DragDropInfo.CurrentDragData = sdata.Data;
			
			if (sdata.ImageBackend != null)
				Gtk.Drag.SetIconPixbuf (args.Context, (Gdk.Pixbuf) sdata.ImageBackend, (int)sdata.HotX, (int)sdata.HotY);
			
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
			Gdk.DragAction action = ConvertDragAction (sdata.DragAction);
			DragDropInfo.CurrentDragData = sdata.Data;
			Widget.DragBegin += HandleDragBegin;
			if (sdata.ImageBackend != null)
				IconInitializer.Init (Widget, (Gdk.Pixbuf) sdata.ImageBackend, sdata.HotX, sdata.HotY);
			Gtk.Drag.Begin (Widget, Util.BuildTargetTable (sdata.Data.DataTypes), action, 1, Gtk.Global.CurrentEvent);
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
			Widget.DragEnd -= HandleWidgetDragEnd;
			Widget.DragDataGet -= HandleWidgetDragDataGet;
			Widget.DragFailed -= HandleDragFailed;
			Widget.DragDataDelete -= HandleDragDataDelete;
			Widget.DragBegin -= HandleDragBegin; // This event is subscribed only when manualy starting a drag
			Toolkit.Invoke (delegate {
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
			Gtk.Drag.DestSet (Widget, Gtk.DestDefaults.Highlight, table, actions);
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
}

