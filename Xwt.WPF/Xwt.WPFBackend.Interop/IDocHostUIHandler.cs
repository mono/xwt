//
// IDocHostUIHandler.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2016 (c) Vsevolod Kukol
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
using System.Runtime.InteropServices.ComTypes;

namespace Xwt.NativeMSHTML
{
	enum DOCHOSTUIDBLCLK : uint
	{
		DOCHOSTUIDBLCLK_DEFAULT = 0,
		DOCHOSTUIDBLCLK_SHOWPROPERTIES = 1,
		DOCHOSTUIDBLCLK_SHOWCODE = 2
	}

	[Flags]
	enum DOCHOSTUIFLAG : uint
	{
		DOCHOSTUIFLAG_DIALOG = 0x00000001,
		DOCHOSTUIFLAG_DISABLE_HELP_MENU = 0x00000002,
		DOCHOSTUIFLAG_NO3DBORDER = 0x00000004,
		DOCHOSTUIFLAG_SCROLL_NO = 0x00000008,
		DOCHOSTUIFLAG_DISABLE_SCRIPT_INACTIVE = 0x00000010,
		DOCHOSTUIFLAG_OPENNEWWIN = 0x00000020,
		DOCHOSTUIFLAG_DISABLE_OFFSCREEN = 0x00000040,
		DOCHOSTUIFLAG_FLAT_SCROLLBAR = 0x00000080,
		DOCHOSTUIFLAG_DIV_BLOCKDEFAULT = 0x00000100,
		DOCHOSTUIFLAG_ACTIVATE_CLIENTHIT_ONLY = 0x00000200,
		DOCHOSTUIFLAG_OVERRIDEBEHAVIORFACTORY = 0x00000400,
		DOCHOSTUIFLAG_CODEPAGELINKEDFONTS = 0x00000800,
		DOCHOSTUIFLAG_URL_ENCODING_DISABLE_UTF8 = 0x00001000,
		DOCHOSTUIFLAG_URL_ENCODING_ENABLE_UTF8 = 0x00002000,
		DOCHOSTUIFLAG_ENABLE_FORMS_AUTOCOMPLETE = 0x00004000,
		DOCHOSTUIFLAG_ENABLE_INPLACE_NAVIGATION = 0x00010000,
		DOCHOSTUIFLAG_IME_ENABLE_RECONVERSION = 0x00020000,
		DOCHOSTUIFLAG_THEME = 0x00040000,
		DOCHOSTUIFLAG_NOTHEME = 0x00080000,
		DOCHOSTUIFLAG_NOPICS = 0x00100000,
		DOCHOSTUIFLAG_NO3DOUTERBORDER = 0x00200000,
		DOCHOSTUIFLAG_DISABLE_EDIT_NS_FIXUP = 0x00400000,
		DOCHOSTUIFLAG_LOCAL_MACHINE_ACCESS_CHECK = 0x00800000,
		DOCHOSTUIFLAG_DISABLE_UNTRUSTEDPROTOCOL = 0x01000000,
		DOCHOSTUIFLAG_HOST_NAVIGATES = 0x02000000,
		DOCHOSTUIFLAG_ENABLE_REDIRECT_NOTIFICATION = 0x04000000,
		DOCHOSTUIFLAG_USE_WINDOWLESS_SELECTCONTROL = 0x08000000,
		DOCHOSTUIFLAG_USE_WINDOWED_SELECTCONTROL = 0x10000000,
		DOCHOSTUIFLAG_ENABLE_ACTIVEX_INACTIVATE_MODE = 0x20000000,
		DOCHOSTUIFLAG_DPI_AWARE = 0x40000000
	}

	enum DOCHOSTUICONTEXTMENU : uint
	{
		CONTEXT_MENU_DEFAULT = 0,
		CONTEXT_MENU_IMAGE = 1,
		CONTEXT_MENU_CONTROL = 2,
		CONTEXT_MENU_TABLE = 3,
		CONTEXT_MENU_TEXTSELECT = 4,
		CONTEXT_MENU_ANCHOR = 5,
		CONTEXT_MENU_UNKNOWN = 6,
		CONTEXT_MENU_IMGDYNSRC = 7,
		CONTEXT_MENU_DEBUG = 8,
		CONTEXT_MENU_VSCROLL = 9,
		CONTEXT_MENU_HSCROLL = 10,
		CONTEXT_MENU_MEDIA = 11
	}

	[StructLayout (LayoutKind.Sequential)]
	struct DOCHOSTUIINFO
	{
		[MarshalAs (UnmanagedType.U4)]
		public uint cbSize;
		[MarshalAs (UnmanagedType.U4)]
		public DOCHOSTUIFLAG dwFlags;
		[MarshalAs (UnmanagedType.U4)]
		public DOCHOSTUIDBLCLK dwDoubleClick;
		[MarshalAs (UnmanagedType.LPWStr)]
		public string pchHostCss;
		[MarshalAs (UnmanagedType.LPWStr)]
		public string pchHostNS;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct MSG
	{
		public IntPtr hwnd;
		public uint message;
		public UIntPtr wParam;
		public IntPtr lParam;
		public uint time;
		public POINT pt;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct RECT
	{
		public IntPtr left;
		public IntPtr top;
		public IntPtr right;
		public IntPtr bottom;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct POINT
	{
		public IntPtr x;
		public IntPtr y;
	}

	[ComImport, Guid ("3050F3F0-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	interface ICustomDoc
	{
		[PreserveSig]
		int SetUIHandler (IDocHostUIHandler pUIHandler);
	}

	[ComImport, Guid ("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"), InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	interface IDocHostUIHandler
	{
		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		int ShowContextMenu (
			[MarshalAs(UnmanagedType.U4)] DOCHOSTUICONTEXTMENU dwID,
			ref POINT ppt,
			[MarshalAs(UnmanagedType.IUnknown)] object pcmdtReserved,
			[MarshalAs(UnmanagedType.IDispatch)] object pdispReserved
		);

		void GetHostInfo (ref DOCHOSTUIINFO pInfo);

		void ShowUI (uint dwID, ref object pActiveObject, ref object pCommandTarget, ref object pFrame, ref object pDoc);

		void HideUI ();

		void UpdateUI ();

		void EnableModeless ([In, MarshalAs(UnmanagedType.Bool)] bool fEnable);

		void OnDocWindowActivate ([In, MarshalAs(UnmanagedType.Bool)] bool fActivate);

		void OnFrameWindowActivate ([In, MarshalAs(UnmanagedType.Bool)] bool fActivate);

		void ResizeBorder (ref RECT prcBorder, object pUIWindow, [In, MarshalAs (UnmanagedType.Bool)] bool fFrameWindow);

		[PreserveSig]
		[return: MarshalAs (UnmanagedType.Error)]
		int TranslateAccelerator (ref MSG lpMsg, ref Guid pguidCmdGroup, uint nCmdID);

		void GetOptionKeyPath ([MarshalAs(UnmanagedType.BStr)] out string pchKey, uint dw);

		[PreserveSig]
		[return: MarshalAs (UnmanagedType.Error)]
		int GetDropTarget (
			[In, MarshalAs (UnmanagedType.Interface)] object pDropTarget,
			[Out, MarshalAs (UnmanagedType.Interface)] out object ppDropTarget
		);

		void GetExternal ([MarshalAs (UnmanagedType.IDispatch)] out object ppDispatch);

		[PreserveSig]
		[return: MarshalAs (UnmanagedType.Error)]
		int TranslateUrl (
			uint dwTranslate,
			[MarshalAs (UnmanagedType.BStr)] string pchURLIn,
			[MarshalAs (UnmanagedType.BStr)] out string ppchURLOut
		);

		[PreserveSig]
		[return: MarshalAs (UnmanagedType.Error)]
		int FilterDataObject (IDataObject pDO, out IDataObject pDOppDORet);
	}
}
