//
// DialogBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using System.Collections.Generic;
using MonoMac.AppKit;

namespace Xwt.Mac
{
	public class DialogBackend: WindowBackend, IDialogBackend
	{
		NSView mainBox;
		HBox buttonBox;
		NSView buttonBoxView;
		Widget dialogChild;
		Size minSize;
		Dictionary<DialogButton,Button> buttons = new Dictionary<DialogButton, Button> ();
		WidgetSpacing buttonBoxPadding = new WidgetSpacing (12, 6, 12, 12);

		public DialogBackend ()
		{
		}

		public override void InitializeBackend (object frontend, ApplicationContext context)
		{
			base.InitializeBackend (frontend, context);

			buttonBox = new HBox () {
				Spacing = 0,
				Margin = 0
			};
			buttonBoxView = ((ViewBackend)buttonBox.GetBackend ()).Widget;
			ContentView.AddSubview (buttonBoxView);
		}

		public override void LayoutWindow ()
		{
			var frame = ContentView.Frame;
			var ps = buttonBox.Surface.GetPreferredSize (true);
			buttonBoxView.Frame = new System.Drawing.RectangleF ((float)buttonBoxPadding.Left, (float)buttonBoxPadding.Bottom, frame.Width - (float)buttonBoxPadding.HorizontalSpacing, (float)ps.Height);
			buttonBox.Surface.Reallocate ();
			var boxHeight = (float)ps.Height + (float)buttonBoxPadding.VerticalSpacing;
			LayoutContent (new System.Drawing.RectangleF (0, boxHeight, frame.Width, frame.Height - boxHeight));
		}

		public override void GetMetrics (out Size minSize, out Size decorationSize)
		{
			var ps = buttonBox.Surface.GetPreferredSize (true);
			minSize = new Size (ps.Width + buttonBoxPadding.VerticalSpacing, 0);
			decorationSize = new Size (0, ps.Height + buttonBoxPadding.HorizontalSpacing);
		}

		#region IDialogBackend implementation

		public void SetButtons (System.Collections.Generic.IEnumerable<DialogButton> buttonList)
		{
			buttonBox.Clear ();

			foreach (var b in buttonList) {
				var button = new Button ();
				var tb = b;
				button.Clicked += delegate {
					OnClicked (tb);
				};
				buttonBox.PackEnd (button);
				buttons [b] = button;
				UpdateButton (b, button);
			}
			if (minSize != Size.Zero)
				SetMinSize (minSize);
		}

		void OnClicked (DialogButton button)
		{
			ApplicationContext.InvokeUserCode (delegate {
				((IDialogEventSink)EventSink).OnDialogButtonClicked (button);
			});
		}
		public void UpdateButton (DialogButton b)
		{
			Button realButton;
			if (buttons.TryGetValue (b, out realButton)) {
				UpdateButton (b, realButton);
				if (minSize != Size.Zero)
					SetMinSize (minSize);
			}
		}

		public void UpdateButton (DialogButton b, Button realButton)
		{
			realButton.Label = b.Label;
			realButton.Image = b.Image;
			realButton.Sensitive = b.Sensitive;
			realButton.Visible = b.Visible;
		}

		public void RunLoop (IWindowFrameBackend parent)
		{
			Visible = true;
			NSApplication.SharedApplication.RunModalForWindow (this);
		}

		public void EndLoop ()
		{
			NSApplication.SharedApplication.StopModal ();
		}

		#endregion
	}
}

