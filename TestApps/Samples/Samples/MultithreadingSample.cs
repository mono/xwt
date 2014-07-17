//
// MultithreadingSample.cs
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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xwt;

namespace Samples
{
	public class MultithreadingSample: VBox
	{
		SampleBGWorker worker4, worker5, worker6;
		Label l1, l2, l3, l4, l5, l6;
		ToggleButton btn1, btn2, btn3, btn4, btn5, btn6;

		public MultithreadingSample ()
		{
			int r = 0;
			var tbl = new Table ();
			PackStart (tbl);

			tbl.Add (new Label("Application.Invoke:"), 0, r);
			tbl.Add (l1 = new Label ("0"), 1, r);
			tbl.Add (btn1 = new ToggleButton("Run"), 2, r++);

			Task worker1;
			CancellationTokenSource worker1cancel = new CancellationTokenSource ();;
			btn1.Toggled += delegate {
				if (btn1.Active) {

					worker1cancel = new CancellationTokenSource ();
					worker1 = Task.Factory.StartNew (delegate {

						int cnt = 0;
						while (!worker1cancel.Token.IsCancellationRequested) {
							Thread.Sleep (50);
							cnt++;
							string txt = cnt.ToString ();
							Application.Invoke (() => l1.Text = txt);
						}
					}, worker1cancel.Token, TaskCreationOptions.LongRunning);
				}
				else
					worker1cancel.Cancel();
			};


			tbl.Add (new Label("Application.UITaskScheduler:"), 0, r);
			tbl.Add (l2 = new Label ("0"), 1, r);
			tbl.Add (btn2 = new ToggleButton("Run"), 2, r++);

			Task worker2;
			CancellationTokenSource worker2cancel = new CancellationTokenSource ();;
			btn2.Toggled += delegate {
				if (btn2.Active) {

					worker2cancel = new CancellationTokenSource ();
					worker2 = Task.Factory.StartNew (delegate {

						int cnt = 0;
						while (!worker2cancel.Token.IsCancellationRequested) {
							Thread.Sleep (50);
							cnt++;
							var txt = cnt.ToString ();
							Task.Factory.StartNew(delegate {
								l2.Text = txt;
							}, worker2cancel.Token, TaskCreationOptions.None, Application.UITaskScheduler);
						}
					}, worker2cancel.Token, TaskCreationOptions.LongRunning);
				}
				else
					worker2cancel.Cancel();
			};


			tbl.Add (new Label("BackgroundWorker:\n(XwtSynchronizationContext.Post)"), 0, r);
			tbl.Add (l3 = new Label ("0"), 1, r);
			tbl.Add (btn3 = new ToggleButton("Run"), 2, r++);

			var worker3 = new BackgroundWorker ();
			worker3.WorkerReportsProgress = true;
			worker3.WorkerSupportsCancellation = true;
			worker3.DoWork += BackgroundWorkerDoWork;
			worker3.ProgressChanged += (sender, e) => l3.Text = e.ProgressPercentage.ToString ();

			btn3.Toggled += delegate {
				if (btn3.Active)
					worker3.RunWorkerAsync ();
				else
					worker3.CancelAsync ();
			};


			tbl.Add (new Label("XwtSynchronizationContext.Send:"), 0, r);
			tbl.Add (l4 = new Label ("0"), 1, r);
			tbl.Add (btn4 = new ToggleButton("Run"), 2, r++);

			worker4 = new SampleBGWorker ();
			worker4.SynchronizationContext = SynchronizationContext.Current;
			worker4.InvokeSynchronized = true;
			worker4.Count += HandleCount4;

			btn4.Toggled += delegate {
				if (btn4.Active)
					worker4.Start ();
				else
					worker4.Stop ();
			};


			tbl.Add (new Label("ISynchronizeInvoke.BeginInvoke:"), 0, r);
			tbl.Add (l5 = new Label ("0"), 1, r);
			tbl.Add (btn5 = new ToggleButton("Run"), 2, r++);

			worker5 = new SampleBGWorker ();
			worker5.InvokeSynchronized = false;
			worker5.Count += HandleCount5;

			btn5.Toggled += delegate {
				if (btn5.Active)
					worker5.Start ();
				else
					worker5.Stop ();
			};


			tbl.Add (new Label("ISynchronizeInvoke.Invoke:"), 0, r);
			tbl.Add (l6 = new Label ("0"), 1, r);
			tbl.Add (btn6 = new ToggleButton("Run"), 2, r++);

			worker6 = new SampleBGWorker ();
			worker6.InvokeSynchronized = true;
			worker6.Count += HandleCount6;

			btn6.Toggled += delegate {
				if (btn6.Active)
					worker6.Start ();
				else
					worker6.Stop ();
			};
		}

		void BackgroundWorkerDoWork (object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = (BackgroundWorker)sender;

			int cnt2 = 0;
			while (!worker.CancellationPending) {
				Thread.Sleep (50);
				worker.ReportProgress(cnt2++);
			}
			e.Cancel = true;
		}

		void HandleCount4 (object sender, CounterEventArgs e)
		{
			l4.Text = e.Cnt.ToString ();
		}

		void HandleCount5 (object sender, CounterEventArgs e)
		{
			l5.Text = e.Cnt.ToString ();
		}

		void HandleCount6 (object sender, CounterEventArgs e)
		{
			l6.Text = e.Cnt.ToString ();
		}
	}

	class SampleBGWorker
	{
		CancellationTokenSource stopReq;
		Task task;

		public event EventHandler<CounterEventArgs> Count;

		public SynchronizationContext SynchronizationContext { get; set; }

		public bool InvokeSynchronized { get; set; }

		public void Start ()
		{
			stopReq = new CancellationTokenSource ();
			task = Task.Factory.StartNew (CounterThread, stopReq.Token);
		}

		public void Stop ()
		{
			stopReq.Cancel ();
		}

		void CounterThread ()
		{
			int cnt = 0;
			while (!stopReq.Token.IsCancellationRequested) {
				Thread.Sleep (50);
				cnt++;
				if (Count != null) {
					if (SynchronizationContext == null) {
						foreach (var del in Count.GetInvocationList()) {
							var target = del.Target as ISynchronizeInvoke;
							if (target != null) {
								if (InvokeSynchronized)
									target.Invoke (del, new object[] { this, new CounterEventArgs (cnt) });
								else
									target.BeginInvoke (del, new object[] { this, new CounterEventArgs (cnt) });
							}
						}
					} else {
						var args = new CounterEventArgs (cnt);
						if (InvokeSynchronized) {
							SynchronizationContext.Send (delegate {
								Count (this, args);
							}, null);
						} else {
							SynchronizationContext.Post (delegate {
								Count (this, args);
							}, null);
						}
					}
				}
			}
		}
	}

	class CounterEventArgs : EventArgs
	{
		public int Cnt { get; private set; }

		public CounterEventArgs (int cnt)
		{
			Cnt = cnt;
		}
	}
}

