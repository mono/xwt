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

using System.Collections.Generic;
using System.Reflection;
using Xwt.Backends;

namespace Xwt
{
	[System.ComponentModel.DesignerCategory ("Code")]
	public abstract class XwtComponent : Component, IFrontend
	{
		BackendHost backendHost;
        object tag;
		
		public XwtComponent ()
		{
			backendHost = CreateBackendHost ();
			backendHost.Parent = this;
		}

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information about this component.
        /// </summary>
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
		
		protected virtual BackendHost CreateBackendHost ()
		{
			return new BackendHost ();
		}

		protected BackendHost BackendHost {
			get { return backendHost; }
		}
		
		Toolkit IFrontend.ToolkitEngine {
			get { return backendHost.ToolkitEngine; }
		}
		
		object IFrontend.Backend {
			get { return backendHost.Backend; }
		}

		protected static void MapEvent (object eventId, Type type, string methodName)
		{
			EventHost.MapEvent (eventId, type, methodName);
		}
		
		internal void VerifyConstructorCall<T> (T t)
		{
			if (GetType () != typeof(T))
				throw new InvalidConstructorInvocation (typeof(T));
		}
	}
}

