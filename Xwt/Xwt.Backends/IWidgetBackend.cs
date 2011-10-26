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

namespace Xwt.Backends
{
	public interface IWidgetBackend: IBackend, IDisposable
	{
		void Initialize (IWidgetEventSink eventSink);
		
		bool Visible { get; set; }
		bool Sensitive { get; set; }
		Size Size { get; }
		
		void UpdateLayout ();
		WidgetSize GetPreferredWidth ();
		WidgetSize GetPreferredHeightForWidth (double width);
		WidgetSize GetPreferredHeight ();
		WidgetSize GetPreferredWidthForHeight (double height);
		
		object NativeWidget { get; }
		
		void DragStart (TransferDataSource data, DragDropAction dragAction);
		void SetDragSource (string[] types, DragDropAction dragAction);
		void SetDragTarget (string[] types, DragDropAction dragAction);
	}
	
	public interface IWidgetEventSink
	{
		void OnDragOverCheck (DragOverCheckEventArgs args);
		void OnDragOver (DragOverEventArgs args);
		void OnDragDropCheck (DragCheckEventArgs args);
		void OnDragDrop (DragEventArgs args);
		void OnDragLeave (EventArgs args);
	}
	
	[Flags]
	public enum WidgetEvent
	{
		DragOverCheck = 1,
		DragOver = 2,
		DragDropCheck = 4,
		DragDrop = 8,
		DragLeave = 16
	}
	
	public interface DragOperationEventSink
	{
		
	}
	
	public interface ITransferDataSource
	{
		string[] Types { get; }
	}
}

