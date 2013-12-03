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


namespace Xwt.Backends
{
	public class BackendHost<T,B>: BackendHost where B:IBackend
	{
		public new T Parent {
			get { return (T)base.Parent; }
			set { base.Parent = value; }
		}
		
		public new B Backend {
			get { return (B)base.Backend; }
		}
	}

	public class BackendHost
	{
		IBackend backend;
		bool usingCustomBackend;
		Toolkit engine;

		HashSet<object> defaultEnabledEvents;
		
		public BackendHost ()
		{
			engine = Toolkit.CurrentEngine;
		}

		public void SetCustomBackend (IBackend backend)
		{
			this.backend = backend;
			usingCustomBackend = true;
			OnBackendCreated ();
		}
		
		public object Parent { get; internal set; }
		
		public IBackend Backend {
			get {
				LoadBackend ();
				return backend;
			}
		}

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

		internal ToolkitEngineBackend EngineBackend {
			get { return ToolkitEngine.Backend; }
		}

		internal bool BackendCreated {
			get { return backend != null; }
		}
		
		protected virtual void OnBackendCreated ()
		{
			foreach (var ev in DefaultEnabledEvents)
				Backend.EnableEvent (ev);
		}
		
		protected virtual IBackend OnCreateBackend ()
		{
			return EngineBackend.CreateBackendForFrontend (Parent.GetType ());
		}
		
		internal void EnsureBackendLoaded ()
		{
			if (backend == null)
				LoadBackend ();
		}
		
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
		
		public void OnBeforeEventAdd (object eventId, Delegate eventDelegate)
		{
			if (eventDelegate == null && !DefaultEnabledEvents.Contains (eventId))
				Backend.EnableEvent (eventId);
		}
		
		public void OnAfterEventRemove (object eventId, Delegate eventDelegate)
		{
			if (eventDelegate == null && !DefaultEnabledEvents.Contains (eventId))
				Backend.DisableEvent (eventId);
		}
		
		internal HashSet<object> DefaultEnabledEvents {
			get {
				if (defaultEnabledEvents == null) {
					defaultEnabledEvents = EventUtil.GetDefaultEnabledEvents (Parent.GetType (), GetDefaultEnabledEvents);
				}
				return defaultEnabledEvents;
			}
		}

		/// <summary>
		/// Gets the events which are enabled by default for this widget
		/// </summary>
		/// <returns>The enabled events (must be valid event enum values)</returns>
		protected virtual IEnumerable<object> GetDefaultEnabledEvents ()
		{
			yield break;
		}
	}
}

