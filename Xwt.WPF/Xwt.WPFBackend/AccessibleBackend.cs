using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Xwt.Accessibility;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
	class AccessibleBackend : IAccessibleBackend
	{
		UIElement element;

		public bool IsAccessible { get; set; }

		public string Identifier {
			get { return AutomationProperties.GetAutomationId (element); }
			set { AutomationProperties.SetAutomationId (element, value); }
		}
		public string Label {
			get { return AutomationProperties.GetName (element); }
			set { AutomationProperties.SetName (element, value); }
		}
		public string Description {
			get { return AutomationProperties.GetHelpText (element); }
			set { AutomationProperties.SetHelpText (element, value); }
		}

		public string Title { get; set; }
		public string Value { get; set; }
		public Role Role { get; set; }
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
			var parent = parentWidget as WidgetBackend;
			Initialize (parent.Widget, eventSink);
		}

		public void Initialize (object parentWidget, IAccessibleEventSink eventSink)
		{
			this.element = parentWidget as UIElement;
			if (element == null)
				throw new ArgumentException ("Widget is not a UIElement");
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}

		public void AddChild (object nativeChild)
		{
		}

		public void RemoveAllChildren ()
		{
		}

		public void RemoveChild (object nativeChild)
		{
		}
	}
}
