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
using Xwt.Engine;

namespace Xwt
{
	public class DragOperation
	{
		TransferDataSource data = new TransferDataSource ();
		Widget source;
		DragDropAction action;
		bool started;
		Image image;
		double hotX;
		double hotY;
		
		public event EventHandler<DragFinishedEventArgs> Finished;
		
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
		
		public TransferDataSource Data {
			get { return data; }
		}
		
		public void SetDragImage (Image image, double hotX, double hotY)
		{
			if (started)
				throw new InvalidOperationException ("The drag image must be set before starting the drag operation");
			source.Surface.ToolkitEngine.ValidateObject (image);
			this.image = image;
			this.hotX = hotX;
			this.hotY = hotY;
		}
		
		public void Start ()
		{
			if (!started) {
				started = true;
				source.DragStart (GetStartData ());
			}
		}

		internal void NotifyFinished (DragFinishedEventArgs args)
		{
			if (Finished != null)
				Finished (this, args);
		}
		
		internal DragStartData GetStartData ()
		{
			return new DragStartData (data, action, Toolkit.GetBackend (image), hotX, hotY);
		}
		
	}
	
	public interface ITransferData
	{
		string Text { get; }
		Uri[] Uris { get; }
		Xwt.Drawing.Image Image { get; }
		
		object GetValue (TransferDataType type);
		T GetValue<T> () where T:class;
		bool HasType (TransferDataType type);
	}
}

