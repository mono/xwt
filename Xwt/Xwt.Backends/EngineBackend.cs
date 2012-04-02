// 
// EngineBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2011-2012 Xamarin Inc
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
using System.Reflection;
using System.IO;
using Xwt.Drawing;
using Xwt.Engine;

namespace Xwt.Backends
{
	public abstract class EngineBackend
	{
		public virtual void InitializeApplication ()
		{
		}
		
		public abstract void RunApplication ();
		
		public abstract void ExitApplication ();
		
		/// <summary>
		/// Asynchronously invokes <paramref name="action"/> on the engine UI thread.
		/// </summary>
		/// <param name="action">The action to invoke.</param>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
		public abstract void InvokeAsync (Action action);

		/// <summary>
		/// Begins invoking <paramref name="action"/> on a timer period of <paramref name="timeSpan"/>.
		/// </summary>
		/// <param name="action">The function to invoke. Returning <c>false</c> stops the timer.</param>
		/// <param name="timeSpan">The period before the initial invoke and between subsequent invokes.</param>
		/// <returns>An identifying object that can be used to cancel the timer with <seealso cref="CancelTimerInvoke"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
		/// <seealso cref="CancelTimerInvoke"/>
		public abstract object TimerInvoke (Func<bool> action, TimeSpan timeSpan);

		/// <summary>
		/// Cancels an invoke timer started from <see cref="TimerInvoke"/>.
		/// </summary>
		/// <param name="id">The unique object returned from <see cref="TimerInvoke"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="id"/> is <c>null</c>.</exception>
		public abstract void CancelTimerInvoke (object id);
		
		public abstract object GetNativeWidget (Widget w);
		
		public virtual object GetNativeImage (Image image)
		{
			return WidgetRegistry.GetBackend (image);
		}
		
		public abstract IWindowFrameBackend GetBackendForWindow (object nativeWindow);
		
		public virtual object GetNativeParentWindow (Widget w)
		{
			return null;
		}
		
		public virtual bool HandlesSizeNegotiation {
			get { return false; }
		}
	}
}

