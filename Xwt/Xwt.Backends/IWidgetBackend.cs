// 
// IWidgetBackend.cs
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
using Xwt.Drawing;

namespace Xwt.Backends
{
	/// <summary>
	/// The Xwt widget backend base interface. All Xwt widget backends implement it.
	/// </summary>
	public interface IWidgetBackend: IBackend
	{
		/// <summary>
		/// Initialize the backend with the specified EventSink.
		/// </summary>
		/// <param name="eventSink">The frontend event sink (usually the <see cref="Xwt.Backends.BackendHost"/> of the frontend).</param>
		void Initialize (IWidgetEventSink eventSink);

		/// <summary>
		/// Releases all resources used by the widget
		/// </summary>
		/// <remarks>
		/// This method is called to free all managed and unmanaged resources held by the backend.
		/// In general, the backend should destroy the native widget at this point.
		/// When a widget that has children is disposed, the Dispose method is called on all
		/// backends of all widgets in the children hierarchy.
		/// </remarks>
		void Dispose ();
		
		/// <summary>
		/// Gets or sets a value indicating whether this widget is visible.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		bool Visible { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this widget is sensitive/enabled.
		/// </summary>
		/// <value><c>true</c> if sensitive; otherwise, <c>false</c>.</value>
		bool Sensitive { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this widget can get focus.
		/// </summary>
		/// <value><c>true</c> if this instance can get focus; otherwise, <c>false</c>.</value>
		bool CanGetFocus { get; set; }

		/// <summary>
		/// Gets a value indicating whether this widget has the focus.
		/// </summary>
		/// <value><c>true</c> if this widget has the focus; otherwise, <c>false</c>.</value>
		bool HasFocus { get; }

		/// <summary>
		/// Gets or sets the opacity of this widget.
		/// </summary>
		/// <value>The opacity.</value>
		double Opacity { get; set; }

		/// <summary>
		/// Gets the final size of this widget.
		/// </summary>
		/// <value>The size.</value>
		Size Size { get; }

		/// <summary>
		/// Converts widget relative coordinates to screen coordinates.
		/// </summary>
		/// <returns>The screen coordinates.</returns>
		/// <param name="widgetCoordinates">The relative widget coordinates.</param>
		Point ConvertToScreenCoordinates (Point widgetCoordinates);
		
		/// <summary>
		/// Sets the minimum size of the widget
		/// </summary>
		/// <param name='width'>
		/// Minimum width. If the value is -1, it means no minimum width.
		/// </param>
		/// <param name='height'>
		/// Minimum height. If the value is -1, it means no minimum height.
		/// </param>
		void SetMinSize (double width, double height);

		/// <summary>
		/// Sets the size request / natural size of this widget.
		/// </summary>
		/// <param name="width">Natural width, or -1 if no custom natural width has been set.</param>
		/// <param name="height">Natural height, or -1 if no custom natural height has been set.</param>
		void SetSizeRequest (double width, double height);
		
		/// <summary>
		/// Sets the focus on this widget.
		/// </summary>
		void SetFocus ();
		
		/// <summary>
		/// Updates the layout of this widget.
		/// </summary>
		void UpdateLayout ();

		/// <summary>
		/// Gets the preferred size of this widget.
		/// </summary>
		/// <returns>The widgets preferred size.</returns>
		/// <param name="widthConstraint">Width constraint.</param>
		/// <param name="heightConstraint">Height constraint.</param>
		Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint);

		/// <summary>
		/// Gets the native toolkit widget.
		/// </summary>
		/// <value>The native widget.</value>
		object NativeWidget { get; }
		
		/// <summary>
		/// Starts a drag operation originated in this widget
		/// </summary>
		/// <param name='data'>
		/// Drag operation arguments
		/// </param>
		void DragStart (DragStartData data);
		
		/// <summary>
		/// Sets up a widget so that XWT will start a drag operation when the user clicks and drags on the widget.
		/// </summary>
		/// <param name='types'>
		/// Types of data that can be dragged from this widget
		/// </param>
		/// <param name='dragAction'>
		/// Bitmask of possible actions for a drag from this widget
		/// </param>
		/// <remarks>
		/// When a drag operation is started, the backend should fire the OnDragStarted event
		/// </remarks>
		void SetDragSource (TransferDataType[] types, DragDropAction dragAction);
		
		/// <summary>
		/// Sets a widget as a potential drop destination
		/// </summary>
		/// <param name='types'>
		/// Types of data that can be dropped on this widget
		/// </param>
		/// <param name='dragAction'>
		/// Bitmask of possible actions for a drop on this widget
		/// </param>
		void SetDragTarget (TransferDataType[] types, DragDropAction dragAction);
		
		/// <summary>
		/// Gets or sets the native font of this widget.
		/// </summary>
		/// <value>The font.</value>
		object Font { get; set; }

		/// <summary>
		/// Gets or sets the background color of this widget.
		/// </summary>
		/// <value>The background color.</value>
		Color BackgroundColor { get; set; }

		/// <summary>
		/// Gets or sets the tooltip text.
		/// </summary>
		/// <value>The tooltip text.</value>
		string TooltipText { get; set; }
		
		/// <summary>
		/// Sets the cursor shape to be used when the mouse is over the widget
		/// </summary>
		/// <param name='cursorType'>
		/// The cursor type.
		/// </param>
		void SetCursor (CursorType cursorType);
	}
	
	/// <summary>
	/// The widget event sink routes backend events to the frontend
	/// </summary>
	public interface IWidgetEventSink
	{
		/// <summary>
		/// Notifies the frontend that the mouse is moved over the widget in a drag operation,
		/// to check whether a drop operation is allowed.
		/// </summary>
		/// <param name="args">The drag over check event arguments.</param>
		/// <remarks>
		/// This event handler provides information about the type of the data that is going
		/// to be dropped, but not the actual data.
		/// The frontend decides which of the proposed actions will be performed when
		/// the item is dropped by setting <see cref="Xwt.DragOverCheckEventArgs.AllowedAction"/>.
		/// If the value is not set or it is set to <see cref="Xwt.DragDropAction.Default"/>,
		/// the action data should be provided using <see cref="OnDragOver"/>. If the proposed action
		/// is not allowed, the backend should not allow the user to perform the drop action.
		/// </remarks>
		void OnDragOverCheck (DragOverCheckEventArgs args);

		/// <summary>
		/// Notifies the frontend that the mouse is moved over the widget in a drag operation,
		/// to check whether a drop operation is allowed for the data being dragged.
		/// </summary>
		/// <param name="args">The drag over event arguments.</param>
		/// <remarks>
		/// This event handler provides information about the actual data that is going to be dropped. 
		/// The frontend decides which of the proposed actions will be performed when the
		/// item is dropped by setting <see cref="Xwt.DragOverEventArgs.AllowedAction"/>. If the proposed action
		/// is not allowed, the backend should not perform the drop action.
		/// </remarks>
		void OnDragOver (DragOverEventArgs args);

		/// <summary>
		/// Notifies the frontend that there is a pending drop operation, to check whether it is allowed.
		/// </summary>
		/// <param name="args">The drop check event arguments.</param>
		/// <remarks>
		/// This event handler provides information about the type of the data that is going
		/// to be dropped, but not the actual data. The frontend decides whether the action
		/// is allowed or not by setting <see cref="Xwt.DragCheckEventArgs.Result"/>.
		/// The backend should abort the drop operation, if the result is <see cref="Xwt.DragDropResult.Canceled"/>,
		/// or provide more information including actual data using <see cref="OnDragDrop"/> otherwise.
		/// </remarks>
		void OnDragDropCheck (DragCheckEventArgs args);

		/// <summary>
		/// Notifies the frontend of a drop operation to perform.
		/// </summary>
		/// <param name="args">The drop event arguments.</param>
		/// <remarks>
		/// This event handler provides information about the dropped data and the actual data.
		/// The frontend will set <see cref="Xwt.DragEventArgs.Success"/> to <c>true</c> when the drop
		/// was successful, <c>false</c> otherwise.
		/// </remarks>
		void OnDragDrop (DragEventArgs args);

		/// <summary>
		/// Notifies the frontend that the mouse is leaving the widget in a drag operation.
		/// </summary>
		void OnDragLeave (EventArgs args);
		
		/// <summary>
		/// Notifies the frontend that the drag&amp;drop operation has finished.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		void OnDragFinished (DragFinishedEventArgs args);
		
		/// <summary>
		/// Notifies the frontend about a starting drag operation and retrieves the data for the drag&amp;drop operation.
		/// </summary>
		/// <returns>
		/// The information about the starting drag operation and the data to be transferred,
		/// or <c>null</c> to abort dragging.
		/// </returns>
		DragStartData OnDragStarted ();

		/// <summary>
		/// Notifies the frontend that a key has been pressed.
		/// </summary>
		/// <param name="args">The Key arguments.</param>
		void OnKeyPressed (KeyEventArgs args);

		/// <summary>
		/// Notifies the frontend that a key has been released.
		/// </summary>
		/// <param name="args">The Key arguments.</param>
		void OnKeyReleased (KeyEventArgs args);

		/// <summary>
		/// Notifies the frontend that a text has been entered.
		/// </summary>
		/// <param name="args">The text input arguments.</param>
		void OnTextInput (TextInputEventArgs args);

		/// <summary>
		/// Notifies the frontend that the widget has received the focus.
		/// </summary>
		void OnGotFocus ();

		/// <summary>
		/// Notifies the frontend that the widget has lost the focus.
		/// </summary>
		void OnLostFocus ();

		/// <summary>
		/// Notifies the frontend that the mouse has entered the widget.
		/// </summary>
		void OnMouseEntered ();

		/// <summary>
		/// Notifies the frontend that the mouse has left the widget.
		/// </summary>
		void OnMouseExited ();

		/// <summary>
		/// Notifies the frontend that a mouse button has been pressed.
		/// </summary>
		/// <param name="args">The button arguments.</param>
		void OnButtonPressed (ButtonEventArgs args);

		/// <summary>
		/// Notifies the frontend that a mouse button has been released.
		/// </summary>
		/// <param name="args">The button arguments.</param>
		void OnButtonReleased (ButtonEventArgs args);

		/// <summary>
		/// Notifies the frontend that the mouse has moved.
		/// </summary>
		/// <param name="args">The mouse movement arguments.</param>
		void OnMouseMoved (MouseMovedEventArgs args);

		/// <summary>
		/// Notifies the frontend that the widget bounds have changed.
		/// </summary>
		void OnBoundsChanged ();
		
		/// <summary>
		/// Notifies the frontend about a scroll action.
		/// </summary>
		/// <param name="args">The mouse scrolled arguments.</param>
		void OnMouseScrolled(MouseScrolledEventArgs args);

		/// <summary>
		/// Gets the preferred size from the frontend (it will not include the widget margin).
		/// </summary>
		/// <returns>The size preferred by the frontend without widget margin.</returns>
		/// <param name="widthConstraint">The width constraint.</param>
		/// <param name="heightConstraint">The height constraint.</param>
		/// <remarks>
		/// The returned size is >= 0. If a constraint is specified, the returned size will not
		/// be bigger than the constraint. In most cases the frontend will retrieve the preferred size
		/// from the backend using <see cref="Xwt.Backends.IWidgetBackend.GetPreferredSize"/> and adjust it
		/// optionally.
		/// </remarks>
		Size GetPreferredSize (SizeConstraint widthConstraint = default(SizeConstraint), SizeConstraint heightConstraint = default(SizeConstraint));

		/// <summary>
		/// Notifies the frontend that the preferred size of this widget has changed
		/// </summary>
		/// <remarks>
		/// This method must be called when the widget changes its preferred size.
		/// This method doesn't need to be called if the resize is the result of changing
		/// a widget property. For example, it is not necessary to call it when the text
		/// of a label is changed (the fronted will automatically rise this event when
		/// the property changes). However, it has to be called when the the shape of the
		/// widget changes on its own, for example if the size of a button changes as
		/// a result of clicking on it.
		/// </remarks>
		void OnPreferredSizeChanged ();

		/// <summary>
		/// Gets a value indicating whether the frontend supports custom scrolling.
		/// </summary>
		/// <returns><c>true</c>, if custom scrolling is supported, <c>false</c> otherwise.</returns>
		/// <remarks>
		/// If the frontend supports custom scrolling, the backend must set the scroll adjustments
		/// using <see cref="SetScrollAdjustments"/> to allow the frontend to handle scrolling.
		/// </remarks>
		bool SupportsCustomScrolling ();

		/// <summary>
		/// Sets the scroll adjustments for custom scrolling.
		/// </summary>
		/// <param name="horizontal">The horizontal adjustment backend.</param>
		/// <param name="vertical">The vertical adjustment backend.</param>
		void SetScrollAdjustments (IScrollAdjustmentBackend horizontal, IScrollAdjustmentBackend vertical);
		
		/// <summary>
		/// Gets the default natural size of the widget
		/// </summary>
		/// <returns>
		/// The default natural size.
		/// </returns>
		/// <remarks>
		/// This method should only be used if there isn't a platform-specific natural
		/// size for the widget. There may be widgets for which XWT can't provide
		/// a default natural width or height, in which case it return 0.
		/// </remarks>
		Size GetDefaultNaturalSize ();
	}

	
	/// <summary>
	/// Event identifiers supported by all Xwt widgets to subscribe to
	/// </summary>
	[Flags]
	public enum WidgetEvent
	{
		/// <summary>  The widget can/wants to validate the type of a drag over operation. </summary>
		DragOverCheck = 1,
		/// <summary>  The widget can/wants to validate the data of a drag over operation. </summary>
		DragOver = 1 << 1,
		/// <summary>  The widget can/wants to validate a drop operation by its type. </summary>
		DragDropCheck = 1 << 2,
		/// <summary>  The widget can/wants to validate the data of and perform a drop operation. </summary>
		DragDrop = 1 << 3,
		/// <summary>  The widget can/wants to be notified of leaving drag operations. </summary>
		DragLeave = 1 << 4,
		/// <summary>  The widget can/wants to be notified of pressed keys. </summary>
		KeyPressed = 1 << 5,
		/// <summary>  The widget can/wants to be notified of released keys. </summary>
		KeyReleased = 1 << 6,
		/// <summary>  The widget wants to check its size when it changes. </summary>
		PreferredSizeCheck = 1 << 7,
		/// <summary>  The widget can/wants to be notified when it receives the focus. </summary>
		GotFocus = 1 << 8,
		/// <summary>  The widget can/wants to be notified when it looses the focus. </summary>
		LostFocus = 1 << 9,
		/// <summary>  The widget can/wants to be notified when the mouse enters it. </summary>
		MouseEntered = 1 << 10,
		/// <summary>  The widget can/wants to be notified when the mouse leaves it. </summary>
		MouseExited = 1 << 11,
		/// <summary>  The widget can/wants to be notified of pressed buttons. </summary>
		ButtonPressed = 1 << 12,
		/// <summary>  The widget can/wants to be notified of released buttons. </summary>
		ButtonReleased = 1 << 13,
		/// <summary>  The widget can/wants to be notified of mouse movements. </summary>
		MouseMoved = 1 << 14,
		/// <summary>  The widget can/wants to be notified when a drag operation starts. </summary>
		DragStarted = 1 << 15,
		/// <summary>  The widget can/wants to be notified when its bounds change. </summary>
		BoundsChanged = 1 << 16,
		/// <summary>  The widget can/wants to be notified of scroll events. </summary>
		MouseScrolled = 1 << 17,
		/// <summary>  The widget can/wants to be notified of text input events. </summary>
		TextInput = 1 << 18
	}
	
	/// <summary>
	/// Arguments for a starting drag&amp;drop operation.
	/// </summary>
	public class DragStartData
	{
		/// <summary>
		/// Gets the collection of data to be transferred through the drag operation.
		/// </summary>
		/// <value>The data to transfer.</value>
		public TransferDataSource Data { get; private set; }

		/// <summary>
		/// Gets the type of the drag action.
		/// </summary>
		/// <value>The drag action type.</value>
		public DragDropAction DragAction { get; private set; }

		/// <summary>
		/// Gets the image backend of the drag image.
		/// </summary>
		/// <value>The drag icon backend.</value>
		public object ImageBackend { get; private set; }

		/// <summary>
		/// Gets X coordinate of the drag image hotspot.
		/// </summary>
		/// <value>The image hotspot X coordinate.</value>
		public double HotX { get; private set; }

		/// <summary>
		/// Gets Y coordinate of the drag image hotspot.
		/// </summary>
		/// <value>The image hotspot Y coordinate.</value>
		public double HotY { get; private set; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Backends.DragStartData"/> class.
		/// </summary>
		/// <param name="data">The collection of data to be transferred through drag operation.</param>
		/// <param name="action">The type of the drag action.</param>
		/// <param name="imageBackend">The image backend of the drag image.</param>
		/// <param name="hotX">The image hotspot X coordinate.</param>
		/// <param name="hotY">The image hotspot Y coordinate.</param>
		internal DragStartData (TransferDataSource data, DragDropAction action, object imageBackend, double hotX, double hotY)
		{
			Data = data;
			DragAction = action;
			ImageBackend = imageBackend;
			HotX = hotX;
			HotY = hotY;
		}
	}
}

