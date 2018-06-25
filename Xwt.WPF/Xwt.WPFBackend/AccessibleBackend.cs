using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xwt.Accessibility;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	class AccessibleBackend : IAccessibleBackend
	{
		UIElement element;
		IAccessibleEventSink eventSink;
		ApplicationContext context;

		public bool IsAccessible { get; set; }

		private string identifier;
		public string Identifier {
			get { return AutomationProperties.GetAutomationId (element); }
			set {
				identifier = value;
				AutomationProperties.SetAutomationId (element, value);
			}
		}

		private string label;
		public string Label {
			get { return AutomationProperties.GetName (element); }
			set {
				label = value;
				AutomationProperties.SetName (element, value);
			}
		}

		private string description;
		public string Description {
			get { return AutomationProperties.GetHelpText (element); }
			set {
				description = value;
				AutomationProperties.SetHelpText (element, value);
			}
		}

		private Widget labelWidget;
		public Widget LabelWidget {
			set {
				labelWidget = value;
				AutomationProperties.SetLabeledBy (element, (Toolkit.GetBackend (value) as WidgetBackend)?.Widget);
			}
		}

		/// <summary>
		/// In some cases (just Popovers currently) we need to wait to set the automation properties until the element
		/// that needs them comes into existence (a PopoverRoot in the case of a Popover, when it's shown).  This is
		/// used for that delayed initialization.
		/// </summary>
		/// <param name="element">UIElement on which to set the properties</param>
		public void InitAutomationProperties (UIElement element)
		{
			if (identifier != null)
				AutomationProperties.SetAutomationId (element, identifier);
			if (label != null)
				AutomationProperties.SetName (element, label);
			if (description != null)
				AutomationProperties.SetHelpText (element, description);
			if (labelWidget != null)
				AutomationProperties.SetLabeledBy (element, (Toolkit.GetBackend (labelWidget) as WidgetBackend)?.Widget);
		}

		public string Title { get; set; }
		public string Value { get; set; }
		public Role Role { get; set; } = Role.Custom;
		public Uri Uri { get; set; }
		public Rectangle Bounds { get; set; }
		public string RoleDescription { get; set; }

		public void DisableEvent (object eventId)
		{
		}

		public void EnableEvent (object eventId)
		{
		}

		public void Initialize (IWidgetBackend parentWidget, IAccessibleEventSink eventSink)
		{
			Initialize (parentWidget.NativeWidget, eventSink);
			var wpfBackend = parentWidget as WidgetBackend;
			if (wpfBackend != null)
				wpfBackend.HasAccessibleObject = true;
		}

		public void Initialize (IPopoverBackend parentPopover, IAccessibleEventSink eventSink)
		{
			var popoverBackend = (PopoverBackend) parentPopover;
			Popup popup = popoverBackend.NativeWidget;
			Initialize (popup, eventSink);
		}

		public void Initialize (object parentWidget, IAccessibleEventSink eventSink)
		{
			this.element = parentWidget as UIElement;
			if (element == null)
				throw new ArgumentException ("Widget is not a UIElement");
			this.eventSink = eventSink;
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			this.context = context;
		}

		internal void PerformInvoke ()
		{
			context.InvokeUserCode (() => eventSink.OnPress ());
		}

		// The following child methods are only supported for Canvas based widgets
		public void AddChild (object nativeChild)
		{
			var peer = nativeChild as AutomationPeer;
			var canvas = element as CustomCanvas;
			if (peer != null && canvas != null)
				canvas.AutomationPeer?.AddChild (peer);
		}

		public void RemoveAllChildren ()
		{
			var canvas = element as CustomCanvas;
			if (canvas != null)
				canvas.AutomationPeer?.RemoveAllChildren ();
		}

		public void RemoveChild (object nativeChild)
		{
			var peer = nativeChild as AutomationPeer;
			var canvas = element as CustomCanvas;
			if (peer != null && canvas != null)
				canvas.AutomationPeer?.RemoveChild (peer);
		}

		public static AutomationControlType RoleToControlType (Role role)
		{
			switch (role) {
			case Role.Button:
			case Role.MenuButton:
			case Role.ToggleButton:
				return AutomationControlType.Button;
			case Role.CheckBox:
				return AutomationControlType.CheckBox;
			case Role.RadioButton:
				return AutomationControlType.RadioButton;
			case Role.RadioGroup:
				return AutomationControlType.Group;
			case Role.ComboBox:
				return AutomationControlType.ComboBox;
			case Role.List:
				return AutomationControlType.List;
			case Role.Popup:
			case Role.ToolTip:
				return AutomationControlType.ToolTip;
			case Role.ToolBar:
				return AutomationControlType.ToolBar;
			case Role.Label:
				return AutomationControlType.Text;
			case Role.Link:
				return AutomationControlType.Hyperlink;
			case Role.Image:
				return AutomationControlType.Image;
			case Role.Cell:
				return AutomationControlType.DataItem;
			case Role.Table:
				return AutomationControlType.DataGrid;
			case Role.Paned:
				return AutomationControlType.Pane;
			default:
				return AutomationControlType.Custom;
			}
		}
	}
}
