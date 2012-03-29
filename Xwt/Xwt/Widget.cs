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
		static Widget[] emptyList = new Widget[0];
		List<Widget> children;
		WidgetSpacing margin;
		WidgetSize width;
		WidgetSize height;
		bool widthCached;
		bool heightCached;
		EventSink eventSink;
		DragOperation currentDragOperation;
		Widget contentWidget;
		WindowFrame parentWindow;
		double minWidth = -1, minHeight = -1;
		double naturalWidth = -1, naturalHeight = -1;
		CursorType cursor;
		
		EventHandler<DragOverCheckEventArgs> dragOverCheck;
		EventHandler<DragOverEventArgs> dragOver;
		EventHandler<DragCheckEventArgs> dragDropCheck;
		EventHandler<DragEventArgs> dragDrop;
		EventHandler<DragStartedEventArgs> dragStarted;
		EventHandler dragLeave;
		EventHandler<KeyEventArgs> keyPressed;
		EventHandler<KeyEventArgs> keyReleased;
		EventHandler mouseEntered;
		EventHandler mouseExited;
		EventHandler<ButtonEventArgs> buttonPressed;
		EventHandler<ButtonEventArgs> buttonReleased;
		EventHandler<MouseMovedEventArgs> mouseMoved;
		EventHandler boundsChanged;
		
		EventHandler gotFocus;
		EventHandler lostFocus;
		
		protected class EventSink: IWidgetEventSink, ISpacingListener
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
			
			public void OnPreferredSizeChanged ()
			{
				Parent.OnPreferredSizeChanged ();
			}
			
			public void OnDragFinished (DragFinishedEventArgs args)
			{
				Parent.OnDragFinished (args);
			}
			
			public DragStartData OnDragStarted ()
			{
				return Parent.InternalDragStarted ();
			}
			
			public void OnKeyPressed (KeyEventArgs args)
			{
				Parent.OnKeyPressed (args);
			}
			
			public void OnKeyReleased (KeyEventArgs args)
			{
				Parent.OnKeyReleased (args);
			}
			
			public virtual void OnSpacingChanged (WidgetSpacing source)
			{
				if (source == Parent.margin)
					Parent.OnPreferredSizeChanged ();
			}
			
			public WidgetSize OnGetPreferredWidth ()
			{
				return Parent.OnGetPreferredWidth ();
			}
			
			public WidgetSize OnGetPreferredHeight ()
			{
				return Parent.OnGetPreferredWidth ();
			}
			
			public WidgetSize OnGetPreferredHeightForWidth (double width)
			{
				return Parent.OnGetPreferredHeightForWidth (width);
			}
			
			public WidgetSize OnGetPreferredWidthForHeight (double height)
			{
				return Parent.OnGetPreferredWidthForHeight (height);
			}
			
			public SizeRequestMode GetSizeRequestMode ()
			{
				return ((IWidgetSurface)Parent).SizeRequestMode;
			}
			
			public void OnGotFocus ()
			{
				Parent.OnGotFocus (EventArgs.Empty);
			}
			
			public void OnLostFocus ()
			{
				Parent.OnLostFocus (EventArgs.Empty);
			}
			
			public void OnMouseEntered ()
			{
				Parent.OnMouseEntered (EventArgs.Empty);
			}
			
			public void OnMouseExited ()
			{
				Parent.OnMouseExited (EventArgs.Empty);
			}
			
			public void OnButtonPressed (ButtonEventArgs args)
			{
				Parent.OnButtonPressed (args);
			}
			
			public void OnButtonReleased (ButtonEventArgs args)
			{
				Parent.OnButtonReleased (args);
			}
			
			public void OnMouseMoved (MouseMovedEventArgs args)
			{
				Parent.OnMouseMoved (args);
			}
			
			public bool SupportsCustomScrolling ()
			{
				return Parent.SupportsCustomScrolling;
			}
			
			public void SetScrollAdjustments (IScrollAdjustmentBackend horizontal, IScrollAdjustmentBackend vertical)
			{
				var h = new ScrollAdjustment (horizontal);
				var v = new ScrollAdjustment (vertical);
				Parent.SetScrollAdjustments (h, v);
			}
			
			public void OnBoundsChanged ()
			{
				Parent.OnBoundsChanged ();
			}
			
			/// <summary>
			/// Gets the default natural size for this type of widget
			/// </summary>
			/// <returns>
			/// The default natural size.
			/// </returns>
			public virtual Size GetDefaultNaturalSize ()
			{
				return new Size (0, 0);
			}
		}
		
		public Widget ()
		{
			eventSink = CreateEventSink ();
			eventSink.Parent = this;
			margin = new Xwt.WidgetSpacing (eventSink);
		}
		
		static Widget ()
		{
			MapEvent (WidgetEvent.DragOverCheck, typeof(Widget), "OnDragOverCheck");
			MapEvent (WidgetEvent.DragOver, typeof(Widget), "OnDragOver");
			MapEvent (WidgetEvent.DragDropCheck, typeof(Widget), "OnDragDropCheck");
			MapEvent (WidgetEvent.DragDrop, typeof(Widget), "OnDragDrop");
			MapEvent (WidgetEvent.DragLeave, typeof(Widget), "OnDragLeave");
			MapEvent (WidgetEvent.KeyPressed, typeof(Widget), "OnKeyPressed");
			MapEvent (WidgetEvent.KeyReleased, typeof(Widget), "OnKeyPressed");
			MapEvent (WidgetEvent.GotFocus, typeof(Widget), "OnGotFocus");
			MapEvent (WidgetEvent.LostFocus, typeof(Widget), "OnLostFocus");
			MapEvent (WidgetEvent.MouseEntered, typeof(Widget), "OnMouseEntered");
			MapEvent (WidgetEvent.MouseExited, typeof(Widget), "OnMouseExited");
			MapEvent (WidgetEvent.ButtonPressed, typeof(Widget), "OnButtonPressed");
			MapEvent (WidgetEvent.ButtonReleased, typeof(Widget), "OnButtonReleased");
			MapEvent (WidgetEvent.MouseMoved, typeof(Widget), "OnMouseMoved");
			MapEvent (WidgetEvent.DragStarted, typeof(Widget), "OnDragStarted");
			MapEvent (WidgetEvent.BoundsChanged, typeof(Widget), "OnBoundsChanged");
			MapEvent (WidgetEvent.PreferredHeightCheck, typeof (Widget), "OnGetPreferredHeight");
			MapEvent (WidgetEvent.PreferredWidthCheck, typeof (Widget), "OnGetPreferredWidth");
			MapEvent (WidgetEvent.PreferredHeightForWidthCheck, typeof (Widget), "OnGetPreferredHeightForWidth");
			MapEvent (WidgetEvent.PreferredWidthForHeightCheck, typeof (Widget), "OnGetPreferredWidthForHeight");
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			// Don't dispose the backend if this object is being finalized
			// The backend has to handle the finalizing on its own
			if (disposing && BackendCreated)
				Backend.Dispose ();
		}
		
		public WindowFrame ParentWindow {
			get {
				if (Parent != null)
					return Parent.ParentWindow;
				else if (parentWindow == null) {
					var p = Application.EngineBackend.GetNativeParentWindow (this);
					if (p != null)
						parentWindow = WidgetRegistry.WrapWindow (p);
				}
				return parentWindow;
			}
		}
		
		internal void SetParentWindow (WindowFrame win)
		{
			parentWindow = win;
		}
		
		protected virtual EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		protected override IBackend OnCreateBackend ()
		{
			var backend = base.OnCreateBackend ();
			if (backend == null) {
				// If this is a custom widget, not implemented in Xwt, then we provide the default
				// backend, which allows setting a content widget
				Type t = GetType ();
				Type wt = typeof(Widget);
				while (t != wt) {
					if (t.Assembly == wt.Assembly)
						return null; // It's a core widget
					t = t.BaseType;
				}
				return WidgetRegistry.CreateBackend<IBackend> (wt);
			}
			return backend;
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
			Visible = true;
		}
		
		public void Hide ()
		{
			Visible = false;
		}
		
		[DefaultValue (true)]
		public bool Visible {
			get { return Backend.Visible; }
			set {
				Backend.Visible = value; 
				OnPreferredSizeChanged ();
			}
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
		
		protected Widget Content {
			get { return contentWidget; }
			set {
				ICustomWidgetBackend bk = Backend as ICustomWidgetBackend;
				if (bk == null)
					throw new InvalidOperationException ("The Content widget can only be set when directly subclassing Xwt.Widget");
				bk.SetContent ((IWidgetBackend)GetBackend (value));
				if (contentWidget != null)
					UnregisterChild (contentWidget);
				if (value != null)
					RegisterChild (value);
				contentWidget = value;
			}
		}
		
		public Size Size {
			get { return Backend.Size; }
		}
		
		/// <summary>
		/// Gets or sets the minimum width.
		/// </summary>
		/// <value>
		/// The minimum width.
		/// </value>
		/// <remarks>
		/// Minimum width for the widget. If set to -1, the widget's default minimun size will be used.
		/// </remarks>
		[DefaultValue((double)-1)]
		public double MinWidth {
			get { return minWidth; }
			set {
				minWidth = value;
				Backend.SetMinSize (minWidth >= 0 ? minWidth : -1, minHeight >= 0 ? minHeight : -1);
				OnPreferredSizeChanged ();
			}
		}
		
		/// <summary>
		/// Gets or sets the minimum height.
		/// </summary>
		/// <value>
		/// The minimum height.
		/// </value>
		/// <remarks>
		/// Minimum height for the widget. If set to -1, the widget's default minimun size will be used.
		/// </remarks>
		[DefaultValue((double)-1)]
		public double MinHeight {
			get { return minHeight; }
			set {
				minHeight = value;
				Backend.SetMinSize (minWidth >= 0 ? minWidth : -1, minHeight >= 0 ? minHeight : -1);
				OnPreferredSizeChanged ();
			}
		}
		
		/// <summary>
		/// Gets or sets the natural width.
		/// </summary>
		/// <value>
		/// The natural width, or -1 if no custom natural width has been set
		/// </value>
		/// <remarks>
		/// Natural width for the widget. If set to -1, the default natural size is used.
		/// </remarks>
		[DefaultValue((double)-1)]
		public double NaturalWidth {
			get { return minWidth; }
			set {
				naturalWidth = value;
				Backend.SetNaturalSize (naturalWidth >= 0 ? naturalWidth : -1, naturalHeight >= 0 ? naturalHeight : -1);
				OnPreferredSizeChanged ();
			}
		}
		
		/// <summary>
		/// Gets or sets the natural height.
		/// </summary>
		/// <value>
		/// The natural height, or -1 if no custom natural height has been set
		/// </value>
		/// <remarks>
		/// Natural height for the widget. If set to -1, the default natural size is used.
		/// </remarks>
		[DefaultValue((double)-1)]
		public double NaturalHeight {
			get { return naturalHeight; }
			set {
				naturalHeight = value;
				Backend.SetNaturalSize (naturalWidth >= 0 ? naturalWidth : -1, naturalHeight >= 0 ? naturalHeight : -1);
				OnPreferredSizeChanged ();
			}
		}
		
		/// <summary>
		/// Gets or sets the font of the widget.
		/// </summary>
		/// <value>
		/// The font.
		/// </value>
		public Font Font {
			get {
				return new Font (Backend.Font);
			}
			set {
				Backend.Font = WidgetRegistry.GetBackend (value);
			}
		}
		
		public Color BackgroundColor {
			get { return Backend.BackgroundColor; }
			set { Backend.BackgroundColor = value; }
		}
		
		public string TooltipText {
			get { return Backend.TooltipText; }
			set { Backend.TooltipText = value; }
		}
		
		/// <summary>
		/// Gets or sets the cursor shape to be used when the mouse is over the widget
		/// </summary>
		/// <value>
		/// The cursor.
		/// </value>
		public CursorType Cursor {
			get { return cursor ?? CursorType.Arrow; }
			set {
				cursor = value;
				Backend.SetCursor (value);
			}
		}
		
		public Point ConvertToScreenCoordinates (Point widgetCoordinates)
		{
			return Backend.ConvertToScreenCoordinates (widgetCoordinates);
		}
		
		public Rectangle ScreenBounds {
			get { return new Rectangle (ConvertToScreenCoordinates (new Point (0,0)), Size); }
		}
		
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
			currentDragOperation = new DragOperation (this);
			return currentDragOperation;
		}
		
		internal void DragStart (DragStartData sdata)
		{
			Backend.DragStart (sdata);
		}
		
		public void SetDragDropTarget (params TransferDataType[] types)
		{
			Backend.SetDragTarget (types, DragDropAction.All);
		}
		
		public void SetDragDropTarget (params Type[] types)
		{
			Backend.SetDragTarget (types.Select (t => TransferDataType.FromType (t)).ToArray (), DragDropAction.All);
		}
		
		public void SetDragDropTarget (DragDropAction dragAction, params TransferDataType[] types)
		{
			Backend.SetDragTarget (types, dragAction);
		}
		
		public void SetDragDropTarget (DragDropAction dragAction, params Type[] types)
		{
			Backend.SetDragTarget (types.Select (t => TransferDataType.FromType (t)).ToArray(), dragAction);
		}
		
		public void SetDragSource (params TransferDataType[] types)
		{
			Backend.SetDragSource (types, DragDropAction.All);
		}
		
		public void SetDragSource (params Type[] types)
		{
			Backend.SetDragSource (types.Select (t => TransferDataType.FromType (t)).ToArray(), DragDropAction.All);
		}
		
		public void SetDragSource (DragDropAction dragAction, params TransferDataType[] types)
		{
			Backend.SetDragSource (types, dragAction);
		}
		
		public void SetDragSource (DragDropAction dragAction, params Type[] types)
		{
			Backend.SetDragSource (types.Select (t => TransferDataType.FromType (t)).ToArray(), dragAction);
		}
		
		internal protected virtual bool SupportsCustomScrolling {
			get { return false; }
		}
		
		protected virtual void SetScrollAdjustments (ScrollAdjustment horizontal, ScrollAdjustment vertical)
		{
		}

		protected override void OnBackendCreated ()
		{
			Backend.Initialize (eventSink);
			base.OnBackendCreated ();
		}
		
		/// <summary>
		/// Raises the DragOverCheck event.
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		internal protected virtual void OnDragOverCheck (DragOverCheckEventArgs args)
		{
			if (dragOverCheck != null)
				dragOverCheck (this, args);
		}
		
		/// <summary>
		/// Raises the DragOver event.
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		internal protected virtual void OnDragOver (DragOverEventArgs args)
		{
			if (dragOver != null)
				dragOver (this, args);
		}
		
		/// <summary>
		/// Raises the DragDropCheck event.
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		internal protected virtual void OnDragDropCheck (DragCheckEventArgs args)
		{
			if (dragDropCheck != null)
				dragDropCheck (this, args);
		}
		
		/// <summary>
		/// Raises the DragDrop event.
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		internal protected virtual void OnDragDrop (DragEventArgs args)
		{
			if (dragDrop != null)
				dragDrop (this, args);
		}
		
		/// <summary>
		/// Raises the DragLeave event.
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		internal protected virtual void OnDragLeave (EventArgs args)
		{
			if (dragLeave != null)
				dragLeave (this, args);
		}
		
		protected DragStartData InternalDragStarted ()
		{
			DragStartedEventArgs args = new DragStartedEventArgs ();
			args.DragOperation = new DragOperation (this);
			currentDragOperation = args.DragOperation;
			OnDragStarted (args);
			return args.DragOperation.GetStartData ();
		}

		/// <summary>
		/// Raises the DragStarted event.
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		protected virtual void OnDragStarted (DragStartedEventArgs args)
		{
			if (dragStarted != null)
				dragStarted (this, args);
		}
		
		internal void OnDragFinished (DragFinishedEventArgs args)
		{
			if (currentDragOperation != null) {
				var dop = currentDragOperation;
				currentDragOperation = null;
				dop.NotifyFinished (args);
			}
		}
		
		internal protected virtual void OnKeyPressed (KeyEventArgs args)
		{
			if (keyPressed != null)
				keyPressed (this, args);
		}
		
		internal protected virtual void OnKeyReleased (KeyEventArgs args)
		{
			if (keyReleased != null)
				keyReleased (this, args);
		}
		
		internal protected virtual void OnGotFocus (EventArgs args)
		{
			if (gotFocus != null)
				gotFocus (this, args);
		}
		
		internal protected virtual void OnLostFocus (EventArgs args)
		{
			if (lostFocus != null)
				lostFocus (this, args);
		}
		
		/// <summary>
		/// Called when the mouse enters the widget
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		protected virtual void OnMouseEntered (EventArgs args)
		{
			if (mouseEntered != null)
				mouseEntered (this, args);
		}
		
		/// <summary>
		/// Called when the mouse leaves the widget
		/// </summary>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		protected virtual void OnMouseExited (EventArgs args)
		{
			if (mouseExited != null)
				mouseExited (this, args);
		}
		
		protected virtual void OnButtonPressed (ButtonEventArgs args)
		{
			if (buttonPressed != null)
				buttonPressed (this, args);
		}
		
		protected virtual void OnButtonReleased (ButtonEventArgs args)
		{
			if (buttonReleased != null)
				buttonReleased (this, args);
		}
		
		protected virtual void OnMouseMoved (MouseMovedEventArgs args)
		{
			if (mouseMoved != null)
				mouseMoved (this, args);
		}
		
		protected virtual void OnBoundsChanged ()
		{
			if (boundsChanged != null)
				boundsChanged (this, EventArgs.Empty);
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
			reallocationQueue.Remove (this);
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
				if (!Application.EngineBackend.HandlesSizeNegotiation)
					widthCached = true;
				if (minWidth != -1 && naturalWidth != -1)
					return new WidgetSize (minWidth, naturalWidth);
				width = OnGetPreferredWidth () + Margin.HorizontalSpacing;
				if (naturalWidth != -1)
					width.NaturalSize = naturalWidth;
				if (minWidth != -1)
					width.MinSize = minWidth;
				if (width.NaturalSize < width.MinSize)
					width.NaturalSize = width.MinSize;
				return width;
			}
		}
		
		WidgetSize IWidgetSurface.GetPreferredHeight ()
		{
			if (heightCached)
				return height;
			else {
				if (!Application.EngineBackend.HandlesSizeNegotiation)
					heightCached = true;
				if (minHeight != -1 && naturalHeight != -1)
					return new WidgetSize (minHeight, naturalHeight);
				height = OnGetPreferredHeight () + Margin.VerticalSpacing;
				if (naturalHeight != -1)
					height.NaturalSize = naturalHeight;
				if (minHeight != -1)
					height.MinSize = minHeight;
				if (height.NaturalSize < height.MinSize)
					height.NaturalSize = height.MinSize;
				return height;
			}
		}
		
		WidgetSize IWidgetSurface.GetPreferredHeightForWidth (double width)
		{
			if (heightCached)
				return height;
			else {
				if (!Application.EngineBackend.HandlesSizeNegotiation)
					heightCached = true;
				if (minHeight != -1 && naturalHeight != -1)
					return new WidgetSize (minHeight, naturalHeight);
				// Horizontal margin is substracted here because that's space which
				// can't really be used to render the widget
				width = Math.Max (width - Margin.HorizontalSpacing, 0);
				height = OnGetPreferredHeightForWidth (width) + Margin.VerticalSpacing;
				if (naturalHeight != -1)
					height.NaturalSize = naturalHeight;
				if (minHeight != -1)
					height.MinSize = minHeight;
				if (height.NaturalSize < height.MinSize)
					height.NaturalSize = height.MinSize;
				return height;
			}
		}
		
		WidgetSize IWidgetSurface.GetPreferredWidthForHeight (double height)
		{
			if (widthCached)
				return width;
			else {
				if (!Application.EngineBackend.HandlesSizeNegotiation)
					widthCached = true;
				if (minWidth != -1 && naturalWidth != -1)
					return new WidgetSize (minWidth, naturalWidth);
				// Vertical margin is substracted here because that's space which
				// can't really be used to render the widget
				height = Math.Max (height - Margin.VerticalSpacing, 0);
				width = OnGetPreferredWidthForHeight (height) + Margin.HorizontalSpacing;
				if (naturalWidth != -1)
					width.NaturalSize = naturalWidth;
				if (minWidth != -1)
					width.MinSize = minWidth;
				if (width.NaturalSize < width.MinSize)
					width.NaturalSize = width.MinSize;
				return width;
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
		
		
		/// <summary>
		/// Gets the preferred width of the widget (it must not include the widget margin)
		/// </summary>
		protected virtual WidgetSize OnGetPreferredWidth ()
		{
			return Backend.GetPreferredWidth ();
		}
		
		/// <summary>
		/// Gets the preferred height of the widget (it must not include the widget margin)
		/// </summary>
		protected virtual WidgetSize OnGetPreferredHeight ()
		{
			return Backend.GetPreferredHeight ();
		}
		
		/// <summary>
		/// Gets the preferred height of the widget for a given width (it must not include the widget margin)
		/// </summary>
		protected virtual WidgetSize OnGetPreferredHeightForWidth (double width)
		{
			return Backend.GetPreferredHeightForWidth (width);
		}
		
		/// <summary>
		/// Gets the preferred width of the widget for a given height (it must not include the widget margin)
		/// </summary>
		protected virtual WidgetSize OnGetPreferredWidthForHeight (double height)
		{
			return Backend.GetPreferredWidthForHeight (height);
		}
		
		void OnChildPreferredSizeChanged ()
		{
			IWidgetSurface surface = this;
			
			if (Parent != null && resizeRequestQueue.Contains (Parent)) {
				// Size for this widget will be checked when checking the parent
				surface.ResetCachedSizes ();
				return;
			}
			
			// Determine if the size change of the child implies a size change
			// of this widget. If it does, the size change notification
			// has to be propagated to the parent
			
			var oldWidth = width;
			var oldHeight = height;
			
			surface.ResetCachedSizes ();
			
			bool changed = true;
			
			if (surface.SizeRequestMode == SizeRequestMode.HeightForWidth) {
				var nw = surface.GetPreferredWidth ();
				if (nw == oldWidth) {
					var nh = surface.GetPreferredHeightForWidth (Backend.Size.Width);
					if (nh == oldHeight)
						changed = false;
				}
			} else {
				var nh = surface.GetPreferredHeight ();
				if (nh == oldHeight) {
					var nw = surface.GetPreferredWidthForHeight (Backend.Size.Height);
					if (nw == oldWidth)
						changed = false;
				}
			}
			if (changed)
				NotifySizeChangeToParent ();
			else
				QueueForReallocate (this);
		}
		
		static HashSet<Widget> resizeRequestQueue = new HashSet<Widget> ();
		static HashSet<Widget> reallocationQueue = new HashSet<Widget> ();
		static List<int> resizeDepths = new List<int> ();
		static List<Widget> resizeWidgets = new List<Widget> ();
		static List<Window> resizeWindows = new List<Window> ();
		static bool delayedSizeNegotiationRequested;
		
		protected virtual void OnPreferredSizeChanged ()
		{
			// When the preferred size changes, we reset the sizes we have cached
			// The parent also has to be notified of the size change, since it
			// may imply a change of the size of the parent. However, we don't do
			// it immediately, but we queue the resizing request
			
			IWidgetSurface surface = this;
			surface.ResetCachedSizes ();
			Backend.UpdateLayout ();
			if (!Application.EngineBackend.HandlesSizeNegotiation)
				NotifySizeChangeToParent ();
		}
		
		void NotifySizeChangeToParent ()
		{
			if (Parent != null) {
				QueueForSizeCheck (Parent);
				if (!delayedSizeNegotiationRequested) {
					delayedSizeNegotiationRequested = true;
					Toolkit.QueueExitAction (DelayedResizeRequest);
				}
			} else if (ParentWindow is Window) {
				QueueWindowSizeNegotiation ((Window)ParentWindow);
			}
		}

		internal static void QueueWindowSizeNegotiation (Window window)
		{
			resizeWindows.Add ((Window)window);
			if (!delayedSizeNegotiationRequested) {
				delayedSizeNegotiationRequested = true;
				Toolkit.QueueExitAction (DelayedResizeRequest);
			}
		}
		
		void QueueForReallocate (Widget w)
		{
			reallocationQueue.Add (w);
		}
		
		void QueueForSizeCheck (Widget w)
		{
			if (resizeRequestQueue.Add (w)) {
				int depth = w.Depth;
				bool inserted = false;
				for (int n=0; n<resizeDepths.Count; n++) {
					if (resizeDepths[n] < depth) {
						resizeDepths.Insert (n, depth);
						resizeWidgets.Insert (n, w);
						inserted = true;
						break;
					}
				}
				if (!inserted) {
					resizeDepths.Add (depth);
					resizeWidgets.Add (w);
				}
			}
		}
		
		static void DelayedResizeRequest ()
		{
			// First of all, query the preferred size for those
			// widgets that were changed

			try {
				int n = 0;
				while (n < resizeWidgets.Count) {
					var w = resizeWidgets[n];
					w.OnChildPreferredSizeChanged ();
					n++;
				}
				
				// Now reallocate the widgets whose size has actually changed
				
				var toReallocate = reallocationQueue.OrderBy (w => w.Depth).ToArray ();
				foreach (var w in toReallocate) {
					// The widget may already have been reallocated as a result of reallocating the parent
					// so we have to check if it is still in the queue
					if (reallocationQueue.Contains (w))
						((IWidgetSurface)w).Reallocate ();
				}
				foreach (var w in resizeWindows.ToArray ()) {
					w.AdjustSize ();
					w.Reallocate ();
				}
			} finally {
				resizeRequestQueue.Clear ();
				resizeDepths.Clear ();
				resizeWidgets.Clear ();
				reallocationQueue.Clear ();
				resizeWindows.Clear ();
				delayedSizeNegotiationRequested = false;
			}
		}
		
		int Depth {
			get {
				if (Parent != null)
					return Parent.Depth + 1;
				return 0;
			}
		}
		
		IEnumerable<Widget> IWidgetSurface.Children {
			get {
				return (IEnumerable<Widget>)children ?? (IEnumerable<Widget>) emptyList; 
			}
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
		
		/// <summary>
		/// Raised when the mouse is moved over the widget in a drag&drop operation
		/// </summary>
		/// <remarks>
		/// The subscriber of the event should set the value of AllowedAction in the
		/// provided event args object. If the value is not set or it is set to Default,
		/// the action will be determined by the result of the DragOver event.
		/// 
		/// This event provides information about the type of the data that is going
		/// to be dropped, but not the actual data. If you need the actual data
		/// to decide if the drop is allowed or not, you have to subscribe the DragOver
		/// event.
		/// </remarks>
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
		
		/// <summary>
		/// Raised when the mouse is moved over the widget in a drag&drop operation
		/// </summary>
		/// <remarks>
		/// The subscriber of the event should set the value of AllowedAction in the
		/// provided event args object. If the value is not set or it is set to Default,
		/// the action will be determined by the result of the DragDropCheck event.
		/// 
		/// This event provides information about the actual data that is going
		/// to be dropped. Getting the data may be inneficient in some cross-process drag&drop scenarios,
		/// so if you don't need the actual data to decide the allowed drop operation, 
		/// and knowing the type of the data is enough, then the DragOverCheck event is a better option.
		/// </remarks>
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
		
		/// <summary>
		/// Raised to check if a drop operation is allowed on the widget
		/// </summary>
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
		
		/// <summary>
		/// Raised when dropping on the widget
		/// </summary>
		/// <remarks>
		/// The subscriber of the event should set the value of Success in the
		/// provided event args object.
		/// </remarks>
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
		
		public event EventHandler<DragStartedEventArgs> DragStarted {
			add {
				OnBeforeEventAdd (WidgetEvent.DragStarted, dragStarted);
				dragStarted += value;
			}
			remove {
				dragStarted -= value;
				OnAfterEventRemove (WidgetEvent.DragStarted, dragStarted);
			}
		}
		
		public event EventHandler<KeyEventArgs> KeyPressed {
			add {
				OnBeforeEventAdd (WidgetEvent.KeyPressed, keyPressed);
				keyPressed += value;
			}
			remove {
				keyPressed -= value;
				OnAfterEventRemove (WidgetEvent.KeyPressed, keyPressed);
			}
		}
		
		public event EventHandler<KeyEventArgs> KeyReleased {
			add {
				OnBeforeEventAdd (WidgetEvent.KeyReleased, keyReleased);
				keyReleased += value;
			}
			remove {
				keyReleased -= value;
				OnAfterEventRemove (WidgetEvent.KeyReleased, keyReleased);
			}
		}
		
		/// <summary>
		/// Raised when the widget gets the focus
		/// </summary>
		public event EventHandler GotFocus {
			add {
				OnBeforeEventAdd (WidgetEvent.GotFocus, gotFocus);
				gotFocus += value;
			}
			remove {
				gotFocus -= value;
				OnAfterEventRemove (WidgetEvent.GotFocus, gotFocus);
			}
		}
		
		/// <summary>
		/// Raised when the widget loses the focus
		/// </summary>
		public event EventHandler LostFocus {
			add {
				OnBeforeEventAdd (WidgetEvent.LostFocus, lostFocus);
				lostFocus += value;
			}
			remove {
				lostFocus -= value;
				OnAfterEventRemove (WidgetEvent.LostFocus, lostFocus);
			}
		}

		/// <summary>
		/// Occurs when the mouse enters the widget
		/// </summary>
		public event EventHandler MouseEntered {
			add {
				OnBeforeEventAdd (WidgetEvent.MouseEntered, mouseEntered);
				mouseEntered += value;
			}
			remove {
				mouseEntered -= value;
				OnAfterEventRemove (WidgetEvent.MouseEntered, mouseEntered);
			}
		}

		/// <summary>
		/// Occurs when the mouse exits the widget
		/// </summary>
		public event EventHandler MouseExited {
			add {
				OnBeforeEventAdd (WidgetEvent.MouseExited, mouseExited);
				mouseExited += value;
			}
			remove {
				mouseExited -= value;
				OnAfterEventRemove (WidgetEvent.MouseExited, mouseExited);
			}
		}
		
		public event EventHandler<ButtonEventArgs> ButtonPressed {
			add {
				OnBeforeEventAdd (WidgetEvent.ButtonPressed, buttonPressed);
				buttonPressed += value;
			}
			remove {
				buttonPressed -= value;
				OnAfterEventRemove (WidgetEvent.ButtonPressed, buttonPressed);
			}
		}
		
		public event EventHandler<ButtonEventArgs> ButtonReleased {
			add {
				OnBeforeEventAdd (WidgetEvent.ButtonReleased, buttonReleased);
				buttonReleased += value;
			}
			remove {
				buttonReleased -= value;
				OnAfterEventRemove (WidgetEvent.ButtonReleased, buttonReleased);
			}
		}
		
		public event EventHandler<MouseMovedEventArgs> MouseMoved {
			add {
				OnBeforeEventAdd (WidgetEvent.MouseMoved, mouseMoved);
				mouseMoved += value;
			}
			remove {
				mouseMoved -= value;
				OnAfterEventRemove (WidgetEvent.MouseMoved, mouseMoved);
			}
		}
		
		public event EventHandler BoundsChanged {
			add {
				OnBeforeEventAdd (WidgetEvent.BoundsChanged, boundsChanged);
				boundsChanged += value;
			}
			remove {
				boundsChanged -= value;
				OnAfterEventRemove (WidgetEvent.BoundsChanged, boundsChanged);
			}
		}
	}
	
	class EventMap
	{
		public string MethodName;
		public object EventId;
	}
}

