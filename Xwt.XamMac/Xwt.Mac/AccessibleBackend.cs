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
		ViewBackend backend;
		IAccessibleEventSink eventSink;
		ApplicationContext context;

		public void InitializeAccessible (IAccessibleEventSink eventSink)
		{
			this.eventSink = eventSink;
			eventProxy = backend.Widget as INSAccessibleEventSource;
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			this.context = context;
			var parent = (XwtComponent)frontend;
			backend = parent.GetBackend () as ViewBackend;
		}

		bool PerformPress ()
		{
			bool res = false;
			context.InvokeUserCode (() => {
				res = eventSink.OnAccessiblePress ();
			});
			return res;
		}

		string IAccessibleBackend.AccessibleLabel {
			get {
				return backend.Widget.AccessibilityLabel;
			}
			set {
				backend.Widget.AccessibilityLabel = value;
			}
		}

		string IAccessibleBackend.AccessibleTitle {
			get {
				return backend.Widget.AccessibilityTitle;
			}
			set {
				backend.Widget.AccessibilityTitle = value;
			}
		}

		string IAccessibleBackend.AccessibleDescription {
			get {
				return backend.Widget.AccessibilityHelp;
			}
			set {
				backend.Widget.AccessibilityHelp = value;
			}
		}

		bool IAccessibleBackend.IsAccessible {
			get {
				return backend.Widget.AccessibilityElement;
			}

			set {
				backend.Widget.AccessibilityElement = value;
			}
		}

		string IAccessibleBackend.AccessibleValue {
			get {
				return backend.Widget.AccessibilityValue.ToString ();
			}

			set {
				backend.Widget.AccessibilityValue = new NSString (value);
			}
		}

		Rectangle IAccessibleBackend.AccessibleBounds {
			get {
				return backend.Widget.AccessibilityFrame.ToXwtRect ();
			}

			set {
				backend.Widget.AccessibilityFrame = value.ToCGRect ();
			}
		}

		Role IAccessibleBackend.AccessibleRole {
			get {
				return Util.GetXwtRole (backend.Widget);
			}
			set {
				backend.Widget.AccessibilityRole = value.GetMacRole ();
				backend.Widget.AccessibilitySubrole = value.GetMacSubrole ();
			}
		}

		string IAccessibleBackend.AccessibleRoleDescription {
			get {
				return backend.Widget.AccessibilityRoleDescription;
			}
			set {
				backend.Widget.AccessibilityRoleDescription = value;
			}
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
