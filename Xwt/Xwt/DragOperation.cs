// 
// DragOperation.cs
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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xwt.Drawing;
using Xwt.Backends;


namespace Xwt
{
	/// <summary>
	/// Drag operation, used to initialize a new drag operation.
	/// </summary>
	public class DragOperation
	{
		TransferDataSource data = new TransferDataSource ();
		Widget source;
		DragDropAction action;
		bool started;
		Image image;
		double hotX;
		double hotY;
		
		/// <summary>
		/// Occurs when the drag operation has finished.
		/// </summary>
		public event EventHandler<DragFinishedEventArgs> Finished;
		
		/// <summary>
		/// Initializes a new <see cref="Xwt.DragOperation"/> from a specified widget.
		/// </summary>
		/// <param name="w">The widget to start the drag operation from.</param>
		internal DragOperation (Widget w)
		{
			source = w;
			AllowedActions = DragDropAction.All;
		}
		
		/// <summary>
		/// A bitmask of the allowed drag actions for this drag.
		/// </summary>
		public DragDropAction AllowedActions {
			get { return action; } 
			set {
				if (started)
					throw new InvalidOperationException ("The drag action must be set before starting the drag operation");
				action = value;
			}
		}
		
		/// <summary>
		/// Gets the data collection of data transferred by this drag.
		/// </summary>
		/// <value>The data.</value>
		public TransferDataSource Data {
			get { return data; }
		}
		
		/// <summary>
		/// Sets the drag image.
		/// </summary>
		/// <param name="image">The drag Image.</param>
		/// <param name="hotX">The image hotspot X coordinate.</param>
		/// <param name="hotY">The image hotspot Y coordinate.</param>
		public void SetDragImage (Image image, double hotX, double hotY)
		{
			if (started)
				throw new InvalidOperationException ("The drag image must be set before starting the drag operation");
			this.image = image;
			this.hotX = hotX;
			this.hotY = hotY;
		}
		
		/// <summary>
		/// Start this drag operation.
		/// </summary>
		public void Start ()
		{
			if (!started) {
				started = true;
				source.DragStart (GetStartData ());
			}
		}

		/// <summary>
		/// Notifies subscriptors that the drag operation is finished.
		/// </summary>
		/// <param name="args">The drag finished arguments.</param>
		internal void NotifyFinished (DragFinishedEventArgs args)
		{
			if (Finished != null)
				Finished (this, args);
		}
		
		/// <summary>
		/// Gets the arguments for the starting drag operation.
		/// </summary>
		/// <returns>The drag start arguments.</returns>
		/// <exception cref="System.InvalidOperationException">The drag image is not set.</exception>
		internal DragStartData GetStartData ()
		{
			if (image == null)
				throw new InvalidOperationException ("The drag image must be set before starting the drag operation");
			image.InitForToolkit (source.Surface.ToolkitEngine);
			return new DragStartData (data, action, image.ToBitmap ().GetBackend (), hotX, hotY);
		}
		
	}
	
	/// <summary>
	/// Interface implemented by data stores containing data for drag &amp; drop and copy &amp; paste operations
	/// </summary>
	public interface ITransferData
	{
		/// <summary>
		/// Gets the transferred text.
		/// </summary>
		/// <value>The transferred text.</value>
		string Text { get; }

		/// <summary>
		/// Gets the transferred uris.
		/// </summary>
		/// <value>The transferred uris.</value>
		/// <remarks>This is used for e.g. file and url copy and drag operations.</remarks>
		Uri[] Uris { get; }

		/// <summary>
		/// Gets the transferred image.
		/// </summary>
		/// <value>The transferred image.</value>
		Xwt.Drawing.Image Image { get; }
		
		/// <summary>
		/// Gets the value identified by a specific transfer data type.
		/// </summary>
		/// <returns>The transferred value, or <c>null</c> if the store contains no value with the specific type.</returns>
		/// <param name="type">The specific transfer data type.</param>
		object GetValue (TransferDataType type);

		/// <summary>
		/// Gets the value identified by a specific <see cref="System.Type"/>.
		/// </summary>
		/// <returns>The transferred value.</returns>
		/// <typeparam name="T">The Type of the transferred value.</typeparam>
		T GetValue<T> () where T:class;

		/// <summary>
		/// Determines whether a value of the specified type is transferred.
		/// </summary>
		/// <returns><c>true</c> if this store contains a value of the specified type; otherwise, <c>false</c>.</returns>
		/// <param name="type">The specific transfer data type.</param>
		bool HasType (TransferDataType type);
	}
}

