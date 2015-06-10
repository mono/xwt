// 
// Gradient.cs
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


namespace Xwt.Drawing
{
	public abstract class Gradient: Pattern
	{
		List<KeyValuePair<double, Color>> stops;

		public Gradient AddColorStop (double pos, Color color)
		{
			ToolkitEngine.GradientBackendHandler.AddColorStop (Backend, pos, color);
			(stops ?? (stops = new List<KeyValuePair<double, Color>> ())).Add (new KeyValuePair<double, Color> (pos, color));
			return this;
		}

		public void InitForToolkit (Toolkit t)
		{
			if (ToolkitEngine != t) {
				var handler = t.GradientBackendHandler;
				var backend = CreateGradientBackend (handler);
				SetBackend (handler, backend);
				if (stops != null)
					foreach (var stop in stops)
						handler.AddColorStop (backend, stop.Key, stop.Value);
			}
		}

		protected override object OnCreateBackend ()
		{
			var handler = ToolkitEngine.GradientBackendHandler;
			var backend = CreateGradientBackend (handler);
			SetBackend (handler, backend);
			return backend;
		}

		protected abstract object CreateGradientBackend (GradientBackendHandler handler);
	}
}

