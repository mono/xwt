﻿//
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
	public sealed class Accessible : IFrontend
	{
		readonly XwtComponent parentComponent;
		readonly object parentNativeObject;

		readonly AccessibleBackendHost backendHost;

		class AccessibleBackendHost : BackendHost<Accessible, IAccessibleBackend>, IAccessibleEventSink
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultNoOpAccessibleBackend ();
				return b;
			}
			
			protected override void OnBackendCreated ()
			{
				var parentBackend = Parent.parentComponent?.GetBackend () as IWidgetBackend;
				if (parentBackend != null)
					Backend.Initialize (parentBackend, this);
				else
					Backend.Initialize (Parent.parentNativeObject, this);
			}

			public bool OnPress ()
			{
				return Parent.OnPress ();
			}
		}

		internal Accessible (Widget parent)
		{
			if (parent == null)
				throw new ArgumentNullException (nameof (parent));
			parentComponent = parent;
			backendHost = new AccessibleBackendHost ();
			backendHost.Parent = this;
		}

		internal Accessible (object nativeParent)
		{
			if (nativeParent == null)
				throw new ArgumentNullException (nameof (nativeParent));
			parentNativeObject = nativeParent;
			backendHost = new AccessibleBackendHost ();
			backendHost.Parent = this;
		}

		IAccessibleBackend Backend {
			get { return backendHost.Backend; }
		}

		object IFrontend.Backend {
			get {
				return Backend;
			}
		}

		Toolkit IFrontend.ToolkitEngine {
			get { return backendHost.ToolkitEngine; }
		}

		public bool IsAccessible {
			get {
				return Backend.IsAccessible;
			}
			set {
				Backend.IsAccessible = value;
			}
		}

		public string Identifier {
			get {
				return Backend.Identifier;
			}
			set {
				Backend.Identifier = value;
			}
		}

		public string Label {
			get {
				return Backend.Label;
			}
			set {
				Backend.Label = value;
			}
		}

		public string Title {
			get {
				return Backend.Title;
			}
			set {
				Backend.Title = value;
			}
		}

		public string Description {
			get {
				return Backend.Description;
			}
			set {
				Backend.Description = value;
			}
		}

		public string Value {
			get {
				return Backend.Value;
			}
			set {
				Backend.Value = value;
			}
		}

		public Uri Uri {
			get {
				return Backend.Uri;
			}
			set {
				Backend.Uri = value;
			}
		}

		public Rectangle Bounds {
			get {
				return Backend.Bounds;
			}
			set {
				Backend.Bounds = value;
			}
		}

		public Role Role {
			get {
				return Backend.Role;
			}
			set {
				Backend.Role = value;
			}
		}

		public string RoleDescription {
			get {
				return Backend.RoleDescription;
			}
			set {
				Backend.RoleDescription = value;
			}
		}

		bool OnPress ()
		{
			var args = new WidgetEventArgs ();
			press?.Invoke (this, args);
			return args.Handled;
		}

		event EventHandler<WidgetEventArgs> press;
		public event EventHandler<WidgetEventArgs> Press {
			add {
				backendHost.OnBeforeEventAdd (AccessibleEvent.Press, press);
				press += value;
			}
			remove {
				press -= value;
				backendHost.OnAfterEventRemove (AccessibleEvent.Press, press);
			}
		}
	}

	class DefaultNoOpAccessibleBackend : IAccessibleBackend
	{
		public Rectangle Bounds { get; set; }

		public string Description { get; set; }

		public string Identifier { get; set; }

		public string Label { get; set; }

		public Role Role { get; set; }

		public string RoleDescription { get; set; }

		public string Title { get; set; }

		public string Value { get; set; }

		public Uri Uri { get; set; }

		public bool IsAccessible { get; set; }

		public void DisableEvent (object eventId)
		{
		}

		public void EnableEvent (object eventId)
		{
		}

		public void Initialize (IWidgetBackend parentWidget, IAccessibleEventSink eventSink)
		{
		}

		public void Initialize (object parentWidget, IAccessibleEventSink eventSink)
		{
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
		}

		public void AddChild (object nativeAccessible)
		{
		}

		public void RemoveChild (object nativeAccessible)
		{
		}

		public void RemoveAllChildren ()
		{
		}
	}
}
