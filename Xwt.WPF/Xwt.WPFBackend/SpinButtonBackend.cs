// 
// SpinButtonBackend.cs
//  
// Author:
//       David Karlaš <david.karlas@gmail.com>
// 
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
using Xwt.Backends;

using System.Collections.Generic;

namespace Xwt.WPFBackend
{
    public class SpinButtonBackend
        : WidgetBackend, ISpinButtonBackend
    {
        public SpinButtonBackend()
        {
            Widget = new WindowsSpinButton();
        }

        public bool Wrap
        {
            get
            {
                return SpinButton.Wrap;
            }
            set
            {
                SpinButton.Wrap = value;
            }
        }

        public double Value
        {
            get
            {
                return SpinButton.Value;
            }
            set
            {
                SpinButton.Value = value;
            }
        }

        public void SetButtonStyle(Xwt.ButtonStyle buttonStyle)
        {
            //TODO: impelement SetButtonStyle in WpfSpinButton
        }

        public double MinimumValue
        {
            get
            {
                return SpinButton.MinimumValue;
            }
            set
            {
                SpinButton.MinimumValue = value;
            }
        }

        public double MaximumValue
        {
            get
            {
                return SpinButton.MaximumValue;
            }
            set
            {
                SpinButton.MaximumValue = value;
            }
        }

        public bool IsIndeterminate
        {
            get
            {
                return SpinButton.IsIndeterminate;
            }
            set
            {
                SpinButton.IsIndeterminate = value;
            }
        }

        public string IndeterminateMessage
        {
            get
            {
                return SpinButton.IndeterminateMessage;
            }
            set
            {
                SpinButton.IndeterminateMessage = value;
            }
        }

        public double IncrementValue
        {
            get
            {
                return SpinButton.Increment;
            }
            set
            {
                SpinButton.Increment = value;
            }
        }

        public int Digits
        {
            get
            {
                return SpinButton.DecimalPlaces;
            }
            set
            {
                SpinButton.DecimalPlaces = value;
            }
        }

        public double ClimbRate
        {
            get
            {
                return SpinButton.ClimbRate;
            }
            set
            {
                SpinButton.ClimbRate = value;
            }
        }


        public override void EnableEvent(object eventId)
        {
            base.EnableEvent(eventId);
            if (eventId is SpinButtonEvent)
            {
                switch ((SpinButtonEvent)eventId)
                {
                    case SpinButtonEvent.ValueChanged:
                        SpinButton.OnValueChanged += SpinButton_OnValueChanged;
                        break;
                }
            }
        }


        public override void DisableEvent(object eventId)
        {
            base.DisableEvent(eventId);
            if (eventId is SpinButtonEvent)
            {
                switch ((SpinButtonEvent)eventId)
                {
                    case SpinButtonEvent.ValueChanged:
                        SpinButton.OnValueChanged -= SpinButton_OnValueChanged;
                        break;
                }
            }
        }

        void SpinButton_OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Context.InvokeUserCode(SpinButtonEventSink.ValueChanged);
        }

        protected ISpinButtonEventSink SpinButtonEventSink
        {
            get { return (ISpinButtonEventSink)EventSink; }
        }

        protected WindowsSpinButton SpinButton
        {
            get { return (WindowsSpinButton)Widget; }
        }
    }
}
