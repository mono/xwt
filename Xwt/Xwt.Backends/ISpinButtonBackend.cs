//
// ISpinButtonBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
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

using System;

namespace Xwt.Backends
{
	public interface ISpinButtonBackend : IWidgetBackend
	{
		double ClimbRate { get; set; }
		int Digits { get; set; }
		double Value { get; set; }
		string Text { get; set; }
		bool Wrap { get; set; }
		double MinimumValue { get; set; }
		double MaximumValue { get; set; }
		double IncrementValue { get; set; }
		void SetButtonStyle (ButtonStyle style);
		string IndeterminateMessage { get; set; }
		bool IsIndeterminate { get; set; }
	}

	public interface ISpinButtonEventSink: IWidgetEventSink
	{
		void ValueChanged ();
		void ValueInput (SpinButtonInputEventArgs intputArgs);
		void ValueOutput (WidgetEventArgs args);
	}
	
	public enum SpinButtonEvent
	{
		ValueChanged,
		ValueInput,
		ValueOutput
	}
}

