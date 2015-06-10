﻿//
// Messaging.cs
//
// Author:
//       Michael Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2014 Xamarin Inc.
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
using System.Runtime.InteropServices;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
#endif

namespace Xwt.Mac
{
	static class Messaging
	{
		const string LIBOBJC_DYLIB = "objc";

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		public static extern void void_objc_msgSend_int_NSRange (IntPtr handle, IntPtr sel, IntPtr a1, NSRange a2);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		public static extern void void_objc_msgSend (IntPtr handle, IntPtr sel);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		public static extern bool bool_objc_msgSend_IntPtr_IntPtr (IntPtr handle, IntPtr sel, IntPtr a1, IntPtr a2);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSendSuper")]
		public static extern IntPtr IntPtr_objc_msgSendSuper_IntPtr (IntPtr superHandle, IntPtr sel, IntPtr a1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		public static extern IntPtr IntPtr_objc_msgSend_IntPtr (IntPtr handle, IntPtr sel, IntPtr a1);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		public static extern IntPtr IntPtr_objc_msgSend (IntPtr handle, IntPtr sel);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		public static extern IntPtr IntPtr_objc_msgSend_IntPtr_IntPtr (IntPtr handle, IntPtr sel, IntPtr a1, IntPtr a2);

		[DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
		public static extern void void_objc_msgSend_bool (IntPtr handle, IntPtr sel, bool a1);
	}
}
