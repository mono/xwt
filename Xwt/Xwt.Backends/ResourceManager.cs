//
// ResourceManager.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
	static class ResourceManager
	{
		static bool finalized;
		static Dictionary<object,Action<object>> resources = new Dictionary<object,Action<object>> (ReferenceEqualsEqualityComparer<object>.Instance);
		static Dictionary<object,Action<object>> freedResources = new Dictionary<object,Action<object>> (ReferenceEqualsEqualityComparer<object>.Instance);

		public static void RegisterResource (object res, Action<object> disposeCallback = null)
		{
			if (finalized)
				return;
			lock (resources)
				resources [res] = disposeCallback;
		}

		public static bool FreeResource (object res)
		{
			if (finalized || res == null)
				return true;

			lock (resources) {
				Action<object> disposer;
				if (!resources.TryGetValue (res, out disposer))
					return false;

				resources.Remove (res);

				if (System.Threading.Thread.CurrentThread == Application.UIThread) {
					DisposeResource (res, disposer);
				} else {
					lock (freedResources) {
						if (freedResources.Count == 0)
							Application.Invoke (DisposeResources);
						freedResources [res] = disposer;
					}
				}
				return true;
			}
		}

		static void DisposeResource (object res, Action<object> disposer)
		{
			try {
				if (disposer != null)
					disposer (res);
				else if (res is IDisposable)
					((IDisposable)res).Dispose ();
			} catch (Exception ex) {
				Application.NotifyException (ex);
			}
		}

		internal static void DisposeResources ()
		{
			lock (freedResources) {
				if (finalized)
					return;
				foreach (var r in freedResources) {
					DisposeResource (r.Key, r.Value);
				}
				freedResources.Clear ();
			}
		}

		internal static void Dispose ()
		{
			lock (freedResources) {
				DisposeResources ();
				finalized = true;
			}
		}

		class ReferenceEqualsEqualityComparer<T> : IEqualityComparer<T> where T : class
		{
			public static ReferenceEqualsEqualityComparer<T> Instance = new ReferenceEqualsEqualityComparer<T>();

			public bool Equals(T x, T y) => ReferenceEquals(x, y);

			public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
		}
	}
}

