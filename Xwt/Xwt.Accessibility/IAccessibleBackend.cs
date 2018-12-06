//
// AccessibleBackendHandler.cs
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
using System.Collections.Generic;
using Xwt.Accessibility;

namespace Xwt.Backends
{
	public interface IAccessibleBackend : IBackend
	{
		void Initialize (IWidgetBackend parentWidget, IAccessibleEventSink eventSink);

		void Initialize (IPopoverBackend parentPopover, IAccessibleEventSink eventSink);

		void Initialize (IMenuBackend parentMenu, IAccessibleEventSink eventSink);
		void Initialize (IMenuItemBackend parentMenuItem, IAccessibleEventSink eventSink);

		void Initialize (object parentWidget, IAccessibleEventSink eventSink);

		bool IsAccessible { get; set; }

		string Identifier { get; set; }

		string Label { get; set; }

		Widget LabelWidget { set; }

		string Title { get; set; }

		string Description { get; set; }

		string Value { get; set; }

		Uri Uri { get; set; }

		Rectangle Bounds { get; set; }

		Role Role { get; set; }

		string RoleDescription { get; set; }

		void AddChild (object nativeChild);
		void RemoveChild (object nativeChild);
		void RemoveAllChildren ();
		IEnumerable<object> GetChildren ();
	}

	public interface IAccessibleEventSink
	{
		bool OnPress ();
		//void OnPerformAccessibleAction (AccessibleActionEventArgs args);
	}

	public enum AccessibleEvent
	{
		Press,
		//PerformAction,
	}
}
