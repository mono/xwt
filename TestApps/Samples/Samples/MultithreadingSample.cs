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
using System.Threading;
using System.Threading.Tasks;
using Xwt;

namespace Samples
{
	public class MultithreadingSample: VBox
	{
		Label l1, l2;
		ToggleButton btn1, btn2;

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
		}
	}
}

