//  Copyright (c) 2006, Gustavo Franco
//  Email:  gustavo_franco@hotmail.com
//  All rights reserved.

//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:

//  Redistributions of source code must retain the above copyright notice, 
//  this list of conditions and the following disclaimer. 
//  Redistributions in binary form must reproduce the above copyright notice, 
//  this list of conditions and the following disclaimer in the documentation 
//  and/or other materials provided with the distribution. 

//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.

using System;
using System.Runtime.InteropServices;

namespace Xwt.Gtk.Windows
{
	public static class Win32
	{
		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		};

		public const uint SHGFI_ICON = 0x100;
		public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
		public const uint SHGFI_SMALLICON = 0x1; // 'Small icon
		public const uint SHGFI_SHELLICONSIZE = 0x4; // 'Shell icon
		public const uint SHGFI_ICONLOCATION = 0x1000;
		public const uint SHGFI_TYPENAME = 0x400;
		public const uint SHGFI_USEFILEATTRIBUTES = 0x10;
		public const uint FILE_ATTRIBUTES_NORMAL = 0x80;
		internal const string USER32 = "user32.dll";
		internal const string SHELL32 = "shell32.dll";

		#region USER32
		[DllImport (Win32.USER32)]
		public static extern bool DestroyIcon ([In] IntPtr hIcon);
		[DllImport (Win32.SHELL32, CharSet = CharSet.Auto)]
		public static extern IntPtr SHGetFileInfo ([In] string pszPath, uint dwFileAttributes, [In, Out] ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
		#endregion
	}
}
