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

namespace Xwt.WPFBackend.Interop
{
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