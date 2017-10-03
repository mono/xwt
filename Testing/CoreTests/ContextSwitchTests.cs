//
// ContextSwitchTests.cs
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

using NUnit.Framework;
using System;
using Xwt;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CoreTests
{
	[TestFixture ()]
	public class ContextSwitchTests
	{
		Toolkit mainToolkit;
		Toolkit secToolkit;

		[Test]
		public void AsyncContextSwitch ()
		{
			// Test that after an await, the current toolkit is changed to
			// the toolkit active before the await.

			Exception error = null;
			Application.UnhandledException += (object sender, ExceptionEventArgs e) => {
				error = e.ErrorException;
				// If somethig goes wrong, make sure the main loop is exited
				Application.Exit ();
			};

			// Load the main toolkit

			Application.Initialize (typeof (MainFakeToolkit).AssemblyQualifiedName);
			mainToolkit = Toolkit.CurrentEngine;

			// Load a second toolkit

			secToolkit = Toolkit.Load (typeof (SecondaryFakeToolkit).AssemblyQualifiedName);

			// The main toolkit is the default toolkit by default

			Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);

			// Run the tests in the simulated ui loop
			var task = AsyncContextSwitchLoop ();

			Application.Run ();
			task.Wait ();

			if (error != null) {
				Console.WriteLine (error);
				throw error;
			}

			Application.Dispose ();
		}

		async Task AsyncContextSwitchLoop ()
		{
			// We are in the main toolkit
			Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);

			bool callbackInvoked = false;

			var t = secToolkit.Invoke (async delegate {
				
				// Current engine should now be the second toolkit
				Assert.AreSame (secToolkit, Toolkit.CurrentEngine);

				await Task.Delay (200);

				// Current engine should still the second toolkit
				Assert.AreSame (secToolkit, Toolkit.CurrentEngine);
				Console.WriteLine ("Done");
				callbackInvoked = true;
			});

			// Invocation on the second toolkit should not change the
			// current engine
			Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);

			await Task.Delay (2000);

			// During the wait, code in the context of the secondary context
			// has been called, but the main toolkit must have been restored

			Assert.IsTrue (callbackInvoked);
			Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);

			Application.Exit ();
		}

		[Test]
		public void AppInvokeCapturesToolkit ()
		{
			// Test that Application.TimeoutInvoke() will invoke the callback in
			// the context of the active toolkit, even though the event is always
			// dispatched using the primary toolkit.

			Exception error = null;
			Application.UnhandledException += (object sender, ExceptionEventArgs e) => {
				error = e.ErrorException;
				// If somethig goes wrong, make sure the main loop is exited
				Application.Exit ();
			};

			// Load the main toolkit
			Application.Initialize (typeof (MainFakeToolkit).AssemblyQualifiedName);
			mainToolkit = Toolkit.CurrentEngine;

			// Load a second toolkit
			secToolkit = Toolkit.Load (typeof (SecondaryFakeToolkit).AssemblyQualifiedName);

			// The main toolkit is the default toolkit by default
			Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);

			secToolkit.Invoke (delegate {
				// Current engine should now be the second toolkit
				Assert.AreSame (secToolkit, Toolkit.CurrentEngine);
				Application.TimeoutInvoke (100, () => {
					// Current toolkit should still be the second toolkit
					Assert.AreSame (secToolkit, Toolkit.CurrentEngine);
					Application.Exit ();
					return false;
				});
			});

			Application.Run ();

			if (error != null) {
				Console.WriteLine (error);
				throw error;
			}

			Application.Dispose ();
		}

		[Test]
		public void TaskWaitRestoresMainToolkit ()
		{
			// Test that the main toolkit is restored when executing out-of-band UI events

			Exception error = null;
			Application.UnhandledException += (object sender, ExceptionEventArgs e) => {
				error = e.ErrorException;
				// If somethig goes wrong, make sure the main loop is exited
				Application.Exit ();
			};

			// Load the main toolkit

			Application.Initialize (typeof (MainFakeToolkit).AssemblyQualifiedName);
			mainToolkit = Toolkit.CurrentEngine;

			// Load a second toolkit

			secToolkit = Toolkit.Load (typeof (SecondaryFakeToolkit).AssemblyQualifiedName);

			// We are in the main toolkit
			Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);

			bool noXwtCallback = false;

			// Enqueue an event that will be executed directly by the
			// event loop, out of the XWT context
			EventQueue.MainEventQueue.Enqueue (delegate {
				// When outside the XWT context, the default toolkit should be
				// the main toolkit
				Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);
				noXwtCallback = true;
				return false;
			}, TimeSpan.FromMilliseconds (50));

			var t = secToolkit.Invoke (async delegate {

				// Current engine should now be the second toolkit
				Assert.AreSame (secToolkit, Toolkit.CurrentEngine);

				Assert.IsFalse (noXwtCallback);

				await Task.Delay (200);

				// Out of band event was executed during the wait
				Assert.IsTrue (noXwtCallback);

				// Current engine should still the second toolkit
				Assert.AreSame (secToolkit, Toolkit.CurrentEngine);
				Console.WriteLine ("Done");
				Application.Exit ();
			});

			// Invocation on the second toolkit should not change the
			// current engine
			Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);

			Application.Run ();

			if (error != null) {
				Console.WriteLine (error);
				throw error;
			}

			Application.Dispose ();
		}


		[Test]
		public void EventLoopPumpInMainToolkit ()
		{
			// Test that the main toolkit is always active when pumping the event loop.

			Exception error = null;
			Application.UnhandledException += (object sender, ExceptionEventArgs e) => {
				error = e.ErrorException;
				// If somethig goes wrong, make sure the main loop is exited
				Application.Exit ();
			};

			// Load the main toolkit

			Application.Initialize (typeof (MainFakeToolkit).AssemblyQualifiedName);
			mainToolkit = Toolkit.CurrentEngine;

			// Load a second toolkit

			secToolkit = Toolkit.Load (typeof (SecondaryFakeToolkit).AssemblyQualifiedName);

			// We are in the main toolkit
			Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);

			bool noXwtCallback = false;

			// Enqueue an event that will be executed directly by the
			// event loop, out of the XWT context
			EventQueue.MainEventQueue.Enqueue (delegate {
				// When outside the XWT context, the default toolkit should be
				// the main toolkit
				Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);
				noXwtCallback = true;
			});

			secToolkit.Invoke (delegate {

				// Current engine should now be the second toolkit
				Assert.AreSame (secToolkit, Toolkit.CurrentEngine);

				Assert.IsFalse (noXwtCallback);

				Application.MainLoop.DispatchPendingEvents ();

				// Out of band event must have been dispatched
				Assert.IsTrue (noXwtCallback);

				// Current engine should still the second toolkit
				Assert.AreSame (secToolkit, Toolkit.CurrentEngine);
				Console.WriteLine ("Done");
				Application.Exit ();
			});

			// Invocation on the second toolkit should not change the
			// current engine
			Assert.AreEqual (mainToolkit, Toolkit.CurrentEngine);

			Application.Run ();

			if (error != null) {
				Console.WriteLine (error);
				throw error;
			}

			Application.Dispose ();
		}
	}

	class MainFakeToolkit : FakeToolkit
	{
	}

	class SecondaryFakeToolkit : FakeToolkit
	{

	}
}
