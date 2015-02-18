//
// EventHost.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
	/// The EventHost is the base for every <see cref="Xwt.Backends.BackendHost"/> and takes care
	/// of event subscriptions, handlers and their activations.
	/// </summary>
	public class EventHost
	{
		static Dictionary<Type, List<EventMap>> overridenEventMap = new Dictionary<Type, List<EventMap>> ();
		static Dictionary<Type, HashSet<object>> overridenEvents = new Dictionary<Type, HashSet<object>> ();

		/// <summary>
		/// Maps an event handler of an Xwt component to an event identifier.
		/// </summary>
		/// <param name="eventId">The event identifier (must be valid event enum value
		/// like <see cref="Xwt.Backends.WidgetEvent"/>, identifying component specific events).</param>
		/// <param name="type">The Xwt component type.</param>
		/// <param name="methodName">The <see cref="System.Reflection.MethodInfo.Name"/> of the event handler.</param>
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

		/// <summary>
		/// Gets the default enabled events.
		/// </summary>
		/// <returns>The default enabled events for a Xwt widget type.</returns>
		/// <param name="type">The Xwt widgets type.</param>
		/// <param name="customEnabledEvents">Function that gets the custom enabled events.</param>
		public static HashSet<object> GetDefaultEnabledEvents (Type type, Func<IEnumerable<object>> customEnabledEvents)
		{
			HashSet<object> defaultEnabledEvents;
			if (!overridenEvents.TryGetValue (type, out defaultEnabledEvents)) {
				defaultEnabledEvents = new HashSet<object> ();
				Type t = type;
				while (t != typeof(Component) && t != typeof(Object)) {
					List<EventMap> emaps;
					if (overridenEventMap.TryGetValue (t, out emaps)) {
						foreach (var emap in emaps) {
							if (IsOverriden (emap, type, t))
								defaultEnabledEvents.Add (emap.EventId);
						}
					}
					t = t.BaseType;
				}
				defaultEnabledEvents.UnionWith (customEnabledEvents ());
				overridenEvents [type] = defaultEnabledEvents;
			}
			return defaultEnabledEvents;
		}

		static bool IsOverriden (EventMap emap, Type thisType, Type t)
		{
			var method = thisType.GetMethod (emap.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (method == null)
				throw new InvalidOperationException ("Invalid event mapping: method '" + emap.MethodName + "' not found in type '" + t + "'");
			return method.DeclaringType != t;
		}

		HashSet<object> defaultEnabledEvents;

		/// <summary>
		/// Gets or sets the parent Xwt widget.
		/// </summary>
		/// <value>The parent Xwt widget.</value>
		public object Parent { get; internal set; }

		/// <summary>
		/// Handles an event subscription.
		/// </summary>
		/// <param name="eventId">Event identifier (must be a valid event enum value).</param>
		/// <param name="eventDelegate">The subscribing handler delegate.</param>
		public void OnBeforeEventAdd (object eventId, Delegate eventDelegate)
		{
			if (eventDelegate == null && !DefaultEnabledEvents.Contains (eventId))
				OnEnableEvent (eventId);
		}

		/// <summary>
		/// Handles an event unsubscription.
		/// </summary>
		/// <param name="eventId">Event identifier (must be a valid event enum value).</param>
		/// <param name="eventDelegate">The handler delegate to remove from the event.</param>
		public void OnAfterEventRemove (object eventId, Delegate eventDelegate)
		{
			if (eventDelegate == null && !DefaultEnabledEvents.Contains (eventId))
				OnDisableEvent (eventId);
		}

		/// <summary>
		/// Enables an event with the specified identifier.
		/// </summary>
		/// <param name="eventId">Event identifier (must be a valid event enum value).</param>
		protected virtual void OnEnableEvent (object eventId)
		{
		}

		/// <summary>
		/// Disables an event with the specified identifier.
		/// </summary>
		/// <param name="eventId">Event identifier (must be a valid event enum value).</param>
		protected virtual void OnDisableEvent (object eventId)
		{
		}

		/// <summary>
		/// Gets the events which are enabled by default for this cell view
		/// </summary>
		/// <returns>The enabled events (must be valid event enum values)</returns>
		protected virtual IEnumerable<object> GetDefaultEnabledEvents ()
		{
			yield break;
		}

		/// <summary>
		/// Gets the default enabled events.
		/// </summary>
		/// <value>The default enabled events.</value>
		internal HashSet<object> DefaultEnabledEvents {
			get {
				if (defaultEnabledEvents == null) {
					defaultEnabledEvents = GetDefaultEnabledEvents (Parent.GetType (), GetDefaultEnabledEvents);
				}
				return defaultEnabledEvents;
			}
		}
	}
	
}
