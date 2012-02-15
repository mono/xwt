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
using Xwt.Engine;

namespace Xwt.Mac
{
	public abstract class ViewBackend<T,S>: IWidgetBackend,IMacViewBackend where T:NSView where S:IWidgetEventSink
	{
		Widget frontend;
		NSView alignment;
		S eventSink;
		IViewObject<T> viewObject;
		WidgetEvent currentEvents;
		
		void IBackend.Initialize (object frontend)
		{
			this.frontend = (Widget) frontend;
			if (viewObject != null)
				viewObject.Frontend = (Widget) frontend;
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
			get {
				return this.frontend;
			}
		}
		
		public object NativeWidget {
			get {
				return Widget;
			}
		}
		
		public T Widget {
			get { return ViewObject.View; }
		}
		
		public IViewObject<T> ViewObject {
			get { return viewObject; }
			set {
				viewObject = value;
				viewObject.Frontend = frontend;
			}
		}
		
		public NSView RootWidget {
			get { return alignment ?? (NSView) Widget; }
		}
		
		public bool Visible {
			get { return !RootWidget.Hidden; }
			set { RootWidget.Hidden = !value; }
		}
		
		public virtual bool Sensitive {
			get { return true; }
			set { }
		}
		
		public virtual bool CanGetFocus {
			get { return true; }
			set { }
		}
		
		public virtual bool HasFocus {
			get { return false; }
		}
		
		public void SetFocus ()
		{
		}
		
		public string TooltipText {
			get {
				return Widget.ToolTip;
			}
			set {
				Widget.ToolTip = value;
			}
		}
		
		public virtual void Dispose (bool disposing)
		{
		}
		
		public virtual SizeRequestMode SizeRequestMode {
			get { return SizeRequestMode.HeightForWidth; }
		}
		
		Size IWidgetBackend.Size {
			get { return new Size (RootWidget.WidgetWidth (), RootWidget.WidgetHeight ()); }
		}
		
		NSView IMacViewBackend.View {
			get { return RootWidget; }
		}
		
		public static NSView GetWidget (IWidgetBackend w)
		{
			return ((IMacViewBackend)w).View;
		}

		public static NSView GetWidget (Widget w)
		{
			return GetWidget ((IWidgetBackend)WidgetRegistry.GetBackend (w));
		}
		
		public virtual object Font {
			get {
				if (Widget is NSControl)
					return ((NSControl)(object)Widget).Font;
				if (Widget is NSText)
					return ((NSText)(object)Widget).Font;
				/// TODO
//				throw new NotImplementedException ();
				return null;
			}
			set {
				/// TODO
//				throw new NotImplementedException ();
			}
		}
		
		public virtual Xwt.Drawing.Color BackgroundColor {
			get {
				if (Widget.Layer != null)
					return Widget.Layer.BackgroundColor.ToXwtColor ();
				else
					return Xwt.Drawing.Color.Black;
			}
			set {
				if (Widget.Layer != null)
					Widget.Layer.BackgroundColor = value.ToCGColor ();
			}
		}
		
		#region IWidgetBackend implementation
		
		public Point ConvertToScreenCoordinates (Point widgetCoordinates)
		{
			var lo = Widget.ConvertPointToBase (new PointF ((float)widgetCoordinates.X, (float)widgetCoordinates.Y));
			lo = Widget.Window.ConvertBaseToScreen (lo);
			return new Point (lo.X, lo.Y);
		}
		
		public virtual WidgetSize GetPreferredWidth ()
		{
//			double w1 = Widget.FittingSize.Width + frontend.Margin.HorizontalSpacing;
			double w = Widget.WidgetWidth() + frontend.Margin.HorizontalSpacing;
			var s = new Xwt.WidgetSize (w, w);
			if (minWidth != -1 && s.MinSize > minWidth)
				s.MinSize = minWidth;
			return s;
		}

		public virtual WidgetSize GetPreferredHeight ()
		{
//			double h1 = Widget.FittingSize.Height + frontend.Margin.VerticalSpacing;
			double h = Widget.WidgetHeight() + frontend.Margin.VerticalSpacing;
			var s = new Xwt.WidgetSize (h, h);
			if (minHeight != -1 && s.MinSize > minHeight)
				s.MinSize = minHeight;
			return s;
		}

		public WidgetSize GetPreferredHeightForWidth (double width)
		{
			return GetPreferredHeight ();
		}

		public WidgetSize GetPreferredWidthForHeight (double height)
		{
			return GetPreferredWidth ();
		}
		
		double minWidth = -1, minHeight = -1;
		
		public void SetMinSize (double width, double height)
		{
			minWidth = width;
			minHeight = height;
		}
		
		public void SetNaturalSize (double width, double height)
		{
			// Nothing to do
		}
		
		public virtual void UpdateLayout ()
		{
			if (frontend.Margin.HorizontalSpacing == 0 && frontend.Margin.VerticalSpacing == 0) {
				if (alignment != null) {
					Widget.RemoveFromSuperview ();
					NSView cont = alignment.Superview;
					if (cont != null)
						MacEngine.ReplaceChild (cont, alignment, Widget);
					alignment.Dispose ();
					alignment = null;
				}
			} else {
				if (alignment == null) {
					alignment = new NSView ();
					alignment.Frame = Widget.Frame;
					NSView cont = Widget.Superview;
					if (cont != null)
						MacEngine.ReplaceChild (cont, Widget, alignment);
					alignment.AddSubview (Widget);
				}
				Rectangle frame = new Rectangle ((int)frontend.Margin.Left, (int)frontend.Margin.Top, (int)alignment.Frame.Width - frontend.Margin.HorizontalSpacing, (int)alignment.Frame.Height - frontend.Margin.VerticalSpacing);
				Widget.SetWidgetBounds (frame);
			}
		}
		
		public virtual void EnableEvent (object eventId)
		{
			if (eventId is WidgetEvent) {
				WidgetEvent ev = (WidgetEvent) eventId;
				currentEvents |= ev;
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
		static HashSet<Type> typesConfiguredForDragDrop = new HashSet<Type> ();
		
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
		
		public void DragStart (DragStartData sdata)
		{
			var lo = RootWidget.ConvertPointToBase (new PointF (Widget.Bounds.X, Widget.Bounds.Y));
			lo = RootWidget.Window.ConvertBaseToScreen (lo);
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
		
		public void SetDragSource (string[] types, DragDropAction dragAction)
		{
		}
		
		public void SetDragTarget (string[] types, DragDropAction dragAction)
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
			IViewObject<T> ob = Runtime.GetNSObject (sender) as IViewObject<T>;
			if (ob == null)
				return NSDragOperation.None;
			var backend = (ViewBackend<T,S>) WidgetRegistry.GetBackend (ob.Frontend);
			
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
			IViewObject<T> ob = Runtime.GetNSObject (sender) as IViewObject<T>;
			if (ob != null) {
				var backend = (ViewBackend<T,S>) WidgetRegistry.GetBackend (ob.Frontend);
				Toolkit.Invoke (delegate {
					backend.eventSink.OnDragLeave (EventArgs.Empty);
				});
			}
		}
		
		static bool PrepareForDragOperation (IntPtr sender, IntPtr sel, IntPtr dragInfo)
		{
			IViewObject<T> ob = Runtime.GetNSObject (sender) as IViewObject<T>;
			if (ob == null)
				return false;
			
			var backend = (ViewBackend<T,S>) WidgetRegistry.GetBackend (ob.Frontend);
			
			NSDraggingInfo di = new NSDraggingInfo (dragInfo);
			var types = di.DraggingPasteboard.Types.Select (t => ToXwtDragType (t)).ToArray ();
			var pos = new Point (di.DraggingLocation.X, di.DraggingLocation.Y);
			
			if ((backend.currentEvents & WidgetEvent.DragDropCheck) != 0) {
				var args = new DragCheckEventArgs (pos, types, ConvertAction (di.DraggingSourceOperationMask));
				bool res = Toolkit.Invoke (delegate {
					backend.eventSink.OnDragDropCheck (args);
				});
				if (args.Result == DragDropResult.Canceled || !res)
					return false;
			}
			return true;
		}
		
		static bool PerformDragOperation (IntPtr sender, IntPtr sel, IntPtr dragInfo)
		{
			IViewObject<T> ob = Runtime.GetNSObject (sender) as IViewObject<T>;
			if (ob == null)
				return false;
			
			var backend = (ViewBackend<T,S>) WidgetRegistry.GetBackend (ob.Frontend);
			
			NSDraggingInfo di = new NSDraggingInfo (dragInfo);
			var pos = new Point (di.DraggingLocation.X, di.DraggingLocation.Y);
			
			if ((backend.currentEvents & WidgetEvent.DragDrop) != 0) {
				TransferDataStore store = new TransferDataStore ();
				FillDataStore (store, di.DraggingPasteboard, ob.View.RegisteredDragTypes ());
				var args = new DragEventArgs (pos, store, ConvertAction (di.DraggingSourceOperationMask));
				Toolkit.Invoke (delegate {
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
			Toolkit.Invoke (delegate {
				eventSink.OnDragOverCheck (args);
			});
		}
		
		protected virtual void OnDragOver (NSDraggingInfo di, DragOverEventArgs args)
		{
			Toolkit.Invoke (delegate {
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
		
		static string ToNSDragType (string type)
		{
			switch (type) {
			case TransferDataType.Text: return NSPasteboard.NSStringType;
			case TransferDataType.Uri: return NSPasteboard.NSFilenamesType;
			case TransferDataType.Image: return NSPasteboard.NSPictType;
			case TransferDataType.Rtf: return NSPasteboard.NSRtfType;
			}
			return type;
		}
		
		static string ToXwtDragType (string type)
		{
			if (type == NSPasteboard.NSStringType)
				return TransferDataType.Text;
			if (type == NSPasteboard.NSFilenamesType)
				return TransferDataType.Uri;
			if (type == NSPasteboard.NSPictType)
				return TransferDataType.Image;
			if (type == NSPasteboard.NSRtfType)
				return TransferDataType.Rtf;
			return type;
		}
		
		#endregion
	}
	
	public interface IMacViewBackend
	{
		NSView View { get; }
	}
}

