// 
// MessageDialog.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt.Backends;


namespace Xwt
{
	public static class MessageDialog
	{
		public static WindowFrame RootWindow { get; set; }
		
		#region ShowError
		public static void ShowError (string primaryText)
		{
			ShowError (RootWindow, primaryText);
		}
		public static void ShowError (WindowFrame parent, string primaryText)
		{
			ShowError (parent, primaryText, null);
		}
		public static void ShowError (string primaryText, string secondaryText)
		{
			ShowError (RootWindow, primaryText, secondaryText);
		}
		public static void ShowError (WindowFrame parent, string primaryText, string secondaryText)
		{
			GenericAlert (parent, StockIcons.Error, primaryText, secondaryText, Command.Ok);
		}
		#endregion
		
		#region ShowWarning
		public static void ShowWarning (string primaryText)
		{
			ShowWarning (RootWindow, primaryText);
		}
		public static void ShowWarning (WindowFrame parent, string primaryText)
		{
			ShowWarning (parent, primaryText, null);
		}
		public static void ShowWarning (string primaryText, string secondaryText)
		{
			ShowWarning (RootWindow, primaryText, secondaryText);
		}
		public static void ShowWarning (WindowFrame parent, string primaryText, string secondaryText)
		{
			GenericAlert (parent, StockIcons.Warning, primaryText, secondaryText, Command.Ok);
		}
		#endregion
		
		
		#region ShowMessage
		public static void ShowMessage (string primaryText)
		{
			ShowMessage (RootWindow, primaryText);
		}
		public static void ShowMessage (WindowFrame parent, string primaryText)
		{
			ShowMessage (parent, primaryText, null);
		}
		public static void ShowMessage (string primaryText, string secondaryText)
		{
			ShowMessage (RootWindow, primaryText, secondaryText);
		}
		public static void ShowMessage (WindowFrame parent, string primaryText, string secondaryText)
		{
			GenericAlert (parent, StockIcons.Information, primaryText, secondaryText, Command.Ok);
		}
		#endregion
		
		#region Confirm
		public static bool Confirm (string primaryText, Command button)
		{
			return Confirm (primaryText, null, button);
		}
		
		public static bool Confirm (string primaryText, string secondaryText, Command button)
		{
			return GenericAlert (RootWindow, StockIcons.Question, primaryText, secondaryText, Command.Cancel, button) == button;
		}
		public static bool Confirm (string primaryText, Command button, bool confirmIsDefault)
		{
			return Confirm (primaryText, null, button, confirmIsDefault);
		}
		
		public static bool Confirm (string primaryText, string secondaryText, Command button, bool confirmIsDefault)
		{
			return GenericAlert (RootWindow, StockIcons.Question, primaryText, secondaryText, confirmIsDefault ? 0 : 1, Command.Cancel, button) == button;
		}
		
		public static bool Confirm (ConfirmationMessage message)
		{
			return GenericAlert (RootWindow, message) == message.ConfirmButton;
		}
		#endregion
		
		#region AskQuestion
		public static Command AskQuestion (string primaryText, params Command[] buttons)
		{
			return AskQuestion (primaryText, null, buttons);
		}
		
		public static Command AskQuestion (string primaryText, string secondaryText, params Command[] buttons)
		{
			return GenericAlert (RootWindow, StockIcons.Question, primaryText, secondaryText, buttons);
		}
		public static Command AskQuestion (string primaryText, int defaultButton, params Command[] buttons)
		{
			return AskQuestion (primaryText, null, defaultButton, buttons);
		}
		
		public static Command AskQuestion (string primaryText, string secondaryText, int defaultButton, params Command[] buttons)
		{
			return GenericAlert (RootWindow, StockIcons.Question, primaryText, secondaryText, defaultButton, buttons);
		}
		
		public static Command AskQuestion (QuestionMessage message)
		{
			return GenericAlert (RootWindow, message);
		}
		#endregion
		
		static Command GenericAlert (WindowFrame parent, Xwt.Drawing.Image icon, string primaryText, string secondaryText, params Command[] buttons)
		{
			return GenericAlert (parent, icon, primaryText, secondaryText, buttons.Length - 1, buttons);
		}
		
		static Command GenericAlert (WindowFrame parent, Xwt.Drawing.Image icon, string primaryText, string secondaryText, int defaultButton, params Command[] buttons)
		{
			GenericMessage message = new GenericMessage () {
				Icon = icon,
				Text = primaryText,
				SecondaryText = secondaryText,
				DefaultButton = defaultButton
			};
			foreach (Command but in buttons)
				message.Buttons.Add (but);
			
			return GenericAlert (parent, message);
		}
		
		static Command GenericAlert (WindowFrame parent, MessageDescription message)
		{
			if (message.ApplyToAllButton != null)
				return message.ApplyToAllButton;

			IAlertDialogBackend backend = Toolkit.CurrentEngine.Backend.CreateBackend<IAlertDialogBackend> ();
			backend.Initialize (Toolkit.CurrentEngine.Context);
			if (message.Icon != null)
				message.Icon.InitForToolkit (Toolkit.CurrentEngine);

			using (backend) {
				var res = backend.Run (parent ?? RootWindow, message);
				
				if (backend.ApplyToAll)
					message.ApplyToAllButton = res;
				
				return res;
			}
		}
	}
	
	public class MessageDescription
	{
		internal MessageDescription ()
		{
			DefaultButton = -1;
			Buttons = new List<Command> ();
			Options = new List<AlertOption> ();
		}
		
		public IList<Command> Buttons { get; private set; }
		public IList<AlertOption> Options { get; private set; }
		
		internal Command ApplyToAllButton { get; set; }
		
		public Xwt.Drawing.Image Icon { get; set; }
		
		public string Text { get; set; }
		public string SecondaryText { get; set; }
		public bool AllowApplyToAll { get; set; }
		public int DefaultButton { get; set; }
		
		public void AddOption (string id, string text, bool setByDefault)
		{
			Options.Add (new AlertOption (id, text) { Value = setByDefault });
		}
		
		public bool GetOptionValue (string id)
		{
			foreach (var op in Options)
				if (op.Id == id)
					return op.Value;
			throw new ArgumentException ("Invalid option id");
		}
		
		public void SetOptionValue (string id, bool value)
		{
			foreach (var op in Options) {
				if (op.Id == id) {
					op.Value = value;
					return;
				}
			}
			throw new ArgumentException ("Invalid option id");
		}
	}
	
	public class AlertOption
	{
		internal AlertOption (string id, string text)
		{
			this.Id = id;
			this.Text = text;
		}

		public string Id { get; private set; }
		public string Text { get; private set; }
		public bool Value { get; set; }
	}
	
	public sealed class GenericMessage: MessageDescription
	{
		public GenericMessage ()
		{
		}
		
		public GenericMessage (string text)
		{
			Text = text;
		}
		
		public GenericMessage (string text, string secondaryText): this (text)
		{
			SecondaryText = secondaryText;
		}
		
		public new IList<Command> Buttons {
			get { return base.Buttons; }
		}
	}
	
	
	public sealed class QuestionMessage: MessageDescription
	{
		public QuestionMessage ()
		{
			Icon = StockIcons.Question;
		}
		
		public QuestionMessage (string text): this ()
		{
			Text = text;
		}
		
		public QuestionMessage (string text, string secondaryText): this (text)
		{
			SecondaryText = secondaryText;
		}
		
		public new IList<Command> Buttons {
			get { return base.Buttons; }
		}
	}
	
	public sealed class ConfirmationMessage: MessageDescription
	{
		Command confirmButton;
		
		public ConfirmationMessage ()
		{
			Icon = StockIcons.Question;
			Buttons.Add (Command.Cancel);
		}
		
		public ConfirmationMessage (Command button): this ()
		{
			ConfirmButton = button;
		}
		
		public ConfirmationMessage (string primaryText, Command button): this (button)
		{
			Text = primaryText;
		}
		
		public ConfirmationMessage (string primaryText, string secondaryText, Command button): this (primaryText, button)
		{
			SecondaryText = secondaryText;
		}
		
		public Command ConfirmButton {
			get { return confirmButton; }
			set {
				if (Buttons.Count == 2)
					Buttons.RemoveAt (1);
				Buttons.Add (value);
				confirmButton = value;
			}
		}
		
		public bool ConfirmIsDefault {
			get {
				return DefaultButton == 1;
			}
			set {
				if (value)
					DefaultButton = 1;
				else
					DefaultButton = 0;
			}
		}
	}
}

