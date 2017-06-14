//
// XwtSynchronizationContext.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using System.Threading;
using Xwt.Backends;

namespace Xwt
{
	public class XwtSynchronizationContext : SynchronizationContext, IDisposable
	{
		Toolkit toolkit;

		static SynchronizationContext previous_context;

		static XwtSynchronizationContext ()
		{
			AutoInstall = true;
			previous_context = SynchronizationContext.Current;
		}

		public static bool AutoInstall {
			get;
			set;
		}

		public XwtSynchronizationContext ()
		{
		}

		internal XwtSynchronizationContext (Toolkit toolkit)
		{
			this.toolkit = toolkit;
		}

		internal Toolkit TargetToolkit {
			get { return toolkit; }
		}

		public override void Post (SendOrPostCallback d, object state)
		{
			Application.Invoke (() => d.Invoke (state), toolkit);
		}

		public override void Send (SendOrPostCallback d, object state)
		{
			if (Application.UIThread != null && Application.UIThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId) {
				var evt = new ManualResetEventSlim (false);
				Exception exception = null;
				Application.Invoke (() => {
					try {
						d.Invoke (state);
					} catch (Exception ex) {
						exception = ex;
					} finally {
						Thread.MemoryBarrier ();
						evt.Set ();
					}
				}, toolkit);
				evt.Wait ();
				if (exception != null)
					throw exception;
			} else {
				d.Invoke (state);
			}
		}

		public static void Uninstall ()
		{
			if (previous_context == null)
				previous_context = new SynchronizationContext ();

			SynchronizationContext.SetSynchronizationContext (previous_context);
		}

		public void Dispose ()
		{
		}

		public override SynchronizationContext CreateCopy ()
		{
			return new XwtSynchronizationContext (toolkit);
		}
	}
}

