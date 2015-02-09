// 
// IBackend.cs
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
	/// <summary>
	/// The Xwt backend base interface. All Xwt backends implement it.
	/// </summary>
	public interface IBackend
	{
		/// <summary>
		/// Initializes the backend.
		/// </summary>
		/// <param name="frontend">The Xwt frontend.</param>
		/// <param name="context">The application context to initialize the backend for.</param>
		void InitializeBackend (object frontend, ApplicationContext context);

		/// <summary>
		/// Enables the event.
		/// </summary>
		/// <param name="eventId">Event identifier (must be a valid event enum value).</param>
		void EnableEvent (object eventId);

		/// <summary>
		/// Disables the event.
		/// </summary>
		/// <param name="eventId">Event identifier (must be a valid event enum value).</param>
		void DisableEvent (object eventId);
	}
}

