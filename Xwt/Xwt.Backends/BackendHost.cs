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
using Xwt.Engine;

namespace Xwt.Backends
{
	public class BackendHost<T,B>: BackendHost where B:IBackend
	{
		public BackendHost ()
		{
		}
		
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
		ToolkitEngine engine;

		HashSet<object> defaultEnabledEvents;
		
		public BackendHost ()
		{
			engine = ToolkitEngine.CurrentEngine;
		}
		
		public void SetCustomBackend (IBackend backend)
		{
			this.backend = backend;
			usingCustomBackend = true;
		}
		
		public object Parent { get; internal set; }
		
		public IBackend Backend {
			get {
				LoadBackend ();
				return backend;
			}
		}

		public ToolkitEngine ToolkitEngine {
			get {
				if (engine != null)
					return engine;
				return engine = ToolkitEngine.CurrentEngine;
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
			Type t = Parent.GetType ();
			while (t != typeof(object)) {
				IBackend b = EngineBackend.CreateBackend<IBackend> (t);
				if (b != null)
					return b;
				t = t.BaseType;
			}
			return null;
		}
		
		public void EnsureBackendLoaded ()
		{
			if (backend == null)
				LoadBackend ();
		}
		
		protected void LoadBackend ()
		{
			if (usingCustomBackend) {
				usingCustomBackend = false;
				backend.InitializeBackend (Parent);
				OnBackendCreated ();
			}
			else if (backend == null) {
				backend = OnCreateBackend ();
				if (backend == null)
					throw new InvalidOperationException ("No backend found for object: " + Parent.GetType ());
				backend.InitializeBackend (Parent);
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
			if (eventDelegate != null && !DefaultEnabledEvents.Contains (eventId))
				Backend.DisableEvent (eventId);
		}
		
		internal HashSet<object> DefaultEnabledEvents {
			get {
				if (defaultEnabledEvents == null)
					defaultEnabledEvents = EventUtil.GetDefaultEnabledEvents (Parent.GetType ());
				return defaultEnabledEvents;
			}
		}	
	}
}

