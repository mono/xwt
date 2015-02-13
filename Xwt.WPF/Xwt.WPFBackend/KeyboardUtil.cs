// 
// KeyboardUtil.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
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
using System.Windows.Input;

using WpfKey = System.Windows.Input.Key;

namespace Xwt.WPFBackend
{
	using Keyboard = System.Windows.Input.Keyboard;

	public static class KeyboardUtil
	{
		public static ModifierKeys GetModifiers ()
		{
			var modifiers = ModifierKeys.None;
			var wpfModifiers = Keyboard.Modifiers;
			if ((wpfModifiers & System.Windows.Input.ModifierKeys.Alt) > 0)
				modifiers |= ModifierKeys.Alt;
			if ((wpfModifiers & System.Windows.Input.ModifierKeys.Control) > 0)
				modifiers |= ModifierKeys.Control;
			if ((wpfModifiers & System.Windows.Input.ModifierKeys.Shift) > 0)
				modifiers |= ModifierKeys.Shift;

			return modifiers;
		}

		const int D_Pos = (int)Key.K0 - (int)WpfKey.D0;
		const int U_Pos = (int)Key.A - (int)WpfKey.A;
		const int L_Pos = (int)Key.a - (int)WpfKey.A;
		const int N_Pos = (int)Key.NumPad0 - (int)WpfKey.NumPad0;
		const int F_Pos = (int)Key.F1 - (int)WpfKey.F1;

		// Missing key translations:
		// * NumPad keys other than 1-9
		// * SysReq, Undo, Redo, Menu, Find, Break, Equal
		// * ShiftLock, MetaLeft, MetaRight
		// * SuperLeft, SuperRight (likely not mappeable for Windows)
		// * Less, Greater, Question, At
		public static Key TranslateToXwtKey (WpfKey key)
		{
			// Letter keys
			if (key >= WpfKey.A && key <= WpfKey.Z) {
				bool upperCase = Keyboard.IsKeyToggled (WpfKey.CapsLock);
				if ((Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) > 0)
					upperCase = !upperCase;
				return upperCase ? (Key)(key + U_Pos) : (Key)(key + L_Pos);
			}

			if (key >= WpfKey.D0 && key <= WpfKey.D9)
				return (Key)(key + D_Pos);

			// Numpad- keys
			if (key >= WpfKey.NumPad0 && key <= WpfKey.NumPad9)
				return (Key)(key + N_Pos);

			// F- keys
			if (key >= WpfKey.F1 && key <= WpfKey.F10)
				return (Key)(key + F_Pos);

			bool isShiftToggled = Keyboard.IsKeyToggled (WpfKey.LeftShift) || Keyboard.IsKeyToggled (WpfKey.RightShift);
			switch (key) {
				case WpfKey.Cancel: return Key.Cancel;
				case WpfKey.Back: return Key.BackSpace;
				case WpfKey.Tab: return Key.Tab;
				case WpfKey.LineFeed: return Key.LineFeed;
				case WpfKey.Clear: return Key.Clear;
				case WpfKey.Return: return Key.Return;
				case WpfKey.Pause: return Key.Pause;
				case WpfKey.CapsLock: return Key.CapsLock;
				case WpfKey.Space: return Key.Space;
				case WpfKey.Escape: return Key.Escape;
				case WpfKey.PageDown: return Key.PageDown;
				case WpfKey.PageUp: return Key.PageUp;
				case WpfKey.End: return Key.End;
				case WpfKey.Home: return Key.Home;
				case WpfKey.Left: return Key.Left;
				case WpfKey.Up: return Key.Up;
				case WpfKey.Right: return Key.Right;
				case WpfKey.Down: return Key.Down;
				case WpfKey.Select: return Key.Select;
				case WpfKey.Print: return Key.Print;
				case WpfKey.Execute: return Key.Execute;
				case WpfKey.Delete: return Key.Delete;
				case WpfKey.Help: return Key.Help;
				case WpfKey.Insert: return Key.Insert;
				case WpfKey.Scroll: return Key.ScrollLock;
				case WpfKey.NumLock: return Key.NumLock;
				case WpfKey.LeftShift: return Key.ShiftLeft;
				case WpfKey.RightShift: return Key.ShiftRight;
				case WpfKey.LeftCtrl: return Key.ControlLeft;
				case WpfKey.RightCtrl: return Key.ControlRight;
				case WpfKey.LeftAlt: return Key.AltLeft;
				case WpfKey.RightAlt: return Key.AltRight;
				case WpfKey.Multiply: return Key.Asterisk;
				case WpfKey.Add: return Key.Plus;
				case WpfKey.OemComma: return isShiftToggled ? Key.Semicolon : Key.Comma;
				case WpfKey.Subtract: return Key.Minus;
				case WpfKey.Divide: return Key.Slash;
				case WpfKey.OemPeriod: return isShiftToggled ? Key.Colon : Key.Period;
				case WpfKey.Oem5: return Key.Caret;
			}

			return (Key)0;
		}
	}
}
