// 
// XwtObject.cs
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

using Xwt.Backends;

namespace Xwt
{
	/// <summary>
	/// The base class for all Xwt objects (not widgets or components).
	/// </summary>
	public abstract class XwtObject: IFrontend
	{
		object backend;

		/// <summary>
		/// Gets the current toolkit engine.
		/// </summary>
		/// <value>The toolkit engine.</value>
		internal Toolkit ToolkitEngine { get; set; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.XwtObject"/> class.
		/// </summary>
		/// <param name="backend">The object backend.</param>
		protected XwtObject (object backend): this (backend, Toolkit.CurrentEngine)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.XwtObject"/> class.
		/// </summary>
		/// <param name="backend">The object backend.</param>
		/// <param name="toolkit">The toolkit, to which the object backend belongs to.</param>
		protected XwtObject (object backend, Toolkit toolkit)
		{
			this.backend = backend;
			ToolkitEngine = toolkit ?? Toolkit.CurrentEngine;
		}

		protected XwtObject ()
		{
			ToolkitEngine = Toolkit.CurrentEngine;
		}

		Toolkit IFrontend.ToolkitEngine {
			get { return ToolkitEngine; }
		}

		/// <summary>
		/// Gets or sets the backend used for this <see cref="Xwt.XwtObject"/>.
		/// </summary>
		/// <value>The backend.</value>
		protected object Backend {
			get {
				LoadBackend ();
				return backend;
			}
			set {
				backend = value;
			}
		}
		
		object IFrontend.Backend {
			get { return Backend; }
		}

		/// <summary>
		/// Loads the backend for this <see cref="Xwt.XwtObject"/>.
		/// </summary>
		protected void LoadBackend ()
		{
			if (backend == null) {
				backend = OnCreateBackend ();
				if (backend == null)
					throw new InvalidOperationException ("No backend found for widget: " + GetType ());
				OnBackendCreated ();
			}
		}
		
		/// <summary>
		/// Called when the backend for this <see cref="Xwt.XwtObject"/> has been created.
		/// </summary>
		protected virtual void OnBackendCreated ()
		{
		}
		
		/// <summary>
		/// Creates the backend for this <see cref="Xwt.XwtObject"/>.
		/// </summary>
		protected virtual object OnCreateBackend ()
		{
			throw new NotImplementedException ();
		}
		
		/// <summary>
		/// Gets the backend used for a specific <see cref="Xwt.XwtObject"/>.
		/// </summary>
		/// <returns>The toolkit backend from the specified XwtObject.</returns>
		/// <param name="w">The XwtObject.</param>
		internal static object GetBackend (XwtObject w)
		{
			return w != null ? w.Backend : null;
		}
	}
}

