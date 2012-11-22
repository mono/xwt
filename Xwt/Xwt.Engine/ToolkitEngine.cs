//
// ToolkitEngine.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc
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
using Xwt.Drawing;

namespace Xwt.Engine
{
	public class ToolkitEngine
	{
		static ToolkitEngine currentEngine;

		ToolkitEngineBackend backend;

		public static ToolkitEngine CurrentEngine {
			get { return currentEngine; }
		}

		internal ToolkitEngineBackend Backend {
			get { return backend; }
		}

		public ToolkitEngine (string backendType)
		{
			ContextBackendHandler = Backend.CreateSharedBackend<IContextBackendHandler> (typeof(Context));
			GradientBackendHandler = Backend.CreateSharedBackend<IGradientBackendHandler> (typeof(Gradient));
			TextLayoutBackendHandler = Backend.CreateSharedBackend<ITextLayoutBackendHandler> (typeof(TextLayout));
			FontBackendHandler = Backend.CreateSharedBackend<IFontBackendHandler> (typeof(Font));
			ClipboardBackend = Backend.CreateSharedBackend<IClipboardBackend> (typeof(Clipboard));
			ImageBuilderBackendHandler = Backend.CreateSharedBackend<IImageBuilderBackendHandler> (typeof(ImageBuilder));
			ImagePatternBackendHandler = Backend.CreateSharedBackend<IImagePatternBackendHandler> (typeof(ImagePattern));
		}

		public object CreateObject<T> () where T:new()
		{
			var oldEngine = currentEngine;
			try {
				currentEngine = this;
				return new T ();
			} finally {
				currentEngine = oldEngine;
			}
		}

		public void Invoke (Action a)
		{
			var oldEngine = currentEngine;
			try {
				currentEngine = this;
				a ();
			} finally {
				currentEngine = oldEngine;
			}
		}

		public WindowFrame WrapWindow (object nativeWindow)
		{
			return new NativeWindowFrame (backend.GetBackendForWindow (nativeWindow));
		}

		public static object GetBackend (object obj)
		{
			if (obj is IFrontend)
				return ((IFrontend)obj).Backend;
			else if (obj == null)
				return null;
			else
				throw new InvalidOperationException ("Object doesn't have a backend");
		}

		internal IContextBackendHandler ContextBackendHandler;
		internal IGradientBackendHandler GradientBackendHandler;
		internal ITextLayoutBackendHandler TextLayoutBackendHandler;
		internal IFontBackendHandler FontBackendHandler;
		internal IClipboardBackend ClipboardBackend;
		internal IImageBuilderBackendHandler ImageBuilderBackendHandler;
		internal IImagePatternBackendHandler ImagePatternBackendHandler;
	}
}

