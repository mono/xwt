// 
// Application.cs
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

using System.Threading.Tasks;
using System.Threading;

namespace Xwt
{
	public static class Application
	{
		static Toolkit toolkit;
		static ToolkitEngineBackend engine;
		static UILoop mainLoop;

		/// <summary>
		/// Gets the task scheduler of the current engine.
		/// </summary>
		/// <value>The toolkit specific task scheduler.</value>
		/// <remarks>
		/// The Xwt task scheduler marshals every Task to the Xwt GUI thread without concurrency.
		/// </remarks>
		public static TaskScheduler UITaskScheduler {
			get { return Toolkit.CurrentEngine.Scheduler; }
		}

		/// <summary>
		/// The main GUI loop.
		/// </summary>
		/// <value>The main loop.</value>
		public static UILoop MainLoop {
			get { return mainLoop; }
		}

		internal static System.Threading.Thread UIThread {
			get;
			private set;
		}

		/// <summary>
		/// Initialize Xwt with the best matching toolkit for the current platform.
		/// </summary>
		public static void Initialize ()
		{
			if (engine != null)
				return;
			Initialize (null);
		}
		
		/// <summary>
		/// Initialize Xwt with the specified type.
		/// </summary>
		/// <param name="type">The toolkit type.</param>
		public static void Initialize (ToolkitType type)
		{
			Initialize (Toolkit.GetBackendType (type));
			toolkit.Type = type;
		}

		/// <summary>
		/// Initialize Xwt with the specified backend type.
		/// </summary>
		/// <param name="backendType">The <see cref="Type.FullName"/> of the backend type.</param>
		public static void Initialize (string backendType)
		{			
			if (engine != null)
				return;

			toolkit = Toolkit.Load (backendType, false);
			toolkit.SetActive ();
			engine = toolkit.Backend;
			mainLoop = new UILoop (toolkit);

			UIThread = System.Threading.Thread.CurrentThread;

			toolkit.EnterUserCode ();
		}
		
		/// <summary>
		/// Initializes Xwt as guest, embedded into an other existing toolkit.
		/// </summary>
		/// <param name="type">The toolkit type.</param>
		public static void InitializeAsGuest (ToolkitType type)
		{
			Initialize (type);
			toolkit.ExitUserCode (null);
		}
		
		/// <summary>
		/// Initializes Xwt as guest, embedded into an other existing toolkit.
		/// </summary>
		/// <param name="backendType">The <see cref="Type.FullName"/> of the backend type.</param>
		public static void InitializeAsGuest (string backendType)
		{
			if (backendType == null)
				throw new ArgumentNullException ("backendType");
			Initialize (backendType);
			toolkit.ExitUserCode (null);
		}

		/// <summary>
		/// Runs the main Xwt GUI thread.
		/// </summary>
		/// <remarks>
		/// Blocks until the main GUI loop exits. Use <see cref="Application.Exit"/>
		/// to stop the Xwt application.
		/// </remarks>
		public static void Run ()
		{
			if (XwtSynchronizationContext.AutoInstall)
			if (SynchronizationContext.Current == null || 
			    (!((engine.IsGuest) || (SynchronizationContext.Current is XwtSynchronizationContext))))
				SynchronizationContext.SetSynchronizationContext (new XwtSynchronizationContext ());

			toolkit.InvokePlatformCode (delegate {
				engine.RunApplication ();
			});
		}
		
		/// <summary>
		/// Exits the Xwt application.
		/// </summary>
		public static void Exit ()
		{
			toolkit.InvokePlatformCode (delegate {
				engine.ExitApplication ();
			});

			if (SynchronizationContext.Current is XwtSynchronizationContext)
				XwtSynchronizationContext.Uninstall ();
		}

		/// <summary>
		/// Releases all resources used by the application
		/// </summary>
		/// <remarks>This method must be called before the application process ends</remarks>
		public static void Dispose ()
		{
			ResourceManager.Dispose ();
			Toolkit.DisposeAll ();
		}


		/// <summary>
		/// Invokes an action in the GUI thread
		/// </summary>
		/// <param name='action'>
		/// The action to execute.
		/// </param>
		public static void Invoke (Action action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");

			engine.InvokeAsync (delegate {
				try {
					toolkit.EnterUserCode ();
					action ();
					toolkit.ExitUserCode (null);
				} catch (Exception ex) {
					toolkit.ExitUserCode (ex);
				}
			});
		}
		
		/// <summary>
		/// Invokes an action in the GUI thread after the provided time span
		/// </summary>
		/// <returns>
		/// A timer object
		/// </returns>
		/// <param name='action'>
		/// The action to execute.
		/// </param>
		/// <remarks>
		/// This method schedules the execution of the provided function. The function
		/// must return 'true' if it has to be executed again after the time span, or 'false'
		/// if the timer can be discarded.
		/// The execution of the funciton can be canceled by disposing the returned object.
		/// </remarks>
		public static IDisposable TimeoutInvoke (int ms, Func<bool> action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			if (ms < 0)
				throw new ArgumentException ("ms can't be negative");

			return TimeoutInvoke (TimeSpan.FromMilliseconds (ms), action);
		}
		
		/// <summary>
		/// Invokes an action in the GUI thread after the provided time span
		/// </summary>
		/// <returns>
		/// A timer object
		/// </returns>
		/// <param name='action'>
		/// The action to execute.
		/// </param>
		/// <remarks>
		/// This method schedules the execution of the provided function. The function
		/// must return 'true' if it has to be executed again after the time span, or 'false'
		/// if the timer can be discarded.
		/// The execution of the funciton can be canceled by disposing the returned object.
		/// </remarks>
		public static IDisposable TimeoutInvoke (TimeSpan timeSpan, Func<bool> action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			if (timeSpan.Ticks < 0)
				throw new ArgumentException ("timeSpan can't be negative");

			Timer t = new Timer ();
			t.Id = engine.TimerInvoke (delegate {
				bool res = false;
				try {
					toolkit.EnterUserCode ();
					res = action ();
					toolkit.ExitUserCode (null);
				} catch (Exception ex) {
					toolkit.ExitUserCode (ex);
				}
				return res;
			}, timeSpan);
			return t;
		}

		/// <summary>
		/// Create a toolkit specific status icon.
		/// </summary>
		/// <returns>The status icon.</returns>
		public static StatusIcon CreateStatusIcon ()
		{
			return new StatusIcon ();
		}

		/// <summary>
		/// Occurs when an exception is not caught.
		/// </summary>
		/// <remarks>Subscribe to handle uncaught exceptions, which could
		/// otherwise block or stop the application.</remarks>
		public static event EventHandler<ExceptionEventArgs> UnhandledException;
		
		class Timer: IDisposable
		{
			public object Id;
			public void Dispose ()
			{
				Application.engine.CancelTimerInvoke (Id);
			}
		}

		/// <summary>
		/// Notifies about unhandled exceptions using the UnhandledException event.
		/// </summary>
		/// <param name="ex">The unhandled Exception.</param>
		internal static void NotifyException (Exception ex)
		{
			var unhandledException = UnhandledException;
			if (unhandledException != null)
			{
				unhandledException (null, new ExceptionEventArgs (ex));
			}
			else
			{
				Console.WriteLine (ex);
			}
		}
	}

	/// <summary>
	/// The UILoop class provides access to the main GUI loop.
	/// </summary>
	public class UILoop
	{
		Toolkit toolkit;

		internal UILoop (Toolkit toolkit)
		{
			this.toolkit = toolkit;
		}

		/// <summary>
		/// Dispatches pending events in the GUI event queue
		/// </summary>
		public void DispatchPendingEvents ()
		{
			try {
				toolkit.ExitUserCode (null);
				toolkit.Backend.DispatchPendingEvents ();
			} finally {
				toolkit.EnterUserCode ();
			}
		}

		/// <summary>
		/// Runs an Action after all user handlers have been processed and
		/// the main GUI loop is about to proceed with its next iteration.
		/// </summary>
		/// <param name="action">Action to execute.</param>
		public void QueueExitAction (Action action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			toolkit.QueueExitAction (action);
		}
	}


	public enum ToolkitType
	{
		Gtk = 1,
		Cocoa = 2,
		Wpf = 3,
		XamMac = 4,
		Gtk3 = 5,
	}
}

