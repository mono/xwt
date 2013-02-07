// XwtTaskScheduler.cs
//
// Author:
//   Alan McGovern <alan@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc
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
//
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xwt
{
	/// <summary>
	/// Marshal every Task to the Xwt UI thread no matter where it's created
	/// (as opposed to TaskScheduler.FromCurrentSynchronizationContext)
	/// </summary>
	class XwtTaskScheduler : TaskScheduler
	{
		Toolkit toolkit;

		public XwtTaskScheduler (Toolkit toolkit)
		{
			this.toolkit = toolkit;
		}

		protected override void QueueTask (Task task)
		{
			if (Application.UIThread != null)
				Xwt.Application.Invoke (() => toolkit.Invoke (() => TryExecuteTask (task)));
			else
				TryExecuteTask (task);
		}

		protected override System.Collections.Generic.IEnumerable<Task> GetScheduledTasks ()
		{
			throw new System.NotImplementedException();
		}

		protected override bool TryDequeue (Task task)
		{
			return false;
		}

		protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
		{
			bool success = true;

			if (Application.UIThread != null && Application.UIThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId) {
				var evt = new ManualResetEvent (false);
				Xwt.Application.Invoke (() => {
					success = TryExecuteTask (task);
					Thread.MemoryBarrier ();
					evt.Set ();
				});
				evt.WaitOne ();
			} else {
				success = TryExecuteTask (task);
			}

			return success;
		}

		public override int MaximumConcurrencyLevel {
			get {
				return 1;
			}
		}
	}
}