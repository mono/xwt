// 
// IPanedBackend.cs
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
	public interface IPanedBackend: IWidgetBackend, IChildPlacementHandler
	{
		/// <summary>
		/// Initializes the paned
		/// </summary>
		/// <param name='dir'>
		/// Orientation of the paned
		/// </param>
		void Initialize (Orientation dir);
		
		/// <summary>
		/// Gets or sets the position of the panel separator
		/// </summary>
		/// <value>
		/// The position.
		/// </value>
		double Position { get; set; }
		
		/// <summary>
		/// Sets the content of a panel
		/// </summary>
		/// <param name='panel'>
		/// Panel number: 1 or 2
		/// </param>
		/// <param name='widget'>
		/// Child widget
		/// </param>
		/// <param name='resize'>
		/// If set to <c>true</c> the panel is resized when the Paned view is resized
		/// </param>
		void SetPanel (int panel, IWidgetBackend widget, bool resize, bool shrink);
		
		/// <summary>
		/// Updates the panel settings
		/// </summary>
		/// <param name='panel'>
		/// Panel number: 1 or 2
		/// </param>
		/// <param name='resize'>
		/// If set to <c>true</c> the panel is resized when the Paned view is resized
		/// </param>
		void UpdatePanel (int panel, bool resize, bool shrink);
		
		/// <summary>
		/// Removes the content of a panel.
		/// </summary>
		/// <param name='panel'>
		/// Panel number: 1 or 2
		/// </param>
		void RemovePanel (int panel);
	}
	
	public interface IPanedEventSink: IWidgetEventSink
	{
		void OnPositionChanged ();
	}
	
	public enum PanedEvent
	{
		PositionChanged
	}
}

