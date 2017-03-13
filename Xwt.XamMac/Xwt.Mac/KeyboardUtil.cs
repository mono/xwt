//
// KeyboardUtil.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using AppKit;

namespace Xwt.Mac
{
	public static class KeyboardUtil
	{
		public static KeyEventArgs ToXwtKeyEventArgs (this NSEvent keyEvent)
		{
			Key key = GetXwtKey(keyEvent);
			ModifierKeys mod = keyEvent.ModifierFlags.ToXwtValue ();
			return new KeyEventArgs (key, keyEvent.KeyCode, mod, keyEvent.IsARepeat, (long)TimeSpan.FromSeconds (keyEvent.Timestamp).TotalMilliseconds);
		}

		static Key GetXwtKey (NSEvent keyEvent)
		{
			// special keys
			switch (keyEvent.KeyCode) {
				case 65: return Key.NumPadDecimal;  // kVK_ANSI_KeypadDecimal        = 0x41
				case 67: return Key.NumPadMultiply; // kVK_ANSI_KeypadMultiply       = 0x43
				case 69: return Key.NumPadAdd;      // kVK_ANSI_KeypadPlus           = 0x45
				case 71: return Key.NumLock;        // kVK_ANSI_KeypadClear          = 0x47
				case 75: return Key.NumPadDivide;   // kVK_ANSI_KeypadDivide         = 0x4B
				case 76: return Key.NumPadEnter;    // kVK_ANSI_KeypadEnter          = 0x4C
				case 78: return Key.NumPadSubtract; // kVK_ANSI_KeypadMinus          = 0x4E
				case 81: return Key.NumPadEnter;    // kVK_ANSI_KeypadEquals         = 0x51
				case 82: return Key.NumPad0;        // kVK_ANSI_Keypad0              = 0x52
				case 83: return Key.NumPad1;        // kVK_ANSI_Keypad1              = 0x53
				case 84: return Key.NumPad2;        // kVK_ANSI_Keypad2              = 0x54
				case 85: return Key.NumPad3;        // kVK_ANSI_Keypad3              = 0x55
				case 86: return Key.NumPad4;        // kVK_ANSI_Keypad4              = 0x56
				case 87: return Key.NumPad5;        // kVK_ANSI_Keypad5              = 0x57
				case 88: return Key.NumPad6;        // kVK_ANSI_Keypad6              = 0x58
				case 89: return Key.NumPad7;        // kVK_ANSI_Keypad7              = 0x59
				case 91: return Key.NumPad8;        // kVK_ANSI_Keypad8              = 0x5B
				case 92: return Key.NumPad9;        // kVK_ANSI_Keypad9              = 0x5C

				case 36: return Key.Return;         // kVK_Return                    = 0x24
				case 48: return Key.Tab;            // kVK_Tab                       = 0x30
				case 49: return Key.Space;          // kVK_Space                     = 0x31
				case 51: return Key.BackSpace;      // kVK_Delete                    = 0x33
				case 53: return Key.Escape;         // kVK_Escape                    = 0x35
				case 55: return Key.MetaLeft;       // kVK_Command                   = 0x37
				case 56: return Key.ShiftLeft;      // kVK_Shift                     = 0x38
				case 57: return Key.CapsLock;       // kVK_CapsLock                  = 0x39
				case 58: return Key.AltLeft;        // kVK_Option                    = 0x3A
				case 59: return Key.ControlLeft;    // kVK_Control                   = 0x3B
				case 60: return Key.ShiftRight;     // kVK_RightShift                = 0x3C
				case 61: return Key.AltRight;       // kVK_RightOption               = 0x3D
				case 62: return Key.ControlRight;   // kVK_RightControl              = 0x3E
				//case 0x3F: return Key.FN;           // kVK_Function                  = 0x3F
				//case 0x40: return Key.F17;          // kVK_F17                       = 0x40
				//case 0x48: return Key.VolUp;        // kVK_VolumeUp                  = 0x48
				//case 0x49: return Key.VolDown;      // kVK_VolumeDown                = 0x49
				//case 0x4A: return Key.Mute;         // kVK_Mute                      = 0x4A
				//case 0x4F: return Key.F18;          // kVK_F18                       = 0x4F
				//case 0x50: return Key.F19;          // kVK_F19                       = 0x50
				//case 0x5A: return Key.F20;          // kVK_F20                       = 0x5A
				case 96: return Key.F5;             // kVK_F5                        = 0x60
				case 97: return Key.F6;             // kVK_F6                        = 0x61
				case 98: return Key.F7;             // kVK_F7                        = 0x62
				case 99: return Key.F3;             // kVK_F3                        = 0x63
				case 100: return Key.F8;            // kVK_F8                        = 0x64
				case 101: return Key.F9;            // kVK_F9                        = 0x65
				//case 0x67: return Key.F11;          // kVK_F11                       = 0x67
				case 105: return Key.Print;         // kVK_F13                       = 0x69
				//case 0x6A: return Key.F16;          // kVK_F16                       = 0x6A
				//case 0x6B: return Key.F14;          // kVK_F14                       = 0x6B
				case 109: return Key.F10;           // kVK_F10                       = 0x6D
				//case 0x6F: return Key.F12;          // kVK_F12                       = 0x6F
				//case 0x71: return Key.F15;          // kVK_F15                       = 0x71
				case 114: return Key.Help;          // kVK_Help                      = 0x72
				case 115: return Key.Home;          // kVK_Home                      = 0x73
				case 116: return Key.PageUp;        // kVK_PageUp                    = 0x74
				case 117: return Key.Delete;        // kVK_ForwardDelete             = 0x75
				case 118: return Key.F4;            // kVK_F4                        = 0x76
				case 119: return Key.End;           // kVK_End                       = 0x77
				case 120: return Key.F2;            // kVK_F2                        = 0x78
				case 121: return Key.PageDown;      // kVK_PageDown                  = 0x79
				case 122: return Key.F1;            // kVK_F1                        = 0x7A
				case 123: return Key.Left;          // kVK_LeftArrow                 = 0x7B
				case 124: return Key.Right;         // kVK_RightArrow                = 0x7C
				case 125: return Key.Down;          // kVK_DownArrow                 = 0x7D
				case 126: return Key.Up;            // kVK_UpArrow                   = 0x7E
			}

			// character keys: use character comparison, since KeyCode is always US
			// and does not honor inernationalization like WPF or GTK
			if (keyEvent.CharactersIgnoringModifiers.Length > 0)
				switch (keyEvent.CharactersIgnoringModifiers[0]) {
				case 'A': return Key.A;
				case 'B': return Key.B;
				case 'C': return Key.C;
				case 'D': return Key.D;
				case 'E': return Key.E;
				case 'F': return Key.F;
				case 'G': return Key.G;
				case 'H': return Key.H;
				case 'I': return Key.I;
				case 'J': return Key.J;
				case 'K': return Key.K;
				case 'L': return Key.L;
				case 'M': return Key.M;
				case 'N': return Key.N;
				case 'O': return Key.O;
				case 'P': return Key.P;
				case 'Q': return Key.Q;
				case 'R': return Key.R;
				case 'S': return Key.S;
				case 'T': return Key.T;
				case 'U': return Key.U;
				case 'V': return Key.V;
				case 'W': return Key.W;
				case 'X': return Key.X;
				case 'Y': return Key.Y;
				case 'Z': return Key.Z;
				case 'a': return Key.a;
				case 'b': return Key.b;
				case 'c': return Key.c;
				case 'd': return Key.d;
				case 'e': return Key.e;
				case 'f': return Key.f;
				case 'g': return Key.g;
				case 'h': return Key.h;
				case 'i': return Key.i;
				case 'j': return Key.j;
				case 'k': return Key.k;
				case 'l': return Key.l;
				case 'm': return Key.m;
				case 'n': return Key.n;
				case 'o': return Key.o;
				case 'p': return Key.p;
				case 'q': return Key.q;
				case 'r': return Key.r;
				case 's': return Key.s;
				case 't': return Key.t;
				case 'u': return Key.u;
				case 'v': return Key.v;
				case 'w': return Key.w;
				case 'x': return Key.x;
				case 'y': return Key.y;
				case 'z': return Key.z;

				case '1': return Key.K1;
				case '2': return Key.K2;
				case '3': return Key.K3;
				case '4': return Key.K4;
				case '5': return Key.K5;
				case '6': return Key.K6;
				case '7': return Key.K7;
				case '8': return Key.K8;
				case '9': return Key.K9;
				case '0': return Key.K0;

				case '^': return Key.Caret;
				case '\'': return Key.Quote;
				case '(': return Key.LeftBracket;
				case ')': return Key.RightBracket;
				case '*': return Key.Asterisk;
				case '+': return Key.Plus;
				case ',': return Key.Comma;
				case '-': return Key.Minus;
				case '.': return Key.Period;
				case '/': return Key.Slash;
				case '\\': return Key.Backslash;
				case ':': return Key.Colon;
				case ';': return Key.Semicolon;
				case '<': return Key.Less;
				case '>': return Key.Greater;
				case '=': return Key.Equal;
				case '?': return Key.Question;
				case '@': return Key.At;
			}
			return (Key)0;
		}
	}
}
