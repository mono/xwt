// 
// WidgetRegistry.cs
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
using System.Collections.Generic;
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.Engine
{
	public sealed class WidgetRegistry
	{
		static WidgetRegistry mainRegistry = new WidgetRegistry ();
		Dictionary<Type,Type> backendTypes = new Dictionary<Type, Type> ();
		Dictionary<Type,object> sharedBackends = new Dictionary<Type, object> ();

		// Get the default widget registry
		public static WidgetRegistry MainRegistry {
			get {
				return mainRegistry;
			}
		}

		public static void RunAsIfDefault (WidgetRegistry registry, Action action)
		{
			var saveRegistry = mainRegistry;
			mainRegistry = registry;
			action ();
			mainRegistry = saveRegistry;
		}
		
		internal T CreateBackend<T> (Type widgetType)
		{
			Type bt = null;

			if (!backendTypes.TryGetValue (widgetType, out bt))
				return default(T);
			object res = Activator.CreateInstance (bt);
			if (!typeof(T).IsInstanceOfType (res))
				throw new InvalidOperationException ("Invalid backend type.");
			return (T) res;
		}

		public EngineBackend FromEngine {
			get;
			set;
		}

		internal T CreateSharedBackend<T> (Type widgetType)
		{
			object res;
			if (!sharedBackends.TryGetValue (widgetType, out res))
				res = sharedBackends [widgetType] = CreateBackend<T> (widgetType);
			return (T)res;
		}
		
		public void RegisterBackend (Type widgetType, Type backendType)
		{
			backendTypes [widgetType] = backendType;
		}
		
		public object GetBackend (object obj)
		{
			if (obj is IFrontend)
				return ((IFrontend)obj).Backend;
			else if (obj == null)
				return null;
			else
				throw new InvalidOperationException ("Object doesn't have a backend");
		}
		
		public T CreateFrontend<T> (object backend)
		{
			return (T) Activator.CreateInstance (typeof(T), backend);
		}
		
		public object GetNativeWidget (Widget w)
		{
			return Application.EngineBackend.GetNativeWidget (w);
		}
		
		public object GetNativeImage (Image image)
		{
			return Application.EngineBackend.GetNativeImage (image);
		}
		
		public WindowFrame WrapWindow (object nativeWindow)
		{
			return new NativeWindowFrame (Application.EngineBackend.GetBackendForWindow (nativeWindow));
		}
	}
	
	class NativeWindowFrame: WindowFrame
	{
		public NativeWindowFrame (IWindowFrameBackend backend)
		{
			BackendHost.SetCustomBackend (backend);
		}
	}
}

