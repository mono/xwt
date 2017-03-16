//
// Accessible.cs
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
using Xwt.Backends;

namespace Xwt.Accessibility
{
	[BackendType (typeof (IAccessibleBackend))]
	public sealed class Accessible : IAccessibleEventSink
	{
		readonly IAccessibleBackend accessibleBackend;

		internal Accessible (Widget owner, IAccessibleBackend backend)
		{
			accessibleBackend = backend ?? new DefaultNoOpAccessibleBackend ();
			accessibleBackend.InitializeBackend (owner, owner.Surface.ToolkitEngine.Context);
			accessibleBackend.InitializeAccessible (this);
		}

		public bool IsAccessible {
			get {
				return accessibleBackend.IsAccessible;
			}
			set {
				accessibleBackend.IsAccessible = value;
			}
		}

		public string Label {
			get {
				return accessibleBackend.AccessibleLabel;
			}
			set {
				accessibleBackend.AccessibleLabel = value;
			}
		}

		public string Title {
			get {
				return accessibleBackend.AccessibleTitle;
			}
			set {
				accessibleBackend.AccessibleTitle = value;
			}
		}

		public string Description {
			get {
				return accessibleBackend.AccessibleDescription;
			}
			set {
				accessibleBackend.AccessibleDescription = value;
			}
		}

		public string Value {
			get {
				return accessibleBackend.AccessibleValue;
			}
			set {
				accessibleBackend.AccessibleValue = value;
			}
		}

		public Rectangle Bounds {
			get {
				return accessibleBackend.AccessibleBounds;
			}
			set {
				accessibleBackend.AccessibleBounds = value;
			}
		}

		public Role Role {
			get {
				return accessibleBackend.AccessibleRole;
			}
			set {
				accessibleBackend.AccessibleRole = value;
			}
		}

		public string RoleDescription {
			get {
				return accessibleBackend.AccessibleRoleDescription;
			}
			set {
				accessibleBackend.AccessibleRoleDescription = value;
			}
		}

		bool IAccessibleEventSink.OnAccessiblePress ()
		{
			var args = new WidgetEventArgs ();
			press?.Invoke (this, args);
			return args.Handled;
		}

		event EventHandler<WidgetEventArgs> press;
		public event EventHandler<WidgetEventArgs> Press {
			add {
				if (press == null)
					accessibleBackend?.EnableEvent (AccessibleEvent.Press);
				press += value;
			}
			remove {
				press -= value;
				if (press == null)
					accessibleBackend?.DisableEvent (AccessibleEvent.Press);
			}
		}
	}

	class DefaultNoOpAccessibleBackend : IAccessibleBackend
	{
		public Rectangle AccessibleBounds { get; set; }

		public string AccessibleDescription { get; set; }

		public string AccessibleLabel { get; set; }

		public Role AccessibleRole { get; set; }

		public string AccessibleRoleDescription { get; set; }

		public string AccessibleTitle { get; set; }

		public string AccessibleValue { get; set; }

		public bool IsAccessible { get; set; }

		public void DisableEvent (object eventId)
		{
		}

		public void EnableEvent (object eventId)
		{
		}

		public void InitializeAccessible (IAccessibleEventSink eventSink)
		{
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}
	}
}
