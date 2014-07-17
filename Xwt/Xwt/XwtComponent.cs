// 
// XwtComponent.cs
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
using System.ComponentModel;

using System.Collections.Generic;
using System.Reflection;
using Xwt.Backends;
using System.Threading;

namespace Xwt
{
	[System.ComponentModel.DesignerCategory ("Code")]
	public abstract class XwtComponent : Component, IFrontend, ISynchronizeInvoke
	{
		BackendHost backendHost;
		
		public XwtComponent ()
		{
			backendHost = CreateBackendHost ();
			backendHost.Parent = this;
		}
		
		protected virtual BackendHost CreateBackendHost ()
		{
			return new BackendHost ();
		}

		protected BackendHost BackendHost {
			get { return backendHost; }
		}
		
		Toolkit IFrontend.ToolkitEngine {
			get { return backendHost.ToolkitEngine; }
		}
		
		object IFrontend.Backend {
			get { return backendHost.Backend; }
		}

		/// <summary>
		/// A value, that can be used to identify this component
		/// </summary>
		public object Tag { get; set; }

		protected static void MapEvent (object eventId, Type type, string methodName)
		{
			EventHost.MapEvent (eventId, type, methodName);
		}
		
		internal void VerifyConstructorCall<T> (T t)
		{
			if (GetType () != typeof(T))
				throw new InvalidConstructorInvocation (typeof(T));
		}

		#region ISynchronizeInvoke implementation

		public IAsyncResult BeginInvoke (Delegate method, object[] args)
		{
			var asyncResult = new AsyncInvokeResult ();
			asyncResult.Invoke (method, args);
			return asyncResult;
		}

		public object EndInvoke (IAsyncResult result)
		{
			var xwtResult = result as AsyncInvokeResult;
			if (xwtResult != null) {
				xwtResult.AsyncResetEvent.Wait ();
				if (xwtResult.Exception != null)
					throw xwtResult.Exception;
			} else {
				result.AsyncWaitHandle.WaitOne ();
			}

			return result.AsyncState;
		}

		public object Invoke (Delegate method, object[] args)
		{
			return EndInvoke(BeginInvoke (method, args));
		}

		public bool InvokeRequired {
			get {
				return Application.UIThread != Thread.CurrentThread;
			}
		}

		#endregion
	}

	class AsyncInvokeResult : IAsyncResult
	{
		ManualResetEventSlim asyncResetEvent = new ManualResetEventSlim (false);

		public AsyncInvokeResult ()
		{
			this.asyncResetEvent = new ManualResetEventSlim ();
		}

		internal void Invoke (Delegate method, object[] args)
		{
			Application.Invoke (delegate {
				try {
					AsyncState = method.DynamicInvoke(args);
				} catch (Exception ex){
					Exception = ex;
				} finally {
					IsCompleted = true;
					asyncResetEvent.Set ();
				}
			});
		}

		#region IAsyncResult implementation

		public object AsyncState {
			get;
			private set;
		}

		public Exception Exception {
			get;
			private set;
		}

		internal ManualResetEventSlim AsyncResetEvent {
			get {
				return asyncResetEvent;
			}
		}

		public WaitHandle AsyncWaitHandle {
			get {
				if (asyncResetEvent == null) {
					asyncResetEvent = new ManualResetEventSlim(false);

					if (IsCompleted)
						asyncResetEvent.Set();
				}
				return asyncResetEvent.WaitHandle;
			}
		}

		public bool CompletedSynchronously { get { return false; } }


		public bool IsCompleted {
			get;
			private set;
		}

		#endregion


	}
}

