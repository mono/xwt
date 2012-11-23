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
using System.Reflection;
using System.Collections.Generic;

namespace Xwt.Engine
{
	public class ToolkitEngine
	{
		static ToolkitEngine currentEngine;

		ToolkitEngineBackend backend;
		ApplicationContext context;

		int inUserCode;
		Queue<Action> exitActions = new Queue<Action> ();
		bool exitCallbackRegistered;

		public static ToolkitEngine CurrentEngine {
			get { return currentEngine; }
		}

		internal ApplicationContext Context {
			get { return context; }
		}

		internal ToolkitEngineBackend Backend {
			get { return backend; }
		}

		private ToolkitEngine ()
		{
			context = new ApplicationContext (this);
		}

		public static ToolkitEngine Load (string fullTypeName)
		{
			ToolkitEngine t = new ToolkitEngine ();
			if (fullTypeName != null && t.LoadBackend (fullTypeName))
				return t;
			
			if (t.LoadBackend (ToolkitType.Gtk))
				return t;
			
			if (t.LoadBackend (ToolkitType.Cocoa))
				return t;
			
			if (t.LoadBackend (ToolkitType.Wpf))
				return t;
			
			throw new InvalidOperationException ("Xwt engine not found");
		}

		public static ToolkitEngine Load (ToolkitType type)
		{
			ToolkitEngine t = new ToolkitEngine ();
			if (t.LoadBackend (type))
				return t;
			else
				throw new InvalidOperationException ("Xwt engine not found");
		}

		bool LoadBackend (ToolkitType type)
		{
			switch (type) {
			case ToolkitType.Gtk:
				return LoadBackend ("Xwt.GtkBackend.GtkEngine, Xwt.Gtk, Version=1.0.0.0");
			case ToolkitType.Cocoa:
				return LoadBackend ("Xwt.Mac.MacEngine, Xwt.Mac, Version=1.0.0.0");
			case ToolkitType.Wpf:
				return LoadBackend ("Xwt.WPFBackend.WPFEngine, Xwt.WPF, Version=1.0.0.0");
			default:
				throw new ArgumentException ("Invalid toolkit type");
			}
		}

		bool LoadBackend (string type)
		{
			int i = type.IndexOf (',');
			string assembly = type.Substring (i+1).Trim ();
			type = type.Substring (0, i).Trim ();
			try {
				Assembly asm = Assembly.Load (assembly);
				if (asm != null) {
					Type t = asm.GetType (type);
					if (t != null) {
						backend = (ToolkitEngineBackend) Activator.CreateInstance (t);
						Initialize ();
						return true;
					}
				}
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}

		void Initialize ()
		{
			backend.Initialize (this);
			ContextBackendHandler = Backend.CreateSharedBackend<ContextBackendHandler> (typeof(Context));
			GradientBackendHandler = Backend.CreateSharedBackend<GradientBackendHandler> (typeof(Gradient));
			TextLayoutBackendHandler = Backend.CreateSharedBackend<TextLayoutBackendHandler> (typeof(TextLayout));
			FontBackendHandler = Backend.CreateSharedBackend<FontBackendHandler> (typeof(Font));
			ClipboardBackend = Backend.CreateSharedBackend<ClipboardBackend> (typeof(Clipboard));
			ImageBuilderBackendHandler = Backend.CreateSharedBackend<ImageBuilderBackendHandler> (typeof(ImageBuilder));
			ImagePatternBackendHandler = Backend.CreateSharedBackend<ImagePatternBackendHandler> (typeof(ImagePattern));
			ImageBackendHandler = Backend.CreateSharedBackend<ImageBackendHandler> (typeof(Image));
		}

		public object GetNativeWidget (Widget w)
		{
			ValidateObject (w);
			return backend.GetNativeWidget (w);
		}

		public object GetNativeImage (Image image)
		{
			ValidateObject (image);
			return backend.GetNativeImage (image);
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

		public bool Invoke (Action a)
		{
			var oldEngine = currentEngine;
			try {
				currentEngine = this;
				EnterUserCode ();
				a ();
				ExitUserCode (null);
				return true;
			} catch (Exception ex) {
				ExitUserCode (ex);
				return false;
			} finally {
				currentEngine = oldEngine;
			}
		}
		
		internal void InvokePlatformCode (Action a)
		{
			try {
				ExitUserCode (null);
				a ();
			} finally {
				EnterUserCode ();
			}
		}
		
		internal void EnterUserCode ()
		{
			inUserCode++;
		}
		
		internal void ExitUserCode (Exception error)
		{
			if (error != null) {
				Invoke (delegate {
					Application.NotifyException (error);
				});
			}
			if (inUserCode == 1) {
				while (exitActions.Count > 0) {
					try {
						exitActions.Dequeue ()();
					} catch (Exception ex) {
						Invoke (delegate {
							Application.NotifyException (ex);
						});
					}
				}
			}
			inUserCode--;
		}

		void DispatchExitActions ()
		{
			// This pair of calls will flush the exit action queue
			exitCallbackRegistered = false;
			EnterUserCode ();
			ExitUserCode (null);
		}
		
		internal void QueueExitAction (Action a)
		{
			exitActions.Enqueue (a);

			if (inUserCode == 0) {
				// Not in an XWT handler. This may happen when embedding XWT in another toolkit and
				// XWT widgets are manipulated from event handlers of the native toolkit which
				// are not invoked using ApplicationContext.InvokeUserCode.
				if (!exitCallbackRegistered) {
					exitCallbackRegistered = true;
					// Try to use a native method of queuing exit actions
					ToolkitEngine.CurrentEngine.Backend.InvokeBeforeMainLoop (DispatchExitActions);
				}
			}
		}
		
		public bool InUserCode {
			get { return inUserCode > 0; }
		}
		public WindowFrame WrapWindow (object nativeWindow)
		{
			return new NativeWindowFrame (backend.GetBackendForWindow (nativeWindow));
		}

		public object ValidateObject (object obj)
		{
			if (obj is IFrontend) {
				if (((IFrontend)obj).ToolkitEngine != this)
					throw new InvalidOperationException ("Object belongs to a different toolkit");
			}
			return obj;
		}

		public object GetSafeBackend (object obj)
		{
			ValidateObject (obj);
			return GetBackend (obj);
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

		public T CreateFrontend<T> (object ob)
		{
			throw new NotImplementedException ();
		}

		internal ContextBackendHandler ContextBackendHandler;
		internal GradientBackendHandler GradientBackendHandler;
		internal TextLayoutBackendHandler TextLayoutBackendHandler;
		internal FontBackendHandler FontBackendHandler;
		internal ClipboardBackend ClipboardBackend;
		internal ImageBuilderBackendHandler ImageBuilderBackendHandler;
		internal ImagePatternBackendHandler ImagePatternBackendHandler;
		internal ImageBackendHandler ImageBackendHandler;
	}
}

