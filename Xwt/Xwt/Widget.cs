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
	[BackendType (typeof(ICustomWidgetBackend))]
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
		EventHandler<TextInputEventArgs> textInput;
		EventHandler mouseEntered;
		EventHandler mouseExited;
		EventHandler<ButtonEventArgs> buttonPressed;
		EventHandler<ButtonEventArgs> buttonReleased;
		EventHandler<MouseMovedEventArgs> mouseMoved;
		EventHandler boundsChanged;
		EventHandler<MouseScrolledEventArgs> mouseScrolled;
		
		EventHandler gotFocus;
		EventHandler lostFocus;
		
		/// <summary>
		/// The WidgetBackendHost is the link between an Xwt widget and a toolkit specific widget backend.
		/// </summary>
		/// <typeparam name="T">The Xwt widget type.</typeparam>
		/// <typeparam name="B">The Xwt widget backend interface.</typeparam>
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
		
		/// <summary>
		/// The WidgetBackendHost is the link between an Xwt widget and a toolkit specific widget backend.
		/// </summary>
		protected class WidgetBackendHost: BackendHost<Widget, IWidgetBackend>, IWidgetEventSink
		{
			public WidgetBackendHost ()
			{
			}

			/// <summary>
			/// Called when the backend has been created.
			/// </summary>
			protected override void OnBackendCreated ()
			{
				((IWidgetBackend)Backend).Initialize (this);
				base.OnBackendCreated ();
			}
		
			/// <summary>
			/// Gets the default natural size of the widget
			/// </summary>
			/// <returns>The default natural size.</returns>
			/// <remarks>This method should only be used if there isn't a platform-specific natural
			/// size for the widget. There may be widgets for which XWT can't provide
			/// a default natural width or height, in which case it return 0.</remarks>
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

			void IWidgetEventSink.OnTextInput (TextInputEventArgs args)
			{
				Parent.OnTextInput (args);
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
			MapEvent (WidgetEvent.TextInput, typeof(Widget), "OnTextInput");
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
		
		/// <summary>
		/// Gets the current widget backend.
		/// </summary>
		/// <returns>The widget backend.</returns>
		/// <param name="w">The Xwt widget.</param>
		internal protected static IBackend GetBackend (Widget w)
		{
			if (w != null && w.Backend is XwtWidgetBackend)
				return GetBackend ((XwtWidgetBackend)w.Backend);
			return w != null ? w.Backend : null;
		}
		
		/// <summary>
		/// Gets the backend host.
		/// </summary>
		/// <value>The backend host.</value>
		protected new WidgetBackendHost BackendHost {
			get { return (WidgetBackendHost) base.BackendHost; }
		}
		
		/// <summary>
		/// Creates the backend host.
		/// </summary>
		/// <returns>The backend host.</returns>
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
				if (children != null) {
					foreach (var c in DirectChildren)
						c.Dispose ();
				}
			}
		}
		
		/// <summary>
		/// Gets the parent window of the widget.
		/// </summary>
		/// <value>The parent window.</value>
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
		
		/// <summary>
		/// Sets the parent window.
		/// </summary>
		/// <param name="win">An Xwt window containing the widget.</param>
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
		
		/// <summary>
		/// Gets the backend.
		/// </summary>
		/// <value>The backend.</value>
		IWidgetBackend Backend {
			get { return (IWidgetBackend) BackendHost.Backend; }
		}
		
		/// <summary>
		/// Gets or sets the <see cref="Xwt.Widget"/> margin.
		/// </summary>
		/// <value>The widget margin.</value>
		public WidgetSpacing Margin {
			get { return margin; }
			set {
				margin = value;
				OnPreferredSizeChanged ();
				OnPlacementChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the margin on the left side of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The left margin.</value>
		[DefaultValue (0d)]
		public double MarginLeft {
			get { return margin.Left; }
			set {
				margin.Left = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the margin on the right side of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The right margin.</value>
		[DefaultValue (0d)]
		public double MarginRight {
			get { return margin.Right; }
			set {
				margin.Right = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the margin on the top side of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The top margin.</value>
		[DefaultValue (0d)]
		public double MarginTop {
			get { return margin.Top; }
			set {
				margin.Top = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the margin on the bottom side of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The bottom margin.</value>
		[DefaultValue (0d)]
		public double MarginBottom {
			get { return margin.Bottom; }
			set {
				margin.Bottom = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the vertical placement/alignment of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The vertical placement.</value>
		public WidgetPlacement VerticalPlacement {
			get { return alignVertical; }
			set {
				alignVertical = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		/// <summary>
		/// Gets or sets the horizontal placement/alignment of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The horizontal placement.</value>
		public WidgetPlacement HorizontalPlacement {
			get { return alignHorizontal; }
			set {
				alignHorizontal = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		/// <summary>
		/// Get the placement/alignment of the <see cref="Xwt.Widget"/> for the specified orientation.
		/// </summary>
		/// <returns>The alignment for an orientation.</returns>
		/// <param name="or">The orientation of the container.</param>
		internal WidgetPlacement AlignmentForOrientation (Orientation or)
		{
			if (or == Orientation.Vertical)
				return VerticalPlacement;
			else
				return HorizontalPlacement;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Xwt.Widget"/> expands to fill all available vertical space.
		/// </summary>
		/// <value><c>true</c> if the widget expands vertically; otherwise, <c>false</c>.</value>
		public bool ExpandVertical {
			get { return expandVertical; }
			set {
				expandVertical = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Xwt.Widget"/> expands to fill all available horizontal space.
		/// </summary>
		/// <value><c>true</c> if the widget expands horizontally; otherwise, <c>false</c>.</value>
		public bool ExpandHorizontal {
			get { return expandHorizontal; }
			set {
				expandHorizontal = value;
				OnPreferredSizeChanged (); 
				OnPlacementChanged ();
			}
		}

		/// <summary>
		/// Determines whether this <see cref="Xwt.Widget"/> expands in a specific orientation.
		/// </summary>
		/// <returns><c>true</c>, if the widget expands in the specified orientation, <c>false</c> otherwise.</returns>
		/// <param name="or">Or.</param>
		internal bool ExpandsForOrientation (Orientation or)
		{
			if (or == Orientation.Vertical)
				return ExpandVertical;
			else
				return ExpandHorizontal;
		}

		/// <summary>
		/// Shows this widget.
		/// </summary>
		public void Show ()
		{
			Visible = true;
		}
		
		/// <summary>
		/// Hides this widget.
		/// </summary>
		public void Hide ()
		{
			Visible = false;
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Xwt.Widget"/> is visible.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		[DefaultValue (true)]
		public bool Visible {
			get { return Backend.Visible; }
			set {
				Backend.Visible = value; 
				OnPreferredSizeChanged ();
			}
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Xwt.Widget"/> is sensitive.
		/// </summary>
		/// <value><c>true</c> if sensitive; otherwise, <c>false</c>.</value>
		[DefaultValue (true)]
		public bool Sensitive {
			get { return Backend.Sensitive; }
			set { Backend.Sensitive = value; }
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this this <see cref="Xwt.Widget"/> can get focus.
		/// </summary>
		/// <value><c>true</c> if this widget can get focus; otherwise, <c>false</c>.</value>
		[DefaultValue (true)]
		public bool CanGetFocus {
			get { return Backend.CanGetFocus; }
			set { Backend.CanGetFocus = value; }
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Xwt.Widget"/> has the focus.
		/// </summary>
		/// <value><c>true</c> if this widget has focus; otherwise, <c>false</c>.</value>
		[DefaultValue (true)]
		public bool HasFocus {
			get { return Backend.HasFocus; }
		}
		
		/// <summary>
		/// Gets or sets the opacity of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The opacity of this widget.</value>
		/// <remarks>Not all toolkits support widget opacity. For toolkits that do not support it,
		/// setting the opacity has no effect. Examine the <see cref="Xwt.Toolkit.SupportedFeatures"/>
		/// property of the current <see cref="Xwt.IWidgetSurface.ToolkitEngine"/> (member of <see cref="Xwt.Widget.Surface"/>)
		/// to determine if the current toolkit supports this feature.</remarks>
		[DefaultValue (1d)]
		public double Opacity {
			get { return Backend.Opacity; }
			set { Backend.Opacity = value; }
		}

		/// <summary>
		/// Gets or sets the name of this widget.
		/// </summary>
		/// <value>The widgets name.</value>
		/// <remarks>The name can be used to identify this widget by e.g. designers.</remarks>
		[DefaultValue (null)]
		public string Name { get; set; }
		
		/// <summary>
		/// Gets the parent widget of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The parent.</value>
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Widget Parent { get; private set; }

		internal Widget InternalParent { get; private set; }

		bool IsInternalChild {
			get { return ExternalParent != null; }
		}

		Widget ExternalParent { get; set; }

		/// <summary>
		/// Gets the widgets surface.
		/// </summary>
		/// <value>The surface of this widget.</value>
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IWidgetSurface Surface {
			get { return this; }
		}
		
		/// <summary>
		/// Gets or sets the content of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The content of the widget.</value>
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
		
		/// <summary>
		/// Gets the size of this <see cref="Xwt.Widget"/>.
		/// </summary>
		/// <value>The size of the widget.</value>
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
		/// Minimum width for the widget. If set to -1, the widget's default minimum size will be used.
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
		/// Minimum height for the widget. If set to -1, the widget's default minimum size will be used.
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
		
		/// <summary>
		/// Gets or sets the background color.
		/// </summary>
		/// <value>The background color of the widget.</value>
		public Color BackgroundColor {
			get { return Backend.BackgroundColor; }
			set { Backend.BackgroundColor = value; }
		}

		/// <summary>
		/// Gets or sets the tooltip text.
		/// </summary>
		/// <value>The tooltip text.</value>
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

		/// <summary>
		/// Determines the value whether the cursor is set and should be serialized.
		/// </summary>
		/// <returns><c>true</c>, if the cursor should be serialized , <c>false</c> otherwise.</returns>
		public bool ShouldSerializeCursor ()
		{
			return Cursor != CursorType.Arrow;
		}

		/// <summary>
		/// Converts widget relative coordinates to screen coordinates.
		/// </summary>
		/// <returns>The screen coordinates.</returns>
		/// <param name="widgetCoordinates">The relative widget coordinates.</param>
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
		
		/// <summary>
		/// Determines a value whether the widgets parent should be serialized
		/// </summary>
		/// <returns><c>true</c>, if parent should be serialized, <c>false</c> otherwise.</returns>
		public bool ShouldSerializeParent ()
		{
			return false;
		}
		
		/// <summary>
		/// Sets the focus on this widget.
		/// </summary>
		public void SetFocus ()
		{
			Backend.SetFocus ();
		}
		
		/// <summary>
		/// Creates a new drag operation originating from this widget.
		/// </summary>
		/// <returns>The new drag operation.</returns>
		public DragOperation CreateDragOperation ()
		{
			currentDragOperation = new DragOperation (this);
			return currentDragOperation;
		}
		
		/// <summary>
		/// Starts a drag operation with the specified drag start arguments.
		/// </summary>
		/// <param name="sdata">The drag start arguments to start the drag with.</param>
		internal void DragStart (DragStartData sdata)
		{
			Backend.DragStart (sdata);
		}
		
		/// <summary>
		/// Sets this widget as a potential drop destination.
		/// </summary>
		/// <param name='types'>Types of data that can be dropped on this widget.</param>
		public void SetDragDropTarget (params TransferDataType[] types)
		{
			Backend.SetDragTarget (types, DragDropAction.All);
		}
		
		/// <summary>
		/// Sets this widget as a potential drop destination.
		/// </summary>
		/// <param name='types'>Types of data that can be dropped on this widget.</param>
		public void SetDragDropTarget (params Type[] types)
		{
			Backend.SetDragTarget (types.Select (t => TransferDataType.FromType (t)).ToArray (), DragDropAction.All);
		}
		
		/// <summary>
		/// Sets this widget as a potential drop destination.
		/// </summary>
		/// <param name='types'>Types of data that can be dropped on this widget.</param>
		/// <param name='dragAction'>Bitmask of possible actions for a drop on this widget</param>
		public void SetDragDropTarget (DragDropAction dragAction, params TransferDataType[] types)
		{
			Backend.SetDragTarget (types, dragAction);
		}
		
		/// <summary>
		/// Sets this widget as a potential drop destination.
		/// </summary>
		/// <param name='types'>Types of data that can be dropped on this widget.</param>
		/// <param name='dragAction'>Bitmask of possible actions for a drop on this widget</param>
		public void SetDragDropTarget (DragDropAction dragAction, params Type[] types)
		{
			Backend.SetDragTarget (types.Select (t => TransferDataType.FromType (t)).ToArray(), dragAction);
		}
		
		/// <summary>
		/// Sets up this widget so that XWT will start a drag operation when the user clicks and drags on this widget.
		/// </summary>
		/// <param name='types'>Types of data that can be dragged from this widget</param>
		public void SetDragSource (params TransferDataType[] types)
		{
			Backend.SetDragSource (types, DragDropAction.All);
		}
		
		/// <summary>
		/// Sets up this widget so that XWT will start a drag operation when the user clicks and drags on this widget.
		/// </summary>
		/// <param name='types'>Types of data that can be dragged from this widget</param>
		public void SetDragSource (params Type[] types)
		{
			Backend.SetDragSource (types.Select (t => TransferDataType.FromType (t)).ToArray(), DragDropAction.All);
		}
		
		/// <summary>
		/// Sets up this widget so that XWT will start a drag operation when the user clicks and drags on this widget.
		/// </summary>
		/// <param name='types'>Types of data that can be dragged from this widget</param>
		/// <param name='dragAction'>Bitmask of possible actions for a drag from this widget</param>
		public void SetDragSource (DragDropAction dragAction, params TransferDataType[] types)
		{
			Backend.SetDragSource (types, dragAction);
		}
		
		/// <summary>
		/// Sets up this widget so that XWT will start a drag operation when the user clicks and drags on this widget.
		/// </summary>
		/// <param name='types'>Types of data that can be dragged from this widget</param>
		/// <param name='dragAction'>Bitmask of possible actions for a drag from this widget</param>
		public void SetDragSource (DragDropAction dragAction, params Type[] types)
		{
			Backend.SetDragSource (types.Select (t => TransferDataType.FromType (t)).ToArray(), dragAction);
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Xwt.Widget"/> supports custom scrolling.
		/// </summary>
		/// <value><c>true</c> if this widget supports custom scrolling; otherwise, <c>false</c>.</value>
		internal protected virtual bool SupportsCustomScrolling {
			get { return false; }
		}
		
		/// <summary>
		/// Sets the scroll adjustments for custom scrolling.
		/// </summary>
		/// <param name="horizontal">The horizontal adjustment backend.</param>
		/// <param name="vertical">The vertical adjustment backend.</param>
		protected virtual void SetScrollAdjustments (ScrollAdjustment horizontal, ScrollAdjustment vertical)
		{
		}
		
		/// <summary>
		/// Raises the DragOverCheck event, when the mouse is moved over the widget in a drag&amp;drop operation.
		/// </summary>
		/// <param name="args">The drag over check event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnDragOverCheck"/> to handle the event internally and call
		/// <see cref="Xwt.Widget.OnDragOverCheck"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnDragOverCheck"/>
		/// is overriden.
		/// This event arguments provide information about the type of the data that is going
		/// to be dropped, but not the actual data. Set <see cref="Xwt.DragOverCheckEventArgs.AllowedAction"/>
		/// to the allowed action for this widget, or to <see cref="Xwt.DragDropAction.None"/> if no
		/// dropping is allowed.
		/// Do not set the value or set it to <see cref="Xwt.DragDropAction.Default"/> and override <see cref="OnDragOver"/>
		/// to analyse the actual data transferred by the drag operation.
		/// </remarks>
		internal protected virtual void OnDragOverCheck (DragOverCheckEventArgs args)
		{
			if (dragOverCheck != null)
				dragOverCheck (this, args);
		}
		
		/// <summary>
		/// Raises the DragOver event, when the mouse is moved over the widget in a drag&amp;drop operation.
		/// </summary>
		/// <param name="args">The drag over event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnDragOver"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnDragOver"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnDragOver"/>
		/// is overridden.
		/// This event arguments provide information about the actual data that is going
		/// to be dropped. Set <see cref="Xwt.DragOverEventArgs.AllowedAction"/> to the
		/// allowed action for this widget, or to <see cref="Xwt.DragDropAction.None"/> if no
		/// dropping is allowed.
		/// </remarks>
		internal protected virtual void OnDragOver (DragOverEventArgs args)
		{
			if (dragOver != null)
				dragOver (this, args);
		}
		
		/// <summary>
		/// Raises the DragDropCheck event to check if a drop operation is allowed on the widget.
		/// </summary>
		/// <param name="args">The drag check event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnDragDropCheck"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnDragDropCheck"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnDragDropCheck"/>
		/// is overridden.
		/// This event arguments provide information about the type of the data that is going
		/// to be dropped, but not the actual data. Set <see cref="Xwt.DragCheckEventArgs.Result"/> to
		/// <see cref="DragDropResult.Success"/> to finish the drop action, or <see cref="DragDropResult.Canceled"/>
		/// to abort it.
		/// To request the transferred data do not set the result value or set it to <see cref="Xwt.DragDropAction.None"/>
		/// and override <see cref="Xwt.Widget.OnDragDrop"/> to handle the dropped data.
		/// </remarks>
		internal protected virtual void OnDragDropCheck (DragCheckEventArgs args)
		{
			if (dragDropCheck != null)
				dragDropCheck (this, args);
		}
		
		/// <summary>
		/// Raises the DragDrop event when a drop has been performed.
		/// </summary>
		/// <param name="args">The drop event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnDragDrop"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnDragDrop"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnDragDrop"/>
		/// is overridden.
		/// This event arguments provide information about the dropped data and the actual data.
		/// Set <see cref="Xwt.DragEventArgs.Success"/> to <c>true</c> when the drop
		/// was successful, <c>false</c> otherwise.
		/// </remarks>
		internal protected virtual void OnDragDrop (DragEventArgs args)
		{
			if (dragDrop != null)
				dragDrop (this, args);
		}
		
		/// <summary>
		/// Raises the DragLeave event.
		/// </summary>
		/// <remarks>
		/// Override <see cref="OnDragLeave"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnDragLeave"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnDragLeave"/>
		/// is overridden.
		/// </remarks>
		internal protected virtual void OnDragLeave (EventArgs args)
		{
			if (dragLeave != null)
				dragLeave (this, args);
		}
		
		/// <summary>
		/// Creates a new drag operation.
		/// </summary>
		/// <returns>
		/// The information about the starting drag operation and the data to be transferred,
		/// or <c>null</c> to abort dragging.
		/// </returns>
		/// <remarks>Override to provide custom <see cref="Xwt.Backends.DragStartData"/> for the drag action.</remarks>
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
		/// <param name="args">The drag started event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnDragStarted"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnDragStarted"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnDragStarted"/>
		/// is overridden.
		/// </remarks>
		protected virtual void OnDragStarted (DragStartedEventArgs args)
		{
			if (dragStarted != null)
				dragStarted (this, args);
		}
		
		/// <summary>
		/// Raises the drag finished event.
		/// </summary>
		/// <param name="args">The drag finished event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnDragFinished"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnDragFinished"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnDragFinished"/>
		/// is overridden.
		/// </remarks>
		internal void OnDragFinished (DragFinishedEventArgs args)
		{
			if (currentDragOperation != null) {
				var dop = currentDragOperation;
				currentDragOperation = null;
				dop.NotifyFinished (args);
			}
		}

		/// <summary>
		/// Raises the key pressed event.
		/// </summary>
		/// <param name="args">The key pressed event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnKeyPressed"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnKeyPressed"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnKeyPressed"/>
		/// is overridden.
		/// </remarks>
		internal protected virtual void OnKeyPressed (KeyEventArgs args)
		{
			if (keyPressed != null)
				keyPressed (this, args);
		}

		/// <summary>
		/// Raises the key released event.
		/// </summary>
		/// <param name="args">The key released event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnKeyReleased"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnKeyReleased"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnKeyReleased"/>
		/// is overridden.
		/// </remarks>
		internal protected virtual void OnKeyReleased (KeyEventArgs args)
		{
			if (keyReleased != null)
				keyReleased (this, args);
		}

		/// <summary>
		/// Raises the preview text input event.
		/// </summary>
		/// <param name="args">The preview text input event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnPreviewTextInput"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnPreviewTextInput"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnPreviewTextInput"/>
		/// is overridden.
		/// </remarks>
		internal protected virtual void OnTextInput (TextInputEventArgs args)
		{
			if (textInput != null)
				textInput (this, args);
		}

		/// <summary>
		/// Raises the got focus event.
		/// </summary>
		/// <remarks>
		/// Override <see cref="OnGotFocus"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnGotFocus"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnGotFocus"/>
		/// is overridden.
		/// </remarks>
		internal protected virtual void OnGotFocus (EventArgs args)
		{
			if (gotFocus != null)
				gotFocus (this, args);
		}

		/// <summary>
		/// Raises the lost focus event.
		/// </summary>
		/// <remarks>
		/// Override <see cref="OnLostFocus"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnLostFocus"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnLostFocus"/>
		/// is overridden.
		/// </remarks>
		internal protected virtual void OnLostFocus (EventArgs args)
		{
			if (lostFocus != null)
				lostFocus (this, args);
		}
		
		/// <summary>
		/// Called when the mouse enters the widget
		/// </summary>
		/// <param name="args">The mouse entered event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnMouseEntered"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnMouseEntered"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnMouseEntered"/>
		/// is overridden.
		/// </remarks>
		protected virtual void OnMouseEntered (EventArgs args)
		{
			if (mouseEntered != null)
				mouseEntered (this, args);
		}
		
		/// <summary>
		/// Called when the mouse leaves the widget
		/// </summary>
		/// <param name="args">The mouse exited event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnMouseExited"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnMouseExited"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnMouseExited"/>
		/// is overridden.
		/// </remarks>
		protected virtual void OnMouseExited (EventArgs args)
		{
			if (mouseExited != null)
				mouseExited (this, args);
		}

		/// <summary>
		/// Raises the button pressed event.
		/// </summary>
		/// <param name="args">The button pressed event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnButtonPressed"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnButtonPressed"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnButtonPressed"/>
		/// is overridden.
		/// </remarks>
		protected virtual void OnButtonPressed (ButtonEventArgs args)
		{
			if (buttonPressed != null)
				buttonPressed (this, args);
		}

		/// <summary>
		/// Raises the button released event.
		/// </summary>
		/// <param name="args">The button released event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnButtonReleased"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnButtonReleased"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnButtonReleased"/>
		/// is overridden.
		/// </remarks>
		protected virtual void OnButtonReleased (ButtonEventArgs args)
		{
			if (buttonReleased != null)
				buttonReleased (this, args);
		}

		/// <summary>
		/// Raises the mouse moved event.
		/// </summary>
		/// <param name="args">The mouse moved event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnMouseMoved"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnMouseMoved"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnMouseMoved"/>
		/// is overridden.
		/// </remarks>
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

		/// <summary>
		/// Raises the mouse scrolled event.
		/// </summary>
		/// <param name="args">The mouse scrolled event arguments.</param>
		/// <remarks>
		/// Override <see cref="OnMouseScrolled"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnMouseScrolled"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnMouseScrolled"/>
		/// is overridden.
		/// </remarks>
		protected virtual void OnMouseScrolled (MouseScrolledEventArgs args)
		{
			if (mouseScrolled != null)
				mouseScrolled(this, args);
		}

		/// <summary>
		/// Sets the widget to be a native widget in an other toolkit and to reallocate itself on size changes.
		/// </summary>
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

		/// <summary>
		/// Raises the bounds changed event.
		/// </summary>
		/// <remarks>
		/// Override <see cref="OnBoundsChanged"/> to handle the event internally and call the base
		/// <see cref="Xwt.Widget.OnBoundsChanged"/> to finally raise the event.
		/// The event will be enabled in the backend automatically, if <see cref="Xwt.Widget.OnBoundsChanged"/>
		/// is overridden.
		/// </remarks>
		protected virtual void OnBoundsChanged ()
		{
			if (boundsChanged != null)
				boundsChanged (this, EventArgs.Empty);
		}
		
		/// <summary>
		/// Gets the current widget backend.
		/// </summary>
		/// <returns>The widget backend.</returns>
		/// <param name="w">The Xwt widget.</param>
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
				foreach (Widget w in DirectChildren) {
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

		/// <summary>
		/// Called when the preferred size of a child widget has changed.
		/// </summary>
		protected virtual void OnChildPreferredSizeChanged ()
		{
		}

		void NotifyChildPreferredSizeChanged ()
		{
			OnChildPreferredSizeChanged ();

			if (InternalParent != null && resizeRequestQueue.Contains (InternalParent)) {
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
		
		/// <summary>
		/// Called when the preferred size of the widget has changed.
		/// </summary>
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

		/// <summary>
		/// Propagate a placement/alignment change to the parent
		/// </summary>
		internal void OnPlacementChanged ()
		{
			if (InternalParent != null)
				InternalParent.OnChildPlacementChanged (this);
			else if (parentWindow is Window)
				((Window)parentWindow).OnChildPlacementChanged (this);
		}

		/// <summary>
		/// Called when the placement/alignment of a child widget has changed.
		/// </summary>
		/// <param name="child">The child widget with the changed placement.</param>
		protected virtual void OnChildPlacementChanged (Widget child)
		{
			var ph = Backend as IChildPlacementHandler;
			if (ph != null)
				ph.UpdateChildPlacement (child.GetBackend ());
			else
				QueueForReallocate ();
		}

		/// <summary>
		/// Queues a reallocation of this widget.
		/// </summary>
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
			if (InternalParent != null) {
				QueueForSizeCheck (InternalParent);
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

		/// <summary>
		/// Queues a window size negotiation, if the toolkit backend handles size negotiation on its own.
		/// </summary>
		/// <param name="window">The window to queue the size negotiation.</param>
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
				if (InternalParent != null)
					return InternalParent.Depth + 1;
				return 0;
			}
		}

		string GetWidgetDesc ()
		{
			if (InternalParent != null) {
				int i = InternalParent.Surface.Children.ToList ().IndexOf (this);
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
				return ExternalChildren; 
			}
		}

		IEnumerable<Widget> DirectChildren {
			get { return children != null ? children.Where (c => c.InternalParent == this) : emptyList; }
		}

		IEnumerable<Widget> ExternalChildren {
			get { return children != null ? children.Where (c => !c.IsInternalChild) : emptyList; }
		}

		Widget FindExternalParent ()
		{
			if (IsInternalChild && Parent != null)
				return Parent.FindExternalParent ();
			else
				return this;
		}

		/// <summary>
		/// Registers a widget as a child of this widget.
		/// </summary>
		/// <param name="w">The new child widget.</param>
		protected void RegisterChild (Widget w)
		{
			if (w == null)
				return;

			if (w.Surface.ToolkitEngine != Surface.ToolkitEngine)
				throw new InvalidOperationException ("Widget belongs to a different toolkit");

			var wback = w.Backend as XwtWidgetBackend;

			if (IsInternalChild && !w.IsInternalChild) {
				if (w.Parent == null)
					throw new InvalidOperationException ("Widget must be registered as a child widget of " + FindExternalParent ());
				if (w.Parent != ExternalParent)
					throw new InvalidOperationException ("Widget is already a child of a widget of type " + w.Parent.GetType ());
				w.InternalParent = this;
				if (wback != null)
					wback.InternalParent = this;
			} else {
				if (w.Parent != null)
					throw new InvalidOperationException ("Widget is already a child of a widget of type " + w.Parent.GetType ());
				w.Parent = this;
				w.InternalParent = this;
				if (wback != null) {
					wback.Parent = this;
					wback.InternalParent = this;
				}
			}

			if (children == null)
				children = new List<Widget> ();
			children.Add (w);

			// Make sure the widget is queued for reallocation
			w.OnPreferredSizeChanged ();
		}
		
		/// <summary>
		/// Unregisters a child widget.
		/// </summary>
		/// <param name="w">The width.</param>
		protected void UnregisterChild (Widget w)
		{
			if (w == null)
				return;

			int i;
			if (children == null || (i = children.IndexOf (w)) == -1)
				throw new InvalidOperationException ("Widget is not a child of this widget");

			var wback = w.Backend as XwtWidgetBackend;

			if (w.Parent == this) {
				if (w.InternalParent != this)
					throw new InvalidOperationException ("Child widget must be removed from internal container before unregistering it");
				w.Parent = null;
				w.InternalParent = null;
			} else {
				w.InternalParent = w.Parent;
			}

			children.RemoveAt (i);

			if (wback != null) {
				wback.Parent = w.Parent;
				wback.InternalParent = w.InternalParent;
			}
		}

		/// <summary>
		/// Flags a widget as an internal child of a container
		/// </summary>
		/// <param name="child">A widget</param>
		/// <remarks>
		/// This method must must be called before the child widget is added to any container.
		/// Internal children of a widget are not returned in the Children list of the widget, and they
		/// are not included in the Parent hierarchy chain.
		/// </remarks>
		protected T SetInternalChild<T> (T child) where T:Widget
		{
			if (child.ExternalParent == this)
				return child;
			if (child.ExternalParent != null)
				throw new InvalidOperationException ("Widget is already an internal child of widget " + child.ExternalParent);
			if (child.Parent != null)
				throw new InvalidOperationException ("Widget must be flagged as internal child before being added to a container");
			child.ExternalParent = this;
			return child;
		}
		
		void IAnimatable.BatchBegin ()
		{
		}

		void IAnimatable.BatchCommit ()
		{
		}

		/// <summary>
		/// Raised when the mouse is moved over the widget in a drag&amp;drop operation
		/// </summary>
		/// <remarks>
		/// The subscriber of the event should set the value of <see cref="Xwt.DragOverCheckEventArgs.AllowedAction"/>
		/// in the provided event args object. If the value is not set or it is set to
		/// <see cref="Xwt.DragDropAction.Default"/>, the action will be determined by the
		/// result of the DragOver event. To deny dropping on this widget the allowed action must
		/// be set to <see cref="Xwt.DragDropAction.None"/>.
		/// 
		/// This event provides information about the type of the data that is going
		/// to be dropped, but not the actual data. If you need the actual data
		/// to decide if the drop is allowed or not, you have to subscribe the <see cref="DragOver"/>
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
		/// Raised when the mouse is moved over the widget in a drag&amp;drop operation
		/// </summary>
		/// <remarks>
		/// The subscriber of the event should set the value of <see cref="Xwt.DragOverEventArgs.AllowedAction"/>
		/// in the provided event args object. If the value is not set or it is set to
		/// <see cref="Xwt.DragDropAction.Default"/>, the action will be determined by the
		/// result of the DragDropCheck event. To deny dropping on this widget the allowed action must
		/// be set to <see cref="Xwt.DragDropAction.None"/>.
		/// 
		/// This event provides information about the actual data that is going
		/// to be dropped. Getting the data may be inefficient in some cross-process drag&amp;drop scenarios,
		/// so if you don't need the actual data to decide the allowed drop operation, 
		/// and knowing the type of the data is enough, then the <see cref="DragOverCheck"/>
		/// event is a better option.
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
		/// <remarks>
		/// The subscriber of the event can set the value of <see cref="Xwt.DragCheckEventArgs.Result"/>
		/// in the provided event args object. Set it to <see cref="Xwt.DragDropResult.Success"/> to
		/// finish the drop without requesting the actual data, or to <see cref="Xwt.DragDropResult.Canceled"/>
		/// to abort/deny the drop action.
		/// 
		/// This event provides information about the type of the data that is being dropped,
		/// but not the actual data. If you need the actual data to decide if the drop is allowed
		/// or not, you have to subscribe the <see cref="DragDrop"/> event.
		/// </remarks>
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
		
		/// <summary>
		/// Raised when the mouse is leaving the widget in a drag operation.
		/// </summary>
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
		
		/// <summary>
		/// Raised when a new drag operation has been started.
		/// </summary>
		/// <remarks>
		/// This event provides access to the started drag operation.
		/// Use <see cref="Xwt.DragStartedEventArgs.DragOperation"/> to configure the
		/// drag opration and to add data to transfer.
		/// </remarks>
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
		
		/// <summary>
		/// Raised when a key has been pressed.
		/// </summary>
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
		
		/// <summary>
		/// Raised when a key has been released.
		/// </summary>
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
		/// Raised when a text has been entered.
		/// </summary>
		public event EventHandler<TextInputEventArgs> TextInput {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.TextInput, textInput);
				textInput += value;
			}
			remove {
				textInput -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.TextInput, textInput);
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
		/// Raised when the widget looses the focus
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
		
		/// <summary>
		/// Occurs when a mouse button has been pressed.
		/// </summary>
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
		
		/// <summary>
		/// Occurs when a mouse button has been released.
		/// </summary>
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
		
		/// <summary>
		/// Occurs when the mouse has moved.
		/// </summary>
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
		
		/// <summary>
		/// Occurs when the bounds of this widget have changed.
		/// </summary>
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

		/// <summary>
		/// Occurs when scroll action has been performed.
		/// </summary>
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

