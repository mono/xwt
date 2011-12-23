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

namespace Xwt.Engine
{
	public static class WidgetRegistry
	{
		static Dictionary<Type,Type> backendTypes = new Dictionary<Type, Type> ();
		static Dictionary<Type,object> sharedBackends = new Dictionary<Type, object> ();
		
		internal static T CreateBackend<T> (Type widgetType)
		{
			Type bt;
			if (!backendTypes.TryGetValue (widgetType, out bt))
				return default(T);
			object res = Activator.CreateInstance (bt);
			if (!typeof(T).IsInstanceOfType (res))
				throw new InvalidOperationException ("Invalid backend type.");
			return (T) res;
		}
		
		internal static T CreateSharedBackend<T> (Type widgetType)
		{
			object res;
			if (!sharedBackends.TryGetValue (widgetType, out res))
				res = sharedBackends [widgetType] = CreateBackend<T> (widgetType);
			return (T)res;
		}
		
		public static void RegisterBackend (Type widgetType, Type backendType)
		{
			backendTypes [widgetType] = backendType;
		}
		
		public static object GetBackend (object obj)
		{
			if (obj is XwtComponent)
				return XwtComponent.GetBackend ((XwtComponent)obj);
			else if (obj is XwtObject)
				return XwtObject.GetBackend ((XwtObject)obj);
			else if (obj == null)
				return null;
			else
				throw new InvalidOperationException ("Object doesn't have a backend");
		}
		
		public static T CreateFrontend<T> (object backend)
		{
			return (T) Activator.CreateInstance (typeof(T), backend);
		}
		
		public static object GetNativeWidget (Widget w)
		{
			return Application.EngineBackend.GetNativeWidget (w);
		}
		
		public static WindowFrame WrapWindow (object nativeWindow)
		{
			return new NativeWindowFrame (Application.EngineBackend.GetBackendForWindow (nativeWindow));
		}
	}
	
	class NativeWindowFrame: WindowFrame
	{
		IWindowFrameBackend backend;
		
		public NativeWindowFrame (IWindowFrameBackend backend)
		{
			this.backend = backend;
		}
		
		protected override IBackend OnCreateBackend ()
		{
			return backend;
		}
	}
}

