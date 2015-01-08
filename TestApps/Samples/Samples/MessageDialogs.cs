//
// MessageDialogs.cs
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
using Xwt;

namespace Samples
{
	public class MessageDialogs : VBox
	{
		public MessageDialogs ()
		{
			Table table = new Table ();

			TextEntry txtPrimay = new TextEntry ();
			TextEntry txtSecondary = new TextEntry ();
			txtSecondary.MultiLine = true;
			ComboBox cmbType = new ComboBox ();
			cmbType.Items.Add ("Message");
			cmbType.Items.Add ("Question");
			cmbType.Items.Add ("Confirmation");
			cmbType.Items.Add ("Warning");
			cmbType.Items.Add ("Error");
			cmbType.SelectedIndex = 0;

			Button btnShowMessage = new Button ("Show Message");

			Label lblResult = new Label ();

			table.Add (new Label ("Primary Text:"), 0, 0);
			table.Add (txtPrimay, 1, 0, hexpand: true);
			table.Add (new Label ("Secondary Text:"), 0, 1);
			table.Add (txtSecondary, 1, 1, hexpand: true);
			table.Add (new Label ("Message Type:"), 0, 2);
			table.Add (cmbType, 1, 2, hexpand: true);

			table.Add (btnShowMessage, 1, 3, hexpand: true);
			table.Add (lblResult, 1, 4, hexpand: true);

			btnShowMessage.Clicked += (sender, e) => {

				switch (cmbType.SelectedText) {
					case "Message":
						MessageDialog.ShowMessage (this.ParentWindow, txtPrimay.Text, txtSecondary.Text);
						lblResult.Text = "Result: dialog closed";
						break;
					case "Question":
						var question = new QuestionMessage(txtPrimay.Text, txtSecondary.Text);
						question.Buttons.Add(new Command("Answer 1"));
						question.Buttons.Add(new Command("Answer 2"));
						question.DefaultButton = 1;
						question.AddOption ("option1", "Option 1", false);
						question.AddOption ("option2", "Option 2", true);
						var result = MessageDialog.AskQuestion (question);
						lblResult.Text = "Result: " + result.Id;
						if (question.GetOptionValue ("option1"))
							lblResult.Text += " + Option 1";
						if (question.GetOptionValue ("option2"))
							lblResult.Text += " + Option 2";
						break;
					case "Confirmation":
						var confirmation = new ConfirmationMessage (txtPrimay.Text, txtSecondary.Text, Command.Apply);
						confirmation.AddOption ("option1", "Option 1", false);
						confirmation.AddOption ("option2", "Option 2", true);
						confirmation.AllowApplyToAll = true;

						var success = MessageDialog.Confirm (confirmation);
						lblResult.Text = "Result: " + success;
						if (confirmation.GetOptionValue ("option1"))
							lblResult.Text += " + Option 1";
						if (confirmation.GetOptionValue ("option2"))
							lblResult.Text += " + Option 2";

						lblResult.Text += " + All: " + confirmation.AllowApplyToAll;
						break;
					case "Warning":
						MessageDialog.ShowWarning (this.ParentWindow, txtPrimay.Text, txtSecondary.Text);
						lblResult.Text = "Result: dialog closed";
						break;
					case "Error":
						MessageDialog.ShowError (this.ParentWindow, txtPrimay.Text, txtSecondary.Text);
						lblResult.Text = "Result: dialog closed";
						break;
				}
			};

			PackStart (table, true);
		}
	}
}

