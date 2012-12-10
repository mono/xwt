// 
// Widget.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Wolfgang Silbermayr <wolfgang.silbermayr@gmail.com>
// 
// Copyright (c) 2011 Xamarin Inc
// Copyright (C) 2012 Wolfgang Silbermayr
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
        EventHandler<MouseScrolledEventArgs> mouseScrolled;
		
		EventHandler gotFocus;
		EventHandler lostFocus;
		
		protected class WidgetBackendHost<T,B>: WidgetBackendHost where T:Widget where B:IWidgetBackend
		{
			public new T Parent {
				get { return (T) base.Parent; }
				set { base.Parent = value; }
			}
			public new B Backend {
				get { return (B) base.Backend; }
			}
		}
		
		protected class WidgetBackendHost: BackendHost<Widget, IWidgetBackend>, IWidgetEventSink
		{
			public WidgetBackendHost ()
			{
			}
			
			protected override IBackend OnCreateBackend ()
			{
				var backend = base.OnCreateBackend ();
				if (backend == null || backend is XwtWidgetBackend) {
					// If this is a custom widget, not implemented in Xwt, then we provide the default
					// backend, which allows setting a content widget
					Type t = Parent.GetType ();
					Type wt = typeof(Widget);
					while (t != wt) {
						if (t.Assembly == wt.Assembly)
							return null; // It's a core widget
						t = t.BaseType;
					}
					return ToolkitEngine.Backend.CreateBackend<IBackend> (wt);
				}
				return backend;
			}

			protected override void OnBackendCreated ()
			{
				((IWidgetBackend)Backend).Initialize (this);
				base.OnBackendCreated ();
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
			
			void IWidgetEventSink.OnDragOverCheck (DragOverCheckEventArgs args)
			{
				Parent.OnDragOverCheck (args);
			}

			void IWidgetEventSink.OnDragOver (DragOverEventArgs args)
			{
				Parent.OnDragOver (args);
			}

			void IWidgetEventSink.OnDragDropCheck (DragCheckEventArgs args)
			{
				Parent.OnDragDropCheck (args);
			}

			void IWidgetEventSink.OnDragDrop (DragEventArgs args)
			{
				Parent.OnDragDrop (args);
			}

			void IWidgetEventSink.OnDragLeave (EventArgs args)
			{
				Parent.OnDragLeave (args);
			}
			
			void IWidgetEventSink.OnPreferredSizeChanged ()
			{
				Parent.OnPreferredSizeChanged ();
			}
			
			void IWidgetEventSink.OnDragFinished (DragFinishedEventArgs args)
			{
				Parent.OnDragFinished (args);
			}
			
			DragStartData IWidgetEventSink.OnDragStarted ()
			{
				return Parent.InternalDragStarted ();
			}
			
			void IWidgetEventSink.OnKeyPressed (KeyEventArgs args)
			{
				Parent.OnKeyPressed (args);
			}
			
			void IWidgetEventSink.OnKeyReleased (KeyEventArgs args)
			{
				Parent.OnKeyReleased (args);
			}
			
			WidgetSize IWidgetEventSink.OnGetPreferredWidth ()
			{
				return Parent.OnGetPreferredWidth ();
			}
			
			WidgetSize IWidgetEventSink.OnGetPreferredHeight ()
			{
				return Parent.OnGetPreferredWidth ();
			}
			
			WidgetSize IWidgetEventSink.OnGetPreferredHeightForWidth (double width)
			{
				return Parent.OnGetPreferredHeightForWidth (width);
			}
			
			WidgetSize IWidgetEventSink.OnGetPreferredWidthForHeight (double height)
			{
				return Parent.OnGetPreferredWidthForHeight (height);
			}
			
			SizeRequestMode IWidgetEventSink.GetSizeRequestMode ()
			{
				return Parent.Surface.SizeRequestMode;
			}
			
			void IWidgetEventSink.OnGotFocus ()
			{
				Parent.OnGotFocus (EventArgs.Empty);
			}
			
			void IWidgetEventSink.OnLostFocus ()
			{
				Parent.OnLostFocus (EventArgs.Empty);
			}
			
			void IWidgetEventSink.OnMouseEntered ()
			{
				Parent.OnMouseEntered (EventArgs.Empty);
			}
			
			void IWidgetEventSink.OnMouseExited ()
			{
				Parent.OnMouseExited (EventArgs.Empty);
			}
			
			void IWidgetEventSink.OnButtonPressed (ButtonEventArgs args)
			{
				Parent.OnButtonPressed (args);
			}
			
			void IWidgetEventSink.OnButtonReleased (ButtonEventArgs args)
			{
				Parent.OnButtonReleased (args);
			}
			
			void IWidgetEventSink.OnMouseMoved (MouseMovedEventArgs args)
			{
				Parent.OnMouseMoved (args);
			}
			
			bool IWidgetEventSink.SupportsCustomScrolling ()
			{
				return Parent.SupportsCustomScrolling;
			}
			
			void IWidgetEventSink.SetScrollAdjustments (IScrollAdjustmentBackend horizontal, IScrollAdjustmentBackend vertical)
			{
				var h = new ScrollAdjustment (horizontal);
				var v = new ScrollAdjustment (vertical);
				Parent.SetScrollAdjustments (h, v);
			}
			
			void IWidgetEventSink.OnBoundsChanged ()
			{
				Parent.OnBoundsChanged ();
			}

            void IWidgetEventSink.OnMouseScrolled(MouseScrolledEventArgs args)
            {
                Parent.OnMouseScrolled(args);
            }
		}
		
		public Widget ()
		{
			if (!(base.BackendHost is WidgetBackendHost))
				throw new InvalidOperationException ("CreateBackendHost for Widget did not return a WidgetBackendHost instance");
		}
		
		static Widget ()
		{
			MapEvent (WidgetEvent.DragOverCheck, typeof(Widget), "OnDragOverCheck");
			MapEvent (WidgetEvent.DragOver, typeof(Widget), "OnDragOver");
			MapEvent (WidgetEvent.DragDropCheck, typeof(Widget), "OnDragDropCheck");
			MapEvent (WidgetEvent.DragDrop, typeof(Widget), "OnDragDrop");
			MapEvent (WidgetEvent.DragLeave, typeof(Widget), "OnDragLeave");
			MapEvent (WidgetEvent.KeyPressed, typeof(Widget), "OnKeyPressed");
			MapEvent (WidgetEvent.KeyReleased, typeof(Widget), "OnKeyReleased");
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
			MapEvent (WidgetEvent.MouseScrolled, typeof(Widget), "OnMouseScrolled");
		}
		
		internal protected static IBackend GetBackend (Widget w)
		{
			if (w.Backend is XwtWidgetBackend)
				return GetBackend ((XwtWidgetBackend)w.Backend);
			return w != null ? w.Backend : null;
		}
		
		protected new WidgetBackendHost BackendHost {
			get { return (WidgetBackendHost) base.BackendHost; }
		}
		
		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			// Don't dispose the backend if this object is being finalized
			// The backend has to handle the finalizing on its own
			if (disposing) {
				if (BackendHost.BackendCreated)
					Backend.Dispose ();
				if (children != null)
					children.ForEach (c => c.Dispose ());
			}
		}
		
		public WindowFrame ParentWindow {
			get {
				if (Parent != null)
					return Parent.ParentWindow;
				else if (parentWindow == null) {
					var p = BackendHost.EngineBackend.GetNativeParentWindow (this);
					if (p != null)
						parentWindow = BackendHost.ToolkitEngine.WrapWindow (p);
				}
				return parentWindow;
			}
		}
		
		internal void SetParentWindow (WindowFrame win)
		{
			parentWindow = win;
		}
		
/*		protected virtual WidgetBackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		protected WidgetBackendHost BackendHost {
			get { return backendHost; }
		}*/
		
		IWidgetBackend Backend {
			get { return (IWidgetBackend) BackendHost.Backend; }
		}
		
		public WidgetSpacing Margin {
			get { return margin; }
			set {
				margin = value;
				OnPreferredSizeChanged ();
			}
		}

		public double MarginLeft {
			get { return margin.Left; }
			set {
				margin.Left = value;
				OnPreferredSizeChanged (); 
			}
		}

		public double MarginRight {
			get { return margin.Right; }
			set {
				margin.Right = value;
				OnPreferredSizeChanged (); 
			}
		}

		public double MarginTop {
			get { return margin.Top; }
			set {
				margin.Top = value;
				OnPreferredSizeChanged (); 
			}
		}

		public double MarginBottom {
			get { return margin.Bottom; }
			set {
				margin.Bottom = value;
				OnPreferredSizeChanged (); 
			}
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
		public Widget Parent { get; private set; }
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IWidgetSurface Surface {
			get { return this; }
		}
		
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
				OnPreferredSizeChanged ();
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
				if (value < -1)
					throw new ArgumentException ("MinWidth can't be less that -1");
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
				if (value < -1)
					throw new ArgumentException ("MinHeight can't be less that -1");
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
				if (value < -1)
					throw new ArgumentException ("NaturalWidth can't be less that -1");
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
				if (value < -1)
					throw new ArgumentException ("NaturalHeight can't be less that -1");
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
				Backend.Font = BackendHost.ToolkitEngine.GetSafeBackend (value);
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

		/// <summary>
		/// Gets the bounds of the widget in screen coordinates
		/// </summary>
		/// <value>
		/// The widget bounds
		/// </value>
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
		
		void HandleBoundsChanged ()
		{
			// If bounds have changed and the widget has not a parent, chances are that
			// the widget is embedded in a native application, in which case we
			// have to call Reallocate here (which is normally called by the root XWT window)
			if (!BackendHost.EngineBackend.HandlesSizeNegotiation && Parent == null)
				Surface.Reallocate ();
			
			OnBoundsChanged ();
		}

    protected virtual void OnMouseScrolled(MouseScrolledEventArgs args)
    {
        if (mouseScrolled != null)
            mouseScrolled(this, args);
    }

		internal void SetExtractedAsNative ()
		{
			// If the widget is going to be embedded in another toolkit it is not going
			// to receive Reallocate calls from its parent, so the widget has to reallocate
			// itself when its size changes
			BoundsChanged += delegate {
				OnReallocate ();
			};
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
		
		void ResetCachedSizes ()
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
				if (minWidth != -1 && naturalWidth != -1)
					return new WidgetSize (minWidth, naturalWidth);
				width = OnGetPreferredWidth () + Margin.HorizontalSpacing;
				if (naturalWidth != -1)
					width.NaturalSize = naturalWidth;
				if (minWidth != -1)
					width.MinSize = minWidth;
				if (width.NaturalSize < width.MinSize)
					width.NaturalSize = width.MinSize;
				if (!BackendHost.EngineBackend.HandlesSizeNegotiation)
					widthCached = true;
				return width;
			}
		}
		
		WidgetSize IWidgetSurface.GetPreferredHeight ()
		{
			if (heightCached)
				return height;
			else {
				if (minHeight != -1 && naturalHeight != -1)
					return new WidgetSize (minHeight, naturalHeight);
				height = OnGetPreferredHeight () + Margin.VerticalSpacing;
				if (naturalHeight != -1)
					height.NaturalSize = naturalHeight;
				if (minHeight != -1)
					height.MinSize = minHeight;
				if (height.NaturalSize < height.MinSize)
					height.NaturalSize = height.MinSize;
				if (!BackendHost.EngineBackend.HandlesSizeNegotiation)
					heightCached = true;
				return height;
			}
		}
		
		WidgetSize IWidgetSurface.GetPreferredHeightForWidth (double width)
		{
			if (heightCached)
				return height;
			else {
				if (!BackendHost.EngineBackend.HandlesSizeNegotiation)
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
				if (!BackendHost.EngineBackend.HandlesSizeNegotiation)
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
		
		Toolkit IWidgetSurface.ToolkitEngine {
			get { return BackendHost.ToolkitEngine; }
		}
		
		protected virtual void OnReallocate ()
		{
			if (children != null) {
				foreach (Widget w in children)
					w.Surface.Reallocate ();
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
			if (Parent != null && resizeRequestQueue.Contains (Parent)) {
				// Size for this widget will be checked when checking the parent
				ResetCachedSizes ();
				return;
			}
			
			// Determine if the size change of the child implies a size change
			// of this widget. If it does, the size change notification
			// has to be propagated to the parent
			
			var oldWidth = width;
			var oldHeight = height;
			
			ResetCachedSizes ();
			
			bool changed = true;
			
			if (Surface.SizeRequestMode == SizeRequestMode.HeightForWidth) {
				var nw = Surface.GetPreferredWidth ();
				if (nw == oldWidth) {
					var nh = Surface.GetPreferredHeightForWidth (Backend.Size.Width);
					if (nh == oldHeight)
						changed = false;
				}
			} else {
				var nh = Surface.GetPreferredHeight ();
				if (nh == oldHeight) {
					var nw = Surface.GetPreferredWidthForHeight (Backend.Size.Height);
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
			
			ResetCachedSizes ();
			Backend.UpdateLayout ();
			if (!BackendHost.EngineBackend.HandlesSizeNegotiation)
				NotifySizeChangeToParent ();
		}
		
		void NotifySizeChangeToParent ()
		{
			if (Parent != null) {
				QueueForSizeCheck (Parent);
				if (!delayedSizeNegotiationRequested) {
					delayedSizeNegotiationRequested = true;
					Application.MainLoop.QueueExitAction (DelayedResizeRequest);
				}
			}
			else if (parentWindow is Window) {
				QueueWindowSizeNegotiation ((Window)parentWindow);
			}
			else if (Application.EngineBackend.HasNativeParent (this)) {
				// This may happen when the widget is embedded in another toolkit. In this case,
				// this is the root widget, so it has to reallocate itself

					QueueForReallocate (this);
			}
		}

		internal static void QueueWindowSizeNegotiation (Window window)
		{
			resizeWindows.Add ((Window)window);
			if (!delayedSizeNegotiationRequested) {
				delayedSizeNegotiationRequested = true;
				Application.MainLoop.QueueExitAction (DelayedResizeRequest);
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
						w.Surface.Reallocate ();
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
			if (w.Parent != null)
				throw new InvalidOperationException ("Widget is already a child of another widget");
			if (w.Surface.ToolkitEngine != Surface.ToolkitEngine)
				throw new InvalidOperationException ("Widget belongs to a different toolkit");
			if (children == null)
				children = new List<Widget> ();
			w.Parent = this;
			children.Add (w);
		}
		
		protected void UnregisterChild (Widget w)
		{
			if (children == null || !children.Remove (w))
				throw new InvalidOperationException ("Widget is not a child of this widget");
			w.Parent = null;
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
				BackendHost.OnBeforeEventAdd (WidgetEvent.DragOverCheck, dragOverCheck);
				dragOverCheck += value;
			}
			remove {
				dragOverCheck -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.DragOverCheck, dragOverCheck);
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
				BackendHost.OnBeforeEventAdd (WidgetEvent.DragOver, dragOver);
				dragOver += value;
			}
			remove {
				dragOver -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.DragOver, dragOver);
			}
		}
		
		/// <summary>
		/// Raised to check if a drop operation is allowed on the widget
		/// </summary>
		public event EventHandler<DragCheckEventArgs> DragDropCheck {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.DragDropCheck, dragDropCheck);
				dragDropCheck += value;
			}
			remove {
				dragDropCheck -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.DragDropCheck, dragDropCheck);
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
				BackendHost.OnBeforeEventAdd (WidgetEvent.DragDrop, dragDrop);
				dragDrop += value;
			}
			remove {
				dragDrop -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.DragDrop, dragDrop);
			}
		}
		
		public event EventHandler DragLeave {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.DragLeave, dragLeave);
				dragLeave += value;
			}
			remove {
				dragLeave -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.DragLeave, dragLeave);
			}
		}
		
		public event EventHandler<DragStartedEventArgs> DragStarted {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.DragStarted, dragStarted);
				dragStarted += value;
			}
			remove {
				dragStarted -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.DragStarted, dragStarted);
			}
		}
		
		public event EventHandler<KeyEventArgs> KeyPressed {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.KeyPressed, keyPressed);
				keyPressed += value;
			}
			remove {
				keyPressed -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.KeyPressed, keyPressed);
			}
		}
		
		public event EventHandler<KeyEventArgs> KeyReleased {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.KeyReleased, keyReleased);
				keyReleased += value;
			}
			remove {
				keyReleased -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.KeyReleased, keyReleased);
			}
		}
		
		/// <summary>
		/// Raised when the widget gets the focus
		/// </summary>
		public event EventHandler GotFocus {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.GotFocus, gotFocus);
				gotFocus += value;
			}
			remove {
				gotFocus -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.GotFocus, gotFocus);
			}
		}
		
		/// <summary>
		/// Raised when the widget loses the focus
		/// </summary>
		public event EventHandler LostFocus {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.LostFocus, lostFocus);
				lostFocus += value;
			}
			remove {
				lostFocus -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.LostFocus, lostFocus);
			}
		}

		/// <summary>
		/// Occurs when the mouse enters the widget
		/// </summary>
		public event EventHandler MouseEntered {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.MouseEntered, mouseEntered);
				mouseEntered += value;
			}
			remove {
				mouseEntered -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.MouseEntered, mouseEntered);
			}
		}

		/// <summary>
		/// Occurs when the mouse exits the widget
		/// </summary>
		public event EventHandler MouseExited {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.MouseExited, mouseExited);
				mouseExited += value;
			}
			remove {
				mouseExited -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.MouseExited, mouseExited);
			}
		}
		
		public event EventHandler<ButtonEventArgs> ButtonPressed {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.ButtonPressed, buttonPressed);
				buttonPressed += value;
			}
			remove {
				buttonPressed -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.ButtonPressed, buttonPressed);
			}
		}
		
		public event EventHandler<ButtonEventArgs> ButtonReleased {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.ButtonReleased, buttonReleased);
				buttonReleased += value;
			}
			remove {
				buttonReleased -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.ButtonReleased, buttonReleased);
			}
		}
		
		public event EventHandler<MouseMovedEventArgs> MouseMoved {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.MouseMoved, mouseMoved);
				mouseMoved += value;
			}
			remove {
				mouseMoved -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.MouseMoved, mouseMoved);
			}
		}
		
		public event EventHandler BoundsChanged {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.BoundsChanged, boundsChanged);
				boundsChanged += value;
			}
			remove {
				boundsChanged -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.BoundsChanged, boundsChanged);
			}
		}

		public event EventHandler<MouseScrolledEventArgs> MouseScrolled {
			add {
				BackendHost.OnBeforeEventAdd(WidgetEvent.MouseScrolled, mouseScrolled);
					mouseScrolled += value;
			}
			remove {
				mouseScrolled -= value;
				BackendHost.OnAfterEventRemove(WidgetEvent.MouseScrolled, mouseScrolled);
			}
		}
	}
	
	class EventMap
	{
		public string MethodName;
		public object EventId;
	}
}

