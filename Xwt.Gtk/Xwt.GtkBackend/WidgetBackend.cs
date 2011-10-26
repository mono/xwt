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
		
		static Dictionary<string, Gtk.TargetEntry[]> dragTargets = new Dictionary<string, Gtk.TargetEntry[]> ();
		static Dictionary<string, string> atomToType = new Dictionary<string, string> ();
		static uint targetIdCounter = 0;
		
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
			Console.WriteLine ("DragMotion");
			lastDragPosition = new Point (args.X, args.Y);
			
			DragDropAction ac;
			if (!dragMotionCheckEventEnabled) {
				if (dragMotionEventEnabled)
					ac = DragDropAction.Default;
				else
					ac = ConvertDragAction (destDragAction);
			}
			else {
				DragOverCheckEventArgs da = new DragOverCheckEventArgs (new Point (args.X, args.Y), GetDragTypes (args.Context.Targets), ConvertDragAction (args.Context.Actions));
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
				Console.WriteLine ("-> Status");
				Gdk.Drag.Status (args.Context, ConvertDragAction (ac), args.Time);
			}
		}
		
		void HandleWidgetDragDrop (object o, Gtk.DragDropArgs args)
		{
			Console.WriteLine ("DragDrop");
			lastDragPosition = new Point (args.X, args.Y);

			DragDropResult res;
			if (!dropCheckEventEnabled) {
				if (dropEventEnabled)
					res = DragDropResult.None;
				else
					res = DragDropResult.Canceled;
			}
			else {
				DragCheckEventArgs da = new DragCheckEventArgs (new Point (args.X, args.Y), GetDragTypes (args.Context.Targets), ConvertDragAction (args.Context.Actions));
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
				Console.WriteLine ("-> GetData");
				var at = Gdk.Atom.Intern (t.Target, true);
				data.Add (at);
				Gtk.Drag.GetData (Widget, ctx, at, time);
			}
		}
		
		void HandleWidgetDragDataReceived (object o, Gtk.DragDataReceivedArgs args)
		{
			Console.WriteLine ("DataReceived");
			dragDataRequests--;
			
			string type;
			if (!atomToType.TryGetValue (args.SelectionData.Target.Name, out type)) {
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
					Console.WriteLine ("-> Status");
				}
				else {
					DragEventArgs da = new DragEventArgs (lastDragPosition, dragData, ConvertDragAction (args.Context.Actions));
					EventSink.OnDragDrop (da);
					Gtk.Drag.Finish (args.Context, da.Success, false, args.Time);
					Console.WriteLine ("-> Finish");
				}
			}
		}
		
		public void DragStart (TransferDataSource data, DragDropAction dragAction)
		{
			Gdk.DragAction action = ConvertDragAction (dragAction);
			currentDragData = data;
			Widget.DragEnd += HandleWidgetDragEnd;
			Widget.DragDataGet += HandleWidgetDragDataGet;
			Gtk.Drag.Begin (Widget, BuildTargetTable (data.DataTypes), action, 0, Gtk.Global.CurrentEvent);
		}

		void HandleWidgetDragDataGet (object o, Gtk.DragDataGetArgs args)
		{
			foreach (var t in currentDragData.DataTypes) {
				object val = currentDragData.GetValue (t);
				if (val == null)
					continue;
				if (val is string)
					args.SelectionData.Text = (string)currentDragData.GetValue (t);
				else if (val is Xwt.Drawing.Image)
					args.SelectionData.SetPixbuf ((Gdk.Pixbuf) WidgetRegistry.GetBackend (val));
				else {
					var at = Gdk.Atom.Intern (t, false);
					data.Add (at);
					args.SelectionData.Set (at, 0, TransferDataSource.SerializeValue (val));
				}
			}
		}
		
		List<object> data = new List<object> ();

		void HandleWidgetDragEnd (object o, Gtk.DragEndArgs args)
		{
			Widget.DragEnd -= HandleWidgetDragEnd;
			Widget.DragDataGet -= HandleWidgetDragDataGet;
		}
		
		public void SetDragTarget (string[] types, DragDropAction dragAction)
		{
			destDragAction = ConvertDragAction (dragAction);
			var table = BuildTargetTable (types);
			validDropTypes = (Gtk.TargetEntry[]) table;
			Gtk.Drag.DestSet (Widget, Gtk.DestDefaults.Highlight, validDropTypes, destDragAction);
		}
		
		public void SetDragSource (string[] types, DragDropAction dragAction)
		{
			sourceDragAction = ConvertDragAction (dragAction);
			var table = BuildTargetTable (types);
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
		
		string[] GetDragTypes (Gdk.Atom[] dropTypes)
		{
			List<string> types = new List<string> ();
			foreach (var dt in dropTypes) {
				string type;
				if (atomToType.TryGetValue (dt.ToString (), out type))
					types.Add (type);
			}
			return types.ToArray ();
		}
		
		Gtk.TargetList BuildTargetTable (string[] types)
		{
			var tl = new Gtk.TargetList ();
			foreach (var tt in types)
				tl.AddTable (CreateTargetEntries (tt));
			data.Add (tl);
			return tl;
		}
		
		Gtk.TargetEntry[] CreateTargetEntries (string type)
		{
			lock (dragTargets) {
				Gtk.TargetEntry[] entries;
				if (dragTargets.TryGetValue (type, out entries))
					return entries;
				
				uint id = targetIdCounter++;
				
				switch (type) {
				case TransferDataType.Uri: {
					Gtk.TargetList list = new Gtk.TargetList ();
					list.AddUriTargets (id);
					entries = (Gtk.TargetEntry[]) list;
					break;
				}
				case TransferDataType.Text: {
					Gtk.TargetList list = new Gtk.TargetList ();
					list.AddTextTargets (id);
					//HACK: work around gtk_selection_data_set_text causing crashes on Mac w/ QuickSilver, Clipbard History etc.
					if (Platform.IsMac) {
						list.Remove ("COMPOUND_TEXT");
						list.Remove ("TEXT");
						list.Remove ("STRING");
					}
					entries = (Gtk.TargetEntry[]) list;
					break;
				}
				case TransferDataType.Rtf: {
					Gdk.Atom atom;
					if (Platform.IsMac)
						atom = Gdk.Atom.Intern ("NSRTFPboardType", false); //TODO: use public.rtf when dep on MacOS 10.6
					else
						atom = Gdk.Atom.Intern ("text/rtf", false);
					entries = new Gtk.TargetEntry[] { new Gtk.TargetEntry (atom, 0, id) };
					break;
				}
				default:
					entries = new Gtk.TargetEntry[] { new Gtk.TargetEntry (Gdk.Atom.Intern ("application/" + type, false), 0, id) };
					break;
				}
				foreach (var a in entries.Select (e => e.Target))
					atomToType [a] = type;
				return dragTargets [type] = entries;
			}
		}
	}
	
	public interface IGtkWidgetBackend
	{
		Gtk.Widget Widget { get; }
	}
}

