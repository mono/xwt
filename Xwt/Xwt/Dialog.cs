// 
// Dialog.cs
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
using Xwt.Backends;
using Xwt.Drawing;
using Xwt.Engine;

namespace Xwt
{
	public class Dialog: Window
	{
		DialogButtonCollection commands;
		Command resultCommand;
		bool loopEnded;
		
		public Dialog ()
		{
			commands = new DialogButtonCollection ((WindowBackendHost)BackendHost);
		}
		
		protected new class WindowBackendHost: Window.WindowBackendHost, ICollectionListener, IDialogEventSink
		{
			new Dialog Parent { get { return (Dialog) base.Parent; } }
			
			public virtual void ItemAdded (object collection, object item)
			{
				if (collection == Parent.commands) {
					((DialogButton)item).ParentDialog = Parent;
					Parent.Backend.SetButtons (Parent.commands);
				}
			}

			public virtual void ItemRemoved (object collection, object item)
			{
				if (collection == Parent.commands) {
					((DialogButton)item).ParentDialog = null;
					Parent.Backend.SetButtons (Parent.commands);
				}
			}
			
			public void OnDialogButtonClicked (DialogButton btn)
			{
				btn.RaiseClicked ();
				if (btn.Command != null)
					Parent.OnCommandActivated (btn.Command);
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WindowBackendHost ();
		}
		
		IDialogBackend Backend {
			get { return (IDialogBackend) BackendHost.Backend; } 
		}
		
		public DialogButtonCollection Buttons {
			get { return commands; }
		}
		
		protected virtual void OnCommandActivated (Command cmd)
		{
			Respond (cmd);
		}
		
		public Command Run ()
		{
			return Run (null);
		}
		
		public Command Run (WindowFrame parent)
		{
			Toolkit.InvokePlatformCode (delegate {
				Backend.RunLoop ((IWindowFrameBackend) BackendHost.WidgetRegistry.GetBackend (parent));
			});
			return resultCommand;
		}
		
		public void Respond (Command cmd)
		{
			resultCommand = cmd;
			if (!loopEnded) {
				loopEnded = true;
				Backend.EndLoop ();
			}
		}
		
		public void EnableCommand (Command cmd)
		{
			var btn = Buttons.GetCommandButton (cmd);
			if (btn != null)
				btn.Sensitive = true;
		}
		
		public void DisableCommand (Command cmd)
		{
			var btn = Buttons.GetCommandButton (cmd);
			if (btn != null)
				btn.Sensitive = false;
		}
		
		public void ShowCommand (Command cmd)
		{
			var btn = Buttons.GetCommandButton (cmd);
			if (btn != null)
				btn.Visible = true;
		}
		
		public void HideCommand (Command cmd)
		{
			var btn = Buttons.GetCommandButton (cmd);
			if (btn != null)
				btn.Visible = false;
		}
		
		internal void UpdateButton (DialogButton btn)
		{
			Backend.UpdateButton (btn);
		}
	}
	
	public class DialogButton
	{
		Command command;
		string label;
		Image image;
		bool visible = true;
		bool sensitive = true;
		internal Dialog ParentDialog;
		
		public DialogButton (string label)
		{
			this.label = label;
		}
		
		public DialogButton (string label, Command cmd)
		{
			this.label = label;
			this.command = cmd;
		}
		
		public DialogButton (string label, Image icon)
		{
			this.label = label;
			this.image = icon;
		}
		
		public DialogButton (string label, Image icon, Command cmd)
		{
			this.label = label;
			this.command = cmd;
			this.image = icon;
		}
		
		public DialogButton (Command cmd)
		{
			this.command = cmd;
		}
		
		public Command Command {
			get { return command; }
		}
		
		public string Label {
			get {
				if (label != null)
					return label;
				if (command != null)
					return command.Label;
				return "";
			}
			set {
				label = value;
				ParentDialog.UpdateButton (this);
			}
		}
		
		public Image Image {
			get {
				if (image != null)
					return image;
				return null;
			}
			set {
				image = value;
				ParentDialog.UpdateButton (this);
			}
		}
		
		public bool Visible { 
			get { return visible; }
			set {
				visible = value;
				ParentDialog.UpdateButton (this);
			}
		}
		
		public bool Sensitive { 
			get { return sensitive; }
			set {
				sensitive = value;
				ParentDialog.UpdateButton (this);
			}
		}
		
		internal void RaiseClicked ()
		{
			if (Clicked != null)
				Clicked (this, EventArgs.Empty);
		}
		
		public event EventHandler Clicked;
	}
}

