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
		Label res1 = new Label ("");

		bool insideLabel;
		bool movingOverLabel;
		bool movedOverLabel;

		Button btn = new Button ("Move the mouse here");
		Label res2 = new Label ("");

		bool insideButton;
		bool movedOverButton;
		bool movingOverButton;

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
			PackStart (res1);

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

			PackStart (btn);
			PackStart (res2);

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
				res2.Text = "Button Clicked";
				Console.WriteLine ("[" + sender + "] Clicked");
			};
		}

		void HandleMouseScrolled (object sender, MouseScrolledEventArgs e)
		{
			var msg = "Scrolled " + e.Direction + ": " + e.Position + " " + e.X + "x" + e.Y + " " + e.Timestamp;
			if (sender == la)
				res1.Text = msg;
			if (sender == btn)
				res2.Text = msg;
			Console.WriteLine (msg);
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

		bool CheckMouseOverButton ()
		{
			if (!insideButton) {
				res2.Text = "Mouse has Exited Button";
				btn.Label = "Move the mouse here";
			} else {
				res2.Text = "Mouse has Entered Button";
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

		bool CheckMouseOverLabel ()
		{
			if (!insideLabel) {
				res1.Text = "Mouse has Exited label";
				la.TextColor = Colors.Black;
				la.BackgroundColor = Colors.LightGray;
				la.Text = "Move the mouse here";
			} else {
				res1.Text = "Mouse has Entered label";
				la.TextColor = Colors.White;
				if (movedOverLabel) {
					la.BackgroundColor = Colors.Green;
					la.Text = "Mouse is moving";
					Console.WriteLine ("[" + btn.GetType () + "] Mouse Moving");
					movedOverLabel = false;	// reset and check next time
					movingOverLabel = true;
				} else if (movingOverLabel) {
					la.BackgroundColor = Colors.Red;
					la.Text = "Mouse has stopped";
					Console.WriteLine ("[" + btn.GetType () + "] Mouse Stopped");
					movingOverLabel = false;
				}
			}
			return insideLabel;
		}
	}
}

