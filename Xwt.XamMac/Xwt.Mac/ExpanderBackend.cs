using System;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xwt.Backends;

namespace Xwt.Mac
{
	class ExpanderBackend : ViewBackend<MacExpander, IExpandEventSink>, IExpanderBackend
	{
		ViewBackend child;

		public ExpanderBackend ()
		{
		}

		public override void Initialize ()
		{
			ViewObject = new MacExpander (EventSink, ApplicationContext);
			Widget.DisclosureToggled += HandleDisclosureToggled;
		}

		void HandleDisclosureToggled (object sender, EventArgs args)
		{
			ResetFittingSize ();
			NotifyPreferredSizeChanged ();
			ApplicationContext.InvokeUserCode (EventSink.ExpandChanged);
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

		public override void ReplaceChild (NSView oldChild, NSView newChild)
		{
			Widget.Box.SetContent (newChild);
		}

		protected override Size CalcFittingSize ()
		{
			var s = Widget.CollapsedSize;
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
				Widget.Expander.Font = ((FontData)value).Font;
			}
		}

		bool isDisposed;
		protected override void Dispose(bool disposing)
		{
			if (!isDisposed) {
				isDisposed = true;
				if (disposing && Widget != null) {
					Widget.DisclosureToggled -= HandleDisclosureToggled;
				}
			}
			base.Dispose(disposing);
		}
	}

	class MacExpander: WidgetView
	{
		public event EventHandler DisclosureToggled;

		public ExpanderWidget Expander { get; private set; }

		public CollapsibleBox Box { get; private set; }

		public MacExpander (IWidgetEventSink eventSink, ApplicationContext context): base (eventSink, context)
		{
			Expander = new ExpanderWidget {
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				Target = this,
			};
			Box = new CollapsibleBox { AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable };

			AddSubview (Expander);
			AddSubview (Box);
		}

		public Size CollapsedSize {
			get {
				return Expander.FittingSize.ToXwtSize ();
			}
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public override void SetFrameSize (CGSize newSize)
		{
			base.SetFrameSize (newSize);
			var expanderSize = Expander.FittingSize;
			Expander.Frame = new CGRect(CGPoint.Empty, new CGSize(newSize.Width, expanderSize.Height));
			Box.Frame = new CGRect(0, expanderSize.Height, newSize.Width, Math.Max(0, newSize.Height - expanderSize.Height));
			Box.UpdateContentSize (false);
		}

		public override bool RespondsToSelector(Selector sel)
		{
			if (sel?.Name == "sizeToFit")
				return false;
			return base.RespondsToSelector(sel);
		}

		[Export ("onClicked:")]
		void OnDisclosureToggled (NSObject target)
		{
			Box.Expanded = Expander.On;
			DisclosureToggled?.Invoke (this, EventArgs.Empty);
		}

		bool isDisposed;
		protected override void Dispose (bool disposing)
		{
			if (!isDisposed)
			{
				isDisposed = true;
				if (disposing) {
					Expander?.RemoveFromSuperviewWithoutNeedingDisplay ();
					Expander?.Dispose ();
					Box?.RemoveFromSuperviewWithoutNeedingDisplay ();
					Box?.Dispose ();
				}
				Expander = null;
				Box = null;
			}
			base.Dispose (disposing);
		}
	}

	sealed class ExpanderWidget : NSView
	{
		NSTextField label;
		NSButton disclosure;

		public ExpanderWidget ()
		{
			disclosure = new NSButton {
				BezelStyle = NSBezelStyle.Disclosure,
				AutoresizingMask = NSViewResizingMask.MaxYMargin,
				ImagePosition = NSCellImagePosition.ImageOnly,
				State = NSCellStateValue.Off,
				Action = new Selector("onClicked:"),
			};
			disclosure.SetButtonType (NSButtonType.PushOnPushOff);

			label = new NSTextField {
				Cell = new CustomTextFieldCell (),
				Editable = false,
				Bezeled = false,
				DrawsBackground = false,
				Alignment = NSTextAlignment.Left,
			};

			AutoresizesSubviews = true;

			AddSubview (label);
			AddSubview (disclosure);
		}

		public NSObject Target
		{
			get { return disclosure.Target; }
			set { disclosure.Target = value; }
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
				return label.StringValue;
			}
			set {
				label.StringValue = value;
				disclosure.AccessibilityTitle = value;
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

		public override CGSize FittingSize
		{
			get
			{
				return disclosure.FittingSize + label.FittingSize;
			}
		}

		public override void SetFrameSize(CGSize newSize)
		{
			disclosure.Frame = new CGRect(CGPoint.Empty, new CGSize(disclosure.FittingSize.Width, newSize.Height));
			label.Frame = new CGRect(new CGPoint(disclosure.FittingSize.Width, 0), new CGSize(Math.Max(0, newSize.Width - disclosure.FittingSize.Width), newSize.Height));
			base.SetFrameSize(newSize);
		}

		public override void MouseDown(NSEvent theEvent)
		{
			CGPoint p = label.ConvertPointFromEvent(theEvent);
			if (label.Bounds.Contains(p))
			{
				disclosure.Highlighted = true;
			}
			base.MouseDown(theEvent);
		}

		public override void MouseUp(NSEvent theEvent)
		{
			CGPoint p = label.ConvertPointFromEvent(theEvent);
			if (label.Bounds.Contains(p))
			{
				disclosure.PerformClick(this);
			}
			base.MouseUp(theEvent);
		}

		public override void MouseExited(NSEvent theEvent)
		{
			disclosure.Highlighted = false;
			base.MouseExited(theEvent);
		}

		bool isDisposed;
		protected override void Dispose (bool disposing)
		{
			if (!isDisposed)
			{
				isDisposed = true;
				if (disposing) {
					label?.RemoveFromSuperviewWithoutNeedingDisplay ();
					label?.Dispose ();
					disclosure?.RemoveFromSuperviewWithoutNeedingDisplay ();
					disclosure?.Dispose ();
				}
				label = null;
				disclosure = null;
			}
			base.Dispose (disposing);
		}
	}

	public class CollapsibleBox : NSBox
	{
		internal const float DefaultCollapsedHeight = 1f;
		bool expanded;

		public CollapsibleBox ()
		{
			expanded = false;
			TitlePosition = NSTitlePosition.NoTitle;
			Transparent = true;
			BoxType = NSBoxType.NSBoxPrimary;
			ContentViewMargins = new CGSize (0, 0);
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
				var vo = ContentView as IViewObject;
				if (vo != null && vo.Backend != null) {
					var s = vo.Backend.Frontend.Surface.GetPreferredSize ((float)Frame.Size.Width, SizeConstraint.Unconstrained, true);
					SetFrameSize (new CGSize (Frame.Width, (float)s.Height), animate);
				}
			} else
				SetFrameSize (new CGSize (Frame.Width, DefaultCollapsedHeight), animate);
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		CGRect FrameForNewSizePinnedToTopLeft (CGSize newFrameSize)
		{
			var frame = Frame;
			frame.Size = newFrameSize;
			return frame;
		}

		public void SetFrameSize (CGSize newFrameSize, bool animating)
		{
			CGRect newFrame = FrameForNewSizePinnedToTopLeft (newFrameSize);
			if (animating) {
				NSAnimation animation = new NSViewAnimation (new [] {
					NSDictionary.FromObjectsAndKeys (
						new object[] { this, NSValue.FromCGRect (Frame), NSValue.FromCGRect (newFrame) },
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

