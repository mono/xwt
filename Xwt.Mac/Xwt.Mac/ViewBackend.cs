// 
// ViewBackend.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using Xwt;
using Xwt.Backends;
using MonoMac.CoreGraphics;
using MonoMac.CoreAnimation;


namespace Xwt.Mac
{
	public abstract class ViewBackend<T,S>: ViewBackend where T:NSView where S:IWidgetEventSink
	{
		public new S EventSink {
			get { return (S) base.EventSink; }
		}
		
		public new T Widget {
			get { return (T) base.Widget; }
		}
	}

	public abstract class ViewBackend: IWidgetBackend
	{
		Widget frontend;
		IWidgetEventSink eventSink;
		IViewObject viewObject;
		WidgetEvent currentEvents;
		bool autosize;
		Size lastFittingSize;
		bool sizeCalcPending = true;
		bool sensitive = true;
		bool canGetFocus = true;
		Xwt.Drawing.Color backgroundColor;

		void IBackend.InitializeBackend (object frontend, ApplicationContext context)
		{
			ApplicationContext = context;
			this.frontend = (Widget) frontend;
		}
		
		void IWidgetBackend.Initialize (IWidgetEventSink sink)
		{
			eventSink = (IWidgetEventSink) sink;
			Initialize ();
			ResetFittingSize ();
			canGetFocus = Widget.AcceptsFirstResponder ();
		}

		// To be called when the widget is a root and is not inside a Xwt window. For example, when it is in a popover or a tooltip
		// In that case, the widget has to listen to the change event of the children and resize itself
		public void SetAutosizeMode (bool autosize)
		{
			this.autosize = autosize;
			if (autosize)
				AutoUpdateSize ();
		}

		
		public virtual void Initialize ()
		{
		}
		
		public IWidgetEventSink EventSink {
			get { return eventSink; }
		}
		
		public Widget Frontend {
			get {
				return this.frontend;
			}
		}
		
		public ApplicationContext ApplicationContext {
			get;
			private set;
		}
		
		public object NativeWidget {
			get {
				return Widget;
			}
		}
		
		public NSView Widget {
			get { return (NSView) ViewObject.View; }
		}
		
		public IViewObject ViewObject {
			get { return viewObject; }
			set {
				viewObject = value;
				viewObject.Backend = this;
			}
		}
		
		public bool Visible {
			get { return !Widget.Hidden; }
			set { Widget.Hidden = !value; }
		}

		public double Opacity {
			get { return Widget.AlphaValue; }
			set { Widget.AlphaValue = (float)value; }
		}
		
		public virtual bool Sensitive {
			get { return sensitive; }
			set {
				sensitive = value;
				UpdateSensitiveStatus (Widget, sensitive && ParentIsSensitive ());
			}
		}

		bool ParentIsSensitive ()
		{
			IViewObject parent = Widget.Superview as IViewObject;
			if (parent == null) {
				var wb = Widget.Window as WindowBackend;
				return wb == null || wb.Sensitive;
			}
			if (!parent.Backend.Sensitive)
				return false;
			return parent.Backend.ParentIsSensitive ();
		}

		internal void UpdateSensitiveStatus (NSView view, bool parentIsSensitive)
		{
			if (view is NSControl)
				((NSControl)view).Enabled = parentIsSensitive && sensitive;

			foreach (var s in view.Subviews) {
				if (s is IViewObject)
					((IViewObject)s).Backend.UpdateSensitiveStatus (s, parentIsSensitive);
				else
					UpdateSensitiveStatus (s, sensitive && parentIsSensitive);
			}
		}

		public virtual bool CanGetFocus {
			get { return canGetFocus; }
			set {
				canGetFocus = value;
				if (!Widget.AcceptsFirstResponder ())
					canGetFocus = false;
			}
		}
		
		public virtual bool HasFocus {
			get {
				return Widget.Window != null && Widget.Window.FirstResponder == Widget;
			}
		}
		
		public virtual void SetFocus ()
		{
			if (Widget.Window != null && CanGetFocus)
				Widget.Window.MakeFirstResponder (Widget);
		}
		
		public string TooltipText {
			get {
				return Widget.ToolTip;
			}
			set {
				Widget.ToolTip = value;
			}
		}
		
		public void NotifyPreferredSizeChanged ()
		{
			EventSink.OnPreferredSizeChanged ();
		}
		
		public void SetCursor (CursorType cursor)
		{
			NSCursor ctype;
			if (cursor == CursorType.Arrow)
				ctype = NSCursor.ArrowCursor;
			else if (cursor == CursorType.Crosshair)
				ctype = NSCursor.CrosshairCursor;
			else if (cursor == CursorType.Hand)
				ctype = NSCursor.ClosedHandCursor;
			else if (cursor == CursorType.IBeam)
				ctype = NSCursor.IBeamCursor;
			else if (cursor == CursorType.ResizeDown)
				ctype = NSCursor.ResizeDownCursor;
			else if (cursor == CursorType.ResizeUp)
				ctype = NSCursor.ResizeUpCursor;
			else if (cursor == CursorType.ResizeLeft)
				ctype = NSCursor.ResizeLeftCursor;
			else if (cursor == CursorType.ResizeRight)
				ctype = NSCursor.ResizeRightCursor;
			else if (cursor == CursorType.ResizeLeftRight)
				ctype = NSCursor.ResizeLeftRightCursor;
			else if (cursor == CursorType.ResizeUpDown)
				ctype = NSCursor.ResizeUpDownCursor;
			else
				ctype = NSCursor.ArrowCursor;
			// TODO: assign the cursor
		}
		
		~ViewBackend ()
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
		}
		
		Size IWidgetBackend.Size {
			get { return new Size (Widget.WidgetWidth (), Widget.WidgetHeight ()); }
		}
		
		public static NSView GetWidget (IWidgetBackend w)
		{
			return ((ViewBackend)w).Widget;
		}

		public static NSView GetWidget (Widget w)
		{
			return GetWidget ((IWidgetBackend)Toolkit.GetBackend (w));
		}

		public static NSView GetWidgetWithPlacement (IWidgetBackend childBackend)
		{
			var backend = (ViewBackend)childBackend;
			var child = backend.Widget;
			var wrapper = child.Superview as WidgetPlacementWrapper;
			if (wrapper != null)
				return wrapper;

			if (!NeedsAlignmentWrapper (backend.Frontend))
				return child;

			wrapper = new WidgetPlacementWrapper ();
			wrapper.SetChild (child, backend.Frontend);
			return wrapper;
		}

		public static NSView SetChildPlacement (IWidgetBackend childBackend)
		{
			var backend = (ViewBackend)childBackend;
			var child = backend.Widget;
			var wrapper = child.Superview as WidgetPlacementWrapper;
			var fw = backend.Frontend;

			if (!NeedsAlignmentWrapper (fw)) {
				if (wrapper != null) {
					var parent = wrapper.Superview;
					child.RemoveFromSuperview ();
					ReplaceSubview (wrapper, child);
				}
				return child;
			}

			if (wrapper == null) {
				wrapper = new WidgetPlacementWrapper ();
				var f = child.Frame;
				ReplaceSubview (child, wrapper);
				wrapper.SetChild (child, backend.Frontend);
				wrapper.Frame = f;
			} else
				wrapper.UpdateChildPlacement ();
			return wrapper;
		}

		public static void RemoveChildPlacement (NSView w)
		{
			if (w == null)
				return;
			if (w is WidgetPlacementWrapper) {
				var wp = (WidgetPlacementWrapper)w;
				wp.Subviews [0].RemoveFromSuperview ();
			}
		}
		
		static bool NeedsAlignmentWrapper (Widget fw)
		{
			return fw.HorizontalPlacement != WidgetPlacement.Fill || fw.VerticalPlacement != WidgetPlacement.Fill || fw.Margin.VerticalSpacing != 0 || fw.Margin.HorizontalSpacing != 0;
		}

		public virtual void UpdateChildPlacement (IWidgetBackend childBackend)
		{
			SetChildPlacement (childBackend);
		}

		public static void ReplaceSubview (NSView oldChild, NSView newChild)
		{
			var vo = oldChild as IViewObject;
			if (vo != null && vo.Backend.Frontend.Parent != null) {
				var ba = vo.Backend.Frontend.Parent.GetBackend () as ViewBackend;
				if (ba != null) {
					ba.ReplaceChild (oldChild, newChild);
					return;
				}
			}
			var f = oldChild.Frame;
			oldChild.Superview.ReplaceSubviewWith (oldChild, newChild);
			newChild.Frame = f;
		}

		public virtual void ReplaceChild (NSView oldChild, NSView newChild)
		{
			var f = oldChild.Frame;
			oldChild.Superview.ReplaceSubviewWith (oldChild, newChild);
			newChild.Frame = f;
		}

		public virtual object Font {
			get {
				if (Widget is NSControl)
					return ((NSControl)(object)Widget).Font;
				if (Widget is NSText)
					return ((NSText)(object)Widget).Font;
				return NSFont.ControlContentFontOfSize (NSFont.SystemFontSize);
			}
			set {
				if (Widget is NSControl)
					((NSControl)(object)Widget).Font = (NSFont) value;
				if (Widget is NSText)
					((NSText)(object)Widget).Font = (NSFont) value;
				ResetFittingSize ();
			}
		}
		
		public virtual Xwt.Drawing.Color BackgroundColor {
			get {
				return this.backgroundColor;
			}
			set {
				this.backgroundColor = value;
				if (Widget.Layer == null)
					Widget.WantsLayer = true;
				Widget.Layer.BackgroundColor = value.ToCGColor ();
			}
		}
		
		#region IWidgetBackend implementation
		
		public Point ConvertToScreenCoordinates (Point widgetCoordinates)
		{
			var lo = Widget.ConvertPointToBase (new PointF ((float)widgetCoordinates.X, (float)widgetCoordinates.Y));
			lo = Widget.Window.ConvertBaseToScreen (lo);
			return MacDesktopBackend.ToDesktopRect (new RectangleF (lo.X, lo.Y, 0, Widget.IsFlipped ? 0 : Widget.Frame.Height)).Location;
		}
		
		protected virtual Size GetNaturalSize ()
		{
			if (sizeCalcPending) {
				sizeCalcPending = false;
				var f = Widget.Frame;
				SizeToFit ();
				lastFittingSize = new Size (Widget.WidgetWidth (), Widget.WidgetHeight ());
				Widget.Frame = f;
			}
			return lastFittingSize;
		}

		public virtual Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return GetNaturalSize ();
		}
		
		protected double minWidth = -1, minHeight = -1;
		
		public void SetMinSize (double width, double height)
		{
			minWidth = width;
			minHeight = height;
		}

		protected void ResetFittingSize ()
		{
			sizeCalcPending = true;
		}

		public void SizeToFit ()
		{
			OnSizeToFit ();
//			if (minWidth != -1 && Widget.Frame.Width < minWidth || minHeight != -1 && Widget.Frame.Height < minHeight)
//				Widget.SetFrameSize (new SizeF (Math.Max (Widget.Frame.Width, (float)minWidth), Math.Max (Widget.Frame.Height, (float)minHeight)));
		}
		
		protected virtual Size CalcFittingSize ()
		{
			return Size.Zero;
		}

		static readonly Selector sizeToFitSel = new Selector ("sizeToFit");

		protected virtual void OnSizeToFit ()
		{
			if (Widget.RespondsToSelector (sizeToFitSel)) {
				Messaging.void_objc_msgSend (Widget.Handle, sizeToFitSel.Handle);
			} else {
				var s = CalcFittingSize ();
				if (!s.IsZero)
					Widget.SetFrameSize (new SizeF ((float)s.Width, (float)s.Height));
			}
		}
		
		public void SetSizeRequest (double width, double height)
		{
			// Nothing to do
		}
		
		public virtual void UpdateLayout ()
		{
			if (autosize)
				AutoUpdateSize ();
		}

		void AutoUpdateSize ()
		{	var s = Frontend.Surface.GetPreferredSize ();
			Widget.SetFrameSize (new SizeF ((float)s.Width, (float)s.Height));
		}

		NSObject gotFocusObserver;
		
		public virtual void EnableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				WidgetEvent ev = (WidgetEvent) eventId;
				currentEvents |= ev;
				switch (ev) {
				case WidgetEvent.GotFocus:
				case WidgetEvent.LostFocus:
					SetupFocusEvents (Widget.GetType ());
					break;
				}
			}
		}
		
		public virtual void DisableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				WidgetEvent ev = (WidgetEvent) eventId;
				currentEvents &= ~ev;
			}
		}
		
		static Selector draggingEnteredSel = new Selector ("draggingEntered:");
		static Selector draggingUpdatedSel = new Selector ("draggingUpdated:");
		static Selector draggingExitedSel = new Selector ("draggingExited:");
		static Selector prepareForDragOperationSel = new Selector ("prepareForDragOperation:");
		static Selector performDragOperationSel = new Selector ("performDragOperation:");
		static Selector concludeDragOperationSel = new Selector ("concludeDragOperation:");
		static Selector becomeFirstResponderSel = new Selector ("becomeFirstResponder");
		static Selector resignFirstResponderSel = new Selector ("resignFirstResponder");

		static HashSet<Type> typesConfiguredForDragDrop = new HashSet<Type> ();
		static HashSet<Type> typesConfiguredForFocusEvents = new HashSet<Type> ();

		static void SetupForDragDrop (Type type)
		{
			lock (typesConfiguredForDragDrop) {
				if (typesConfiguredForDragDrop.Add (type)) {
					Class c = new Class (type);
					c.AddMethod (draggingEnteredSel.Handle, new Func<IntPtr,IntPtr,IntPtr,NSDragOperation> (DraggingEntered), "i@:@");
					c.AddMethod (draggingUpdatedSel.Handle, new Func<IntPtr,IntPtr,IntPtr,NSDragOperation> (DraggingUpdated), "i@:@");
					c.AddMethod (draggingExitedSel.Handle, new Action<IntPtr,IntPtr,IntPtr> (DraggingExited), "v@:@");
					c.AddMethod (prepareForDragOperationSel.Handle, new Func<IntPtr,IntPtr,IntPtr,bool> (PrepareForDragOperation), "B@:@");
					c.AddMethod (performDragOperationSel.Handle, new Func<IntPtr,IntPtr,IntPtr,bool> (PerformDragOperation), "B@:@");
					c.AddMethod (concludeDragOperationSel.Handle, new Action<IntPtr,IntPtr,IntPtr> (ConcludeDragOperation), "v@:@");
				}
			}
		}

		static void SetupFocusEvents (Type type)
		{
			lock (typesConfiguredForFocusEvents) {
				if (typesConfiguredForFocusEvents.Add (type)) {
					Class c = new Class (type);
					c.AddMethod (becomeFirstResponderSel.Handle, new Func<IntPtr,IntPtr,bool> (OnBecomeFirstResponder), "B@:");
					c.AddMethod (resignFirstResponderSel.Handle, new Func<IntPtr,IntPtr,bool> (OnResignFirstResponder), "B@:");
				}
			}
		}
		
		public void DragStart (DragStartData sdata)
		{
			var lo = Widget.ConvertPointToBase (new PointF (Widget.Bounds.X, Widget.Bounds.Y));
			lo = Widget.Window.ConvertBaseToScreen (lo);
			var ml = NSEvent.CurrentMouseLocation;
			var pb = NSPasteboard.FromName (NSPasteboard.NSDragPasteboardName);
			if (pb == null)
				throw new InvalidOperationException ("Could not get pasteboard");
			if (sdata.Data == null)
				throw new ArgumentNullException ("data");
			InitPasteboard (pb, sdata.Data);
			var img = (NSImage)sdata.ImageBackend;
			var pos = new PointF (ml.X - lo.X - (float)sdata.HotX, lo.Y - ml.Y - (float)sdata.HotY + img.Size.Height);
			Widget.DragImage (img, pos, new SizeF (0, 0), NSApplication.SharedApplication.CurrentEvent, pb, Widget, true);
		}
		
		public void SetDragSource (TransferDataType[] types, DragDropAction dragAction)
		{
		}
		
		public void SetDragTarget (TransferDataType[] types, DragDropAction dragAction)
		{
			SetupForDragDrop (Widget.GetType ());
			var dtypes = types.Select (t => ToNSDragType (t)).ToArray ();
			Widget.RegisterForDraggedTypes (dtypes);
		}
		
		static NSDragOperation DraggingEntered (IntPtr sender, IntPtr sel, IntPtr dragInfo)
		{
			return DraggingUpdated (sender, sel, dragInfo);
		}
		
		static NSDragOperation DraggingUpdated (IntPtr sender, IntPtr sel, IntPtr dragInfo)
		{
			IViewObject ob = Runtime.GetNSObject (sender) as IViewObject;
			if (ob == null)
				return NSDragOperation.None;
			var backend = ob.Backend;
			
			NSDraggingInfo di = new NSDraggingInfo (dragInfo);
			var types = di.DraggingPasteboard.Types.Select (t => ToXwtDragType (t)).ToArray ();
			var pos = new Point (di.DraggingLocation.X, di.DraggingLocation.Y);
			
			if ((backend.currentEvents & WidgetEvent.DragOverCheck) != 0) {
				var args = new DragOverCheckEventArgs (pos, types, ConvertAction (di.DraggingSourceOperationMask));
				backend.OnDragOverCheck (di, args);
				if (args.AllowedAction == DragDropAction.None)
					return NSDragOperation.None;
				if (args.AllowedAction != DragDropAction.Default)
					return ConvertAction (args.AllowedAction);
			}
			
			if ((backend.currentEvents & WidgetEvent.DragOver) != 0) {
				TransferDataStore store = new TransferDataStore ();
				FillDataStore (store, di.DraggingPasteboard, ob.View.RegisteredDragTypes ());
				var args = new DragOverEventArgs (pos, store, ConvertAction (di.DraggingSourceOperationMask));
				backend.OnDragOver (di, args);
				if (args.AllowedAction == DragDropAction.None)
					return NSDragOperation.None;
				if (args.AllowedAction != DragDropAction.Default)
					return ConvertAction (args.AllowedAction);
			}
			
			return di.DraggingSourceOperationMask;
		}
		
		static void DraggingExited (IntPtr sender, IntPtr sel, IntPtr dragInfo)
		{
			IViewObject ob = Runtime.GetNSObject (sender) as IViewObject;
			if (ob != null) {
				var backend = ob.Backend;
				backend.ApplicationContext.InvokeUserCode (delegate {
					backend.eventSink.OnDragLeave (EventArgs.Empty);
				});
			}
		}
		
		static bool PrepareForDragOperation (IntPtr sender, IntPtr sel, IntPtr dragInfo)
		{
			IViewObject ob = Runtime.GetNSObject (sender) as IViewObject;
			if (ob == null)
				return false;
			
			var backend = ob.Backend;
			
			NSDraggingInfo di = new NSDraggingInfo (dragInfo);
			var types = di.DraggingPasteboard.Types.Select (t => ToXwtDragType (t)).ToArray ();
			var pos = new Point (di.DraggingLocation.X, di.DraggingLocation.Y);
			
			if ((backend.currentEvents & WidgetEvent.DragDropCheck) != 0) {
				var args = new DragCheckEventArgs (pos, types, ConvertAction (di.DraggingSourceOperationMask));
				bool res = backend.ApplicationContext.InvokeUserCode (delegate {
					backend.eventSink.OnDragDropCheck (args);
				});
				if (args.Result == DragDropResult.Canceled || !res)
					return false;
			}
			return true;
		}
		
		static bool PerformDragOperation (IntPtr sender, IntPtr sel, IntPtr dragInfo)
		{
			IViewObject ob = Runtime.GetNSObject (sender) as IViewObject;
			if (ob == null)
				return false;
			
			var backend = ob.Backend;
			
			NSDraggingInfo di = new NSDraggingInfo (dragInfo);
			var pos = new Point (di.DraggingLocation.X, di.DraggingLocation.Y);
			
			if ((backend.currentEvents & WidgetEvent.DragDrop) != 0) {
				TransferDataStore store = new TransferDataStore ();
				FillDataStore (store, di.DraggingPasteboard, ob.View.RegisteredDragTypes ());
				var args = new DragEventArgs (pos, store, ConvertAction (di.DraggingSourceOperationMask));
				backend.ApplicationContext.InvokeUserCode (delegate {
					backend.eventSink.OnDragDrop (args);
				});
				return args.Success;
			} else
				return false;
		}
		
		static void ConcludeDragOperation (IntPtr sender, IntPtr sel, IntPtr dragInfo)
		{
			Console.WriteLine ("ConcludeDragOperation");
		}
		
		protected virtual void OnDragOverCheck (NSDraggingInfo di, DragOverCheckEventArgs args)
		{
			ApplicationContext.InvokeUserCode (delegate {
				eventSink.OnDragOverCheck (args);
			});
		}
		
		protected virtual void OnDragOver (NSDraggingInfo di, DragOverEventArgs args)
		{
			ApplicationContext.InvokeUserCode (delegate {
				eventSink.OnDragOver (args);
			});
		}
		
		void InitPasteboard (NSPasteboard pb, TransferDataSource data)
		{
			pb.ClearContents ();
			foreach (var t in data.DataTypes) {
				if (t == TransferDataType.Text) {
					pb.AddTypes (new string[] { NSPasteboard.NSStringType }, null);
					pb.SetStringForType ((string)data.GetValue (t), NSPasteboard.NSStringType);
				}
			}
		}

		static void FillDataStore (TransferDataStore store, NSPasteboard pb, string[] types)
		{
			foreach (var t in types) {
				if (!pb.Types.Contains (t))
					continue;
				if (t == NSPasteboard.NSStringType)
					store.AddText (pb.GetStringForType (t));
				else if (t == NSPasteboard.NSFilenamesType) {
					string data = pb.GetStringForType (t);
					XmlDocument doc = new XmlDocument ();
					doc.XmlResolver = null; // Avoid DTD validation
					doc.LoadXml (data);
					store.AddUris (doc.SelectNodes ("/plist/array/string").Cast<XmlElement> ().Select (e => new Uri (e.InnerText)).ToArray ());
				}
			}
		}
		
		static NSDragOperation ConvertAction (DragDropAction action)
		{
			NSDragOperation res = (NSDragOperation)0;
			if ((action & DragDropAction.Copy) != 0)
				res |= NSDragOperation.Copy;
			if ((action & DragDropAction.Move) != 0)
				res |= NSDragOperation.Move;
			if ((action & DragDropAction.Link) != 0)
				res |= NSDragOperation.Link;
			return res;
		}
		
		static DragDropAction ConvertAction (NSDragOperation action)
		{
			if (action == NSDragOperation.AllObsolete)
				return DragDropAction.All;
			DragDropAction res = (DragDropAction)0;
			if ((action & NSDragOperation.Copy) != 0)
				res |= DragDropAction.Copy;
			if ((action & NSDragOperation.Move) != 0)
				res |= DragDropAction.Move;
			if ((action & NSDragOperation.Link) != 0)
				res |= DragDropAction.Link;
			return res;
		}
		
		static string ToNSDragType (TransferDataType type)
		{
			if (type == TransferDataType.Text) return NSPasteboard.NSStringType;
			if (type == TransferDataType.Uri) return NSPasteboard.NSFilenamesType;
			if (type == TransferDataType.Image) return NSPasteboard.NSPictType;
			if (type == TransferDataType.Rtf) return NSPasteboard.NSRtfType;
			if (type == TransferDataType.Html) return NSPasteboard.NSHtmlType;
			return type.Id;
		}
		
		static TransferDataType ToXwtDragType (string type)
		{
			if (type == NSPasteboard.NSStringType)
				return TransferDataType.Text;
			if (type == NSPasteboard.NSFilenamesType)
				return TransferDataType.Uri;
			if (type == NSPasteboard.NSPictType)
				return TransferDataType.Image;
			if (type == NSPasteboard.NSRtfType)
				return TransferDataType.Rtf;
			if (type == NSPasteboard.NSHtmlType)
				return TransferDataType.Html;
			return TransferDataType.FromId (type);
		}

		static bool OnBecomeFirstResponder (IntPtr sender, IntPtr sel)
		{
			IViewObject ob = Runtime.GetNSObject (sender) as IViewObject;
			var canGetIt = ob.Backend.canGetFocus;
			if (canGetIt)
				ob.Backend.ApplicationContext.InvokeUserCode (ob.Backend.EventSink.OnGotFocus);
			return canGetIt;
		}
		
		static bool OnResignFirstResponder (IntPtr sender, IntPtr sel)
		{
			IViewObject ob = Runtime.GetNSObject (sender) as IViewObject;
			ob.Backend.ApplicationContext.InvokeUserCode (ob.Backend.EventSink.OnLostFocus);
			return true;
		}

		#endregion
	}

	sealed class WidgetPlacementWrapper: NSControl, IViewObject
	{
		NSView child;
		Widget w;

		public WidgetPlacementWrapper ()
		{
		}

		NSView IViewObject.View {
			get { return this; }
		}

		ViewBackend IViewObject.Backend {
			get {
				var vo = child as IViewObject;
				return vo != null ? vo.Backend : null;
			}
			set {
				var vo = child as IViewObject;
				if (vo != null)
					vo.Backend = value;
			}
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public void SetChild (NSView child, Widget w)
		{
			this.child = child;
			this.w = w;
			AddSubview (child);
		}

		public override void SetFrameSize (SizeF newSize)
		{
			base.SetFrameSize (newSize);
			if (w != null)
				UpdateChildPlacement ();
		}

		public void UpdateChildPlacement ()
		{
			double cheight = Frame.Height - w.Margin.VerticalSpacing;
			double cwidth = Frame.Width - w.Margin.HorizontalSpacing;
			double cx = w.MarginLeft;
			double cy = w.MarginTop;

			var s = w.Surface.GetPreferredSize (cwidth, cheight);
			if (w.HorizontalPlacement != WidgetPlacement.Fill) {
				cx += (cwidth - s.Width) * w.HorizontalPlacement.GetValue ();
				cwidth = s.Width;
			}
			if (w.VerticalPlacement != WidgetPlacement.Fill) {
				cy += (cheight - s.Height) * w.VerticalPlacement.GetValue ();
				cheight = s.Height;
			}
			child.Frame = new RectangleF ((float)cx, (float)cy, (float)cwidth, (float)cheight);
		}

		public override void SizeToFit ()
		{
			base.SizeToFit ();
		}
	}
}

