// 
// Pattern.cs
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

namespace Xwt.Drawing
{
	public abstract class Pattern: XwtObject, IDisposable
	{
		DisposableResourceBackendHandler handler;

		internal Pattern ()
		{
		}

		internal void SetBackend (DisposableResourceBackendHandler handler, object backend)
		{
			if (this.handler != null) {
				Dispose ();
				GC.ReRegisterForFinalize (this);
			}
			Backend = backend;
			this.handler = handler;
			if (handler.DisposeHandleOnUiThread)
				ResourceManager.RegisterResource (backend, handler.Dispose);
			else
				GC.SuppressFinalize (this);
		}

		~Pattern ()
		{
			ResourceManager.FreeResource (Backend);
		}

		public void Dispose ()
		{
			if (handler.DisposeHandleOnUiThread) {
				GC.SuppressFinalize (this);
				ResourceManager.FreeResource (Backend);
			} else
				handler.Dispose (Backend);
		}
	}
}

