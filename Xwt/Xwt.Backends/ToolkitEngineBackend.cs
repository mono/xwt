// 
// EngineBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2011-2012 Xamarin Inc
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
using System.Reflection;
using System.IO;
using Xwt.Drawing;

using System.Collections.Generic;

namespace Xwt.Backends
{
	public abstract class ToolkitEngineBackend
	{
		Dictionary<Type,Type> backendTypes;
		Dictionary<Type,Type> backendTypesByFrontend;
		Toolkit toolkit;
		bool isGuest;

		/// <summary>
		/// Initialize the specified toolkit.
		/// </summary>
		/// <param name="toolkit">Toolkit to initialize.</param>
		/// <param name="isGuest">If set to <c>true</c> the toolkit will be initialized as guest of another toolkit.</param>
		internal void Initialize (Toolkit toolkit, bool isGuest)
		{
			this.toolkit = toolkit;
			this.isGuest = isGuest;
			if (backendTypes == null) {
				backendTypes = new Dictionary<Type, Type> ();
				backendTypesByFrontend = new Dictionary<Type, Type> ();
				InitializeBackends ();
			}
			InitializeApplication ();
		}

		/// <summary>
		/// Gets the toolkit engine backend.
		/// </summary>
		/// <returns>The toolkit backend.</returns>
		/// <typeparam name="T">The Type of the toolkit backend.</typeparam>
		public static T GetToolkitBackend<T> () where T : ToolkitEngineBackend
		{
			return (T)Toolkit.GetToolkitBackend (typeof (T));
		}

		/// <summary>
		/// Gets the application context.
		/// </summary>
		/// <value>The application context.</value>
		public ApplicationContext ApplicationContext {
			get { return toolkit.Context; }
		}

		/// <summary>
		/// Gets a value indicating whether this toolkit is running as a guest of another toolkit
		/// </summary>
		/// <remarks>
		/// A toolkit is a guest toolkit when it is loaded after the main toolkit of an application
		/// </remarks>
		public bool IsGuest {
			get { return isGuest; }
		}

		/// <summary>
		/// Initializes the application.
		/// </summary>
		public virtual void InitializeApplication ()
		{
		}

		/// <summary>
		/// Initializes the widget registry used by the application.
		/// </summary>
		/// <remarks>
		/// Don't do any toolkit initialization there, do them in InitializeApplication.
		/// Override to register the backend classes, by calling RegisterBackend() methods.
		/// </remarks>
		public virtual void InitializeBackends ()
		{
		}

		/// <summary>
		/// Runs the main GUI loop
		/// </summary>
		public abstract void RunApplication ();
		
		/// <summary>
		/// Exits the main GUI loop
		/// </summary>
		public abstract void ExitApplication ();

		/// <summary>
		/// Releases all resource used by the <see cref="Xwt.Backends.ToolkitEngineBackend"/> object.
		/// </summary>
		public virtual void Dispose ()
		{
		}

		/// <summary>
		/// Asynchronously invokes <paramref name="action"/> on the engine UI thread.
		/// </summary>
		/// <param name="action">The action to invoke.</param>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
		public abstract void InvokeAsync (Action action);

		/// <summary>
		/// Begins invoking <paramref name="action"/> on a timer period of <paramref name="timeSpan"/>.
		/// </summary>
		/// <param name="action">The function to invoke. Returning <c>false</c> stops the timer.</param>
		/// <param name="timeSpan">The period before the initial invoke and between subsequent invokes.</param>
		/// <returns>An identifying object that can be used to cancel the timer with <seealso cref="CancelTimerInvoke"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
		/// <seealso cref="CancelTimerInvoke"/>
		public abstract object TimerInvoke (Func<bool> action, TimeSpan timeSpan);

		/// <summary>
		/// Cancels an invoke timer started from <see cref="TimerInvoke"/>.
		/// </summary>
		/// <param name="id">The unique object returned from <see cref="TimerInvoke"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="id"/> is <c>null</c>.</exception>
		public abstract void CancelTimerInvoke (object id);
		
		/// <summary>
		/// Gets a reference to the native widget wrapped by an XWT widget
		/// </summary>
		/// <returns>
		/// The native widget.
		/// </returns>
		/// <param name='w'>
		/// A widget
		/// </param>
		public abstract object GetNativeWidget (Widget w);
		
		/// <summary>
		/// Gets a reference to the image object wrapped by an XWT Image
		/// </summary>
		/// <returns>
		/// The native image.
		/// </returns>
		/// <param name='image'>
		/// An image.
		/// </param>
		public virtual object GetNativeImage (Image image)
		{
			return Toolkit.GetBackend (image);
		}

		/// <summary>
		/// Dispatches pending events in the UI event queue
		/// </summary>
		public abstract void DispatchPendingEvents ();
		
		/// <summary>
		/// Gets the backend for a native window.
		/// </summary>
		/// <returns>
		/// The backend for the window.
		/// </returns>
		/// <param name='nativeWindow'>
		/// A native window reference.
		/// </param>
		public abstract IWindowFrameBackend GetBackendForWindow (object nativeWindow);
		
		/// <summary>
		/// Gets the native parent window of a widget
		/// </summary>
		/// <returns>
		/// The native parent window.
		/// </returns>
		/// <param name='w'>
		/// A widget
		/// </param>
		/// <remarks>
		/// This method is used by XWT to get the window of a widget, when the widget is
		/// embedded in a native application
		/// </remarks>
		public virtual object GetNativeParentWindow (Widget w)
		{
			return null;
		}

		/// <summary>
		/// Gets the backend for a native drawing context.
		/// </summary>
		/// <returns>The backend for context.</returns>
		/// <param name="nativeContext">The native context.</param>
		public virtual object GetBackendForContext (object nativeWidget, object nativeContext)
		{
			return nativeContext;
		}

		/// <summary>
		/// Gets the backend for a native image.
		/// </summary>
		/// <returns>The image backend .</returns>
		/// <param name="nativeImage">The native image.</param>
		public virtual object GetBackendForImage (object nativeImage)
		{
			return nativeImage;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="ToolkitEngineBackend" /> handles size negotiation on its own
		/// </summary>
		/// <value>
		/// <c>true</c> if the engine backend handles size negotiation; otherwise, <c>false</c>.
		/// </value>
		public virtual bool HandlesSizeNegotiation {
			get { return false; }
		}

		void CheckInitialized ()
		{
			if (backendTypes == null)
				throw new InvalidOperationException ("XWT toolkit not initialized");
		}

		/// <summary>
		/// Creates a backend for a frontend.
		/// </summary>
		/// <returns>The backend for the specified frontend.</returns>
		/// <param name="frontendType">The Frontend type.</param>
		internal IBackend CreateBackendForFrontend (Type frontendType)
		{
			CheckInitialized ();

			Type bt = null;
			if (!backendTypesByFrontend.TryGetValue (frontendType, out bt)) {
				var attr = (BackendTypeAttribute) Attribute.GetCustomAttribute (frontendType, typeof(BackendTypeAttribute), true);
				if (attr == null || attr.Type == null)
					throw new InvalidOperationException ("Backend type not specified for type: " + frontendType);
				if (!typeof(IBackend).IsAssignableFrom (attr.Type))
					throw new InvalidOperationException ("Backend type for frontend '" + frontendType + "' is not a IBackend implementation");
				backendTypes.TryGetValue (attr.Type, out bt);
				backendTypesByFrontend [frontendType] = bt;
			}
			if (bt == null)
				return null;
			return (IBackend) Activator.CreateInstance (bt);
		}

		/// <summary>
		/// Creates the backend.
		/// </summary>
		/// <returns>The backend.</returns>
		/// <param name="backendType">The Backend type.</param>
		internal object CreateBackend (Type backendType)
		{
			CheckInitialized ();
			Type bt = null;
			
			if (!backendTypes.TryGetValue (backendType, out bt))
				return null;
			var res = Activator.CreateInstance (bt);
			if (!backendType.IsInstanceOfType (res))
				throw new InvalidOperationException ("Invalid backend type. Expected '" + backendType + "' found '" + res.GetType () + "'");
			if (res is BackendHandler)
				((BackendHandler)res).Initialize (toolkit);
			return res;
		}

		/// <summary>
		/// Creates the backend.
		/// </summary>
		/// <returns>The backend.</returns>
		/// <typeparam name="T">The Backend type.</typeparam>
		internal T CreateBackend<T> ()
		{
			return (T) CreateBackend (typeof(T));
		}

		/// <summary>
		/// Registers a backend for an Xwt backend interface.
		/// </summary>
		/// <typeparam name="Backend">The backend Type</typeparam>
		/// <typeparam name="Implementation">The Xwt interface implemented by the backend</typeparam>
		public void RegisterBackend<Backend, Implementation> () where Implementation: Backend
		{
			CheckInitialized ();
			backendTypes [typeof(Backend)] = typeof(Implementation);
		}

		/// <summary>
		/// Creates the Xwt frontend for a backend.
		/// </summary>
		/// <returns>The Xwt frontend.</returns>
		/// <param name="backend">The backend.</param>
		/// <typeparam name="T">The frontend type.</typeparam>
		public T CreateFrontend<T> (object backend)
		{
			return (T) Activator.CreateInstance (typeof(T), backend);
		}

		/// <summary>
		/// Registers a callback to be invoked just before the execution returns to the main loop
		/// </summary>
		/// <param name='action'>
		/// Callback to execute
		/// </param>
		/// <remarks>
		/// The default implementation does the invocation using InvokeAsync.
		/// </remarks>			
		public virtual void InvokeBeforeMainLoop (Action action)
		{
			InvokeAsync (action);
		}

		/// <summary>
		/// Determines whether a widget has a native parent widget
		/// </summary>
		/// <returns><c>true</c> if the widget has native parent; otherwise, <c>false</c>.</returns>
		/// <param name="w">The widget.</param>
		/// <remarks>This funciton is used to determine if a widget is a child of another non-XWT widget
		/// </remarks>
		public abstract bool HasNativeParent (Widget w);

		/// <summary>
		/// Renders a widget into a bitmap
		/// </summary>
		/// <param name="w">A widget</param>
		/// <returns>An image backend</returns>
		public virtual object RenderWidget (Widget w)
		{
			throw new NotSupportedException ();
		}

		/// <summary>
		/// Renders an image at the provided native context
		/// </summary>
		/// <param name="nativeContext">Native context.</param>
		/// <param name="img">Image.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public virtual void RenderImage (object nativeWidget, object nativeContext, ImageDescription img, double x, double y)
		{
		}

		/// <summary>
		/// Gets the information about Xwt features supported by the toolkit.
		/// </summary>
		/// <value>The supported features.</value>
		public virtual ToolkitFeatures SupportedFeatures {
			get { return ToolkitFeatures.All; }
		}
	}
}

