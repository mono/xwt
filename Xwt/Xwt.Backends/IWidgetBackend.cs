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
	public interface IWidgetBackend: IBackend
	{
		void Initialize (IWidgetEventSink eventSink);

		/// <summary>
		/// Releases all resource used by the widget
		/// </summary>
		/// <remarks>
		/// This method is called to free all managed and unmanaged resources held by the backend.
		/// In general, the backend should destroy the native widget at this point.
		/// When a widget that has children is disposed, the Dispose method is called on all
		/// backends of all widgets in the children hierarchy.
		/// </remarks>
		void Dispose ();
		
		bool Visible { get; set; }
		bool Sensitive { get; set; }
		bool CanGetFocus { get; set; }
		bool HasFocus { get; }
		double Opacity { get; set; }

		Size Size { get; }
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
		void SetSizeRequest (double width, double height);
		
		void SetFocus ();
		
		void UpdateLayout ();

		Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint);

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
		/// Types.
		/// </param>
		/// <param name='dragAction'>
		/// Drag action.
		/// </param>
		void SetDragTarget (TransferDataType[] types, DragDropAction dragAction);
		
		object Font { get; set; }
		Color BackgroundColor { get; set; }
		string TooltipText { get; set; }
		
		/// <summary>
		/// Sets the cursor shape to be used when the mouse is over the widget
		/// </summary>
		/// <param name='cursorType'>
		/// The cursor type.
		/// </param>
		void SetCursor (CursorType cursorType);

        object Tag { get; set; }
	}
	
	public interface IWidgetEventSink
	{
		// Events
		void OnDragOverCheck (DragOverCheckEventArgs args);
		void OnDragOver (DragOverEventArgs args);
		void OnDragDropCheck (DragCheckEventArgs args);
		void OnDragDrop (DragEventArgs args);
		void OnDragLeave (EventArgs args);
		
		void OnDragFinished (DragFinishedEventArgs args);
		
		DragStartData OnDragStarted ();
		void OnKeyPressed (KeyEventArgs args);
		void OnKeyReleased (KeyEventArgs args);
		void OnGotFocus ();
		void OnLostFocus ();
		void OnMouseEntered ();
		void OnMouseExited ();
		void OnButtonPressed (ButtonEventArgs args);
		void OnButtonReleased (ButtonEventArgs args);
		void OnMouseMoved (MouseMovedEventArgs args);
		void OnBoundsChanged ();
        void OnMouseScrolled(MouseScrolledEventArgs args);

		// Events
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

		bool SupportsCustomScrolling ();
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

	
	[Flags]
	public enum WidgetEvent
	{
		DragOverCheck = 1,
		DragOver = 1 << 1,
		DragDropCheck = 1 << 2,
		DragDrop = 1 << 3,
		DragLeave = 1 << 4,
		KeyPressed = 1 << 5,
		KeyReleased = 1 << 6,
		PreferredSizeCheck = 1 << 7,
		GotFocus = 1 << 8,
		LostFocus = 1 << 9,
		MouseEntered = 1 << 10,
		MouseExited = 1 << 11,
		ButtonPressed = 1 << 12,
		ButtonReleased = 1 << 13,
		MouseMoved = 1 << 14,
		DragStarted = 1 << 15,
		BoundsChanged = 1 << 16,
        MouseScrolled = 1 << 17
	}
	
	public class DragStartData
	{
		
		public TransferDataSource Data { get; private set; }
		public DragDropAction DragAction { get; private set; }
		public object ImageBackend { get; private set; }
		public double HotX { get; private set; }
		public double HotY { get; private set; }
		
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

