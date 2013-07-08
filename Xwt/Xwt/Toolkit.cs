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
using System.Linq;

namespace Xwt
{
	public class Toolkit: IFrontend
	{
		static Toolkit currentEngine;
		static Dictionary<Type, Toolkit> toolkits = new Dictionary<Type, Toolkit> ();

		ToolkitEngineBackend backend;
		ApplicationContext context;
		XwtTaskScheduler scheduler;
		ToolkitType toolkitType;

		int inUserCode;
		Queue<Action> exitActions = new Queue<Action> ();
		bool exitCallbackRegistered;

		Dictionary<string,Image> stockIcons = new Dictionary<string, Image> ();

		public static Toolkit CurrentEngine {
			get { return currentEngine; }
		}

		public static IEnumerable<Toolkit> LoadedToolkits {
			get { return toolkits.Values; }
		}

		internal ApplicationContext Context {
			get { return context; }
		}

		internal ToolkitEngineBackend Backend {
			get { return backend; }
		}

		internal XwtTaskScheduler Scheduler {
			get { return scheduler; }
		}

		object IFrontend.Backend {
			get { return backend; }
		}
		Toolkit IFrontend.ToolkitEngine {
			get { return this; }
		}

		private Toolkit ()
		{
			context = new ApplicationContext (this);
			scheduler = new XwtTaskScheduler (this);
		}

		public ToolkitType Type {
			get { return toolkitType; }
		}

		internal static void DisposeAll ()
		{
			foreach (var t in toolkits.Values)
				t.Backend.Dispose ();
		}

		public static Toolkit Load (string fullTypeName)
		{
			return Load (fullTypeName, true);
		}

		internal static Toolkit Load (string fullTypeName, bool isGuest)
		{
			Toolkit t = new Toolkit ();
			if (!string.IsNullOrEmpty (fullTypeName)) {
				t.LoadBackend (fullTypeName, isGuest, true);
				return t;
			}
			
			if (t.LoadBackend (GetBackendType (ToolkitType.Gtk), isGuest, false))
				return t;
			
			if (t.LoadBackend (GetBackendType (ToolkitType.Cocoa), isGuest, false))
				return t;
			
			if (t.LoadBackend (GetBackendType (ToolkitType.Wpf), isGuest, false))
				return t;
			
			throw new InvalidOperationException ("Xwt engine not found");
		}

		public static Toolkit Load (ToolkitType type)
		{
			var et = toolkits.Values.FirstOrDefault (tk => tk.toolkitType == type);
			if (et != null)
				return et;

			Toolkit t = new Toolkit ();
			t.toolkitType = type;
			t.LoadBackend (GetBackendType (type), true, true);
			return t;
		}

		/// <summary>
		/// Tries to load a toolkit
		/// </summary>
		/// <returns><c>true</c>, the toolkit could be loaded, <c>false</c> otherwise.</returns>
		/// <param name="type">Toolkit type</param>
		/// <param name="toolkit">The loaded toolkit</param>
		public static bool TryLoad (ToolkitType type, out Toolkit toolkit)
		{
			var et = toolkits.Values.FirstOrDefault (tk => tk.toolkitType == type);
			if (et != null) {
				toolkit = et;
				return true;
			}

			Toolkit t = new Toolkit ();
			t.toolkitType = type;
			if (t.LoadBackend (GetBackendType (type), true, false)) {
				toolkit = t;
				return true;
			}
			toolkit = null;
			return false;
		}

		internal static string GetBackendType (ToolkitType type)
		{
			string version = typeof(Application).Assembly.GetName ().Version.ToString ();

			switch (type) {
			case ToolkitType.Gtk:
				return "Xwt.GtkBackend.GtkEngine, Xwt.Gtk, Version=" + version;
			case ToolkitType.Cocoa:
				return "Xwt.Mac.MacEngine, Xwt.Mac, Version=" + version;
			case ToolkitType.Wpf:
				return "Xwt.WPFBackend.WPFEngine, Xwt.WPF, Version=" + version;
			case ToolkitType.XamMac:
				return "Xwt.Mac.MacEngine, Xwt.XamMac, Version=" + version;
			default:
				throw new ArgumentException ("Invalid toolkit type");
			}
		}

		bool LoadBackend (string type, bool isGuest, bool throwIfFails)
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
						Initialize (isGuest);
						return true;
					}
				}
			}
			catch (Exception ex) {
				if (throwIfFails)
					throw new Exception ("Toolkit could not be loaded", ex);
				Application.NotifyException (ex);
			}
			if (throwIfFails)
				throw new Exception ("Toolkit could not be loaded");
			return false;
		}

		void Initialize (bool isGuest)
		{
			toolkits[Backend.GetType ()] = this;
			backend.Initialize (this, isGuest);
			ContextBackendHandler = Backend.CreateBackend<ContextBackendHandler> ();
			GradientBackendHandler = Backend.CreateBackend<GradientBackendHandler> ();
			TextLayoutBackendHandler = Backend.CreateBackend<TextLayoutBackendHandler> ();
			FontBackendHandler = Backend.CreateBackend<FontBackendHandler> ();
			ClipboardBackend = Backend.CreateBackend<ClipboardBackend> ();
			ImageBuilderBackendHandler = Backend.CreateBackend<ImageBuilderBackendHandler> ();
			ImagePatternBackendHandler = Backend.CreateBackend<ImagePatternBackendHandler> ();
			ImageBackendHandler = Backend.CreateBackend<ImageBackendHandler> ();
			DrawingPathBackendHandler = Backend.CreateBackend<DrawingPathBackendHandler> ();
			DesktopBackend = Backend.CreateBackend<DesktopBackend> ();
			VectorImageRecorderContextHandler = new VectorImageRecorderContextHandler (this);
		}

		internal static ToolkitEngineBackend GetToolkitBackend (Type type)
		{
			Toolkit t;
			if (toolkits.TryGetValue (type, out t))
				return t.backend;
			else
				return null;
		}

		internal void SetActive ()
		{
			currentEngine = this;
		}

		public object GetNativeWidget (Widget w)
		{
			ValidateObject (w);
			w.SetExtractedAsNative ();
			return backend.GetNativeWidget (w);
		}

		public object GetNativeImage (Image image)
		{
			ValidateObject (image);
			return backend.GetNativeImage (image);
		}

		public T CreateObject<T> () where T:new()
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
			if (inUserCode == 1 && !exitCallbackRegistered) {
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
					Toolkit.CurrentEngine.Backend.InvokeBeforeMainLoop (DispatchExitActions);
				}
			}
		}
		
		public bool InUserCode {
			get { return inUserCode > 0; }
		}

		public WindowFrame WrapWindow (object nativeWindow)
		{
			if (nativeWindow == null)
				return null;
			return new NativeWindowFrame (backend.GetBackendForWindow (nativeWindow));
		}

		public Widget WrapWidget (object nativeWidget)
		{
			var externalWidget = nativeWidget as Widget;
			if (externalWidget != null) {
				if (externalWidget.Surface.ToolkitEngine == this)
					return externalWidget;
				nativeWidget = externalWidget.Surface.ToolkitEngine.GetNativeWidget (externalWidget);
			}
			return new EmbeddedNativeWidget (nativeWidget, externalWidget);
		}

		public Image WrapImage (object nativeImage)
		{
			return new Image (backend.GetBackendForImage (nativeImage));
		}

		public Context WrapContext (object nativeContext)
		{
			return new Context (backend.GetBackendForContext (nativeContext), this);
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

		public Image RenderWidget (Widget widget)
		{
			return new Image (backend.RenderWidget (widget));
		}

		public void RenderImage (object nativeWidget, object nativeContext, Image img, double x, double y)
		{
			img.GetFixedSize (); // Ensure that it has a size
			backend.RenderImage (nativeWidget, nativeContext, img.ImageDescription, x, y);
		}

		internal Image GetStockIcon (string id)
		{
			Image img;
			if (!stockIcons.TryGetValue (id, out img))
				stockIcons [id] = img = ImageBackendHandler.GetStockIcon (id);
			return img;
		}

		internal ContextBackendHandler ContextBackendHandler;
		internal GradientBackendHandler GradientBackendHandler;
		internal TextLayoutBackendHandler TextLayoutBackendHandler;
		internal FontBackendHandler FontBackendHandler;
		internal ClipboardBackend ClipboardBackend;
		internal ImageBuilderBackendHandler ImageBuilderBackendHandler;
		internal ImagePatternBackendHandler ImagePatternBackendHandler;
		internal ImageBackendHandler ImageBackendHandler;
		internal DrawingPathBackendHandler DrawingPathBackendHandler;
		internal DesktopBackend DesktopBackend;
		internal VectorImageRecorderContextHandler VectorImageRecorderContextHandler;
	}

	class NativeWindowFrame: WindowFrame
	{
		public NativeWindowFrame (IWindowFrameBackend backend)
		{
			BackendHost.SetCustomBackend (backend);
		}
	}
}

