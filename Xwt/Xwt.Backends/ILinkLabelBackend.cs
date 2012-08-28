//
// ILinkLabelBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.

using System;

namespace Xwt.Backends
{
	public interface ILinkLabelBackend : ILabelBackend
	{
		Uri Uri { get; set; }
	}

	public interface ILinkLabelEventSink : IWidgetEventSink
	{
		void OnClicked ();
	}

	public enum LinkLabelEvent
	{
		Clicked = 1
	}
}

