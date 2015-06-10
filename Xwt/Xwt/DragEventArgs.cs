// 
// DragEventArgs.cs
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

namespace Xwt
{
	/// <summary>
	/// Arguments for a drag&amp;drop operation type validation event.
	/// </summary>
	public class DragCheckEventArgs: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.DragCheckEventArgs"/> class.
		/// </summary>
		/// <param name="position">The drop coordinates (in widget coordinates).</param>
		/// <param name="types">The types of the transferred data.</param>
		/// <param name="action">The proposed drag&amp;drop action type.</param>
		public DragCheckEventArgs (Point position, TransferDataType[] types, DragDropAction action)
		{
			DataTypes = types;
			Action = action;
			Position = position;
			Result = DragDropResult.None;
		}
		
		/// <summary>
		/// Gets the types of data transferred by the drag&amp;drop operation.
		/// </summary>
		/// <value>The types of the transferred data.</value>
		public TransferDataType[] DataTypes { get; private set; }
		
		/// <summary>
		/// Gets the potential drop coordinates (in widget coordinates)
		/// </summary>
		/// <value>The drop position.</value>
		public Point Position { get; private set; }
		
		/// <summary>
		/// Gets the type of the proposed drag&amp;drop action.
		/// </summary>
		/// <value>The action proposed by the drag&amp;drop operation.</value>
		/// <remarks>The action depends on the control keys being pressed.</remarks>
		public DragDropAction Action { get; private set; }
		
		/// <summary>
		/// Gets or sets the result, indicating whether the widget allows this drop operation.
		/// </summary>
		/// <value>The validation result.</value>
		/// <remarks>
		/// To be set by the handler of the event. Specifies whether the drop action is allowed.
		/// If not specified (<see cref="Xwt.DragDropResult.None"/> , the action will be
		/// validated by the handler of the <see cref="Xwt.Widget.DragDrop"/> event. If set to
		/// <see cref="Xwt.DragDropResult.Canceled"/> the operation will be aborted.
		/// </remarks>
		public DragDropResult Result { get; set; }
	}

	/// <summary>
	/// Arguments for a drop operation event.
	/// </summary>
	public class DragEventArgs: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.DragEventArgs"/> class.
		/// </summary>
		/// <param name="position">The drop coordinates (in widget coordinates).</param>
		/// <param name="dataStore">The collection of transferred data.</param>
		/// <param name="action">The drag&amp;drop action type.</param>
		public DragEventArgs (Point position, Xwt.Backends.TransferDataStore dataStore, DragDropAction action)
		{
			Data = dataStore;
			Position = position;
			Action = action;
			Success = false;
		}
		
		/// <summary>
		/// Gets the data being dropped.
		/// </summary>
		/// <value>The dropped data.</value>
		public ITransferData Data { get; private set; }
		
		/// <summary>
		/// Gets the drop coordinates (in widget coordinates)
		/// </summary>
		/// <value>The widget relative drop position.</value>
		public Point Position { get; private set; }
		
		/// <summary>
		/// Gets the type of the drag&amp;drop action.
		/// </summary>
		/// <value>The drag&amp;drop action.</value>
		/// <remarks>The action depends on the control keys being pressed.</remarks>
		public DragDropAction Action { get; private set; }
		
		/// <summary>
		/// Gets or sets a value indicating whether a <see cref="Xwt.DragEventArgs"/> drop was successful.
		/// </summary>
		/// <value><c>true</c> if drop was successful; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// To be set by the handler of the event.
		/// </remarks>
		public bool Success { get; set; }
	}
	
	/// <summary>
	/// Arguments for a drag over operation type validation event.
	/// </summary>
	public class DragOverCheckEventArgs: EventArgs
	{
		DragDropAction allowedAction;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.DragOverCheckEventArgs"/> class.
		/// </summary>
		/// <param name="position">The potential drop coordinates (in widget coordinates).</param>
		/// <param name="types">The transferred data types.</param>
		/// <param name="action">The proposed drag&amp;drop action type.</param>
		public DragOverCheckEventArgs (Point position, TransferDataType[] types, DragDropAction action)
		{
			DataTypes = types;
			Action = action;
			Position = position;
			AllowedAction = DragDropAction.Default;
		}
		
		/// <summary>
		/// Gets the types of data transferred by the drag operation.
		/// </summary>
		/// <value>The types of the transferred data.</value>
		public TransferDataType[] DataTypes { get; private set; }
		
		/// <summary>
		/// Gets the potential drop coordinates (in widget coordinates)
		/// </summary>
		/// <value>The widget relative drop position.</value>
		public Point Position { get; private set; }
		
		/// <summary>
		/// Gets the proposed drop action.
		/// </summary>
		/// <value>The proposed drop action</value>
		/// <remarks>The action depends on the control keys being pressed.</remarks>
		public DragDropAction Action { get; private set; }

		/// <summary>
		/// Gets or sets the actions allowed for this widget.
		/// </summary>
		/// <value>The allowed drop actions</value>
		/// <remarks>
		/// To be set by the handler of the event. Specifies the action that will be performed if the item is dropped.
		/// If not specified or set to <see cref="Xwt.DragDropAction.Default"/>, the action will be validated
		/// by the handler of the <see cref="Xwt.Widget.DragOver"/> event. If set to <see cref="Xwt.DragDropAction.None"/>
		/// the drop action is not allowed.
		/// </remarks>
		public DragDropAction AllowedAction {
			get { return allowedAction; }
			set {
				switch (value) {
				case DragDropAction.Copy:
				case Xwt.DragDropAction.Default:
				case Xwt.DragDropAction.Link:
				case Xwt.DragDropAction.Move:
				case Xwt.DragDropAction.None:
					allowedAction = value;
					break;
				default:
					throw new ArgumentException ("Allowed action must be one of Copy, Link, Move, None or Default");
				}
			}
		}
	}

	/// <summary>
	/// Arguments for a drag over data type validation event.
	/// </summary>
	public class DragOverEventArgs: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.DragOverEventArgs"/> class.
		/// </summary>
		/// <param name="position">The potential drop coordinates (in widget coordinates).</param>
		/// <param name="dataStore">The collection of transferred data.</param>
		/// <param name="action">The proposed drag&amp;drop action type.</param>
		public DragOverEventArgs (Point position, ITransferData dataStore, DragDropAction action)
		{
			Position = position;
			Data = dataStore;
			Action = action;
			AllowedAction = DragDropAction.Default;
		}
		
		/// <summary>
		/// Gets the data transferred by the drag action.
		/// </summary>
		/// <value>The transferred data.</value>
		public ITransferData Data { get; private set; }
		
		/// <summary>
		/// Gets the potential drop coordinates (in widget coordinates)
		/// </summary>
		/// <value>The widget relative drop position.</value>
		public Point Position { get; private set; }
		
		/// <summary>
		/// Gets the proposed drop action.
		/// </summary>
		/// <value>The proposed drop action</value>
		/// <remarks>The action depends on the control keys being pressed.</remarks>
		public DragDropAction Action { get; private set; }
		
		/// <summary>
		/// Gets or sets the allowed action.
		/// </summary>
		/// <value>The allowed drop actions</value>
		/// <remarks>
		/// To be set by the handler of the event. Specifies the action that will be performed if the item is dropped.
		/// If not specified or set to <see cref="Xwt.DragDropAction.Default"/>, the action will be validated
		/// by the handler of the <see cref="Xwt.Widget.DragDropCheck"/> event. If set to <see cref="Xwt.DragDropAction.None"/>
		/// the drop action is not allowed.
		/// </remarks>
		public DragDropAction AllowedAction { get; set; }
	}
	
	/// <summary>
	/// Drag finished event arguments.
	/// </summary>
	public class DragFinishedEventArgs: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.DragFinishedEventArgs"/> class.
		/// </summary>
		/// <param name="deleteSource">If set to <c>true</c> delete the data source.</param>
		public DragFinishedEventArgs (bool deleteSource)
		{
			DeleteSource = deleteSource;
		}
		
		/// <summary>
		/// Gets a value indicating whether the data source should be deleted.
		/// </summary>
		/// <value><c>true</c> to delete the data source; otherwise, <c>false</c>.</value>
		public bool DeleteSource { get; private set; }
	}
	
	/// <summary>
	/// Drag started event arguments.
	/// </summary>
	public class DragStartedEventArgs: EventArgs
	{
		/// <summary>
		/// Gets or sets the started drag operation.
		/// </summary>
		/// <value>The started drag operation.</value>
		public DragOperation DragOperation { get; internal set; }
	}
	
	/// <summary>
	/// Drag&amp;drop result, indicating whether a drag&amp;drop operation will be/was successful.
	/// </summary>
	public enum DragDropResult
	{
		/// <summary>
		/// drag&amp;drop can be performed, or was successful.
		/// </summary>
		Success,
		/// <summary>
		/// drag&amp;drop denied.
		/// </summary>
		Canceled,
		/// <summary>
		/// drag&amp;drop event is unhandled.
		/// </summary>
		None
	}
}

