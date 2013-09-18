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
using Xwt.Motion;

namespace Xwt
{
	[BackendType (typeof(IWidgetBackend))]
	public abstract class Widget: XwtComponent, IWidgetSurface, IAnimatable
	{
		static bool DebugWidgetLayout = false;
		static int DebugWidgetLayoutIndent = 0;

		static Widget[] emptyList = new Widget[0];
		List<Widget> children;
		WidgetSpacing margin;
		Size cachedSize;
		SizeConstraint cachedWidthConstraint;
		SizeConstraint cachedHeightConstraint;
		bool sizeCached;
		DragOperation currentDragOperation;
		Widget contentWidget;
		WindowFrame parentWindow;
		double minWidth = -1, minHeight = -1;
		double widthRequest = -1, heightRequest = -1;
		CursorType cursor;

		WidgetPlacement alignVertical = WidgetPlacement.Fill;
		WidgetPlacement alignHorizontal = WidgetPlacement.Fill;
		bool expandVertical;
		bool expandHorizontal;

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
				if (backend == null) {
					// If this is a custom widget, not implemented in Xwt, then we provide the default
					// backend, which allows setting a content widget
					if (!(Parent is XwtWidgetBackend)) {
						Type t = Parent.GetType ();
						Type wt = typeof(Widget);
						while (t != wt) {
							if (t.Assembly == wt.Assembly)
								return null; // It's a core widget
							t = t.BaseType;
						}
					}
					return ToolkitEngine.Backend.CreateBackend<ICustomWidgetBackend> ();
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
			
			Size IWidgetEventSink.GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
			{
				return Parent.OnGetPreferredSize (widthConstraint, heightConstraint);
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
			MapEvent (WidgetEvent.PreferredSizeCheck, typeof (Widget), "OnGetPreferredSize");
			MapEvent (WidgetEvent.MouseScrolled, typeof(Widget), "OnMouseScrolled");
		}
		
		internal protected static IBackend GetBackend (Widget w)
		{
			if (w != null && w.Backend is XwtWidgetBackend)
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
				OnPlacementChanged ();
			}
		}

		[DefaultValue (0d)]
		public double MarginLeft {
			get { return margin.Left; }
			set {
				margin.Left = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		[DefaultValue (0d)]
		public double MarginRight {
			get { return margin.Right; }
			set {
				margin.Right = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		[DefaultValue (0d)]
		public double MarginTop {
			get { return margin.Top; }
			set {
				margin.Top = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		[DefaultValue (0d)]
		public double MarginBottom {
			get { return margin.Bottom; }
			set {
				margin.Bottom = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		public WidgetPlacement VerticalPlacement {
			get { return alignVertical; }
			set {
				alignVertical = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		public WidgetPlacement HorizontalPlacement {
			get { return alignHorizontal; }
			set {
				alignHorizontal = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		internal WidgetPlacement AlignmentForOrientation (Orientation or)
		{
			if (or == Orientation.Vertical)
				return VerticalPlacement;
			else
				return HorizontalPlacement;
		}

		public bool ExpandVertical {
			get { return expandVertical; }
			set {
				expandVertical = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		public bool ExpandHorizontal {
			get { return expandHorizontal; }
			set {
				expandHorizontal = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		internal bool ExpandsForOrientation (Orientation or)
		{
			if (or == Orientation.Vertical)
				return ExpandVertical;
			else
				return ExpandHorizontal;
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
		
		[DefaultValue (1d)]
		public double Opacity {
			get { return Backend.Opacity; }
			set { Backend.Opacity = value; }
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
		public double WidthRequest {
			get { return widthRequest; }
			set {
				if (value < -1)
					throw new ArgumentException ("NaturalWidth can't be less that -1");
				widthRequest = value;
				Backend.SetSizeRequest (widthRequest >= 0 ? widthRequest : -1, heightRequest >= 0 ? heightRequest : -1);
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
		public double HeightRequest {
			get { return heightRequest; }
			set {
				if (value < -1)
					throw new ArgumentException ("NaturalHeight can't be less that -1");
				heightRequest = value;
				Backend.SetSizeRequest (widthRequest >= 0 ? widthRequest : -1, heightRequest >= 0 ? heightRequest : -1);
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
				return new Font (Backend.Font, BackendHost.ToolkitEngine);
			}
			set {
				Backend.Font = BackendHost.ToolkitEngine.GetSafeBackend (value);
			}
		}
		
		public Color BackgroundColor {
			get { return Backend.BackgroundColor; }
			set { Backend.BackgroundColor = value; }
		}

		[DefaultValue ("")]
		public string TooltipText {
			get { return Backend.TooltipText ?? ""; }
			set { Backend.TooltipText = value; }
		}
		
		/// <summary>
		/// Gets or sets the cursor shape to be used when the mouse is over the widget
		/// </summary>
		/// <value>
		/// The cursor.
		/// </value>
		[DefaultValue ("")]
		public CursorType Cursor {
			get { return cursor ?? CursorType.Arrow; }
			set {
				cursor = value;
				Backend.SetCursor (value);
			}
		}

		public bool ShouldSerializeCursor ()
		{
			return Cursor != CursorType.Arrow;
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

		protected virtual void OnMouseScrolled (MouseScrolledEventArgs args)
		{
			if (mouseScrolled != null)
				mouseScrolled(this, args);
		}

		internal void SetExtractedAsNative ()
		{
			// If the widget is going to be embedded in another toolkit it is not going
			// to receive Reallocate calls from its parent, so the widget has to reallocate
			// itself when its size changes
			if (boundsChanged == null) {
				BoundsChanged += delegate {
					Reallocate ();
				};
			}
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
			sizeCached = false;
		}

		Rectangle IWidgetSurface.GetPlacementInRect (Rectangle rect)
		{
			rect.X += Margin.Left;
			rect.Y += Margin.Top;
			rect.Width -= Margin.HorizontalSpacing;
			rect.Height -= Margin.VerticalSpacing;
			if (HorizontalPlacement != WidgetPlacement.Fill || VerticalPlacement != WidgetPlacement.Fill) {
				var s = Surface.GetPreferredSize (rect.Width, rect.Height);
				if (s.Width > rect.Width)
					s.Width = rect.Width;
				if (s.Height > rect.Height)
					s.Height = rect.Height;
				if (HorizontalPlacement != WidgetPlacement.Fill) {
					rect.X += (rect.Width - s.Width) * HorizontalPlacement.GetValue ();
					rect.Width = s.Width;
				}
				if (VerticalPlacement != WidgetPlacement.Fill) {
					rect.Y += (rect.Height - s.Height) * VerticalPlacement.GetValue ();
					rect.Height = s.Height;
				}
			}
			if (rect.Width < 0)
				rect.Width = 0;
			if (rect.Height < 0)
				rect.Height = 0;
			return rect;
		}
		
		void IWidgetSurface.Reallocate ()
		{
			Reallocate ();
		}

		Size IWidgetSurface.GetPreferredSize (bool includeMargin)
		{
			return ((IWidgetSurface)this).GetPreferredSize (SizeConstraint.Unconstrained, SizeConstraint.Unconstrained, includeMargin);
		}

		Size IWidgetSurface.GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint, bool includeMargin)
		{
			if (includeMargin) {
				return Surface.GetPreferredSize (widthConstraint - margin.HorizontalSpacing, heightConstraint - margin.VerticalSpacing, false) + new Size (Margin.HorizontalSpacing, Margin.VerticalSpacing);
			}

			if (sizeCached && widthConstraint == cachedWidthConstraint && heightConstraint == cachedHeightConstraint)
				return cachedSize;
			else {
				if (widthRequest != -1 && !widthConstraint.IsConstrained)
					widthConstraint = SizeConstraint.WithSize (widthRequest);

				if (heightRequest != -1 && !heightConstraint.IsConstrained)
					heightConstraint = SizeConstraint.WithSize (heightRequest);

				if (DebugWidgetLayout) {
					LayoutLog ("GetPreferredSize: wc:{0} hc:{1} - {2}", widthConstraint, heightConstraint, GetWidgetDesc ());
					DebugWidgetLayoutIndent += 3;
				}

				if (widthRequest == -1 || heightRequest == -1)
					cachedSize = OnGetPreferredSize (widthConstraint, heightConstraint);

				if (DebugWidgetLayout)
					DebugWidgetLayoutIndent -= 3;

				if (widthRequest != -1)
					cachedSize.Width = widthRequest;
				else if (minWidth > cachedSize.Width)
					cachedSize.Width = minWidth;

				if (heightRequest != -1)
					cachedSize.Height = heightRequest;
				else if (minHeight > cachedSize.Height)
					cachedSize.Height = minHeight;

				if (cachedSize.Width < 0)
					cachedSize.Width = 0;
				if (cachedSize.Height < 0)
					cachedSize.Height = 0;
				if (!BackendHost.EngineBackend.HandlesSizeNegotiation) {
					sizeCached = true;
					cachedWidthConstraint = widthConstraint;
					cachedHeightConstraint = heightConstraint;
				}

				if (DebugWidgetLayout) {
					LayoutLog ("-> {0}", cachedSize);
				}

				return cachedSize;
			}
		}

		object IWidgetSurface.NativeWidget {
			get { return Backend.NativeWidget; }
		}
		
		Toolkit IWidgetSurface.ToolkitEngine {
			get { return BackendHost.ToolkitEngine; }
		}

		void Reallocate ()
		{
			reallocationQueue.Remove (this);
			if (DebugWidgetLayout) {
				LayoutLog ("Reallocate: {0} - {1}", Size, GetWidgetDesc ());
				DebugWidgetLayoutIndent += 3;
			}

			OnReallocate ();

			if (children != null && !BackendHost.EngineBackend.HandlesSizeNegotiation) {
				foreach (Widget w in children) {
					if (w.Visible)
						w.Surface.Reallocate ();
				}
			}

			if (DebugWidgetLayout)
				DebugWidgetLayoutIndent -= 3;
		}

		/// <summary>
		/// Called when the size of this widget has changed and its children have to be relocated
		/// </summary>
		/// <remarks>It is not necessary to call Reallocate on the children. The Widget class will do it after invoking this method.</remarks>
		protected virtual void OnReallocate ()
		{
		}

		/// <summary>
		/// Gets the preferred size of the widget (it must not include the widget margin)
		/// </summary>
		protected virtual Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return Backend.GetPreferredSize (widthConstraint, heightConstraint);
		}

		protected virtual void OnChildPreferredSizeChanged ()
		{
		}

		void NotifyChildPreferredSizeChanged ()
		{
			OnChildPreferredSizeChanged ();

			if (Parent != null && resizeRequestQueue.Contains (Parent)) {
				// Size for this widget will be checked when checking the parent
				ResetCachedSizes ();
				return;
			}
			
			// Determine if the size change of the child implies a size change
			// of this widget. If it does, the size change notification
			// has to be propagated to the parent
			
			var oldSize = cachedSize;

			ResetCachedSizes ();
			
			var s = Surface.GetPreferredSize (SizeConstraint.Unconstrained, SizeConstraint.Unconstrained);
			if (s != oldSize)
				NotifySizeChangeToParent ();
			else
				QueueForReallocate (this);
		}
		
		static HashSet<Widget> resizeRequestQueue = new HashSet<Widget> ();
		static HashSet<Widget> reallocationQueue = new HashSet<Widget> ();
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

		internal void OnPlacementChanged ()
		{
			if (Parent != null)
				Parent.OnChildPlacementChanged (this);
			else if (parentWindow is Window)
				((Window)parentWindow).OnChildPlacementChanged (this);
		}

		protected virtual void OnChildPlacementChanged (Widget child)
		{
			var ph = Backend as IChildPlacementHandler;
			if (ph != null)
				ph.UpdateChildPlacement (child.GetBackend ());
			else
				QueueForReallocate ();
		}

		public void QueueForReallocate ()
		{
			reallocationQueue.Add (this);
			QueueDelayedResizeRequest ();
		}

		static void QueueDelayedResizeRequest ()
		{
			if (!delayedSizeNegotiationRequested) {
				delayedSizeNegotiationRequested = true;
				Application.MainLoop.QueueExitAction (DelayedResizeRequest);
			}
		}
		
		void NotifySizeChangeToParent ()
		{
			if (Parent != null) {
				QueueForSizeCheck (Parent);
				QueueDelayedResizeRequest ();
			}
			else if (parentWindow is Window) {
				QueueWindowSizeNegotiation ((Window)parentWindow);
			}
			else if (BackendHost.EngineBackend.HasNativeParent (this)) {
				// This may happen when the widget is embedded in another toolkit. In this case,
				// this is the root widget, so it has to reallocate itself

					QueueForReallocate (this);
			}
		}

		internal static void QueueWindowSizeNegotiation (Window window)
		{
			resizeWindows.Add ((Window)window);
			QueueDelayedResizeRequest ();
		}
		
		void QueueForReallocate (Widget w)
		{
			reallocationQueue.Add (w);
		}
		
		void QueueForSizeCheck (Widget w)
		{
			if (resizeRequestQueue.Add (w))
				resizeWidgets.Add (w);
		}

		static void DelayedResizeRequest ()
		{
			if (DebugWidgetLayout)
				LayoutLog (">> Begin Delayed Relayout");

			// First of all, query the preferred size for those
			// widgets that were changed

			try {
				// We have to recalculate the size of each widget starting from the leafs and ending
				// at the root of the widget hierarchy. We do it in several waves, since new widgets
				// may be added to the list while doing the size checks

				int n = 0;
				int[] depths = null;
				Widget[] items = null;

				while (n < resizeWidgets.Count) {
					int remaining = resizeWidgets.Count - n;
					if (items == null || items.Length < remaining) {
						depths = new int[remaining];
						items = new Widget[remaining];
					}
					resizeWidgets.CopyTo (n, items, 0, remaining);

					for (int k=0; k<remaining; k++)
						depths[k] = items[k].Depth;

					Array.Sort (depths, items, 0, remaining);

					for (int k=remaining - 1; k>=0; k--)
						items[k].NotifyChildPreferredSizeChanged ();

					n += remaining;
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
				resizeWidgets.Clear ();
				reallocationQueue.Clear ();
				resizeWindows.Clear ();
				delayedSizeNegotiationRequested = false;
				if (DebugWidgetLayout)
					LayoutLog (">> End Delayed Relayout");
			}
		}
		
		int Depth {
			get {
				if (Parent != null)
					return Parent.Depth + 1;
				return 0;
			}
		}

		string GetWidgetDesc ()
		{
			if (Parent != null) {
				int i = Parent.Surface.Children.ToList ().IndexOf (this);
				return this + " [" + GetHashCode() + "] (" + i + ")";
			}
			else
				return this.ToString ();
		}

		static void LayoutLog (string str, params object[] args)
		{
			Console.WriteLine (new String (' ', DebugWidgetLayoutIndent) + string.Format (str, args));
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
			if (w.Backend is XwtWidgetBackend)
				((XwtWidgetBackend)w.Backend).Parent = this;
			children.Add (w);

			// Make sure the widget is queued for reallocation
			w.OnPreferredSizeChanged ();
		}
		
		protected void UnregisterChild (Widget w)
		{
			if (children == null || !children.Remove (w))
				throw new InvalidOperationException ("Widget is not a child of this widget");
			w.Parent = null;
			if (w.Backend is XwtWidgetBackend)
				((XwtWidgetBackend)w.Backend).Parent = null;
		}
		
		void IAnimatable.BatchBegin ()
		{
		}

		void IAnimatable.BatchCommit ()
		{
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

