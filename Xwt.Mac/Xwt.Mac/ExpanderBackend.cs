using System;
using System.Drawing;

using Xwt;
using Xwt.Backends;

using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace Xwt.Mac
{
	class ExpanderBackend : ViewBackend<MacExpander, IExpandEventSink>, IExpanderBackend
	{
		ViewBackend child;

		public ExpanderBackend ()
		{
			SetMinSize (10, 21);
		}

		public override void Initialize ()
		{
			ViewObject = new MacExpander (EventSink, ApplicationContext);
			Widget.Expander.DisclosureToggled += (sender, e) => {
				ResetFittingSize ();
				NotifyPreferredSizeChanged ();
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.ExpandChanged ();
				});
			};
		}

		public string Label {
			get {
				return Widget.Expander.Label;
			}
			set {
				Widget.Expander.Label = value;
			}
		}

		public bool Expanded {
			get {
				return Widget.Box.Expanded;
			}
			set {
				Widget.Box.Expanded = value;
				Widget.Expander.On = value;
				ResetFittingSize ();
			}
		}

		public void SetContent (IWidgetBackend widget)
		{
			if (child != null)
				RemoveChildPlacement (child.Widget);

			child = (ViewBackend)widget;

			Widget.Box.SetContent (GetWidgetWithPlacement (widget));
			ResetFittingSize ();
		}

		protected override Size CalcFittingSize ()
		{
			var s = Widget.SizeOfDecorations;
			if (Widget.Box.Expanded && child != null) {
				s += child.Frontend.Surface.GetPreferredSize ();
			}
			return s;
		}

		public override object Font {
			get {
				return Widget.Expander.Font;
			}
			set {
				Widget.Expander.Font = (NSFont)value;
			}
		}
	}

	class MacExpander: WidgetView
	{
		ExpanderWidget expander;
		CollapsibleBox box;

		public MacExpander (IWidgetEventSink eventSink, ApplicationContext context): base (eventSink, context)
		{
			expander = new ExpanderWidget () {
				Frame = new RectangleF (0, 0, 80, 21),
				AutoresizingMask = NSViewResizingMask.WidthSizable
			};
			box = new CollapsibleBox () { AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable };
			box.SetFrameOrigin (new PointF (0, 21));
			expander.DisclosureToggled += (sender, e) => box.Expanded = expander.On;
			AddSubview (expander);
			AddSubview (box);
		}

		public Size SizeOfDecorations {
			get {
				return new Size (0, 21);
			}
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public ExpanderWidget Expander {
			get {
				return expander;
			}
		}

		public CollapsibleBox Box {
			get {
				return box;
			}
		}

		public void EnableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}

		public void DisableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}

		public override void SetFrameSize (SizeF newSize)
		{
			base.SetFrameSize (newSize);
			box.UpdateContentSize (false);
		}
	}

	class ExpanderWidget : NSView
	{
		public event EventHandler DisclosureToggled;

		NSButton label;
		NSButton disclosure;
		NSGradient backgroundGradient;
		NSColor strokeColor;

		public ExpanderWidget ()
		{
			disclosure = new NSButton {
				BezelStyle = NSBezelStyle.Disclosure,
				AutoresizingMask = NSViewResizingMask.MaxYMargin,
				ImagePosition = NSCellImagePosition.ImageOnly,
				Frame = new RectangleF (5, 4, 13, 13),
				State = NSCellStateValue.Off
			};
			disclosure.SetButtonType (NSButtonType.OnOff);
			disclosure.Activated += delegate {
				if (DisclosureToggled != null)
					DisclosureToggled (this, EventArgs.Empty);
			};

			label = new NSButton {
				Bordered = false,
				AutoresizingMask = NSViewResizingMask.MaxYMargin | NSViewResizingMask.WidthSizable,
				Alignment = NSTextAlignment.Left,
				Frame = new RectangleF (17, 3, 60, 13),
				Target = disclosure,
				Action = new Selector ("performClick:")
			};
			label.SetButtonType (NSButtonType.MomentaryChange);

			AutoresizesSubviews = true;
			backgroundGradient = new NSGradient (NSColor.FromCalibratedRgba (0.93f, 0.93f, 0.97f, 1.0f),
			                                     NSColor.FromCalibratedRgba (0.74f, 0.76f, 0.83f, 1.0f));
			strokeColor = NSColor.FromCalibratedRgba (0.60f, 0.60f, 0.60f, 1.0f);

			AddSubview (label);
			AddSubview (disclosure);
		}

		public NSFont Font {
			get {
				return label.Font;
			}
			set {
				label.Font = value;
			}
		}

		public string Label {
			get {
				return label.Title;
			}
			set {
				label.Title = value;
			}
		}

		public bool On {
			get {
				return disclosure.State == NSCellStateValue.On;
			}
			set {
				disclosure.State = value ? NSCellStateValue.On : NSCellStateValue.Off;
			}
		}

		public override void DrawRect (RectangleF dirtyRect)
		{
			backgroundGradient.DrawInRect (Frame, -90);
			if (dirtyRect == Frame) {
				strokeColor.SetStroke ();
				NSBezierPath.StrokeRect (dirtyRect);
			}
		}
	}

	public class CollapsibleBox : NSBox
	{
		internal const float DefaultCollapsedHeight = 1f;
		bool expanded;
		float otherHeight;

		public CollapsibleBox ()
		{
			expanded = false;
			otherHeight = DefaultCollapsedHeight;
			TitlePosition = NSTitlePosition.NoTitle;
			BorderType = NSBorderType.NoBorder;
			BoxType = NSBoxType.NSBoxPrimary;
			ContentViewMargins = new SizeF (0, 0);
		}

		public void SetContent (NSView view)
		{
			ContentView = view;
			UpdateContentSize (false);
		}

		public bool Expanded {
			get { return expanded; }
			set {
				SetExpanded (value, false);
			}
		}

		public void SetExpanded (bool value, bool animate)
		{
			if (expanded != value) {
				expanded = value;
				UpdateContentSize (animate);
			}
		}

		public void UpdateContentSize (bool animate)
		{
			if (expanded) {
				var s = ((IViewObject)ContentView).Backend.Frontend.Surface.GetPreferredSize ((float)Frame.Size.Width, SizeConstraint.Unconstrained);
				SetFrameSize (new SizeF (Frame.Width, (float)s.Height), animate);
			} else
				SetFrameSize (new SizeF (Frame.Width, DefaultCollapsedHeight), animate);
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		RectangleF FrameForNewSizePinnedToTopLeft (SizeF newFrameSize)
		{
			var frame = Frame;
			frame.Size = newFrameSize;
			return frame;
		}

		public void SetFrameSize (SizeF newFrameSize, bool animating)
		{
			RectangleF newFrame = FrameForNewSizePinnedToTopLeft (newFrameSize);
			if (animating) {
				NSAnimation animation = new NSViewAnimation (new [] {
					NSDictionary.FromObjectsAndKeys (
					    new object[] { this, NSValue.FromRectangleF (Frame), NSValue.FromRectangleF (newFrame) },
						new object[] { NSViewAnimation.TargetKey, NSViewAnimation.StartFrameKey, NSViewAnimation.EndFrameKey }
					)
				});
				animation.AnimationBlockingMode = NSAnimationBlockingMode.Nonblocking;
				animation.Duration = 0.25;
				animation.StartAnimation ();
			} else {
				Superview.SetNeedsDisplayInRect (Frame);
				Frame = newFrame;
				NeedsDisplay = true;
			}
		}
	}
}

