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
using Xwt.Engine;

namespace Xwt
{
	public static class Application
	{
		static EngineBackend engine;
		
		internal static EngineBackend EngineBackend {
			get { return engine; }
		}
		
		public static void Initialize ()
		{
			if (engine != null)
				return;
			InitBackend (null);
			engine.InitializeApplication ();
		}
		
		public static void Initialize (string backendType)
		{
			InitBackend (backendType);
			engine.InitializeApplication ();
		}
		
		public static void Run ()
		{
			Toolkit.InvokePlatformCode (delegate {
				engine.RunApplication ();
			});
		}
		
		public static void Exit ()
		{
			Toolkit.InvokePlatformCode (delegate {
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
					Toolkit.EnterUserCode ();
					action ();
					Toolkit.ExitUserCode (null);
				} catch (Exception ex) {
					Toolkit.ExitUserCode (ex);
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
					Toolkit.EnterUserCode ();
					res = action ();
					Toolkit.ExitUserCode (null);
				} catch (Exception ex) {
					Toolkit.ExitUserCode (ex);
				}
				return res;
			}, timeSpan);
			return t;
		}
		
		public static StatusIcon CreateStatusIcon ()
		{
			return new StatusIcon ();
		}
		
		class Timer: IDisposable
		{
			public object Id;
			public void Dispose ()
			{
				Application.engine.CancelTimerInvoke (Id);
			}
		}
		
		static bool LoadBackend (string type)
		{
			int i = type.IndexOf (',');
			string assembly = type.Substring (i+1).Trim ();
			type = type.Substring (0, i).Trim ();
			try {
				Assembly asm = Assembly.Load (assembly);
				if (asm != null) {
					Type t = asm.GetType (type);
					if (t != null) {
						engine = (EngineBackend) Activator.CreateInstance (t);
						return true;
					}
				}
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}
		
		static void InitBackend (string type)
		{
			Toolkit.EnterUserCode ();
			if (type != null && LoadBackend (type))
				return;
			
			if (LoadBackend ("Xwt.GtkBackend.GtkEngine, Xwt.Gtk, Version=1.0.0.0"))
				return;
			
			if (LoadBackend ("Xwt.Mac.MacEngine, Xwt.Mac, Version=1.0.0.0"))
				return;
			
			if (LoadBackend ("Xwt.WPFBackend.WPFEngine, Xwt.WPF, Version=1.0.0.0"))
				return;
			
			throw new InvalidOperationException ("Xwt engine not found");
		}
		
		internal static void NotifyException (Exception ex)
		{
			Console.WriteLine (ex);
		}
	}
}

