// 
// Widget.cs
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
using System.ComponentModel;
using System.Collections.Generic;
using Xwt.Backends;
using Xwt.Engine;
using Xwt.Drawing;
using System.Reflection;
using System.Xaml;
using System.Linq;

namespace Xwt
{
	public abstract class Widget: XwtComponent, IWidgetSurface
	{
		List<Widget> children;
		WidgetSpacing margin;
		WidgetSize width;
		WidgetSize height;
		bool widthCached;
		bool heightCached;
		
		EventSink eventSink;
		
		protected class EventSink: IWidgetEventSink
		{
			public Widget Parent { get; internal set; }
			
			public void OnDragOverCheck (DragOverCheckEventArgs args)
			{
				Parent.OnDragOverCheck (args);
			}

			public void OnDragOver (DragOverEventArgs args)
			{
				Parent.OnDragOver (args);
			}

			public void OnDragDropCheck (DragCheckEventArgs args)
			{
				Parent.OnDragDropCheck (args);
			}

			public void OnDragDrop (DragEventArgs args)
			{
				Parent.OnDragDrop (args);
			}

			public void OnDragLeave (EventArgs args)
			{
				Parent.OnDragLeave (args);
			}
		}
		
		public Widget ()
		{
			eventSink = CreateEventSink ();
			eventSink.Parent = this;
			margin = new Xwt.WidgetSpacing (this);
		}
		
		static Widget ()
		{
			MapEvent (WidgetEvent.DragOverCheck, typeof(Widget), "OnDragOverCheck");
			MapEvent (WidgetEvent.DragOver, typeof(Widget), "OnDragOver");
			MapEvent (WidgetEvent.DragDropCheck, typeof(Widget), "OnDragDropCheck");
			MapEvent (WidgetEvent.DragDrop, typeof(Widget), "OnDragDrop");
			MapEvent (WidgetEvent.DragLeave, typeof(Widget), "OnDragLeave");
		}
		
		protected virtual EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		protected EventSink WidgetEventSink {
			get { return eventSink; }
		}
		
		new IWidgetBackend Backend {
			get { return (IWidgetBackend) base.Backend; }
		}
		
		public WidgetSpacing Margin {
			get { return margin; }
		}
		
		public void Show ()
		{
			Backend.Visible = true;
		}
		
		[DefaultValue (true)]
		public bool Visible {
			get { return Backend.Visible; }
			set { Backend.Visible = value; }
		}
		
		[DefaultValue (true)]
		public bool Sensitive {
			get { return Backend.Sensitive; }
			set { Backend.Sensitive = value; }
		}
		
		[DefaultValue (true)]
		public bool CanGetFocus {
			get { return Backend.CanGetFocus; }
			set { Backend.CanGetFocus = value; }
		}
		
		[DefaultValue (true)]
		public bool HasFocus {
			get { return Backend.HasFocus; }
		}
		
		[DefaultValue (null)]
		public string Name { get; set; }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Widget Parent { get; set; }
		
		public bool ShouldSerializeParent ()
		{
			return false;
		}
		
		public void SetFocus ()
		{
			Backend.SetFocus ();
		}
		
		public DragOperation CreateDragOperation ()
		{
			return new DragOperation (this);
		}
		
		internal void DragStart (TransferDataSource data, DragDropAction dragAction)
		{
			Backend.DragStart (data, dragAction);
		}
		
		public void SetDragDropTarget (params string[] types)
		{
			Backend.SetDragTarget (types, DragDropAction.All);
		}
		
		public void SetDragDropTarget (params Type[] types)
		{
			Backend.SetDragTarget (types.Select (t => t.FullName).ToArray (), DragDropAction.All);
		}
		
		public void SetDragDropTarget (DragDropAction dragAction, params string[] types)
		{
			Backend.SetDragTarget (types, dragAction);
		}
		
		public void SetDragDropTarget (DragDropAction dragAction, params Type[] types)
		{
			Backend.SetDragTarget (types.Select (t => t.FullName).ToArray(), dragAction);
		}
		
		public void SetDragSource (params string[] types)
		{
			Backend.SetDragSource (types, DragDropAction.All);
		}
		
		public void SetDragSource (params Type[] types)
		{
			Backend.SetDragSource (types.Select (t => t.FullName).ToArray(), DragDropAction.All);
		}
		
		public void SetDragSource (DragDropAction dragAction, params string[] types)
		{
			Backend.SetDragSource (types, dragAction);
		}
		
		public void SetDragSource (DragDropAction dragAction, params Type[] types)
		{
			Backend.SetDragSource (types.Select (t => t.FullName).ToArray(), dragAction);
		}
		
		protected override void OnBackendCreated ()
		{
			Backend.Initialize (eventSink);
			base.OnBackendCreated ();
		}
		
		internal protected virtual void OnDragOverCheck (DragOverCheckEventArgs args)
		{
			if (dragOverCheck != null)
				dragOverCheck (this, args);
		}
		
		internal protected virtual void OnDragOver (DragOverEventArgs args)
		{
			if (dragOver != null)
				dragOver (this, args);
		}
		
		internal protected virtual void OnDragDropCheck (DragCheckEventArgs args)
		{
			if (dragDropCheck != null)
				dragDropCheck (this, args);
		}
		
		internal protected virtual void OnDragDrop (DragEventArgs args)
		{
			if (dragDrop != null)
				dragDrop (this, args);
		}
		
		internal protected virtual void OnDragLeave (EventArgs args)
		{
			if (dragLeave != null)
				dragLeave (this, args);
		}
		
		protected static IWidgetBackend GetWidgetBackend (Widget w)
		{
			return (IWidgetBackend) GetBackend (w);
		}
		
		void IWidgetSurface.ResetCachedSizes ()
		{
			widthCached = false;
			heightCached = false;
		}
		
		void IWidgetSurface.Reallocate ()
		{
			OnReallocate ();
		}
		
		SizeRequestMode IWidgetSurface.SizeRequestMode {
			get { return OnGetSizeRequestMode (); }
		}
		
		WidgetSize IWidgetSurface.GetPreferredWidth ()
		{
			if (widthCached)
				return width;
			else {
				widthCached = true;
				return width = OnGetPreferredWidth ();
			}
		}
		
		WidgetSize IWidgetSurface.GetPreferredHeight ()
		{
			if (heightCached)
				return height;
			else {
				heightCached = true;
				return height = OnGetPreferredHeight ();
			}
		}
		
		WidgetSize IWidgetSurface.GetPreferredHeightForWidth (double width)
		{
			if (heightCached)
				return height;
			else {
				heightCached = true;
				return height = OnGetPreferredHeightForWidth (width);
			}
		}
		
		WidgetSize IWidgetSurface.GetPreferredWidthForHeight (double height)
		{
			if (widthCached)
				return width;
			else {
				widthCached = true;
				return width = OnGetPreferredWidthForHeight (height);
			}
		}
		
		object IWidgetSurface.NativeWidget {
			get { return Backend.NativeWidget; }
		}
		
		protected virtual void OnReallocate ()
		{
			if (children != null) {
				foreach (IWidgetSurface c in children)
					c.Reallocate ();
			}
		}
		
		protected virtual SizeRequestMode OnGetSizeRequestMode ()
		{
			return SizeRequestMode.HeightForWidth;
		}
		
		protected virtual WidgetSize OnGetPreferredWidth ()
		{
			return Backend.GetPreferredWidth ();
		}
		
		protected virtual WidgetSize OnGetPreferredHeight ()
		{
			return Backend.GetPreferredHeight ();
		}
		
		protected virtual WidgetSize OnGetPreferredHeightForWidth (double width)
		{
			return Backend.GetPreferredHeightForWidth (width);
		}
		
		protected virtual WidgetSize OnGetPreferredWidthForHeight (double height)
		{
			return Backend.GetPreferredWidthForHeight (height);
		}
		
		protected virtual void OnChildPreferredSizeChanged (Widget w)
		{
			if (Parent != null && (!widthCached || !heightCached)) {
				Parent.OnChildPreferredSizeChanged (w);
				return;
			}
			
			var oldWidth = width;
			var oldHeight = height;
			
			bool notifyParent = Parent != null;
			IWidgetSurface surface = this;
			surface.ResetCachedSizes ();
			
			if (surface.SizeRequestMode == SizeRequestMode.HeightForWidth) {
				var nw = surface.GetPreferredWidth ();
				if (nw == oldWidth) {
					var nh = surface.GetPreferredHeightForWidth (Backend.Size.Width);
					if (nh == oldHeight)
						notifyParent = false;
				}
			} else {
				var nh = surface.GetPreferredHeight ();
				if (nh == oldHeight) {
					var nw = surface.GetPreferredWidthForHeight (Backend.Size.Height);
					if (nw == oldWidth)
						notifyParent = false;
				}
			}
			if (notifyParent) {
				if (Parent != null)
					Parent.OnChildPreferredSizeChanged (this);
			}
			else
				surface.Reallocate ();
		}
		
		protected virtual void OnPreferredSizeChanged ()
		{
			IWidgetSurface surface = this;
			surface.ResetCachedSizes ();
			Backend.UpdateLayout ();
			if (Parent != null)
				Parent.OnChildPreferredSizeChanged (this);
		}
		
		internal void NotifyPaddingChanged ()
		{
			OnPreferredSizeChanged ();
		}
		
		public Context CreateContext ()
		{
			return new Context (this);
		}
		
		protected void RegisterChild (Widget w)
		{
			if (children == null)
				children = new List<Widget> ();
			w.Parent = this;
			children.Add (w);
		}
		
		protected void UnregisterChild (Widget w)
		{
			if (children == null || !children.Remove (w))
				throw new InvalidOperationException ("Widget is not a child of this widget");
		}
		
		EventHandler<DragOverCheckEventArgs> dragOverCheck;
		EventHandler<DragOverEventArgs> dragOver;
		EventHandler<DragCheckEventArgs> dragDropCheck;
		EventHandler<DragEventArgs> dragDrop;
		EventHandler dragLeave;
		
		public event EventHandler<DragOverCheckEventArgs> DragOverCheck {
			add {
				OnBeforeEventAdd (WidgetEvent.DragOverCheck, dragOverCheck);
				dragOverCheck += value;
			}
			remove {
				dragOverCheck -= value;
				OnAfterEventRemove (WidgetEvent.DragOverCheck, dragOverCheck);
			}
		}
		
		public event EventHandler<DragOverEventArgs> DragOver {
			add {
				OnBeforeEventAdd (WidgetEvent.DragOver, dragOver);
				dragOver += value;
			}
			remove {
				dragOver -= value;
				OnAfterEventRemove (WidgetEvent.DragOver, dragOver);
			}
		}
		
		public event EventHandler<DragCheckEventArgs> DragDropCheck {
			add {
				OnBeforeEventAdd (WidgetEvent.DragDropCheck, dragDropCheck);
				dragDropCheck += value;
			}
			remove {
				dragDropCheck -= value;
				OnAfterEventRemove (WidgetEvent.DragDropCheck, dragDropCheck);
			}
		}
		
		public event EventHandler<DragEventArgs> DragDrop {
			add {
				OnBeforeEventAdd (WidgetEvent.DragDrop, dragDrop);
				dragDrop += value;
			}
			remove {
				dragDrop -= value;
				OnAfterEventRemove (WidgetEvent.DragDrop, dragDrop);
			}
		}
		
		public event EventHandler DragLeave {
			add {
				OnBeforeEventAdd (WidgetEvent.DragLeave, dragLeave);
				dragLeave += value;
			}
			remove {
				dragLeave -= value;
				OnAfterEventRemove (WidgetEvent.DragLeave, dragLeave);
			}
		}
	}
	
	class EventMap
	{
		public string MethodName;
		public object EventId;
	}
	
	public enum EventResult
	{
		Handled,
		NotHandled
	}
	
	public class WidgetSpacing
	{
		Widget parent;
		int top, left, right, bottom;
		
		internal WidgetSpacing (Widget parent)
		{
			this.parent = parent;
		}
		
		void NotifyChanged ()
		{
			parent.NotifyPaddingChanged ();
		}
		
		public int Left {
			get { return left; }
			set { left = value; NotifyChanged (); }
		}
		
		public int Bottom {
			get {
				return this.bottom;
			}
			set {
				bottom = value; NotifyChanged ();
			}
		}
	
		public int Right {
			get {
				return this.right;
			}
			set {
				right = value; NotifyChanged ();
			}
		}
	
		public int Top {
			get {
				return this.top;
			}
			set {
				top = value; NotifyChanged ();
			}
		}
		
		public int HorizontalSpacing {
			get { return left + right; }
		}
		
		public int VerticalSpacing {
			get { return top + bottom; }
		}
		
		public void Set (int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.bottom = bottom;
			this.right = right;
			NotifyChanged ();
		}
		
		public void SetAll (int padding)
		{
			this.left = padding;
			this.top = padding;
			this.bottom = padding;
			this.right = padding;
			NotifyChanged ();
		}
	}
}

