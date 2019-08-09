// 
// ButtonBackend.cs
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
using AppKit;
using CoreGraphics;
using Foundation;
using Xwt.Accessibility;
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.Mac
{
	public class ButtonBackend: ViewBackend<NSButton,IButtonEventSink>, IButtonBackend
	{
		ButtonType currentType;
		ButtonStyle currentStyle;

		public ButtonBackend ()
		{
		}

		#region IButtonBackend implementation
		public override void Initialize ()
		{
			ViewObject = new MacButton (EventSink, ApplicationContext);
			Widget.SetButtonType (NSButtonType.MomentaryPushIn);
		}

		public void EnableEvent (Xwt.Backends.ButtonEvent ev)
		{
			((MacButton)Widget).EnableEvent (ev);
		}

		public void DisableEvent (Xwt.Backends.ButtonEvent ev)
		{
			((MacButton)Widget).DisableEvent (ev);
		}
		
		public void SetContent (string label, bool useMnemonic, ImageDescription image, ContentPosition imagePosition)
		{
			switch (((Button)Frontend).Type) {
			case ButtonType.Help:
			case ButtonType.Disclosure:
				return;
			}
			if (useMnemonic)
				label = label.RemoveMnemonic ();
			if (customLabelColor.HasValue) {
				Widget.Title = label;
				var ns = new NSMutableAttributedString (Widget.AttributedTitle);
				ns.BeginEditing ();
				var r = new NSRange (0, label.Length);
				ns.RemoveAttribute (NSStringAttributeKey.ForegroundColor, r);
				ns.AddAttribute (NSStringAttributeKey.ForegroundColor, customLabelColor.Value.ToNSColor (), r);
				ns.EndEditing ();
				Widget.AttributedTitle = ns;
			} else
				Widget.Title = label ?? "";
			if (string.IsNullOrEmpty (label))
				imagePosition = ContentPosition.Center;
			if (!image.IsNull) {
				var img = image.ToNSImage ();
				Widget.Image = (NSImage)img;
				Widget.Cell.ImageScale = NSImageScale.None;
				switch (imagePosition) {
				case ContentPosition.Bottom: Widget.ImagePosition = NSCellImagePosition.ImageBelow; break;
				case ContentPosition.Left: Widget.ImagePosition = NSCellImagePosition.ImageLeft; break;
				case ContentPosition.Right: Widget.ImagePosition = NSCellImagePosition.ImageRight; break;
				case ContentPosition.Top: Widget.ImagePosition = NSCellImagePosition.ImageAbove; break;
				case ContentPosition.Center: Widget.ImagePosition = string.IsNullOrEmpty (label) ? NSCellImagePosition.ImageOnly : NSCellImagePosition.ImageOverlaps; break;
				}
			} else {
				Widget.ImagePosition = NSCellImagePosition.NoImage;
				Widget.Image = null;
			}
			SetButtonStyle (currentStyle);
			ResetFittingSize ();
		}
		
		public virtual void SetButtonStyle (ButtonStyle style)
		{
			currentStyle = style;
			if (currentType == ButtonType.Normal)
			{
				switch (style) {
				case ButtonStyle.Normal:
					if (Widget.Image != null
						|| Frontend.MinHeight > 0
						|| Frontend.HeightRequest > 0
						|| Widget.Title.Contains (Environment.NewLine))
						Widget.BezelStyle = NSBezelStyle.RegularSquare;
					else
						Widget.BezelStyle = NSBezelStyle.Rounded;
					Widget.ShowsBorderOnlyWhileMouseInside = false;
					break;
				case ButtonStyle.Borderless:
				case ButtonStyle.Flat:
					Widget.BezelStyle = NSBezelStyle.ShadowlessSquare;
					Widget.ShowsBorderOnlyWhileMouseInside = true;
					break;
				}
			}
		}

		public void SetButtonType (ButtonType type)
		{
			currentType = type;
			switch (type) {
			case ButtonType.Disclosure:
				Widget.BezelStyle = NSBezelStyle.Disclosure;
				Widget.Title = "";
				break;
			case ButtonType.Help:
				Widget.BezelStyle = NSBezelStyle.HelpButton;
				Widget.Title = "";
				break;
			default:
					SetButtonStyle (currentStyle);
				break;
			}
		}
		bool isDefault;
		public bool IsDefault {
			get { return isDefault; }
			set {
				isDefault = value;
				if (Widget.Window != null && Widget.Window.DefaultButtonCell != Widget.Cell)
					Widget.Window.DefaultButtonCell = Widget.Cell;
			}
		}

		public void SetFormattedText (FormattedText text)
		{
			Widget.Title = text.Text;
			Widget.AttributedTitle = text.ToAttributedString ();
		}

		#endregion

		public override Color BackgroundColor {
			get { return ((MacButton)Widget).BackgroundColor; }
			set { ((MacButton)Widget).BackgroundColor = value; }
		}

		Color? customLabelColor;
		public Color LabelColor {
			get { return customLabelColor.HasValue ? customLabelColor.Value : NSColor.ControlText.ToXwtColor (); }
			set {
				customLabelColor = value;
				var ns = new NSMutableAttributedString (Widget.AttributedTitle);
				ns.BeginEditing ();
				var r = new NSRange (0, Widget.Title.Length);
				ns.RemoveAttribute (NSStringAttributeKey.ForegroundColor, r);
				ns.AddAttribute (NSStringAttributeKey.ForegroundColor, customLabelColor.Value.ToNSColor (), r);
				ns.EndEditing ();
				Widget.AttributedTitle = ns;
			}
		}
	}
	
	class MacButton: NSButton, IViewObject, INSAccessibleEventSource
	{
		//
		// This is necessary since the Activated event for NSControl in AppKit does 
		// not take a list of handlers, instead it supports only one handler.
		//
		// This event is used by the RadioButton backend to implement radio groups
		//
		internal event Action <MacButton> ActivatedInternal;

		public MacButton (IntPtr p): base (p)
		{
		}
		
		public MacButton (IButtonEventSink eventSink, ApplicationContext context)
		{
			Cell = new ColoredButtonCell ();
			BezelStyle = NSBezelStyle.Rounded;
			Activated += delegate {
				context.InvokeUserCode (delegate {
					eventSink.OnClicked ();
				});
				OnActivatedInternal ();
			};
		}
		
		public MacButton ()
		{
			Activated += delegate {
				OnActivatedInternal ();
			};

		}

		public MacButton (IRadioButtonEventSink eventSink, ApplicationContext context)
		{
			Activated += delegate {
				context.InvokeUserCode (eventSink.OnClicked);
				OnActivatedInternal ();
			};
		}

		public ViewBackend Backend { get; set; }
		
		public NSView View {
			get { return this; }
		}
		
		public void EnableEvent (ButtonEvent ev)
		{
		}

		public void DisableEvent (ButtonEvent ev)
		{
		}

		public override void ViewDidMoveToWindow ()
		{
			if ((Backend as ButtonBackend)?.IsDefault == true && Window != null)
				Window.DefaultButtonCell = Cell;
		}

		void OnActivatedInternal ()
		{
			if (ActivatedInternal == null)
				return;

			ActivatedInternal (this);
		}

		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
			if (Backend.Cursor != null)
				AddCursorRect (Bounds, Backend.Cursor);
		}

		public Color BackgroundColor {
			get {
				return ((ColoredButtonCell)Cell).Color.GetValueOrDefault ();
			}
			set {
				((ColoredButtonCell)Cell).Color = value;
			}
		}

		public override bool AllowsVibrancy {
			get {
				// we don't support vibrancy
				if (EffectiveAppearance.AllowsVibrancy)
					return false;
				return base.AllowsVibrancy;
			}
		}

		NSButtonType buttonType = NSButtonType.MomentaryPushIn;
		public override void SetButtonType (NSButtonType aType)
		{
			buttonType = aType;
			base.SetButtonType (aType);
		}

		public override NSAppearance EffectiveAppearance {
			get {
				// HACK: if vibrancy is enabled (inside popover) radios/checks don't handle background drawing correctly
				// FIXME: this fix doesn't work for the vibrant light theme, the label background is wrong if
				//        the window background is set to a custom color
				if (base.EffectiveAppearance.AllowsVibrancy &&
				    (buttonType == NSButtonType.Switch || buttonType == NSButtonType.Radio))
					Cell.BackgroundStyle = base.EffectiveAppearance.Name.Contains ("Dark") ? NSBackgroundStyle.Dark : NSBackgroundStyle.Light;
				return base.EffectiveAppearance;
			}
		}

		class ColoredButtonCell : NSButtonCell
		{
			public Color? Color { get; set; }

			public override void DrawBezelWithFrame (CGRect frame, NSView controlView)
			{
				controlView.DrawWithColorTransform(Color, delegate { base.DrawBezelWithFrame (frame, controlView); });
			}
		}

		public Func<bool> PerformAccessiblePressDelegate { get; set; }

		public override bool AccessibilityPerformPress ()
		{
			if (PerformAccessiblePressDelegate != null) {
				if (PerformAccessiblePressDelegate ())
					return true;
			}
			return base.AccessibilityPerformPress ();
		}
	}
}

