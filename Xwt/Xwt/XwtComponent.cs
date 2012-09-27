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
using Xwt.Backends;

namespace Xwt
{
	public abstract class XwtComponent: Component, IFrontend
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
		
		object IFrontend.Backend {
			get { return backendHost.Backend; }
		}

		protected static void MapEvent (object eventId, Type type, string methodName)
		{
			EventUtil.MapEvent (eventId, type, methodName);
		}
	}
	
	class EventUtil
	{
		static Dictionary<Type, List<EventMap>> overridenEventMap = new Dictionary<Type, List<EventMap>> ();
		static Dictionary<Type, HashSet<object>> overridenEvents = new Dictionary<Type, HashSet<object>> ();
		
		public static void MapEvent (object eventId, Type type, string methodName)
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
		
		public static HashSet<object> GetDefaultEnabledEvents (Type type)
		{
			HashSet<object> defaultEnabledEvents;
			if (!overridenEvents.TryGetValue (type, out defaultEnabledEvents)) {
				defaultEnabledEvents = new HashSet<object> ();
				Type t = type;
				while (t != typeof(Component)) {
					List<EventMap> emaps;
					if (overridenEventMap.TryGetValue (t, out emaps)) {
						foreach (var emap in emaps) {
							if (IsOverriden (emap, type, t))
								defaultEnabledEvents.Add (emap.EventId);
						}
					}
					t = t.BaseType;
				}
				overridenEvents [type] = defaultEnabledEvents;
			}
			return defaultEnabledEvents;
		}
		
		static bool IsOverriden (EventMap emap, Type thisType, Type t)
		{
			var method = thisType.GetMethod (emap.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			return method.DeclaringType != t;
		}
	}
}

