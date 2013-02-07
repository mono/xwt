using System;
using System.Drawing;

using Xwt;
using Xwt.Backends;

using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Xwt.Mac
{
	public class ExpanderBackend : ViewBackend<MacExpander, IExpandEventSink>, IExpanderBackend
	{
		public ExpanderBackend ()
		{
			ViewObject = new MacExpander ();
			Widget.Expander.DisclosureToggled += (sender, e) => NotifyPreferredSizeChanged ();
			SetMinSize (10, 25);
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
			}
		}

		public void SetContent (IWidgetBackend widget)
		{
			Widget.Box.SetFrameSize (new SizeF ((float)widget.GetPreferredWidth ().NaturalSize,
			                                    (float)widget.GetPreferredHeight ().NaturalSize),
			                         false);
			Widget.Box.SetContent (GetWidget (widget));
			NotifyPreferredSizeChanged ();
		}

		protected override Size GetNaturalSize ()
		{
			return new Size (Math.Max (Widget.Expander.Frame.Width, Widget.Box.Frame.Width),
			                 Widget.Expander.Frame.Height + Widget.Box.Frame.Height);
		}
	}

	public class MacExpander : NSView, IViewObject
	{
		ExpanderWidget expander;
		CollapsibleBox box;

		public MacExpander ()
		{
			expander = new ExpanderWidget () {
				Frame = new RectangleF (0, 0, 80, 21),
				AutoresizingMask = NSViewResizingMask.WidthSizable
			};
			box = new CollapsibleBox () { AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable };
			box.SetFrameOrigin (new PointF (0, 25));
			expander.DisclosureToggled += (sender, e) => box.Expanded = expander.On;
			AddSubview (expander);
			AddSubview (box);
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

		public ViewBackend Backend { get; set; }
		
		public NSView View {
			get { return this; }
		}
		
		public void EnableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}

		public void DisableEvent (Xwt.Backends.ButtonEvent ev)
		{
		}
	}

	public class ExpanderWidget : NSView
	{
		public event EventHandler DisclosureToggled;

		NSTextView label;
		NSButton disclosure;
		NSGradient backgroundGradient;
		NSColor strokeColor;

		public ExpanderWidget ()
		{
			label = new NSTextView () {
				AutoresizingMask = NSViewResizingMask.MaxYMargin | NSViewResizingMask.WidthSizable,
				Alignment = NSTextAlignment.Left,
				Editable = false,
				Selectable = false,
				DrawsBackground = false,
				Frame = new RectangleF (17, 3, 60, 13)
			};
			disclosure = new NSButton () {
				BezelStyle = NSBezelStyle.Disclosure,
				AutoresizingMask = NSViewResizingMask.MaxYMargin,
				ImagePosition = NSCellImagePosition.ImageOnly,
				Frame = new RectangleF (5, 4, 13, 13),
				State = NSCellStateValue.On
			};
			disclosure.SetButtonType (NSButtonType.OnOff);

			disclosure.AddObserver (this, new NSString ("cell.state"), NSKeyValueObservingOptions.New, IntPtr.Zero);
			AutoresizesSubviews = true;
			backgroundGradient = new NSGradient (NSColor.FromCalibratedRgba (0.93f, 0.93f, 0.97f, 1.0f),
			                                     NSColor.FromCalibratedRgba (0.74f, 0.76f, 0.83f, 1.0f));
			strokeColor = NSColor.FromCalibratedRgba (0.60f, 0.60f, 0.60f, 1.0f);

			AddSubview (label);
			AddSubview (disclosure);
		}

		public override void ObserveValue (NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			if (DisclosureToggled != null)
				DisclosureToggled (this, EventArgs.Empty);
		}

		public string Label {
			get {
				return label.Value;
			}
			set {
				label.Value = value;
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
			expanded = true;
			otherHeight = DefaultCollapsedHeight;
			TitlePosition = NSTitlePosition.NoTitle;
			BorderType = NSBorderType.NoBorder;
			BoxType = NSBoxType.NSBoxPrimary;
			ContentViewMargins = new SizeF (0, 0);
		}

		public void SetContent (NSView view)
		{
			ContentView = view;
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
				var frameSize = Frame.Size;
				SizeF newFrameSize = new SizeF (frameSize.Width, otherHeight);
				otherHeight = frameSize.Height;
				SetFrameSize (newFrameSize, animate);
			}
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

