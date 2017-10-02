//
// FakeToolkit.cs
//
// Author:
//       Lluis Sanchez <llsan@microsoft.com>
//
// Copyright (c) 2017 Microsoft
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
using System.Threading.Tasks;
using System.Threading;
using Xwt;
using Xwt.Backends;
using System.Linq;

namespace CoreTests
{
	public class FakeToolkit : ToolkitEngineBackend
	{
		public FakeToolkit ()
		{
		}

		public override void InitializeApplication ()
		{
			base.InitializeApplication ();
			EventQueue.MainEventQueue.Reset ();
		}

		public override void DispatchPendingEvents ()
		{
			EventQueue.MainEventQueue.DispatchPendingEvents (false);
		}

		public override void ExitApplication ()
		{
			EventQueue.MainEventQueue.Exit ();
		}

		public override IWindowFrameBackend GetBackendForWindow (object nativeWindow)
		{
			throw new NotImplementedException ();
		}

		public override object GetNativeWidget (Widget w)
		{
			throw new NotImplementedException ();
		}

		public override object GetNativeWindow (IWindowFrameBackend backend)
		{
			throw new NotImplementedException ();
		}

		public override bool HasNativeParent (Widget w)
		{
			throw new NotImplementedException ();
		}

		public override void InvokeAsync (Action action)
		{
			EventQueue.MainEventQueue.Enqueue (delegate {
				ApplicationContext.InvokeUserCode (action);
			});
		}

		public override void RunApplication ()
		{
			EventQueue.MainEventQueue.DispatchPendingEvents (true);
		}

		public override object TimerInvoke (Func<bool> action, TimeSpan timeSpan)
		{
			return EventQueue.MainEventQueue.Enqueue (delegate {
				bool result = false;
				ApplicationContext.InvokeUserCode (delegate {
					result = action ();
				});
				return result;
			}, timeSpan);
		}

		public override void CancelTimerInvoke (object id)
		{
			((CancellationTokenSource)id).Cancel ();
		}
	}

	public class EventQueue
	{
		Queue<Action> events = new Queue<Action> ();
		bool exit;

		public static EventQueue MainEventQueue { get; } = new EventQueue ();

		public void Reset ()
		{
			exit = false;
		}

		public void DispatchPendingEvents (bool keepWaiting)
		{
			do {
				List<Action> list;
				lock (events) {
					list = events.ToList ();
					events.Clear ();
				}

				foreach (var e in list)
					e ();

				if (!keepWaiting)
					return;

				lock (events) {
					if (events.Count == 0 && !exit)
						Monitor.Wait (events);
				}
			} while (!exit);
		}

		public void Exit ()
		{
			lock (events) {
				exit = true;
				Monitor.PulseAll (events);
			}
		}

		public void Enqueue (Action action)
		{
			lock (events) {
				events.Enqueue (action);
				Monitor.PulseAll (events);
			}
		}

		public CancellationTokenSource Enqueue (Func<bool> action, TimeSpan timeSpan)
		{
			CancellationTokenSource cts = new CancellationTokenSource ();
			ScheduleTimerInvoke (cts, action, timeSpan);
			return cts;
		}

		void ScheduleTimerInvoke (CancellationTokenSource cts, Func<bool> action, TimeSpan timeSpan)
		{
			Task.Delay (timeSpan, cts.Token).ContinueWith (t => {
				Enqueue (delegate {
					if (action ())
						ScheduleTimerInvoke (cts, action, timeSpan);
				});
			}, TaskContinuationOptions.NotOnCanceled);
		}
	}
}
