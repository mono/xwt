// 
// ToggleButtonBackend.cs
//  
// Author:
//	   Eric Maupin <ermau@xamarin.com>
// 
// Copyright (c) 2012 Xamarin, Inc.
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

using System.Windows;
using Xwt.Backends;
using SWC = System.Windows.Controls;
using SWCP = System.Windows.Controls.Primitives;

namespace Xwt.WPFBackend
{
	public class ToggleButtonBackend
		: ButtonBackend, IToggleButtonBackend
	{
		public ToggleButtonBackend()
			:base (new SWCP.ToggleButton())
		{
		}

		public bool Active
		{
			get { return ToggleButton.IsChecked.HasValue && ToggleButton.IsChecked.Value; }
			set { ToggleButton.IsChecked = value; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);

			if (eventId is ToggleButtonEvent)
			{
				switch ((ToggleButtonEvent)eventId)
				{
					case ToggleButtonEvent.Toggled:
						ToggleButton.Checked += OnButtonToggled;
						ToggleButton.Unchecked += OnButtonToggled;
						break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);

			if (eventId is ToggleButtonEvent)
			{
				switch ((ToggleButtonEvent)eventId)
				{
					case ToggleButtonEvent.Toggled:
						ToggleButton.Checked -= OnButtonToggled;
						ToggleButton.Unchecked -= OnButtonToggled;
						break;
				}
			}
		}

		protected new IToggleButtonEventSink EventSink
		{
			get { return (IToggleButtonEventSink) base.EventSink; }
		}

		protected SWCP.ToggleButton ToggleButton
		{
			get { return (SWCP.ToggleButton) NativeWidget; }
		}

		private void OnButtonToggled (object s, RoutedEventArgs e)
		{
			Context.InvokeUserCode (EventSink.OnToggled);
		}
	}
}
