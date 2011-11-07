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

namespace Xwt.GtkBackend
{
	public class WidgetBackend<T,S>: IWidgetBackend, IGtkWidgetBackend where T:Gtk.Widget where S:IWidgetEventSink
	{
		Widget frontend;
		Gtk.Alignment alignment;
		S eventSink;
		
		bool dragEventsSet;
		TransferDataSource currentDragData;
		Gdk.DragAction destDragAction;
		Gdk.DragAction sourceDragAction;
		bool dropCheckEventEnabled;
		bool dropEventEnabled;
		bool dragMotionCheckEventEnabled;
		bool dragMotionEventEnabled;
		int dragDataRequests;
		TransferDataStore dragData;
		bool dragDataForMotion;
		Gtk.TargetEntry[] validDropTypes;
		
		void IBackend.Initialize (object frontend)
		{
			this.frontend = (Widget) frontend;
		}
		
		void IWidgetBackend.Initialize (IWidgetEventSink sink)
		{
			eventSink = (S) sink;
			Initialize ();
		}
		
		public virtual void Initialize ()
		{
		}
		
		public S EventSink {
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
		
		public T Widget { get; set; }
		
		public Gtk.Widget RootWidget {
			get {
				return alignment != null ? alignment : (Gtk.Widget) Widget;
			}
		}
		
		public virtual bool Visible {
			get { return Widget.Visible; }
			set { Widget.Visible = value; }
		}
		
		public virtual bool Sensitive {
			get { return Widget.Sensitive; }
			set { Widget.Sensitive = value; }
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
		
		public virtual void Dispose ()
		{
			if (Widget != null)
				Widget.Destroy ();
		}
		
		public Size Size {
			get {
				return new Size (Widget.Allocation.Width, Widget.Allocation.Height);
			}
		}
		
		public virtual WidgetSize GetPreferredWidth ()
		{
			return new WidgetSize (Widget.SizeRequest ().Width) + frontend.Margin.HorizontalSpacing;
		}
		
		public virtual WidgetSize GetPreferredHeight ()
		{
			return new WidgetSize (Widget.SizeRequest ().Height) + frontend.Margin.VerticalSpacing;
		}
		
		public virtual WidgetSize GetPreferredHeightForWidth (double width)
		{
			return new WidgetSize (Widget.SizeRequest ().Height) + frontend.Margin.VerticalSpacing;
		}
		
		public virtual WidgetSize GetPreferredWidthForHeight (double height)
		{
			return new WidgetSize (Widget.SizeRequest ().Width) + frontend.Margin.HorizontalSpacing;
		}
		
		Gtk.Widget IGtkWidgetBackend.Widget {
			get { return RootWidget; }
		}
		
		public static Gtk.Widget GetWidget (IWidgetBackend w)
		{
			return ((IGtkWidgetBackend)w).Widget;
		}
		
		public virtual void UpdateLayout ()
		{
			if (frontend.Margin.HorizontalSpacing == 0 && frontend.Margin.VerticalSpacing == 0) {
				if (alignment != null) {
					alignment.Remove (Widget);
					Gtk.Container cont = alignment.Parent as Gtk.Container;
					if (cont != null)
						GtkEngine.ReplaceChild (cont, alignment, Widget);
					alignment.Destroy ();
					alignment = null;
				}
			} else {
				if (alignment == null) {
					alignment = new Gtk.Alignment (0, 0, 1, 1);
					Gtk.Container cont = Widget.Parent as Gtk.Container;
					if (cont != null)
						GtkEngine.ReplaceChild (cont, Widget, alignment);
					alignment.Add (Widget);
				}
				alignment.LeftPadding = (uint) frontend.Margin.Left;
				alignment.RightPadding = (uint) frontend.Margin.Right;
				alignment.TopPadding = (uint) frontend.Margin.Top;
				alignment.BottomPadding = (uint) frontend.Margin.Bottom;
			}
		}
		
		public virtual void EnableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				WidgetEvent ev = (WidgetEvent) eventId;
				switch (ev) {
				case WidgetEvent.DragDropCheck:
					dropCheckEventEnabled = true;
					break;
				case WidgetEvent.DragDrop:
					dropEventEnabled = true;
					break;
				case WidgetEvent.DragOverCheck:
					dragMotionCheckEventEnabled = true;
					break;
				case WidgetEvent.DragOver:
					dragMotionEventEnabled = true;
					break;
				case WidgetEvent.DragLeave:
					Widget.DragLeave += HandleWidgetDragLeave;;
					break;
				}
				if (dropEventEnabled || dropCheckEventEnabled || dragMotionCheckEventEnabled || dragMotionEventEnabled) {
					if (!dragEventsSet) {
						dragEventsSet = true;
						Widget.DragDrop += HandleWidgetDragDrop;
						Widget.DragMotion += HandleWidgetDragMotion;
						Widget.DragDataReceived += HandleWidgetDragDataReceived;
					}
				}
			}
		}
		
		public virtual void DisableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				WidgetEvent ev = (WidgetEvent) eventId;
				switch (ev) {
				case WidgetEvent.DragDropCheck:
					dropCheckEventEnabled = false;
					break;
				case WidgetEvent.DragDrop:
					dropEventEnabled = false;
					break;
				case WidgetEvent.DragOverCheck:
					dragMotionCheckEventEnabled = false;
					break;
				case WidgetEvent.DragOver:
					dragMotionEventEnabled = false;
					break;
				case WidgetEvent.DragLeave:
					Widget.DragLeave -= HandleWidgetDragLeave;
					break;
				}
				if (!dropEventEnabled && !dropCheckEventEnabled && !dragMotionCheckEventEnabled && !dragMotionEventEnabled) {
					if (dragEventsSet) {
						dragEventsSet = false;
						Widget.DragDrop -= HandleWidgetDragDrop;
						Widget.DragMotion -= HandleWidgetDragMotion;
						Widget.DragDataReceived -= HandleWidgetDragDataReceived;
					}
				}
			}
		}
		
		Point lastDragPosition;
		
		void HandleWidgetDragMotion (object o, Gtk.DragMotionArgs args)
		{
			lastDragPosition = new Point (args.X, args.Y);
			
			DragDropAction ac;
			if (!dragMotionCheckEventEnabled) {
				if (dragMotionEventEnabled)
					ac = DragDropAction.Default;
				else
					ac = ConvertDragAction (destDragAction);
			}
			else {
				DragOverCheckEventArgs da = new DragOverCheckEventArgs (new Point (args.X, args.Y), Util.GetDragTypes (args.Context.Targets), ConvertDragAction (args.Context.Actions));
				EventSink.OnDragOverCheck (da);
				ac = da.AllowedAction;
				if (!dragMotionEventEnabled && ac == DragDropAction.Default)
					ac = DragDropAction.None;
			}
			
			if (ac == DragDropAction.None) {
				args.RetVal = false;
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

			DragDropResult res;
			if (!dropCheckEventEnabled) {
				if (dropEventEnabled)
					res = DragDropResult.None;
				else
					res = DragDropResult.Canceled;
			}
			else {
				DragCheckEventArgs da = new DragCheckEventArgs (new Point (args.X, args.Y), Util.GetDragTypes (args.Context.Targets), ConvertDragAction (args.Context.Actions));
				EventSink.OnDragDropCheck (da);
				res = da.Result;
				if (!dropEventEnabled && res == DragDropResult.None)
					res = DragDropResult.Canceled;
			}
			if (res == DragDropResult.Canceled) {
				args.RetVal = false;
			}
			else if (res == DragDropResult.Success) {
				args.RetVal = true;
				Gtk.Drag.Finish (args.Context, true, false, args.Time);
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
			eventSink.OnDragLeave (EventArgs.Empty);
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
					EventSink.OnDragOver (da);
//					if (da.AllowedAction != DragDropAction.None)
//						Gtk.Drag.Highlight (Widget);
					Gdk.Drag.Status (args.Context, ConvertDragAction (da.AllowedAction), args.Time);
				}
				else {
					DragEventArgs da = new DragEventArgs (lastDragPosition, dragData, ConvertDragAction (args.Context.Actions));
					EventSink.OnDragDrop (da);
					Gtk.Drag.Finish (args.Context, da.Success, false, args.Time);
				}
			}
		}
		
		public void DragStart (TransferDataSource data, DragDropAction dragAction)
		{
			Gdk.DragAction action = ConvertDragAction (dragAction);
			currentDragData = data;
			Widget.DragEnd += HandleWidgetDragEnd;
			Widget.DragDataGet += HandleWidgetDragDataGet;
			Gtk.Drag.Begin (Widget, Util.BuildTargetTable (data.DataTypes), action, 0, Gtk.Global.CurrentEvent);
		}

		void HandleWidgetDragDataGet (object o, Gtk.DragDataGetArgs args)
		{
			Util.SetDragData (currentDragData, args);
		}

		void HandleWidgetDragEnd (object o, Gtk.DragEndArgs args)
		{
			Widget.DragEnd -= HandleWidgetDragEnd;
			Widget.DragDataGet -= HandleWidgetDragDataGet;
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

