// 
// NativeMethods.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Text;

namespace Xwt.WPFBackend.Interop
{
	enum HResult
	{
		S_OK = 0x00000000,
		S_FALSE = 0x00000001,
	}

	internal static class NativeMethods
	{
		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		struct StockIconInfo
		{
			internal uint StructureSize;
			internal IntPtr Handle;
			internal int ImageIndex;
			internal int Indentifier;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 260)]
			internal string Path;
		}

		[DllImport ("Shell32.dll", CharSet = CharSet.Unicode)]
		static extern int SHGetStockIconInfo (NativeStockIcon icon, NativeStockIconOptions options,
		                                               ref StockIconInfo info);

		[DllImport ("User32.dll", SetLastError = true)]
		static extern bool DestroyIcon (IntPtr handle);

		enum MapType : uint
		{
			MAPVK_VK_TO_VSC = 0x0,
			MAPVK_VSC_TO_VK = 0x1,
			MAPVK_VK_TO_CHAR = 0x2,
			MAPVK_VSC_TO_VK_EX = 0x3,
		}

		[DllImport("user32.dll")]
		static extern int ToUnicode(
			uint wVirtKey,
			uint wScanCode,
			byte[] lpKeyState,
			[Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] 
			StringBuilder pwszBuff,
			int cchBuff,
			uint wFlags);

		[DllImport("user32.dll")]
		static extern bool GetKeyboardState(byte[] lpKeyState);

		[DllImport("user32.dll")]
		static extern uint MapVirtualKey(uint uCode, MapType uMapType);

		internal static char GetCharFromKey(System.Windows.Input.Key key)
		{
			char ch = new char();

			int virtualKey = KeyInterop.VirtualKeyFromKey(key);
			byte[] keyboardState = new byte[256];
			GetKeyboardState(keyboardState);

			uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
			StringBuilder stringBuilder = new StringBuilder(2);

			int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
			switch (result)
			{
			case -1: 
				break;
			case 0: 
				break;
			case 1:
				{
					ch = stringBuilder[0];
					break;
				}
			default:
				{
					ch = stringBuilder[0];
					break;
				}
			}
			return ch;
		}

		internal static BitmapSource GetImage (NativeStockIcon icon, NativeStockIconOptions options)
		{
			options |= NativeStockIconOptions.Handle;

			StockIconInfo info = new StockIconInfo();
			info.StructureSize = (uint) Marshal.SizeOf (typeof (StockIconInfo));

			int hresult = SHGetStockIconInfo (icon, options, ref info);
			if (hresult < 0)
				throw new COMException ("SHGetStockIconInfo failed", hresult);

			BitmapSource bitmap;
			try {
				bitmap = Imaging.CreateBitmapSourceFromHIcon (info.Handle, Int32Rect.Empty, null);
			}
			finally {
				DestroyIcon (info.Handle);
			}

			return bitmap;
		}
	}
}