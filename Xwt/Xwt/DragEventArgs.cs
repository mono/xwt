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
	public class DragCheckEventArgs: EventArgs
	{
		public DragCheckEventArgs (Point position, TransferDataType[] types, DragDropAction action)
		{
			DataTypes = types;
			Action = action;
			Position = position;
			Result = DragDropResult.None;
		}
		
		public TransferDataType[] DataTypes { get; private set; }
		
		public Point Position { get; private set; }
		
		public DragDropAction Action { get; private set; }
		
		public DragDropResult Result { get; set; }
	}

	public class DragEventArgs: EventArgs
	{
		public DragEventArgs (Point position, TransferDataStore dataStore, DragDropAction action)
		{
			Data = dataStore;
			Position = position;
			Action = action;
			Success = false;
		}
		
		public ITransferData Data { get; private set; }
		
		public Point Position { get; private set; }
		
		public DragDropAction Action { get; private set; }
		
		public bool Success { get; set; }
	}
	
	public class DragOverCheckEventArgs: EventArgs
	{
		DragDropAction allowedAction;
		
		public DragOverCheckEventArgs (Point position, TransferDataType[] types, DragDropAction action)
		{
			DataTypes = types;
			Action = action;
			Position = position;
			AllowedAction = DragDropAction.Default;
		}
		
		/// <summary>
		/// Type of the data being dropped
		/// </summary>
		public TransferDataType[] DataTypes { get; private set; }
		
		/// <summary>
		/// Drop coordinates (in widget coordinates)
		/// </summary>
		public Point Position { get; private set; }
		
		/// <summary>
		/// Proposed drop action, which depends on the control keys being pressed
		/// </summary>
		public DragDropAction Action { get; private set; }

		/// <summary>
		/// Allowed action
		/// </summary>
		/// <remarks>
		/// To be set by the handler of the event. Specifies the action that will be performed if the item is dropped.
		/// If not specified or set to Default, the action will be determined by the handler of DragOver.
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

	public class DragOverEventArgs: EventArgs
	{
		public DragOverEventArgs (Point position, ITransferData dataStore, DragDropAction action)
		{
			Position = position;
			Data = dataStore;
			Action = action;
			AllowedAction = DragDropAction.Default;
		}
		
		public ITransferData Data { get; private set; }
		
		/// <summary>
		/// Drop coordinates (in widget coordinates)
		/// </summary>
		public Point Position { get; private set; }
		
		/// <summary>
		/// Proposed drop action, which depends on the control keys being pressed
		/// </summary>
		public DragDropAction Action { get; private set; }
		
		/// <summary>
		/// Allowed action
		/// </summary>
		/// <remarks>
		/// To be set by the handler of the event. Specifies the action that will be performed if the item is dropped.
		/// If not specified or set to Default, the action will be determined by the handler of DragDropCheck.
		/// </remarks>
		public DragDropAction AllowedAction { get; set; }
	}
	
	public class DragFinishedEventArgs: EventArgs
	{
		public DragFinishedEventArgs (bool deleteSource)
		{
			DeleteSource = deleteSource;
		}
		
		public bool DeleteSource { get; private set; }
	}
	
	public class DragStartedEventArgs: EventArgs
	{
		public DragOperation DragOperation { get; internal set; }
	}
	
	public enum DragDropResult
	{
		Success,
		Canceled,
		None
	}
}

