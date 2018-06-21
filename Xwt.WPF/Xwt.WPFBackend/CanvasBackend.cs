
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.Backends;
using System.Windows;
using SWC = System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Xwt.WPFBackend
{
	class CanvasBackend
		: WidgetBackend, ICanvasBackend
	{
		#region ICanvasBackend Members

		public CanvasBackend ()
		{
			Canvas = new CustomCanvas (this);
			Canvas.RenderAction = Render;
		}

		private CustomCanvas Canvas
		{
			get { return (CustomCanvas)Widget; }
			set { Widget = value; }
		}

		private ICanvasEventSink CanvasEventSink
		{
			get { return (ICanvasEventSink) EventSink; }
		}

		protected override void SetWidgetColor (Drawing.Color value)
		{
			// Do nothing
		}

		private void Render (System.Windows.Media.DrawingContext dc)
		{
			if (BackgroundColorSet) {
				SolidColorBrush mySolidColorBrush = new SolidColorBrush ();
				mySolidColorBrush.Color = BackgroundColor.ToWpfColor ();
				Rect myRect = new Rect (0, 0, Widget.ActualWidth, Widget.ActualHeight);
				dc.DrawRectangle (mySolidColorBrush, null, myRect);
			}
			
			var ctx = new Xwt.WPFBackend.DrawingContext (dc, Widget.GetScaleFactor ());
            ctx.Context.PushClip(new RectangleGeometry(new Rect(0, 0, Widget.ActualWidth, Widget.ActualHeight)));
            CanvasEventSink.OnDraw(ctx, new Rectangle(0, 0, Widget.ActualWidth, Widget.ActualHeight));
		}

		public void QueueDraw ()
		{
			Canvas.InvalidateVisual ();
		}

		public void QueueDraw (Rectangle rect)
		{
			Canvas.InvalidateVisual ();
		}

		public void AddChild (IWidgetBackend widget, Rectangle bounds)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException ();

			if (!Canvas.Children.Contains (element))
				Canvas.Children.Add (element);

			SetChildBounds (widget, bounds);
		}

		List<IWidgetBackend> children = new List<IWidgetBackend> ();
		List<Rectangle> childrenBounds = new List<Rectangle> ();

		public void SetChildBounds (IWidgetBackend widget, Rectangle bounds)
		{
			int i = children.IndexOf (widget);
			if (i == -1) {
				children.Add (widget);
				childrenBounds.Add (bounds);
			}
			else {
				childrenBounds[i] = bounds;
			}
			Canvas.SetAllocation (children.ToArray (), childrenBounds.ToArray ());
		}

		public void RemoveChild (IWidgetBackend widget)
		{
			UIElement element = widget.NativeWidget as UIElement;
			if (element == null)
				throw new ArgumentException ();

			Canvas.Children.Remove (element);
			int i = children.IndexOf (widget);
			if (i != -1) {
				children.RemoveAt (i);
				childrenBounds.RemoveAt (i);
			}
		}

		#endregion
	}

	class CustomCanvas : CustomPanel
	{
		CanvasBackend backend;
		CustomCanvasAutomationPeer automationPeer;

		public CustomCanvas (CanvasBackend backend)
		{
			this.backend = backend;
		}

		internal CustomCanvasAutomationPeer AutomationPeer => automationPeer ?? (automationPeer = new CustomCanvasAutomationPeer (this));

		protected override AutomationPeer OnCreateAutomationPeer () => AutomationPeer;

		internal class CustomCanvasAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
		{
			List<AutomationPeer> childrenPeers = null;

			public CustomCanvasAutomationPeer (CustomCanvas canvas) : base (canvas)
			{
			}

			CanvasBackend Backend => ((CustomCanvas)Owner)?.backend;
			Xwt.Widget Frontend => Backend?.Frontend;
			AccessibleBackend Accessible => (AccessibleBackend)Toolkit.GetBackend (Frontend.Accessible);

			protected override AutomationControlType GetAutomationControlTypeCore ()
			{
				var backend = Backend;
				if (backend == null || !backend.HasAccessibleObject)
					return AutomationControlType.Custom;
				var role = Frontend.Accessible.Role;
				return AccessibleBackend.RoleToControlType (role);
			}

			public override object GetPattern (PatternInterface patternInterface)
			{
				if (patternInterface == PatternInterface.Invoke && Backend.HasAccessibleObject)
					return this;
				return base.GetPattern (patternInterface);
			}

			public void Invoke ()
			{
				if (Backend.HasAccessibleObject)
					Accessible.PerformInvoke ();
			}

			protected override List<AutomationPeer> GetChildrenCore ()
			{
				if (childrenPeers != null)
					return childrenPeers;
				return base.GetChildrenCore ();
			}

			protected override string GetNameCore ()
			{
				return nameof (CustomCanvas);
			}

			public void AddChild (AutomationPeer peer)
			{
				if (childrenPeers == null)
					childrenPeers = new List<AutomationPeer> ();
				childrenPeers.Add (peer);
			}

			public void RemoveAllChildren ()
			{
				if (childrenPeers == null)
					return;
				childrenPeers.Clear ();
			}

			public void RemoveChild (AutomationPeer peer)
			{
				if (childrenPeers == null)
					return;
				childrenPeers.Remove (peer);
			}
		}
	}
}
