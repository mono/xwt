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
		
		TransferDataSource currentDragData;
		Gdk.DragAction destDragAction;
		Gdk.DragAction sourceDragAction;
		int dragDataRequests;
		TransferDataStore dragData;
		bool dragDataForMotion;
		bool minSizeSet;
		Gtk.TargetEntry[] validDropTypes;

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
		
		public virtual void Dispose (bool disposing)
		{
			if (Widget != null && !disposing && Widget.Parent == null)
				Widget.Destroy ();
		}
		
		public Size Size {
			get {
				return new Size (Widget.Allocation.Width, Widget.Allocation.Height);
			}
		}
		
		public Point ConvertToScreenCoordinates (Point widgetCoordinates)
		{
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
					Widget.DragLeave += HandleWidgetDragLeave;
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
			Toolkit.Invoke (delegate {
				EventSink.OnMouseExited ();
			});
		}

		void HandleEnterNotifyEvent (object o, Gtk.EnterNotifyEventArgs args)
		{
			Toolkit.Invoke (delegate {
				EventSink.OnMouseEntered ();
			});
		}
		
		Point lastDragPosition;
		
		void HandleWidgetDragMotion (object o, Gtk.DragMotionArgs args)
		{
			lastDragPosition = new Point (args.X, args.Y);
			
			DragDropAction ac;
			if ((enabledEvents & WidgetEvent.DragOverCheck) == 0) {
				if ((enabledEvents & WidgetEvent.DragOver) != 0)
					ac = DragDropAction.Default;
				else
					ac = ConvertDragAction (destDragAction);
			}
			else {
				DragOverCheckEventArgs da = new DragOverCheckEventArgs (new Point (args.X, args.Y), Util.GetDragTypes (args.Context.Targets), ConvertDragAction (args.Context.Actions));
				Toolkit.Invoke (delegate {
					EventSink.OnDragOverCheck (da);
				});
				ac = da.AllowedAction;
				if ((enabledEvents & WidgetEvent.DragOver) == 0 && ac == DragDropAction.Default)
					ac = DragDropAction.None;
			}
			
			if (ac == DragDropAction.None) {
				args.RetVal = true;
				Gdk.Drag.Status (args.Context, (Gdk.DragAction)0, args.Time);
			}
			else if (ac == DragDropAction.Default) {
				// Undefined, we need more data
				args.RetVal = true;
				QueryDragData (args.Context, args.Time, true);
			}
			else {
//				Gtk.Drag.Highlight (Widget);
				args.RetVal = true;
				Gdk.Drag.Status (args.Context, ConvertDragAction (ac), args.Time);
			}
		}
		
		void HandleWidgetDragDrop (object o, Gtk.DragDropArgs args)
		{
			lastDragPosition = new Point (args.X, args.Y);
			var cda = ConvertDragAction (args.Context.Action);

			DragDropResult res;
			if ((enabledEvents & WidgetEvent.DragDropCheck) == 0) {
				if ((enabledEvents & WidgetEvent.DragDrop) != 0)
					res = DragDropResult.None;
				else
					res = DragDropResult.Canceled;
			}
			else {
				DragCheckEventArgs da = new DragCheckEventArgs (new Point (args.X, args.Y), Util.GetDragTypes (args.Context.Targets), cda);
				Toolkit.Invoke (delegate {
					EventSink.OnDragDropCheck (da);
				});
				res = da.Result;
				if ((enabledEvents & WidgetEvent.DragDrop) == 0 && res == DragDropResult.None)
					res = DragDropResult.Canceled;
			}
			if (res == DragDropResult.Canceled) {
				args.RetVal = true;
				Gtk.Drag.Finish (args.Context, false, false, args.Time);
			}
			else if (res == DragDropResult.Success) {
				args.RetVal = true;
				Gtk.Drag.Finish (args.Context, true, cda == DragDropAction.Move, args.Time);
			}
			else {
				// Undefined, we need more data
				args.RetVal = true;
				QueryDragData (args.Context, args.Time, false);
			}
		}

		void HandleWidgetDragLeave (object o, Gtk.DragLeaveArgs args)
		{
//			Gtk.Drag.Unhighlight (Widget);
			Toolkit.Invoke (delegate {
				eventSink.OnDragLeave (EventArgs.Empty);
			});
		}
		
		void QueryDragData (Gdk.DragContext ctx, uint time, bool isMotionEvent)
		{
			dragDataForMotion = isMotionEvent;
			dragData = new TransferDataStore ();
			dragDataRequests = validDropTypes.Length;
			foreach (var t in validDropTypes) {
				var at = Gdk.Atom.Intern (t.Target, true);
				Gtk.Drag.GetData (Widget, ctx, at, time);
			}
		}
		
		void HandleWidgetDragDataReceived (object o, Gtk.DragDataReceivedArgs args)
		{
			dragDataRequests--;
			
			string type = Util.AtomToType (args.SelectionData.Target.Name);
			if (type == null) {
				args.RetVal = false;
				return;
			}

			if (args.SelectionData.Length > 0) {
				if (type == TransferDataType.Text)
					dragData.AddText (args.SelectionData.Text);
				else if (args.SelectionData.TargetsIncludeImage (false))
					dragData.AddImage (WidgetRegistry.CreateFrontend<Xwt.Drawing.Image> (args.SelectionData.Pixbuf));
				else if (type == TransferDataType.Uri) {
					var uris = System.Text.Encoding.UTF8.GetString (args.SelectionData.Data).Split ('\n').Where (u => !string.IsNullOrEmpty(u)).Select (u => new Uri (u)).ToArray ();
					dragData.AddUris (uris);
				}
				else
					dragData.AddValue (type, args.SelectionData.Data);
			}
			if (dragDataRequests == 0) {
				if (dragDataForMotion) {
					DragOverEventArgs da = new DragOverEventArgs (lastDragPosition, dragData, ConvertDragAction (args.Context.Actions));
					Toolkit.Invoke (delegate {
						EventSink.OnDragOver (da);
					});
					Gdk.Drag.Status (args.Context, ConvertDragAction (da.AllowedAction), args.Time);
				}
				else {
					// Use Context.Action here since that's the action selected in DragOver
					var cda = ConvertDragAction (args.Context.Action);
					DragEventArgs da = new DragEventArgs (lastDragPosition, dragData, cda);
					Toolkit.Invoke (delegate {
						EventSink.OnDragDrop (da);
					});
					Gtk.Drag.Finish (args.Context, da.Success, cda == DragDropAction.Move, args.Time);
				}
			}
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
		
		public void DragStart (TransferDataSource data, DragDropAction dragAction, object imageBackend, double hotX, double hotY)
		{
			Gdk.DragAction action = ConvertDragAction (dragAction);
			currentDragData = data;
			Widget.DragBegin += HandleDragBegin;
			IconInitializer.Init (Widget, (Gdk.Pixbuf) imageBackend, hotX, hotY);
			Gtk.Drag.Begin (Widget, Util.BuildTargetTable (data.DataTypes), action, 1, Gtk.Global.CurrentEvent);
		}

		void HandleDragBegin (object o, Gtk.DragBeginArgs args)
		{
			Widget.DragEnd += HandleWidgetDragEnd;
			Widget.DragFailed += HandleDragFailed;
			Widget.DragDataDelete += HandleDragDataDelete;
			Widget.DragDataGet += HandleWidgetDragDataGet;
		}
		
		void HandleWidgetDragDataGet (object o, Gtk.DragDataGetArgs args)
		{
			Util.SetDragData (currentDragData, args);
		}

		void HandleDragFailed (object o, Gtk.DragFailedArgs args)
		{
			Console.WriteLine ("FAILED");
		}

		void HandleDragDataDelete (object o, Gtk.DragDataDeleteArgs args)
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
			Widget.DragBegin -= HandleDragBegin;
			Toolkit.Invoke (delegate {
				eventSink.OnDragFinished (new DragFinishedEventArgs (delete));
			});
		}
		
		public void SetDragTarget (string[] types, DragDropAction dragAction)
		{
			destDragAction = ConvertDragAction (dragAction);
			var table = Util.BuildTargetTable (types);
			validDropTypes = (Gtk.TargetEntry[]) table;
			Gtk.Drag.DestSet (Widget, Gtk.DestDefaults.Highlight, validDropTypes, destDragAction);
		}
		
		public void SetDragSource (string[] types, DragDropAction dragAction)
		{
			sourceDragAction = ConvertDragAction (dragAction);
			var table = Util.BuildTargetTable (types);
			Gtk.Drag.SourceSet (Widget, (Gdk.ModifierType)0, (Gtk.TargetEntry[]) table, sourceDragAction);
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

