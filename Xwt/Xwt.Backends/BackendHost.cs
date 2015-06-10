// 
// BackendHost.cs
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
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;


namespace Xwt.Backends
{
	/// <summary>
	/// The BackendHost is the link between an Xwt frontend and a toolkit backend.
	/// </summary>
	/// <typeparam name="T">The Xwt frontend type.</typeparam>
	/// <typeparam name="B">The Xwt backend interface.</typeparam>
	public class BackendHost<T,B>: BackendHost where B:IBackend
	{
		/// <summary>
		/// Gets or sets the parent Xwt widget.
		/// </summary>
		/// <value>The parent Xwt widget.</value>
		public new T Parent {
			get { return (T)base.Parent; }
			set { base.Parent = value; }
		}
		
		/// <summary>
		/// Gets the toolkit backend.
		/// </summary>
		/// <value>The toolkit backend.</value>
		public new B Backend {
			get { return (B)base.Backend; }
		}
	}

	/// <summary>
	/// The BackendHost is the link between an Xwt frontend and a toolkit backend.
	/// </summary>
	public class BackendHost: EventHost
	{
		IBackend backend;
		bool usingCustomBackend;
		Toolkit engine;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Backends.BackendHost"/> class.
		/// </summary>
		public BackendHost ()
		{
			engine = Toolkit.CurrentEngine;
		}

		/// <summary>
		/// Sets a custom backend to be used instead of the default registered backend.
		/// </summary>
		/// <param name="backend">The custom backend.</param>
		public void SetCustomBackend (IBackend backend)
		{
			this.backend = backend;
			usingCustomBackend = true;
			LoadBackend ();
		}
		
		/// <summary>
		/// Gets the toolkit backend.
		/// </summary>
		/// <value>The toolkit backend.</value>
		public IBackend Backend {
			get {
				LoadBackend ();
				return backend;
			}
		}

		/// <summary>
		/// Gets or sets the toolkit engine.
		/// </summary>
		/// <value>The toolkit engine.</value>
		public Toolkit ToolkitEngine {
			get {
				if (engine != null)
					return engine;
				return engine = Toolkit.CurrentEngine;
			}
			internal set {
				engine = value;
			}
		}

		/// <summary>
		/// Gets the toolkit engine backend.
		/// </summary>
		/// <value>The engine backend.</value>
		internal ToolkitEngineBackend EngineBackend {
			get { return ToolkitEngine.Backend; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="Xwt.Backends.BackendHost.Backend"/> has been created.
		/// </summary>
		/// <value><c>true</c> if backend created; otherwise, <c>false</c>.</value>
		internal bool BackendCreated {
			get { return backend != null; }
		}
		
		/// <summary>
		/// Called when the backend has been created.
		/// </summary>
		protected virtual void OnBackendCreated ()
		{
			foreach (var ev in DefaultEnabledEvents)
				Backend.EnableEvent (ev);
		}
		
		/// <summary>
		/// Creates the backend for the connected frontend
		/// </summary>
		protected virtual IBackend OnCreateBackend ()
		{
			return EngineBackend.CreateBackendForFrontend (Parent.GetType ());
		}
		
		/// <summary>
		/// Ensures that the backend is loaded.
		/// </summary>
		internal void EnsureBackendLoaded ()
		{
			if (backend == null)
				LoadBackend ();
		}
		
		/// <summary>
		/// Loads the backend.
		/// </summary>
		protected void LoadBackend ()
		{
			if (usingCustomBackend) {
				usingCustomBackend = false;
				backend.InitializeBackend (Parent, engine.Context);
				OnBackendCreated ();
			}
			else if (backend == null) {
				backend = OnCreateBackend ();
				if (backend == null)
					throw new InvalidOperationException ("No backend found for object: " + Parent.GetType ());
				backend.InitializeBackend (Parent, engine.Context);
				OnBackendCreated ();
			}
		}

		/// <summary>
		/// Enables an event with the specified identifier.
		/// </summary>
		/// <param name="eventId">Event identifier (must be a valid event enum value).</param>
		protected override void OnEnableEvent (object eventId)
		{
			Backend.EnableEvent (eventId);
		}

		/// <summary>
		/// Disables an event with the specified identifier.
		/// </summary>
		/// <param name="eventId">Event identifier (must be a valid event enum value).</param>
		protected override void OnDisableEvent (object eventId)
		{
			Backend.DisableEvent (eventId);
		}
	}
}

