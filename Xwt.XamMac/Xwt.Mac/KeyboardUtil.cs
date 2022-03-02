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
			var characters = keyEvent.Characters;
			var charactersIgnoringModifiers = keyEvent.CharactersIgnoringModifiers;
			var keyCode = keyEvent.KeyCode;
			var modifierFlags = keyEvent.ModifierFlags;
			
			Key key = GetXwtKey(keyCode);
			return new KeyEventArgs (key, keyCode, modifierFlags.ToXwtValue(), keyEvent.IsARepeat, (long)TimeSpan.FromSeconds (keyEvent.Timestamp).TotalMilliseconds, characters, charactersIgnoringModifiers, keyEvent);
		}

		static Key GetXwtKey (ushort keyCode)
		{
			// NSEvent.KeyCode is the physical key pressed according to an ANSI US keyboard.
			// With this pressing the м key on a Cyrillac keyboard or ㅍ in Korean both return v
            // Parsing keyboard shortcuts should be based off this.
            // Handling text input should be based off the Characters and CharactersIgnoringModifiers fields.
			// For compatibility reasons the Key field parses Characters/CharactersIgnoringModifiers into a
			// Xwt.Key enum value, but most of unicode characters are not parsed so KeyEventArgs.Key probably
			// should be considered obsolete.
			// special keys
			switch (keyCode) {
				case 0: return Key.a;
				case 1: return Key.s; 
				case 2: return Key.d; 
				case 3: return Key.f; 
				case 4: return Key.h; 
				case 5: return Key.g; 
				case 6: return Key.z; 
				case 7: return Key.x; 
				case 8: return Key.c; 
				case 9: return Key.v; 
				case 11: return Key.b; 
				case 12: return Key.q; 
				case 13: return Key.w; 
				case 14: return Key.e; 
				case 15: return Key.r; 
				case 16: return Key.y; 
				case 17: return Key.t; 
				case 18: return Key.K1; 
				case 19: return Key.K2; 
				case 20: return Key.K3; 
				case 21: return Key.K4; 
				case 22: return Key.K6; 
				case 23: return Key.K5; 
				case 24: return Key.Equal; 
				case 25: return Key.K9; 
				case 26: return Key.K7; 
				case 27: return Key.Minus; 
				case 28: return Key.K8; 
				case 29: return Key.K0; 
				case 30: return Key.RightSquareBracket; 
				case 31: return Key.o; 
				case 32: return Key.u; 
				case 33: return Key.LeftSquareBracket; 
				case 34: return Key.I; 
				case 35: return Key.P; 
				case 37: return Key.L; 
				case 38: return Key.J; 
				case 39: return Key.Quote; 
				case 40: return Key.K; 
				case 41: return Key.Semicolon; 
				case 42: return Key.Backslash; 
				case 43: return Key.Comma; 
				case 44: return Key.Slash; 
				case 45: return Key.N; 
				case 46: return Key.M; 
				case 47: return Key.Period; 
				case 50: return Key.Backtick; 

				case 65: return Key.NumPadDecimal; // kVK_ANSI_KeypadDecimal        = 0x41
				case 67: return Key.NumPadMultiply; // kVK_ANSI_KeypadMultiply       = 0x43
				case 69: return Key.NumPadAdd; // kVK_ANSI_KeypadPlus           = 0x45
				case 71: return Key.NumLock; // kVK_ANSI_KeypadClear          = 0x47
				case 75: return Key.NumPadDivide; // kVK_ANSI_KeypadDivide         = 0x4B
				case 76: return Key.NumPadEnter; // kVK_ANSI_KeypadEnter          = 0x4C
				case 78: return Key.NumPadSubtract; // kVK_ANSI_KeypadMinus          = 0x4E
				case 81: return Key.NumPadEnter; // kVK_ANSI_KeypadEquals         = 0x51
				case 82: return Key.NumPad0; // kVK_ANSI_Keypad0              = 0x52
				case 83: return Key.NumPad1; // kVK_ANSI_Keypad1              = 0x53
				case 84: return Key.NumPad2; // kVK_ANSI_Keypad2              = 0x54
				case 85: return Key.NumPad3; // kVK_ANSI_Keypad3              = 0x55
				case 86: return Key.NumPad4; // kVK_ANSI_Keypad4              = 0x56
				case 87: return Key.NumPad5; // kVK_ANSI_Keypad5              = 0x57
				case 88: return Key.NumPad6; // kVK_ANSI_Keypad6              = 0x58
				case 89: return Key.NumPad7; // kVK_ANSI_Keypad7              = 0x59
				case 91: return Key.NumPad8; // kVK_ANSI_Keypad8              = 0x5B
				case 92: return Key.NumPad9; // kVK_ANSI_Keypad9              = 0x5C

				case 36: return Key.Return; // kVK_Return					= 0x24
				case 48: return Key.Tab; // kVK_Tab                       = 0x30
				case 49: return Key.Space; // kVK_Space                     = 0x31
				case 51: return Key.BackSpace; // kVK_Delete                    = 0x33
				case 53: return Key.Escape; // kVK_Escape                    = 0x35
				case 55: return Key.MetaLeft; // kVK_Command                   = 0x37
				case 56: return Key.ShiftLeft; // kVK_Shift                     = 0x38
				case 57: return Key.CapsLock; // kVK_CapsLock                  = 0x39
				case 58: return Key.AltLeft; // kVK_Option                    = 0x3A
				case 59: return Key.ControlLeft; // kVK_Control                   = 0x3B
				case 60: return Key.ShiftRight; // kVK_RightShift                = 0x3C
				case 61: return Key.AltRight; // kVK_RightOption               = 0x3D
				case 62: return Key.ControlRight; // kVK_RightControl              = 0x3E
				case 0x3F: return Key.function; // kVK_Function                  = 0x3F
				case 0x40: return Key.F17; // kVK_F17                       = 0x40
				case 0x48: return Key.AudioRaiseVolume; // kVK_VolumeUp                  = 0x48
				case 0x49: return Key.AudioLowerVolume; // kVK_VolumeDown                = 0x49
				case 0x4A: return Key.AudioMute; // kVK_Mute                      = 0x4A
				case 0x4F: return Key.F18; // kVK_F18                       = 0x4F
				case 0x50: return Key.F19; // kVK_F19                       = 0x50
				case 0x5A: return Key.F20; // kVK_F20                       = 0x5A
				case 96: return Key.F5; // kVK_F5                        = 0x60
				case 97: return Key.F6; // kVK_F6                        = 0x61
				case 98: return Key.F7; // kVK_F7                        = 0x62
				case 99: return Key.F3; // kVK_F3                        = 0x63
				case 100: return Key.F8; // kVK_F8                        = 0x64
				case 101: return Key.F9; // kVK_F9                        = 0x65
				case 0x67: return Key.F11; // kVK_F11                       = 0x67
                case 105: return Key.Print; // kVK_F13                       = 0x69
				case 0x6A: return Key.F16; // kVK_F16                       = 0x6A
                case 0x6B: return Key.F14; // kVK_F14                       = 0x6B
                case 109: return Key.F10; // kVK_F10                       = 0x6D
                case 0x6F: return Key.F12; // kVK_F12                       = 0x6F
                case 0x71: return Key.F15; // kVK_F15                       = 0x71
				case 114: return Key.Help; // kVK_Help                      = 0x72
				case 115: return Key.Home; // kVK_Home                      = 0x73
				case 116: return Key.PageUp; // kVK_PageUp                    = 0x74
				case 117: return Key.Delete; // kVK_ForwardDelete             = 0x75
				case 118: return Key.F4; // kVK_F4                        = 0x76
				case 119: return Key.End; // kVK_End                       = 0x77
				case 120: return Key.F2; // kVK_F2                        = 0x78
				case 121: return Key.PageDown; // kVK_PageDown                  = 0x79
				case 122: return Key.F1; // kVK_F1                        = 0x7A
				case 123: return Key.Left; // kVK_LeftArrow                 = 0x7B
				case 124: return Key.Right; // kVK_RightArrow                = 0x7C
				case 125: return Key.Down; // kVK_DownArrow                 = 0x7D
				case 126: return Key.Up; // kVK_UpArrow                   = 0x7E
			}

			return (Key)0;
		}
	}
}
