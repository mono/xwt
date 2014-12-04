// 
// CustomWidgetBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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



namespace Xwt.Backends
{
	/// <summary>
	/// Base backend class for XWT-based widgets
	/// </summary>
	/// <remarks>
	/// This class can be used to implement backends fully based on XWT widgets.
	/// For example, XWT has an implementation of a color selector. If a backend
	/// doesn't have native color selector, the XWT one will be used. 
	/// </remarks>
	public class XwtWidgetBackend: Widget, IWidgetBackend
	{
		protected Widget Frontend {
			get; private set;
		}
		
		protected IWidgetEventSink EventSink {
			get; private set;
		}
		
		public XwtWidgetBackend ()
		{
		}

		#region IBackend implementation
		void IBackend.InitializeBackend (object frontend, ApplicationContext toolkit)
		{
			Frontend = (Widget) frontend;
		}

		public virtual void EnableEvent (object eventId)
		{
		}

		public virtual void DisableEvent (object eventId)
		{
		}
		#endregion

		#region IWidgetBackend implementation
		public virtual void Initialize (IWidgetEventSink eventSink)
		{
			EventSink = eventSink;
		}

		void IWidgetBackend.SetMinSize (double width, double height)
		{
			MinWidth = width;
			MinHeight = height;
		}

		void IWidgetBackend.SetSizeRequest (double width, double height)
		{
			WidthRequest = width;
			HeightRequest = height;
		}

		void IWidgetBackend.UpdateLayout ()
		{
			Surface.Reallocate ();
		}

		Size IWidgetBackend.GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return Surface.GetPreferredSize (widthConstraint, heightConstraint);
		}

		void IWidgetBackend.SetDragSource (TransferDataType[] types, DragDropAction dragAction)
		{
			this.SetDragSource (dragAction, types);
		}

		void IWidgetBackend.SetDragTarget (TransferDataType[] types, DragDropAction dragAction)
		{
			this.SetDragDropTarget (dragAction, types);
		}

		void IWidgetBackend.SetCursor (CursorType cursorType)
		{
			Cursor = cursorType;
		}

		object IWidgetBackend.Font {
			get {
				return XwtObject.GetBackend (Font);
			}
			set {
				Font = new Xwt.Drawing.Font (value);
			}
		}

		public IWidgetBackend NativeBackend {
			get { return BackendHost.Backend; }
		}
		
		object IWidgetBackend.NativeWidget {
			get { return BackendHost.Backend.NativeWidget; }
		}
		
		void IWidgetBackend.DragStart (DragStartData data)
		{
			BackendHost.Backend.DragStart (data);
		}
		
		#endregion
	}
}

