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
using Xwt.Engine;
using System.Collections.Generic;
using System.Reflection;

namespace Xwt
{
	public abstract class XwtComponent: Component
	{
		IBackend backend;
		
		HashSet<object> defaultEnabledEvents;
		static Dictionary<Type, List<EventMap>> overridenEventMap = new Dictionary<Type, List<EventMap>> ();
		static Dictionary<Type, HashSet<object>> overridenEvents = new Dictionary<Type, HashSet<object>> ();
		
		protected IBackend Backend {
			get {
				LoadBackend ();
				return backend;
			}
		}
		
		protected virtual void OnBackendCreated ()
		{
			foreach (var ev in DefaultEnabledEvents)
				Backend.EnableEvent (ev);
		}
		
		protected virtual IBackend OnCreateBackend ()
		{
			Type t = GetType ();
			while (t != typeof(XwtComponent)) {
				IBackend b = WidgetRegistry.CreateBackend<IBackend> (t);
				if (b != null)
					return b;
				t = t.BaseType;
			}
			return null;
		}
		
		protected override void Dispose (bool disposing)
		{
			IDisposable disp = backend as IDisposable;
			if (disp != null)
				disp.Dispose ();
		}
		
		protected void LoadBackend ()
		{
			if (backend == null) {
				backend = OnCreateBackend ();
				if (backend == null)
					throw new InvalidOperationException ("No backend found for widget: " + GetType ());
				backend.Initialize (this);
				OnBackendCreated ();
			}
		}
		
		internal protected static IBackend GetBackend (XwtComponent w)
		{
			return w != null ? w.Backend : null;
		}
		
		protected static void MapEvent (object eventId, Type type, string methodName)
		{
			List<EventMap> events;
			if (!overridenEventMap.TryGetValue (type, out events)) {
				events = new List<EventMap> ();
				overridenEventMap [type] = events;
			}
			EventMap emap = new EventMap () {
				MethodName = methodName,
				EventId = eventId
			};
			events.Add (emap);
		}
		
		protected void OnBeforeEventAdd (object eventId, Delegate eventDelegate)
		{
			if (eventDelegate == null && !DefaultEnabledEvents.Contains (eventId))
				Backend.EnableEvent (eventId);
		}
		
		protected void OnAfterEventRemove (object eventId, Delegate eventDelegate)
		{
			if (eventDelegate != null && !DefaultEnabledEvents.Contains (eventId))
				Backend.DisableEvent (eventId);
		}
		
		HashSet<object> DefaultEnabledEvents {
			get {
				if (defaultEnabledEvents == null) {
					Type thisType = GetType ();
					if (!overridenEvents.TryGetValue (thisType, out defaultEnabledEvents)) {
						defaultEnabledEvents = new HashSet<object> ();
						Type t = thisType;
						while (t != typeof(XwtComponent)) {
							List<EventMap> emaps;
							if (overridenEventMap.TryGetValue (t, out emaps)) {
								foreach (var emap in emaps) {
									if (IsOverriden (emap, thisType, t))
										defaultEnabledEvents.Add (emap.EventId);
								}
							}
							t = t.BaseType;
						}
						overridenEvents [GetType ()] = defaultEnabledEvents;
					}
				}
				return defaultEnabledEvents;
			}
		}
		
		bool IsOverriden (EventMap emap, Type thisType, Type t)
		{
			var method = thisType.GetMethod (emap.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			return method.DeclaringType != t;
		}
	}
}

