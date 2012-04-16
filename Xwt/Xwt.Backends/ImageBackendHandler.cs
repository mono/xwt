// 
// ImageBackendHandler.cs
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
using System.IO;
using System.Reflection;

namespace Xwt.Backends
{
	public abstract class ImageBackendHandler: IBackendHandler
	{
		public virtual object CreateBackend ()
		{
			throw new NotSupportedException ();
		}
		
		public virtual void Dispose (object backend)
		{
		}
		
		public virtual object LoadFromResource (Assembly asm, string name)
		{
			using (var s = asm.GetManifestResourceStream (name)) {
				if (s == null)
					throw new InvalidOperationException ("Resource not found: " + name);
				return LoadFromStream (s, name);
			}
		}
		
		public virtual object LoadFromFile (string file)
		{
			using (var s = File.OpenRead (file))
				return LoadFromStream (s, file);
		}
		
		public abstract object LoadFromStream (Stream stream, string name);
		
		public abstract object LoadFromIcon (string id, IconSize size);
		
		public abstract Size GetSize (object handle);
		
		public abstract object Resize (object handle, double width, double height);
		
		public abstract object Copy (object handle);

		public abstract void CopyArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY);

		public abstract object Crop (object handle, int srcX, int srcY, int width, int height);

		public abstract object ChangeOpacity (object backend, double opacity);
	}
}

