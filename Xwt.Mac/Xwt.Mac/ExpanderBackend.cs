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
		public ExpanderBackend ()
		{
			SetMinSize (10, 25);
		}

		public override void Initialize ()
		{
			ViewObject = new MacExpander (EventSink, ApplicationContext);
		}

		public string Label {
			get {
				return Widget.Title;
			}
			set {
				Widget.Title = value;
			}
		}

		public bool Expanded {
			get {
				return Widget.Expanded;
			}
			set {
				Widget.Expanded = value;
			}
		}

		public override object Font {
			get {
				return Widget.Font;
			}
			set {
				Widget.Font = (NSFont)value;
			}
		}

		public void SetContent (IWidgetBackend widget)
		{
			Widget.SetContent (GetWidget (widget));
		}
	}

	class MacExpander: WidgetView
	{
		ExpanderWidget expander;
		NSView content;

		public string Title {
			get { return expander.Title; }
			set { expander.Title = value; }
		}

		public NSFont Font {
			get { return expander.Font; }
			set { expander.Font = value; }
		}

		public bool Expanded {
			get { return expander.On; }
			set { expander.On = value; }
		}

		public MacExpander (IExpandEventSink eventSink, ApplicationContext context): base (eventSink, context)
		{
			expander = new ExpanderWidget {
				Frame = new RectangleF (0, 0, 80, 21),
				AutoresizingMask = NSViewResizingMask.WidthSizable
			};
			expander.DisclosureToggled += delegate {
				Update ();
				context.InvokeUserCode (delegate {
					eventSink.OnPreferredSizeChanged ();
					eventSink.ExpandChanged ();
				});
			};

			AutoresizesSubviews = true;
			AddSubview (expander);
			Update ();
		}

		public void SetContent (NSView widget)
		{
			if (content == widget)
				return;
			if (content != null) {
				content.RemoveFromSuperview ();
				content.AutoresizingMask = NSViewResizingMask.NotSizable;
				content = null;
			}
			if (widget == null)
				return;

			content = widget;
			content.SetFrameOrigin (new PointF (0, expander.Frame.Height));
			AddSubview (content);

			Update ();
			NeedsDisplay = true;
		}

		void Update ()
		{
			SizeF contentSize = (content != null) ? content.Frame.Size : SizeF.Empty;
			SizeF newSize = new SizeF ((float)Math.Max (expander.Frame.Width, contentSize.Width), expander.Frame.Height);

			if (content != null) {
				if (Expanded) {
					newSize.Height += contentSize.Height;
					contentSize.Width = newSize.Width;
					content.SetFrameSize (contentSize);

					content.Hidden = false;
					content.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
				} else {
					content.Hidden = true;
					content.AutoresizingMask = NSViewResizingMask.NotSizable;
				}
			}
			SetFrameSize (newSize);
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

		public string Title {
			get {
				return label.Title;
			}
			set {
				label.Title = value;
			}
		}

		public NSFont Font {
			get {
				return label.Font;
			}
			set {
				label.Font = value;
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
}

