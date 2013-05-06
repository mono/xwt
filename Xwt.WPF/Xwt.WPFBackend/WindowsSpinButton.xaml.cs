﻿//
// WindowsSpinButton.cs
//
// Author:
//       David Karlaš <david.karlas@gmail.com>
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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Globalization;
using System.ComponentModel;

namespace Xwt.WPFBackend
{
    using Key = System.Windows.Input.Key;
    public partial class WindowsSpinButton : UserControl
    {
        #region DependencyProperty
        public static readonly DependencyProperty IncrementProperty =
             DependencyProperty.Register("Increment", typeof(double),
             typeof(WindowsSpinButton), new FrameworkPropertyMetadata(0.1));

        public double Increment
        {
            get { return (double)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        public static readonly DependencyProperty DecimalPlacesProperty =
             DependencyProperty.Register("DecimalPlaces", typeof(int),
             typeof(WindowsSpinButton), new FrameworkPropertyMetadata(1));

        public int DecimalPlaces
        {
            get { return (int)GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
             DependencyProperty.Register("Value", typeof(double),
             typeof(WindowsSpinButton), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnValuePropertyChanged)));
        
        public static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var sl = sender as WindowsSpinButton;
            if (sl != null)
                sl.RaiseValueChangedEvent(e);
        }

        private void RaiseValueChangedEvent(DependencyPropertyChangedEventArgs e)
        {
            if (this.OnValueChanged != null)
                this.OnValueChanged(this, e);
        }

        public event PropertyChangedCallback OnValueChanged;

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, Math.Round(value, DecimalPlaces)); }
        }

        public static readonly DependencyProperty WrapProperty =
             DependencyProperty.Register("Wrap", typeof(bool),
             typeof(WindowsSpinButton), new FrameworkPropertyMetadata(false));

        public bool Wrap
        {
            get { return (bool)GetValue(WrapProperty); }
            set { SetValue(WrapProperty, value); }
        }

        public static readonly DependencyProperty MinimumValueProperty =
             DependencyProperty.Register("MinimumValue", typeof(double),
             typeof(WindowsSpinButton), new FrameworkPropertyMetadata(0.0));

        public double MinimumValue
        {
            get { return (double)GetValue(MinimumValueProperty); }
            set { SetValue(MinimumValueProperty, value); }
        }

        public static readonly DependencyProperty MaximumValueProperty =
             DependencyProperty.Register("MaximumValue", typeof(double),
             typeof(WindowsSpinButton), new FrameworkPropertyMetadata(1.0));

        public double MaximumValue
        {
            get { return (double)GetValue(MaximumValueProperty); }
            set { SetValue(MaximumValueProperty, value); }
        }

        public static readonly DependencyProperty ClimbRateProperty =
             DependencyProperty.Register("ClimbRate", typeof(double),
             typeof(WindowsSpinButton), new FrameworkPropertyMetadata(0.0));

        public double ClimbRate
        {
            get { return (double)GetValue(ClimbRateProperty); }
            set { SetValue(ClimbRateProperty, value); }
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
             DependencyProperty.Register("IsIndeterminate", typeof(bool),
             typeof(WindowsSpinButton), new FrameworkPropertyMetadata(false));

        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public static readonly DependencyProperty IndeterminateMessageProperty =
             DependencyProperty.Register("IndeterminateMessage", typeof(string),
             typeof(WindowsSpinButton), new FrameworkPropertyMetadata(""));

        public string IndeterminateMessage
        {
            get { return (string)GetValue(IndeterminateMessageProperty); }
            set { SetValue(IndeterminateMessageProperty, value); }
        }
        #endregion

        #region General
        public WindowsSpinButton()
        {
            InitializeComponent();
        }
        private bool valueChangedByUser = false;
        private void UpdateTextbox()
        {
            if (IsIndeterminate && !valueChangedByUser)
                textBox.Text = IndeterminateMessage;
            else
                textBox.Text = string.Format("{0:N" + DecimalPlaces + "}", Value);
            if (!Wrap)
            {
                buttonUp.IsEnabled = Value != MaximumValue && IsEnabled;
                buttonDown.IsEnabled = Value != MinimumValue && IsEnabled;
            }
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTextbox();
        }

        private void parseTextBox()
        {
            //Simulating GTK behavior by ignoring Culture and GroupSeperator
            string stringValue = textBox.Text;
            string[] stringSplitedValue = stringValue.Split('.', ',');
            if (stringSplitedValue.Length > 1)
                stringValue = stringSplitedValue[0] + "." + stringSplitedValue[1];
            else
                stringValue = stringSplitedValue[0];
            double newValue = 0;
            if (double.TryParse(stringValue, System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, out newValue))
            {
                if (newValue > MaximumValue)
                    newValue = MaximumValue;
                else if (newValue < MinimumValue)
                    newValue = MinimumValue;
                Value = newValue;
            }
            else
                Value = MinimumValue;
            valueChangedByUser = true;
            UpdateTextbox();
        }

        private void DecreaseValue(double valueToSubstract)
        {
            valueChangedByUser = true;
            if (Wrap && Value == MinimumValue)
            {
                Value = MaximumValue;
            }
            else
            {
                if (Value - valueToSubstract < MinimumValue)
                    Value = MinimumValue;
                else
                    Value -= valueToSubstract;
            }
            UpdateTextbox();
        }

        private void IncreaseValue(double valueToAdd)
        {
            valueChangedByUser = true;
            if (Wrap && Value == MaximumValue)
            {
                Value = MinimumValue;
            }
            else
            {
                if (Value + valueToAdd > MaximumValue)
                    Value = MaximumValue;
                else
                    Value += valueToAdd;
            }
            UpdateTextbox();
        }

        #endregion

        #region UpDownButtons
        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            double valueToAdd = 0;
            if (!buttonUpJustPressed)
                valueToAdd += ClimbRate;
            buttonUpJustPressed = false;
            valueToAdd += Increment;
            IncreaseValue(valueToAdd);
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            double valueToSubstract = 0;
            if (!buttonDownJustPressed)
                valueToSubstract += ClimbRate;
            buttonDownJustPressed = false;
            valueToSubstract += Increment;
            DecreaseValue(valueToSubstract);
        }

        private void buttonUp_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Value = MaximumValue;
            UpdateTextbox();
        }

        private void buttonDown_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Value = MinimumValue;
            UpdateTextbox();
        }

        private bool buttonUpJustPressed = false;
        private bool buttonDownJustPressed = false;

        private void buttonUp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            buttonUpJustPressed = true;
        }

        private void buttonDown_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            buttonDownJustPressed = true;
        }
        #endregion

        #region Keyboard
        DispatcherTimer keyboardTimer;


        private void textBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Tab)
                parseTextBox();
            else if (e.Key == Key.Up)
            {
                IncreaseValue(Increment);
                StartKeyboardTimer();
            }
            else if (e.Key == Key.Down)
            {
                DecreaseValue(Increment);
                StartKeyboardTimer();
            }
            else if (e.Key == Key.PageDown)
            {
                DecreaseValue(Increment * 10);
                StartKeyboardTimer();
            }
            else if (e.Key == Key.PageUp)
            {
                IncreaseValue(Increment * 10);
                StartKeyboardTimer();
            }
            pressedKey = e.Key;
        }

        private void textBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Down || e.Key == Key.Up)
            {
                StopKeyboardTimer();
            }
        }

        private void StartKeyboardTimer()
        {
            if (keyboardTimer == null)
            {
                keyboardTimer = new DispatcherTimer();
                keyboardTimer.Tick += new EventHandler(OnKeyboardTimeout);
            }
            else
                if (keyboardTimer.IsEnabled)
                    return;

            keyboardTimer.Interval = TimeSpan.FromMilliseconds(buttonDown.Delay);
            keyboardTimer.Start();
        }
        private void StopKeyboardTimer()
        {
            if (keyboardTimer != null)
                keyboardTimer.Stop();
        }

        Key pressedKey;

        private void OnKeyboardTimeout(object sender, EventArgs e)
        {
            TimeSpan interval = TimeSpan.FromMilliseconds(buttonDown.Interval);
            if (keyboardTimer.Interval != interval)
                keyboardTimer.Interval = interval;
            switch (pressedKey)
            {
                case Key.Up:
                    IncreaseValue((Increment + ClimbRate));
                    break;
                case Key.Down:
                    DecreaseValue((Increment + ClimbRate));
                    break;
                case Key.PageUp:
                    IncreaseValue((Increment + ClimbRate) * 10);
                    break;
                case Key.PageDown:
                    DecreaseValue((Increment + ClimbRate) * 10);
                    break;
                default:
                    keyboardTimer.Stop();
                    break;
            }
        }
        #endregion

        #region MouseWheel
        private void textBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                IncreaseValue(Increment);
            else
                DecreaseValue(Increment);
        }
        #endregion
    }
}
