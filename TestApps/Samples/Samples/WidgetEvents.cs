// 
// WidgetEvents.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class WidgetEvents: VBox
	{
		Label laLeft = new Label ("Left");
		Label laMiddle = new Label ("Middle");
		Label laRight = new Label ("Right");

		Label la = new Label ("Move the mouse here");
		Label resLa = new Label ("");

		bool insideLabel;
		bool movingOverLabel;
		bool movedOverLabel;

		SpinButton spn = new SpinButton ();
		Label resSpn = new Label ("");

		bool insideSpn;
		bool movedOverSpn;
		bool movingOverSpn;

		TextEntry te = new TextEntry ();
		Label resTe = new Label ("");

		bool insideTe;
		bool movedOverTe;
		bool movingOverTe;

		Button btn = new Button ("Move the mouse here");
		Label resBtn = new Label ("");

		bool insideButton;
		bool movedOverButton;
		bool movingOverButton;

		SimpleBox canvas = new SimpleBox (10, 50);
		Label resCanvas = new Label ("");

		bool insideCanvas;
		bool movedOverCanvas;
		bool movingOverCanvas;

		public WidgetEvents ()
		{
			laLeft.BackgroundColor = Colors.White;
			laMiddle.BackgroundColor = Colors.White;
			laRight.BackgroundColor = Colors.White;

			var mouseBtnBox = new HBox ();
			mouseBtnBox.PackStart (new Label("Mouse butons:"));
			mouseBtnBox.PackStart (laLeft);
			mouseBtnBox.PackStart (laMiddle);
			mouseBtnBox.PackStart (laRight);
			PackStart (mouseBtnBox);

			PackStart (la);
			PackStart (resLa);

			la.MouseEntered += (sender, e) => {
				insideLabel = true;
				Application.TimeoutInvoke (100, CheckMouseOverLabel);
				Console.WriteLine ("[" + sender + "] Mouse Entered");
			};
			la.MouseExited += (sender, e) => {
				insideLabel = false;
				Console.WriteLine ("[" + sender + "] Mouse Exited");
			};
			la.MouseMoved += (sender, e) => movedOverLabel = true;
			la.ButtonPressed += HandleButtonPressed;
			la.ButtonReleased += HandleButtonReleased;
			la.MouseScrolled += HandleMouseScrolled;

			PackStart (te);
			PackStart (resTe);

			te.MouseEntered += (sender, e) => {
				insideTe = true;
				Application.TimeoutInvoke (100, CheckMouseOverTe);
				Console.WriteLine ("[" + sender + "] Mouse Entered");
				resTe.Text = "Mouse has Entered Text Entry";
			};
			te.MouseExited += (sender, e) => {
				insideTe = false;
				Console.WriteLine ("[" + sender + "] Mouse Exited");
				resTe.Text = "Mouse has Exited Text Entry";
			};
			te.MouseMoved += (sender, e) => movedOverTe = true;
			te.ButtonPressed += HandleButtonPressed;
			te.ButtonReleased += HandleButtonReleased;
			te.MouseScrolled += HandleMouseScrolled;

			PackStart (spn);
			PackStart (resSpn);

			spn.MouseEntered += (sender, e) => {
				insideSpn = true;
				Application.TimeoutInvoke (100, CheckMouseOverSpn);
				Console.WriteLine ("[" + sender + "] Mouse Entered");
			};
			spn.MouseExited += (sender, e) => {
				insideSpn = false;
				Console.WriteLine ("[" + sender + "] Mouse Exited");
			};
			spn.MouseMoved += (sender, e) => movedOverSpn = true;
			spn.ButtonPressed += HandleButtonPressed;
			spn.ButtonReleased += HandleButtonReleased;
			spn.MouseScrolled += HandleMouseScrolled;

			PackStart (btn);
			PackStart (resBtn);

			btn.MouseEntered += (sender, e) =>  {
				insideButton = true;
				Application.TimeoutInvoke (100, CheckMouseOverButton);
				Console.WriteLine ("[" + sender + "] Mouse Entered");
			};
			btn.MouseExited += (sender, e) => {
				insideButton = false;
				Console.WriteLine ("[" + sender + "] Mouse Exited");
			};
			btn.MouseMoved += (sender, e) => movedOverButton = true;
			btn.ButtonPressed += HandleButtonPressed;
			btn.ButtonReleased += HandleButtonReleased;
			btn.Clicked += (sender, e) => {
				resBtn.Text = "Button Clicked";
				Console.WriteLine ("[" + sender + "] Clicked");
			};

			PackStart (canvas);
			PackStart (resCanvas);

			canvas.MouseEntered += (sender, e) =>  {
				insideCanvas = true;
				Application.TimeoutInvoke (100, CheckMouseOverCanvas);
				Console.WriteLine ("[" + sender + "] Mouse Entered");
			};
			canvas.MouseExited += (sender, e) => {
				insideCanvas = false;
				Console.WriteLine ("[" + sender + "] Mouse Exited");
			};
			canvas.MouseMoved += (sender, e) => movedOverCanvas = true;
			canvas.MouseScrolled += HandleMouseScrolled;
			canvas.ButtonPressed += HandleButtonPressed;
			canvas.ButtonReleased += HandleButtonReleased;
		}

		int laScrollCnt, spnScrollCnt, btnScrollCnt, canvasScrollCnt;
		void HandleMouseScrolled (object sender, MouseScrolledEventArgs e)
		{
			var msg = "Scrolled " + e.Direction;
			int incrUpDown = 0;
			if (e.Direction == ScrollDirection.Up)
				incrUpDown++;
			else if (e.Direction == ScrollDirection.Down)
				incrUpDown--;

			if (sender == la) {
				msg += " " + (laScrollCnt += incrUpDown);
				resLa.Text = msg;
			}
			if (sender == te) {
				resTe.Text = msg;
			}
			if (sender == spn) {
				msg += " " + (spnScrollCnt += incrUpDown);
				resSpn.Text = msg;
			}
			if (sender == btn) {
				msg += " " + (btnScrollCnt += incrUpDown);
				resBtn.Text = msg;
			}
			if (sender == canvas) {
				msg += " " + (canvasScrollCnt += incrUpDown);
				resCanvas.Text = msg;
			}
			Console.WriteLine ("[" + sender + "] " + msg);
		}

		void HandleButtonPressed (object sender, ButtonEventArgs e)
		{
			switch (e.Button) {
				case PointerButton.Left: laLeft.BackgroundColor = Colors.Green; break;
				case PointerButton.Middle: laMiddle.BackgroundColor = Colors.Green; break;
				case PointerButton.Right: laRight.BackgroundColor = Colors.Green; break;
			}
			Console.WriteLine ("[" + sender + "] ButtonPressed : " + e.MultiplePress + "x " + e.Button + " ");
		}

		void HandleButtonReleased (object sender, ButtonEventArgs e)
		{
			switch (e.Button) {
				case PointerButton.Left: laLeft.BackgroundColor = Colors.White; break;
				case PointerButton.Middle: laMiddle.BackgroundColor = Colors.White; break;
				case PointerButton.Right: laRight.BackgroundColor = Colors.White; break;
			}
			Console.WriteLine ("[" + sender + "] ButtonReleased : " + e.MultiplePress + "x " + e.Button + " ");
		}

		bool CheckMouseOverLabel ()
		{
			if (!insideLabel) {
				resLa.Text = "Mouse has Exited label";
				la.TextColor = Colors.Black;
				la.BackgroundColor = Colors.LightGray;
				la.Text = "Move the mouse here";
			} else {
				resLa.Text = "Mouse has Entered label";
				la.TextColor = Colors.White;
				if (movedOverLabel) {
					la.BackgroundColor = Colors.Green;
					la.Text = "Mouse is moving";
					Console.WriteLine ("[" + la.GetType () + "] Mouse Moving");
					movedOverLabel = false;	// reset and check next time
					movingOverLabel = true;
				} else if (movingOverLabel) {
					la.BackgroundColor = Colors.Red;
					la.Text = "Mouse has stopped";
					Console.WriteLine ("[" + la.GetType () + "] Mouse Stopped");
					movingOverLabel = false;
				}
			}
			return insideLabel;
		}

		bool CheckMouseOverTe ()
		{
			if (!insideTe) {
				te.Text = "Move the mouse here";
			} else {
				if (movedOverTe) {
					te.Text = "Mouse is moving";
					Console.WriteLine ("[" + te.GetType () + "] Mouse Moving");
					movedOverTe = false;	// reset and check next time
					movingOverTe = true;
				} else if (movingOverTe) {
					te.Text = "Mouse has stopped";
					Console.WriteLine ("[" + te.GetType () + "] Mouse Stopped");
					movingOverTe = false;
				}
			}
			return insideTe;
		}

		bool CheckMouseOverSpn ()
		{
			if (!insideSpn) {
				resSpn.Text = "Mouse has Exited Spin Button";
			} else {
				if (movedOverSpn) {
					resSpn.Text = "Mouse Moving over Spin Button";
					Console.WriteLine ("[" + spn.GetType () + "] Mouse Moving");
					movedOverSpn = false;	// reset and check next time
					movingOverSpn = true;
				} else if (movingOverSpn) {
					resSpn.Text = "Mouse Stopped Moving over Canvas";
					Console.WriteLine ("[" + spn.GetType () + "] Mouse Stopped");
					movingOverSpn = false;
				}
			}
			return insideSpn;
		}

		bool CheckMouseOverButton ()
		{
			if (!insideButton) {
				resBtn.Text = "Mouse has Exited Button";
				btn.Label = "Move the mouse here";
			} else {
				resBtn.Text = "Mouse has Entered Button";
				if (movedOverButton) {
					btn.Label = "Mouse is moving";
					Console.WriteLine ("[" + btn.GetType () + "] Mouse Moving");
					movedOverButton = false;	// reset and check next time
					movingOverButton = true;
				} else if (movingOverButton) {
					btn.Label = "Mouse has stopped";
					Console.WriteLine ("[" + btn.GetType () + "] Mouse Stopped");
					movingOverButton = false;	// reset and check next time
				}
			}
			return insideButton;
		}

		bool CheckMouseOverCanvas ()
		{
			if (!insideCanvas) {
				resCanvas.Text = "Mouse has Exited Canvas";
			} else {
				if (movedOverCanvas) {
					resCanvas.Text = "Mouse Moving over Canvas";
					Console.WriteLine ("[" + canvas.GetType () + "] Mouse Moving");
					movedOverCanvas = false;	// reset and check next time
					movingOverCanvas = true;
				} else if (movingOverCanvas) {
					resCanvas.Text = "Mouse Stopped Moving over Canvas";
					Console.WriteLine ("[" + canvas.GetType () + "] Mouse Stopped");
					movingOverCanvas = false;
				}
			}
			return insideCanvas;
		}
	}
}

