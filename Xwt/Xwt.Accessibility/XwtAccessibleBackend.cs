//
// XwtAccessibleBackend.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2019 (c) Microsoft Corporation
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
using System.Collections.Generic;
using Xwt.Backends;

namespace Xwt.Accessibility
{
	public class XwtAccessibleBackend : IAccessibleBackend
	{
		protected Widget widget;
		protected XwtWidgetBackend widgetBackend;
		private IAccessibleBackend nativeBackend;
		IAccessibleEventSink eventSink;
		ApplicationContext context;

		public XwtAccessibleBackend ()
		{
		}

		public void Initialize (IAccessibleEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public void Initialize (IWidgetBackend parentWidget, IAccessibleEventSink eventSink)
		{
			widgetBackend = parentWidget as XwtWidgetBackend;
			Initialize (widgetBackend?.Frontend, eventSink);
		}

		public void Initialize (IPopoverBackend parentPopover, IAccessibleEventSink eventSink)
		{
		}

		public void Initialize (IMenuBackend parentMenu, IAccessibleEventSink eventSync)
		{
		}

		public void Initialize (IMenuItemBackend parentMenuItem, IAccessibleEventSink eventSink)
		{
		}

		public void Initialize (object parentWidget, IAccessibleEventSink eventSink)
		{
			this.eventSink = eventSink;
			widget = parentWidget as Widget;
			if (widgetBackend == null)
				widgetBackend = widget?.GetBackend () as XwtWidgetBackend;
			if (widgetBackend == null)
				throw new ArgumentException ("The widget is not a custom Xwt.Widget", nameof(parentWidget));
			nativeBackend = context.Toolkit.Backend.CreateBackendForFrontend (typeof(Accessible)) as IAccessibleBackend;
			nativeBackend.Initialize (widgetBackend.Content.Surface.NativeWidget, eventSink);
		}

		public void InitializeBackend(object frontend, ApplicationContext context)
		{
			this.context = context;
		}

		public Rectangle Bounds
		{
			get {
				return nativeBackend.Bounds;
			}
			set {
				nativeBackend.Bounds = value;
			}
		}

		public string Description
		{
			get {
				return nativeBackend.Description;
			}
			set {
				nativeBackend.Description = value;
			}
		}

		public virtual string Label
		{
			get {
				return nativeBackend.Label;
			}
			set {
				nativeBackend.Label = value;
			}
		}

		public string Identifier
		{
			get {
				return nativeBackend.Identifier;
			}
			set {
				nativeBackend.Identifier = value;
			}
		}

		public Role Role
		{
			get {
				return nativeBackend.Role;
			}
			set {
				nativeBackend.Role = value;
			}
		}

		public string RoleDescription
		{
			get {
				return nativeBackend.RoleDescription;
			}
			set {
				nativeBackend.RoleDescription = value;
			}
		}

		public string Title
		{
			get {
				return nativeBackend.Title;
			}
			set {
				nativeBackend.Title = value;
			}
		}

		public string Value
		{
			get {
				return nativeBackend.Value;
			}
			set {
				nativeBackend.Value = value;
			}
		}

		public Widget LabelWidget
		{
			set {
				nativeBackend.LabelWidget = value;
			}
		}

		public virtual Uri Uri
		{
			get {
				return nativeBackend.Uri;
			}
			set {
				nativeBackend.Uri = value;
			}
		}

		public bool IsAccessible
		{
			get {
				return nativeBackend.IsAccessible;
			}
			set {
				nativeBackend.IsAccessible = value;
			}
		}

		public virtual void AddChild (object nativeChild)
		{
			nativeBackend.AddChild (nativeChild);
		}

		public virtual void RemoveChild (object nativeChild)
		{
			nativeBackend.RemoveChild (nativeChild);
		}

		public virtual void RemoveAllChildren ()
		{
			nativeBackend.RemoveAllChildren ();
		}

		public void DisableEvent (object eventId)
		{
			nativeBackend.DisableEvent (eventId);
		}

		public void EnableEvent (object eventId)
		{
			nativeBackend.EnableEvent (eventId);
		}

		public IEnumerable<object> GetChildren()
		{
			return nativeBackend.GetChildren ();
		}
	}
}
