// 
// PasswordEntryBackend.cs
//  
// Author:
//       Henrique Esteves <henriquemotaesteves@gmail.com>
// 
// Copyright (c) 2012 Henrique Esteves
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;

namespace Xwt.WPFBackend
{
	public class PasswordEntryBackend
		: WidgetBackend, IPasswordEntryBackend
	{
		public PasswordEntryBackend()
		{
			Widget = new PasswordBox();
		}

		protected override double DefaultNaturalWidth
		{
			get { return -1; }
		}

		public virtual string Password
		{
			get { return PasswordBox.Password; }
			set { PasswordBox.Password = value; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);

			if (eventId is PasswordEntryEvent)
			{
				switch ((PasswordEntryEvent)eventId)
				{
					// TODO: Should we ignore this for placeholder changes?
					case PasswordEntryEvent.Changed:
						PasswordBox.PasswordChanged += OnPasswordChanged;
						break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);

			if (eventId is PasswordEntryEvent)
			{
				switch ((PasswordEntryEvent)eventId)
				{
					case PasswordEntryEvent.Changed:
						PasswordBox.PasswordChanged -= OnPasswordChanged;
						break;
				}
			}
		}

		protected PasswordBox PasswordBox
		{
			get { return (PasswordBox)Widget; }
		}

		protected new IPasswordEntryEventSink EventSink {
			get { return (IPasswordEntryEventSink)base.EventSink; }
		}

		private void OnPasswordChanged(object s, RoutedEventArgs e)
		{
			Context.InvokeUserCode (EventSink.OnChanged);
		}
	}
}