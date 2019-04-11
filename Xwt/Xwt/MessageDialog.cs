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
			Icon = Toolkit.CurrentEngine.Defaults.MessageDialog.QuestionIcon;
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
			Icon = Toolkit.CurrentEngine.Defaults.MessageDialog.QuestionIcon;
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

	public sealed class MessageDialogDefaults
	{
		Drawing.Image informationIcon;
		Drawing.Image warningIcon;
		Drawing.Image errorIcon;
		Drawing.Image confirmationIcon;
		Drawing.Image questionIcon;

		/// <summary>
		/// Gets or sets the icon shown in Information dialogs.
		/// </summary>
		/// <value>The information icon.</value>
		public Drawing.Image InformationIcon {
			get {
				if (informationIcon != null)
					return informationIcon;
				return StockIcons.Information;
			}

			set {
				informationIcon = value;
			}
		}

		/// <summary>
		/// Gets or sets the icon shown in Warning dialogs.
		/// </summary>
		/// <value>The warning icon.</value>
		public Drawing.Image WarningIcon {
			get {
				if (warningIcon != null)
					return warningIcon;
				return StockIcons.Warning;
			}

			set {
				warningIcon = value;
			}
		}

		/// <summary>
		/// Gets or sets the icon shown in Error dialogs.
		/// </summary>
		/// <value>The error icon.</value>
		public Drawing.Image ErrorIcon {
			get {
				if (errorIcon != null)
					return errorIcon;
				return StockIcons.Error;
			}

			set {
				errorIcon = value;
			}
		}

		/// <summary>
		/// Gets or sets the icon shown in Confirmation dialogs.
		/// </summary>
		/// <value>The confirmation icon.</value>
		public Drawing.Image ConfirmationIcon {
			get {
				if (confirmationIcon != null)
					return confirmationIcon;
				return StockIcons.Question;
			}

			set {
				confirmationIcon = value;
			}
		}

		/// <summary>
		/// Gets or sets the icon shown in Question dialogs.
		/// </summary>
		/// <value>The question icon.</value>
		public Drawing.Image QuestionIcon {
			get {
				if (questionIcon != null)
					return questionIcon;
				return StockIcons.Question;
			}

			set {
				questionIcon = value;
			}
		}
	}
}

