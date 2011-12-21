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
		void Dispose (bool disposing);
		
		bool Visible { get; set; }
		bool Sensitive { get; set; }
		bool CanGetFocus { get; set; }
		bool HasFocus { get; }
		Size Size { get; }
		Point ConvertToScreenCoordinates (Point widgetCoordinates);
		
		/// <summary>
		/// Sets the minimum size of the widget
		/// </summary>
		/// <param name='width'>
		/// Minimun width. If the value is -1, it means no minimum width.
		/// </param>
		/// <param name='height'>
		/// Minimum height. If the value is -1, it means no minimum height.
		/// </param>
		void SetMinSize (double width, double height);
		
		void SetFocus ();
		
		void UpdateLayout ();
		WidgetSize GetPreferredWidth ();
		WidgetSize GetPreferredHeightForWidth (double width);
		WidgetSize GetPreferredHeight ();
		WidgetSize GetPreferredWidthForHeight (double height);
		
		object NativeWidget { get; }
		
		void DragStart (TransferDataSource data, DragDropAction allowedDragActions, object imageBackend, double hotX, double hotY);
		void SetDragSource (string[] types, DragDropAction dragAction);
		void SetDragTarget (string[] types, DragDropAction dragAction);
		
		object Font { get; set; }
		Color BackgroundColor { get; set; }
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
		void OnKeyPressed (KeyEventArgs args);
		void OnKeyReleased (KeyEventArgs args);
		void OnGotFocus ();
		void OnLostFocus ();

		// Events
		WidgetSize OnGetPreferredWidth ();
		WidgetSize OnGetPreferredHeight ();
		WidgetSize OnGetPreferredHeightForWidth (double width);
		WidgetSize OnGetPreferredWidthForHeight (double height);
		
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
		SizeRequestMode GetSizeRequestMode ();
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
		PreferredWidthCheck = 1 << 7,
		PreferredHeightCheck = 1 << 8,
		PreferredWidthForHeightCheck = 1 << 9,
		PreferredHeightForWidthCheck = 1 << 10,
		GotFocus = 1 << 11,
		LostFocus = 1 << 12
	}
	
	public interface DragOperationEventSink
	{
		
	}
	
	public interface ITransferDataSource
	{
		string[] Types { get; }
	}
}

