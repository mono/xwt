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
			NSEventModifierMask mask;
			Key key = GetXwtKey(keyEvent, out mask, out var rawKey);
			ModifierKeys mod = mask.ToXwtValue ();
			return new KeyEventArgs (key, keyEvent.KeyCode, mod, keyEvent.IsARepeat, (long)TimeSpan.FromSeconds (keyEvent.Timestamp).TotalMilliseconds, keyEvent.Characters, keyEvent.CharactersIgnoringModifiers, keyEvent, rawKey);
		}

		static Key RemoveShift(Key key, ref NSEventModifierMask mask)
        {
			mask &= ~NSEventModifierMask.ShiftKeyMask;
			return key;
        }

		static Key GetXwtKey (NSEvent keyEvent, out NSEventModifierMask modMask, out Key rawKey)
		{
			// NSEvent.KeyCode is the physical key pressed according to an ANSI US keyboard.
			// With this pressing the м key on a Cyrillac keyboard or ㅍ in Korean both return v
            // Parsing keyboard shortcuts should be based off this.
            // Handling text input should be based off the Characters and CharactersIgnoringModifiers fields.
			// For compatibility reasons the Key field parses Characters/CharactersIgnoringModifiers into a
			// Xwt.Key enum value, but most of unicode characters are not parsed so KeyEventArgs.Key probably
			// should be considered obsolete.
			modMask = keyEvent.ModifierFlags;
			rawKey = 0;

			// special keys
			switch (keyEvent.KeyCode) {
				case 0: rawKey = Key.a; break;
				case 1: rawKey = Key.s; break;
				case 2: rawKey = Key.d; break;
				case 3: rawKey = Key.f; break;
				case 4: rawKey = Key.h; break;
				case 5: rawKey = Key.g; break;
				case 6: rawKey = Key.z; break;
				case 7: rawKey = Key.x; break;
				case 8: rawKey = Key.c; break;
				case 9: rawKey = Key.v; break;
				case 11: rawKey = Key.b; break;
				case 12: rawKey = Key.q; break;
				case 13: rawKey = Key.w; break;
				case 14: rawKey = Key.e; break;
				case 15: rawKey = Key.r; break;
				case 16: rawKey = Key.y; break;
				case 17: rawKey = Key.t; break;
				case 18: rawKey = Key.K1; break;
				case 19: rawKey = Key.K2; break;
				case 20: rawKey = Key.K3; break;
				case 21: rawKey = Key.K4; break;
				case 22: rawKey = Key.K6; break;
				case 23: rawKey = Key.K5; break;
				case 24: rawKey = Key.Equal; break;
				case 25: rawKey = Key.K9; break;
				case 26: rawKey = Key.K7; break;
				case 27: rawKey = Key.Minus; break;
				case 28: rawKey = Key.K8; break;
				case 29: rawKey = Key.K0; break;
				case 30: rawKey = Key.RightSquareBracket; break;
				case 31: rawKey = Key.o; break;
				case 32: rawKey = Key.u; break;
				case 33: rawKey = Key.LeftSquareBracket; break;
				case 34: rawKey = Key.I; break;
				case 35: rawKey = Key.P; break;
				case 37: rawKey = Key.L; break;
				case 38: rawKey = Key.J; break;
				case 39: rawKey = Key.Quote; break;
				case 40: rawKey = Key.K; break;
				case 41: rawKey = Key.Semicolon; break;
				case 42: rawKey = Key.Backslash; break;
				case 43: rawKey = Key.Comma; break;
				case 44: rawKey = Key.Slash; break;
				case 45: rawKey = Key.N; break;
				case 46: rawKey = Key.M; break;
				case 47: rawKey = Key.Period; break;
				case 50: rawKey = Key.Backtick; break;

				case 65: rawKey = Key.NumPadDecimal; return Key.NumPadDecimal;  // kVK_ANSI_KeypadDecimal        = 0x41
				case 67: rawKey = Key.NumPadMultiply; return Key.NumPadMultiply; // kVK_ANSI_KeypadMultiply       = 0x43
				case 69: rawKey = Key.NumPadAdd; return Key.NumPadAdd;      // kVK_ANSI_KeypadPlus           = 0x45
				case 71: rawKey = Key.NumLock; return Key.NumLock;        // kVK_ANSI_KeypadClear          = 0x47
				case 75: rawKey = Key.NumPadDivide; return Key.NumPadDivide;   // kVK_ANSI_KeypadDivide         = 0x4B
				case 76: rawKey = Key.NumPadEnter; return Key.NumPadEnter;    // kVK_ANSI_KeypadEnter          = 0x4C
				case 78: rawKey = Key.NumPadSubtract; return Key.NumPadSubtract; // kVK_ANSI_KeypadMinus          = 0x4E
				case 81: rawKey = Key.NumPadEnter; return Key.NumPadEnter;    // kVK_ANSI_KeypadEquals         = 0x51
				case 82: rawKey = Key.NumPad0; return Key.NumPad0;        // kVK_ANSI_Keypad0              = 0x52
				case 83: rawKey = Key.NumPad1; return Key.NumPad1;        // kVK_ANSI_Keypad1              = 0x53
				case 84: rawKey = Key.NumPad2; return Key.NumPad2;        // kVK_ANSI_Keypad2              = 0x54
				case 85: rawKey = Key.NumPad3; return Key.NumPad3;        // kVK_ANSI_Keypad3              = 0x55
				case 86: rawKey = Key.NumPad4; return Key.NumPad4;        // kVK_ANSI_Keypad4              = 0x56
				case 87: rawKey = Key.NumPad5; return Key.NumPad5;        // kVK_ANSI_Keypad5              = 0x57
				case 88: rawKey = Key.NumPad6; return Key.NumPad6;        // kVK_ANSI_Keypad6              = 0x58
				case 89: rawKey = Key.NumPad7; return Key.NumPad7;        // kVK_ANSI_Keypad7              = 0x59
				case 91: rawKey = Key.NumPad8; return Key.NumPad8;        // kVK_ANSI_Keypad8              = 0x5B
				case 92: rawKey = Key.NumPad9; return Key.NumPad9;        // kVK_ANSI_Keypad9              = 0x5C

				case 36: rawKey = Key.Return; return Key.Return;         // kVK_Return					= 0x24
				case 48: rawKey = Key.Tab; return Key.Tab;            // kVK_Tab                       = 0x30
				case 49: rawKey = Key.Space; return Key.Space;          // kVK_Space                     = 0x31
				case 51: rawKey = Key.BackSpace; return Key.BackSpace;      // kVK_Delete                    = 0x33
				case 53: rawKey = Key.Escape; return Key.Escape;         // kVK_Escape                    = 0x35
				case 55: rawKey = Key.MetaLeft; return Key.MetaLeft;       // kVK_Command                   = 0x37
				case 56: rawKey = Key.ShiftLeft; return Key.ShiftLeft;      // kVK_Shift                     = 0x38
				case 57: rawKey = Key.CapsLock; return Key.CapsLock;       // kVK_CapsLock                  = 0x39
				case 58: rawKey = Key.AltLeft; return Key.AltLeft;        // kVK_Option                    = 0x3A
				case 59: rawKey = Key.ControlLeft; return Key.ControlLeft;    // kVK_Control                   = 0x3B
				case 60: rawKey = Key.ShiftRight; return Key.ShiftRight;     // kVK_RightShift                = 0x3C
				case 61: rawKey = Key.AltRight; return Key.AltRight;       // kVK_RightOption               = 0x3D
				case 62: rawKey = Key.ControlRight; return Key.ControlRight;   // kVK_RightControl              = 0x3E
				case 0x3F: rawKey = Key.function; return Key.function;           // kVK_Function                  = 0x3F
				case 0x40: rawKey = Key.F17; return Key.F17;          // kVK_F17                       = 0x40
				case 0x48: rawKey = Key.AudioRaiseVolume; return Key.AudioRaiseVolume;        // kVK_VolumeUp                  = 0x48
				case 0x49: rawKey = Key.AudioLowerVolume; return Key.AudioLowerVolume;      // kVK_VolumeDown                = 0x49
				case 0x4A: rawKey = Key.AudioMute; return Key.AudioMute;         // kVK_Mute                      = 0x4A
				case 0x4F: rawKey = Key.F18; return Key.F18;          // kVK_F18                       = 0x4F
				case 0x50: rawKey = Key.F19; return Key.F19;          // kVK_F19                       = 0x50
				case 0x5A: rawKey = Key.F20; return Key.F20;          // kVK_F20                       = 0x5A
				case 96: rawKey = Key.F5; return Key.F5;             // kVK_F5                        = 0x60
				case 97: rawKey = Key.F6; return Key.F6;             // kVK_F6                        = 0x61
				case 98: rawKey = Key.F7; return Key.F7;             // kVK_F7                        = 0x62
				case 99: rawKey = Key.F3; return Key.F3;             // kVK_F3                        = 0x63
				case 100: rawKey = Key.F8; return Key.F8;            // kVK_F8                        = 0x64
				case 101: rawKey = Key.F9; return Key.F9;            // kVK_F9                        = 0x65
				case 0x67: rawKey = Key.F11; return Key.F11;          // kVK_F11                       = 0x67
                case 105: rawKey = Key.Print; return Key.Print;         // kVK_F13                       = 0x69
				case 0x6A: rawKey = Key.F16; return Key.F16;          // kVK_F16                       = 0x6A
                case 0x6B: rawKey = Key.F14; return Key.F14;          // kVK_F14                       = 0x6B
                case 109: rawKey = Key.F10; return Key.F10;           // kVK_F10                       = 0x6D
                case 0x6F: rawKey = Key.F12; return Key.F12;          // kVK_F12                       = 0x6F
                case 0x71: rawKey = Key.F15; return Key.F15;          // kVK_F15                       = 0x71
				case 114: rawKey = Key.Help; return Key.Help;          // kVK_Help                      = 0x72
				case 115: rawKey = Key.Home; return Key.Home;          // kVK_Home                      = 0x73
				case 116: rawKey = Key.PageUp; return Key.PageUp;        // kVK_PageUp                    = 0x74
				case 117: rawKey = Key.Delete; return Key.Delete;        // kVK_ForwardDelete             = 0x75
				case 118: rawKey = Key.F4; return Key.F4;            // kVK_F4                        = 0x76
				case 119: rawKey = Key.End; return Key.End;           // kVK_End                       = 0x77
				case 120: rawKey = Key.F2; return Key.F2;            // kVK_F2                        = 0x78
				case 121: rawKey = Key.PageDown; return Key.PageDown;      // kVK_PageDown                  = 0x79
				case 122: rawKey = Key.F1; return Key.F1;            // kVK_F1                        = 0x7A
				case 123: rawKey = Key.Left; return Key.Left;          // kVK_LeftArrow                 = 0x7B
				case 124: rawKey = Key.Right; return Key.Right;         // kVK_RightArrow                = 0x7C
				case 125: rawKey = Key.Down; return Key.Down;          // kVK_DownArrow                 = 0x7D
				case 126: rawKey = Key.Up; return Key.Up;            // kVK_UpArrow                   = 0x7E
			}

			// How non-roman keyboards seem to work in Cocoa, for example a Russian keyboard, is like so
			// If you press the м key on the keyboard (which is the 'v' key on a roman/english/qwerty keyboard)
			// keyEvent.Characters and keyEvent.CharactersIgnoringModifers both contain м
			// If you press the cmd+м on the keyboard, keyEvent.Characters contains a 'v' and keyEvent.CharactersIgnoringModifiers
			// contains the м
			// This way it can map cmd+м to paste like on a roman keyboard.
			var characters = string.IsNullOrWhiteSpace(keyEvent.Characters) ? keyEvent.CharactersIgnoringModifiers : keyEvent.Characters;
			if (characters.Length > 0)
				switch (characters[0]) {
				case 'A': return Key.a;
				case 'B': return Key.b;
				case 'C': return Key.c;
				case 'D': return Key.d;
				case 'E': return Key.e;
				case 'F': return Key.f;
				case 'G': return Key.g;
				case 'H': return Key.h;
				case 'I': return Key.i;
				case 'J': return Key.j;
				case 'K': return Key.k;
				case 'L': return Key.l;
				case 'M': return Key.m;
				case 'N': return Key.n;
				case 'O': return Key.o;
				case 'P': return Key.p;
				case 'Q': return Key.q;
				case 'R': return Key.r;
				case 'S': return Key.s;
				case 'T': return Key.t;
				case 'U': return Key.u;
				case 'V': return Key.v;
				case 'W': return Key.w;
				case 'X': return Key.x;
				case 'Y': return Key.y;
				case 'Z': return Key.z;
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

				case '^': return RemoveShift(Key.Caret, ref modMask);
				case '\'': return RemoveShift(Key.Quote, ref modMask);
				case '(': return RemoveShift(Key.LeftParenthesis, ref modMask);
				case ')': return RemoveShift(Key.RightParenthesis, ref modMask);
				case '*': return RemoveShift(Key.Asterisk, ref modMask);
				case '+': return RemoveShift(Key.Plus, ref modMask);
				case ',': return RemoveShift(Key.Comma, ref modMask);
				case '-': return RemoveShift(Key.Minus, ref modMask);
				case '.': return RemoveShift(Key.Period, ref modMask);
				case '/': return RemoveShift(Key.Slash, ref modMask);
				case '\\': return RemoveShift(Key.Backslash, ref modMask);
				case ':': return RemoveShift(Key.Colon, ref modMask);
				case ';': return RemoveShift(Key.Semicolon, ref modMask);
				case '<': return RemoveShift(Key.Less, ref modMask);
				case '>': return RemoveShift(Key.Greater, ref modMask);
				case '=': return RemoveShift(Key.Equal, ref modMask);
				case '?': return RemoveShift(Key.Question, ref modMask);
				case '@': return RemoveShift(Key.At, ref modMask);
				case '_': return RemoveShift(Key.Underscore, ref modMask);
				case '!': return RemoveShift(Key.Exclamation, ref modMask);
				case '#': return RemoveShift(Key.Hash, ref modMask);
				case '$': return RemoveShift(Key.Dollar, ref modMask);
				case '%': return RemoveShift(Key.Percentage, ref modMask);
				case '&': return RemoveShift(Key.Ampersand, ref modMask);
				case '[': return RemoveShift(Key.LeftSquareBracket, ref modMask);
				case ']': return RemoveShift(Key.RightSquareBracket, ref modMask);
				case '{': return RemoveShift(Key.LeftBrace, ref modMask);
				case '}': return RemoveShift(Key.RightBrace, ref modMask);
				case '\"': return RemoveShift(Key.Quote, ref modMask);
				case '|': return RemoveShift(Key.Pipe, ref modMask);
				case '`': return RemoveShift(Key.Backtick, ref modMask);
				case '~': return RemoveShift(Key.Tilde, ref modMask);
				}
			return (Key)0;
		}
	}
}
