//
// Dialog.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc
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
using NUnit.Framework;

namespace Xwt
{
	public class DialogTests: XwtTest
	{
		[Test]
		public void TwoResponds ()
		{
			// When Respond is called twice before leaving an event handler, the last call is the good one

			using (var win = new Dialog ()) {
				Application.TimeoutInvoke (10, delegate {
					win.Respond (Command.Ok);
					win.Respond (Command.Apply);
					return false;
				});
				var cmd = win.Run ();
				Assert.AreEqual (Command.Apply, cmd);
			}
		}

		[Test]
		public void RespondDoesNotFireClose ()
		{
			// The close event is not fired after running a dialog

			bool closed = false, closing = false;
			using (var win = new Dialog ()) {
				win.Buttons.Add (new DialogButton (Command.Ok));
				win.CloseRequested += (sender, e) => closing = true;
				win.Closed += (sender, e) => closed = true;
				Application.TimeoutInvoke (10, delegate {
					win.Respond (Command.Apply);
					return false;
				});
				var cmd = win.Run ();
				Assert.IsFalse (closing, "CloseRequested event should not be fired");
				Assert.IsFalse (closed, "Close event should not be fired");
				Assert.AreEqual (Command.Apply, cmd);
			}
			Assert.IsFalse (closing, "CloseRequested event should not be fired when disposing");
			Assert.IsFalse (closed, "Close event should not be fired when disposing");
		}

		[Test]
		public void Close ()
		{
			// The Close method can be used to stop running a dialog

			bool closing = false, closed = false, closeResult = false;
			using (var win = new Dialog ()) {
				win.Buttons.Add (new DialogButton (Command.Ok));
				win.CloseRequested += delegate(object sender, CloseRequestedEventArgs args) {
					Assert.IsTrue (args.AllowClose);
					closing = true;
				};
				win.Closed += (sender, e) => closed = true;
				Application.TimeoutInvoke (10, delegate {
					closeResult = win.Close ();
					return false;
				});
				var cmd = win.Run ();
				Assert.IsNull (cmd);
				Assert.IsTrue (closing, "CloseRequested event not fired");
				Assert.IsTrue (closed, "Window not closed");
				Assert.IsTrue (closeResult, "Window not closed");
			}
		}

		[Test]
		public void RespondOnClosing ()
		{
			// Respond can be used in a CloseRequest handler to provide a result for the Run method

			using (var win = new Dialog ()) {
				bool closeResult = false;
				win.Buttons.Add (new DialogButton (Command.Ok));
				win.CloseRequested += delegate(object sender, CloseRequestedEventArgs args) {
					win.Respond (Command.Apply);
				};
				Application.TimeoutInvoke (10, delegate {
					closeResult = win.Close ();
					return false;
				});
				var cmd = win.Run ();
				Assert.AreEqual (Command.Apply, cmd);
				Assert.IsTrue (closeResult);
			}
		}

		[Test]
		public void RespondOnClosingWithCancel ()
		{
			// If Respond if called on a CloseRequest, but the close is canceled, the respose call is ignored

			using (var win = new Dialog ()) {
				bool closeCanceled = false, closeResult1 = false, closeResult2 = false;
				win.Buttons.Add (new DialogButton (Command.Ok));
				win.CloseRequested += HandleCloseRequested;
				Application.TimeoutInvoke (10, delegate {
					closeResult1 = win.Close ();
					Application.TimeoutInvoke (10, delegate {
						win.CloseRequested -= HandleCloseRequested;
						closeCanceled = true;
						closeResult2 = win.Close ();
						return false;
					});
					return false;
				});
				var cmd = win.Run ();
				Assert.IsTrue (closeCanceled, "Close not canceled");
				Assert.IsFalse (closeResult1, "First close should return false");
				Assert.IsTrue (closeResult2, "Second close should return true");
				Assert.IsNull (cmd);
			}
		}

		void HandleCloseRequested (object sender, CloseRequestedEventArgs args)
		{
			var dlg = (Dialog)sender;
			dlg.Respond (Command.Apply);
			args.AllowClose = false;
		}
	}
}

