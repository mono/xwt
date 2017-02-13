// 
// AlertDialogBackend.cs
//  
// Author:
//       Thomas Ziegler <ziegler.thomas@web.de>
// 
// Copyright (c) 2012 Thomas Ziegler
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
using AppKit;
using CoreGraphics;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class AlertDialogBackend : NSAlert, IAlertDialogBackend
	{
		ApplicationContext Context;

		public AlertDialogBackend ()
		{
		}
		
		public AlertDialogBackend (IntPtr intptr)
		{
		}

		public virtual void Initialize (ApplicationContext actx)
		{
			Context = actx;
		}

		#region IAlertDialogBackend implementation
		public Command Run (WindowFrame transientFor, MessageDescription message)
		{
			this.MessageText = message.Text ?? String.Empty;
			this.InformativeText = message.SecondaryText ?? String.Empty;

			if (message.Icon != null)
				Icon = message.Icon.ToImageDescription (Context).ToNSImage ();

			var sortedButtons = new Command [message.Buttons.Count];
			var j = 0;
			if (message.DefaultButton >= 0) {
				sortedButtons [0] = message.Buttons [message.DefaultButton];
				this.AddButton (message.Buttons [message.DefaultButton].Label);
				j = 1;
			}
			for (var i = 0; i < message.Buttons.Count; i++) {
				if (i == message.DefaultButton)
					continue;
				sortedButtons [j++] = message.Buttons [i];
				this.AddButton (message.Buttons [i].Label);
			}
			for (var i = 0; i < sortedButtons.Length; i++) {
				if (sortedButtons [i].Icon != null) {
					Buttons [i].Image = sortedButtons [i].Icon.WithSize (IconSize.Small).ToImageDescription (Context).ToNSImage ();
					Buttons [i].ImagePosition = NSCellImagePosition.ImageLeft;
				}
			}

			if (message.AllowApplyToAll) {
				ShowsSuppressionButton = true;
				SuppressionButton.State = NSCellStateValue.Off;
				SuppressionButton.Activated += (sender, e) => ApplyToAll = SuppressionButton.State == NSCellStateValue.On;
			}

			if (message.Options.Count > 0) {
				AccessoryView = new NSView ();
				var optionsSize = new CGSize (0, 3);

				foreach (var op in message.Options) {
					var chk = new NSButton ();
					chk.SetButtonType (NSButtonType.Switch);
					chk.Title = op.Text;
					chk.State = op.Value ? NSCellStateValue.On : NSCellStateValue.Off;
					chk.Activated += (sender, e) => message.SetOptionValue (op.Id, chk.State == NSCellStateValue.On);

					chk.SizeToFit ();
					chk.Frame = new CGRect (new CGPoint (0, optionsSize.Height), chk.FittingSize);

					optionsSize.Height += chk.FittingSize.Height + 6;
					optionsSize.Width = (float) Math.Max (optionsSize.Width, chk.FittingSize.Width);

					AccessoryView.AddSubview (chk);
					chk.NeedsDisplay = true;
				}

				AccessoryView.SetFrameSize (optionsSize);
			}

			var win = Context.Toolkit.GetNativeWindow (transientFor) as NSWindow;
			if (win != null)
				return sortedButtons [(int)this.RunSheetModal (win) - 1000];
			return sortedButtons [(int)this.RunModal () - 1000];
		}

		public bool ApplyToAll { get; set; }
		#endregion
	}
}
