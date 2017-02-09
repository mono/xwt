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
using System.Linq;
using Xwt.Backends;
using Xwt.Drawing;
using System.Collections.Generic;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGRect = System.Drawing.RectangleF;
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
#else
using Foundation;
using AppKit;
using CoreGraphics;
#endif

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
		DialogButton defaultButton;
		WidgetSpacing buttonBoxPadding = new WidgetSpacing (12, 6, 12, 12);

		bool modalSessionRunning;

		public DialogButton DefaultButton {
			get {
				return defaultButton;
			}

			set {
				defaultButton = value;
				SetButtons (buttons.Keys.ToArray ());
			}
		}

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

		[Export ("cancelOperation:")]
		void HandleCancelOperation (NSObject semder)
		{
			PerformClose (this);
			//FIXME: should we check whether there is a Command.Cancel button and
			//       respond with the command instead of a simple close (null response)?
		}

		protected override void OnClosed ()
		{
			base.OnClosed ();
			if (modalSessionRunning)
				EndLoop ();
		}

		public override void LayoutWindow ()
		{
			var frame = ContentView.Frame;
			var boxHeight = 0f;
			if (buttonBox.Children.Any ()) {
				var ps = buttonBox.Surface.GetPreferredSize (true);
				buttonBoxView.Frame = new CGRect ((nfloat)buttonBoxPadding.Left, (nfloat)buttonBoxPadding.Bottom, frame.Width - (nfloat)buttonBoxPadding.HorizontalSpacing, (float)ps.Height);
				buttonBox.Surface.Reallocate ();
				boxHeight = (float)ps.Height + (float)buttonBoxPadding.VerticalSpacing;
			}
			LayoutContent (new CGRect (0, boxHeight, frame.Width, frame.Height - boxHeight));
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

			foreach (var b in buttonList.OrderBy (b => b == DefaultButton).Reverse ()) {
				var button = new Button { Font = Font.SystemFont };
				var tb = b;
				button.Clicked += delegate {
					OnClicked (tb);
				};
				button.MinWidth = 77; // Dialog buttons have a minimal width of 77px on Mac

				if (b.PackOrigin == PackOrigin.End)
					buttonBox.PackEnd (button);
				else
					buttonBox.PackStart (button);
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

			// Dialog buttons on Mac have a 8px horizontal padding
			realButton.WidthRequest = -1;
			var s = realButton.Surface.GetPreferredSize ();
			realButton.WidthRequest = s.Width + 16;
			if (b == defaultButton) {
				var nativeButton = realButton.Surface.NativeWidget as NSButton;
				nativeButton.Window.DefaultButtonCell = nativeButton.Cell;
			}
		}

		public void RunLoop (IWindowFrameBackend parent)
		{
			if (parent != null)
				StyleMask &= ~NSWindowStyle.Miniaturizable;
			else
				StyleMask |= NSWindowStyle.Miniaturizable;
			Visible = true;
			modalSessionRunning = true;
			var win = parent as NSWindow ?? ApplicationContext.Toolkit.GetNativeWindow (parent) as NSWindow;
			if (win != null) {
				win.AddChildWindow (this, NSWindowOrderingMode.Above);
				// always use NSWindow for alignment when running in guest mode and
				// don't rely on AddChildWindow to position the window correctly
				if (!(parent is WindowBackend)) {
					var parentBounds = MacDesktopBackend.ToDesktopRect (win.ContentRectFor (win.Frame));
					var bounds = ((IWindowFrameBackend)this).Bounds;
					bounds.X = parentBounds.Center.X - (Frame.Width / 2);
					bounds.Y = parentBounds.Center.Y - (Frame.Height / 2);
					((IWindowFrameBackend)this).Bounds = bounds;
				}
			}
			NSApplication.SharedApplication.RunModalForWindow (this);
		}

		public void EndLoop ()
		{
			modalSessionRunning = false;
			var parent = ParentWindow;
			if (parent != null)
				parent.RemoveChildWindow (this);
			OrderOut (this);
			Close();
			NSApplication.SharedApplication.StopModal ();
			if (parent != null)
				parent.MakeKeyAndOrderFront (parent);
		}

		#endregion
	}
}

