//
// AccessibleBackend.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corporation
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
using AppKit;
using Foundation;
using Xwt.Accessibility;
using Xwt.Backends;
namespace Xwt.Mac
{
	public interface INSAccessibleEventSource
	{
		Func<bool> PerformAccessiblePressDelegate { get; set; }
	}
	
	public class AccessibleBackend : IAccessibleBackend
	{
		INSAccessibleEventSource eventProxy;
		INSAccessibility widget;
		IAccessibleEventSink eventSink;
		ApplicationContext context;

		public void Initialize (IWidgetBackend parentWidget, IAccessibleEventSink eventSink)
		{
			var parentBackend = parentWidget as ViewBackend;
			Initialize (parentBackend?.Widget, eventSink);
		}

		public void Initialize (IPopoverBackend parentPopover, IAccessibleEventSink eventSink)
		{
			// Not currently supported
		}

		public void Initialize (object parentWidget, IAccessibleEventSink eventSink)
		{
			this.eventSink = eventSink;
			widget = parentWidget as INSAccessibility;
			if (widget == null)
				throw new ArgumentException ("The widget does not implement INSAccessibility.", nameof (parentWidget));
			eventProxy = widget as INSAccessibleEventSource;
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			this.context = context;
			if (!(frontend is Accessible))
				throw new ArgumentException ("Invalid frontend type. Expected '" + typeof (Accessible) + "' found '" + frontend.GetType () + "'");
		}

		bool PerformPress ()
		{
			bool res = false;
			context.InvokeUserCode (() => {
				res = eventSink.OnPress ();
			});
			return res;
		}

		string IAccessibleBackend.Identifier {
			get {
				return widget.AccessibilityIdentifier;
			}
			set {
				widget.AccessibilityIdentifier = value;
			}
		}

		string IAccessibleBackend.Label {
			get {
				return widget.AccessibilityLabel;
			}
			set {
				widget.AccessibilityLabel = value;
			}
		}

		string IAccessibleBackend.Title {
			get {
				return widget.AccessibilityTitle;
			}
			set {
				widget.AccessibilityTitle = value;
			}
		}

		string IAccessibleBackend.Description {
			get {
				return widget.AccessibilityHelp;
			}
			set {
				widget.AccessibilityHelp = value;
			}
		}

		bool IAccessibleBackend.IsAccessible {
			get {
				return widget.AccessibilityElement;
			}

			set {
				widget.AccessibilityElement = value;
			}
		}

		string IAccessibleBackend.Value {
			get {
				return widget.AccessibilityValue.ToString ();
			}

			set {
				widget.AccessibilityValue = new NSString (value);
			}
		}

		Uri IAccessibleBackend.Uri {
			get {
				return new Uri (widget.AccessibilityUrl.AbsoluteString);
			}

			set {
				widget.AccessibilityUrl = new NSUrl (value.AbsoluteUri);
			}
		}

		Rectangle IAccessibleBackend.Bounds {
			get {
				return widget.AccessibilityFrame.ToXwtRect ();
			}

			set {
				widget.AccessibilityFrame = value.ToCGRect ();
			}
		}

		Role IAccessibleBackend.Role {
			get {
				return Util.GetXwtRole (widget);
			}
			set {
				if (value == Role.Filler) {
					widget.AccessibilityElement = false;
				} else {
					widget.AccessibilityElement = true;
					widget.AccessibilityRole = value.GetMacRole ();
					widget.AccessibilitySubrole = value.GetMacSubrole ();
				}
			}
		}

		string IAccessibleBackend.RoleDescription {
			get {
				return widget.AccessibilityRoleDescription;
			}
			set {
				widget.AccessibilityRoleDescription = value;
			}
		}

		Widget IAccessibleBackend.LabelWidget {
			set {
				widget.AccessibilityTitleUIElement = (Toolkit.GetBackend (value) as ViewBackend)?.Widget;
			}
		}

		public void AddChild (object nativeChild)
		{
			var accessible = nativeChild as INSAccessibility;
			if (accessible == null) {
				throw new ArgumentException ($"{nativeChild.GetType ().ToString ()} - should be INSAccessibility", nameof (nativeChild));
			}

			var child = nativeChild as NSAccessibilityElement;
			var element = widget as NSAccessibilityElement;
			if (element != null && child != null) {
				element.AccessibilityAddChildElement (child);
			} else {
				var children = accessible.AccessibilityChildren;
				if (children == null || children.Length == 0) {
					accessible.AccessibilityChildren = new NSObject[] {(NSObject)accessible};
				} else {
					var length = children.Length;
					var newChildren = new NSObject[children.Length + 1];
					Array.Copy (children, newChildren, children.Length);
					accessible.AccessibilityChildren = newChildren;
				}
			}
		}

		public void RemoveChild (object nativeChild)
		{
			var accessible = widget as INSAccessibility;
			if (accessible == null) {
				return;
			}

			accessible.AccessibilityChildren = accessible.AccessibilityChildren.Where(c => c != nativeChild).ToArray ();
		}

		public void RemoveAllChildren ()
		{
			var accessible = widget as INSAccessibility;
			if (accessible == null) {
				return;
			}

			accessible.AccessibilityChildren = null;
		}

		public void EnableEvent (object eventId)
		{
			if (eventId is AccessibleEvent) {
				AccessibleEvent ev = (AccessibleEvent)eventId;
				switch (ev) {
					case AccessibleEvent.Press:
						if (eventProxy != null)
							eventProxy.PerformAccessiblePressDelegate = PerformPress;
					break;
				}
			}
		}

		public void DisableEvent (object eventId)
		{
			if (eventId is AccessibleEvent) {
				AccessibleEvent ev = (AccessibleEvent)eventId;
				switch (ev) {
					case AccessibleEvent.Press:
						if (eventProxy != null)
							eventProxy.PerformAccessiblePressDelegate = null;
					break;
				}
			}
		}
	}
}
