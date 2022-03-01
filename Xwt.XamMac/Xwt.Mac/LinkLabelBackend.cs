//
// LinkLabelBackend.cs
//
// Author:
//       Alex Corrado <corrado@xamarin.com>
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
using AppKit;
using CoreGraphics;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class LinkLabelBackend : LabelBackend, ILinkLabelBackend
	{
		Uri uri;

		new ILinkLabelEventSink EventSink {
			get { return (ILinkLabelEventSink)base.EventSink; }
		}

		public Uri Uri {
			get { return uri; }
			set {
				uri = value;
				if (value != null) {
					Widget.AccessibilityRole = NSAccessibilityRoles.LinkRole;
					Widget.AccessibilityUrl = value;
				} else {
					Widget.AccessibilityRole = NSAccessibilityRoles.StaticTextRole;
					if (Widget.AccessibilityUrl != null)
						Widget.AccessibilityUrl = new NSUrl (string.Empty);
				}
			}
		}

		public override string Text {
			get { return base.Text; }
			set {
				base.Text = value;
				Widget.Cell.AttributedStringValue = GetAttributedString (value);
				Widget.AccessibilityTitle = value;
			}
		}

		public LinkLabelBackend () : base (new LinkLabelView ())
		{
		}

		public override void Initialize ()
		{
			base.Initialize ();
			CanGetFocus = true;
			Widget.AllowsEditingTextAttributes = true;
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is LinkLabelEvent) {
				switch ((LinkLabelEvent) eventId) {
				case LinkLabelEvent.NavigateToUrl:
					((LinkLabelView)Widget).Clicked += HandleClicked;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is LinkLabelEvent) {
				switch ((LinkLabelEvent) eventId) {
				case LinkLabelEvent.NavigateToUrl:
					((LinkLabelView)Widget).Clicked -= HandleClicked;
					break;
				}
			}
		}

		void HandleClicked (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (() => {
				NSWorkspace.SharedWorkspace.OpenUrl (uri);
			});
		}

		static NSAttributedString GetAttributedString (string text)
		{
			var attrStr = new NSMutableAttributedString (text);
			var range = new NSRange (0, attrStr.Length);

			var singleUnderlineStyle = NSNumber.FromInt32 ((int)NSUnderlineStyle.Single);
			attrStr.AddAttribute (NSStringAttributeKey.ForegroundColor, Toolkit.CurrentEngine.Defaults.FallbackLinkColor.ToNSColor (), range);
			attrStr.AddAttribute (NSStringAttributeKey.UnderlineStyle, singleUnderlineStyle, range);

			return attrStr;
		}

		bool canGetFocus;
		public override bool CanGetFocus
		{
			get { return canGetFocus; }
			set { canGetFocus = value; }
		}

		public override void SetFocus()
		{
			if (Widget.Window != null && CanGetFocus)
				Widget.Window.MakeFirstResponder(Widget);
		}

		public override bool HasFocus
		{
			get {
				return Widget.Window != null && Widget.Window.FirstResponder == Widget;
			}
		}
	}

	class LinkLabelView : TextFieldView
	{
		public event EventHandler Clicked;

		public LinkLabelView ()
		{
			FocusRingType = NSFocusRingType.Default;
		}

		public override void ResetCursorRects ()
		{
			AddCursorRect (Bounds, NSCursor.PointingHandCursor);
		}

		public override void MouseUp (NSEvent theEvent)
		{
			// Unfortunately, cocoa calls MouseUp even if the mouse is not still over this control so we have to check that
			var location = ConvertPointFromView (theEvent.LocationInWindow, null);
			if (Clicked != null && IsMouseInRect (location, Cell.DrawingRectForBounds (Bounds)))
				Clicked (this, EventArgs.Empty);
		}

		public override void KeyDown (NSEvent theEvent)
		{
			var key = theEvent.ToXwtKeyEventArgs ().Key;
			if ((key == Key.Return || key == Key.Space) && Clicked != null)
				Clicked(this, EventArgs.Empty);
			else
				base.KeyDown(theEvent);
		}

		public override bool AcceptsFirstResponder ()
		{
			return Backend?.CanGetFocus ?? false;
		}

		public override CGRect FocusRingMaskBounds { get { return Bounds; } }

		public override void DrawFocusRingMask ()
		{
			NSGraphicsContext.CurrentContext.GraphicsPort.FillRect (Bounds);
		}
	}
}

