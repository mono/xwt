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
using System.Reflection;
using Xwt.Backends;

using System.Threading.Tasks;

namespace Xwt
{
	public static class Application
	{
		static Toolkit toolkit;
		static ToolkitEngineBackend engine;
		static UILoop mainLoop;

		public static TaskScheduler UITaskScheduler {
			get { return Toolkit.CurrentEngine.Scheduler; }
		}

		public static UILoop MainLoop {
			get { return mainLoop; }
		}

		internal static System.Threading.Thread UIThread {
			get;
			private set;
		}

		public static void Initialize ()
		{
			if (engine != null)
				return;
			Initialize (null);
		}
		
		public static void Initialize (ToolkitType type)
		{
			Initialize (Toolkit.GetBackendType (type));
		}

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
		
		public static void InitializeAsGuest (ToolkitType type)
		{
			Initialize (type);
			toolkit.ExitUserCode (null);
		}
		
		public static void InitializeAsGuest (string backendType)
		{
			Initialize (backendType);
			toolkit.ExitUserCode (null);
		}

		public static void Run ()
		{
			toolkit.InvokePlatformCode (delegate {
				engine.RunApplication ();
			});
		}
		
		public static void Exit ()
		{
			toolkit.InvokePlatformCode (delegate {
				engine.ExitApplication ();
			});
		}
			/// <summary>
		/// Invokes an action in the GUI thread
		/// </summary>
		/// <param name='action'>
		/// The action to execute.
		/// </param>
		public static void Invoke (Action action)
		{
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

		public static StatusIcon CreateStatusIcon ()
		{
			return new StatusIcon ();
		}

		public static event EventHandler<ExceptionEventArgs> UnhandledException;
		
		class Timer: IDisposable
		{
			public object Id;
			public void Dispose ()
			{
				Application.engine.CancelTimerInvoke (Id);
			}
		}

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

	public class UILoop
	{
		Toolkit toolkit;

		internal UILoop (Toolkit toolkit)
		{
			this.toolkit = toolkit;
		}

		public void DispatchPendingEvents ()
		{
			try {
				toolkit.ExitUserCode (null);
				toolkit.Backend.DispatchPendingEvents ();
			} finally {
				toolkit.EnterUserCode ();
			}
		}

		public void QueueExitAction (Action a)
		{
			toolkit.QueueExitAction (a);
		}
	}


	public enum ToolkitType
	{
		Gtk,
		Cocoa,
		Wpf,
		XamMac
	}
}

