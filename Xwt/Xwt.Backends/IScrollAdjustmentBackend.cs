// 
// IScrollAdjustmentBackend.cs
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
	/// A backend for a scrollbar
	/// </summary>
	/// <remarks>
	/// XWT supports creating standalone ScrollAdjustment instances, but toolkits don't need to provide
	/// a backend for those cases, since XWT uses a default platform-agnostic implementation. 
	/// </remarks>
	public interface IScrollAdjustmentBackend: IBackend
	{
		/// <summary>
		/// Called to initialize the backend
		/// </summary>
		/// <param name='eventSink'>
		/// The event sink to be used to report events
		/// </param>
		void Initialize (IScrollAdjustmentEventSink eventSink);
		
		/// <summary>
		/// Gets or sets the current position of the scrollbar.
		/// </summary>
		/// <value>
		/// The position
		/// </value>
		/// <remarks>
		/// Value is the position of top coordinate of the visible rect (or left for horizontal scrollbars).
		/// So for example, if you set Value=35 and PageSize=100, the visible range will be 35 to 135.
		/// Value must be >= LowerValue and &lt;= (UpperValue - PageSize).
		/// </remarks>
		double Value { get; set; }
		
		/// <summary>
		/// Sets the scroll range and the inital value
		/// </summary>
		/// <param name="lowerValue">The lowest value of the scroll range</param>
		/// <param name="upperValue">The highest value of the scroll range</param>
		/// <param name="pageSize">Size of the visible range</param>
		/// <param name="pageIncrement">How much Value will be incremented when you click on the scrollbar to move
		/// to the next page (when the scrollbar supports it)</param>
		/// <param name="stepIncrement">How much the Value is incremented/decremented when you click on the down/up button in the scrollbar</param>
		/// <param name="value">Value.</param>
		void SetRange (double lowerValue, double upperValue, double pageSize, double pageIncrement, double stepIncrement, double value);
	}

	public interface IScrollAdjustmentEventSink
	{
		/// <summary>
		/// Raised when the position of the scrollbar changes
		/// </summary>
		void OnValueChanged ();
	}
	
	public enum ScrollAdjustmentEvent
	{
		ValueChanged
	}
}

