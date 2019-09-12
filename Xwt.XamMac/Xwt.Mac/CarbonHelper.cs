//
// CarbonHelper.cs
//
// Author:
//       iain <iaholmes@microsoft.com>
//
// Copyright (c) 2019 
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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Xwt.Mac
{
	internal static class CarbonHelper
	{
		internal delegate CarbonEventHandlerStatus EventDelegate (IntPtr callRef, IntPtr eventRef, IntPtr userData);
		const string CarbonLib = "/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon";

		internal enum EventStatus // this is an OSStatus
		{
			Ok = 0,

			//event manager
			EventAlreadyPostedErr = -9860,
			EventTargetBusyErr = -9861,
			EventClassInvalidErr = -9862,
			EventClassIncorrectErr = -9864,
			EventHandlerAlreadyInstalledErr = -9866,
			EventInternalErr = -9868,
			EventKindIncorrectErr = -9869,
			EventParameterNotFoundErr = -9870,
			EventNotHandledErr = -9874,
			EventLoopTimedOutErr = -9875,
			EventLoopQuitErr = -9876,
			EventNotInQueueErr = -9877,
			EventHotKeyExistsErr = -9878,
			EventHotKeyInvalidErr = -9879,
		}

		internal enum CarbonEventHandlerStatus //this is an OSStatus
		{
			Handled = 0,
			NotHandled = -9874,
			UserCancelled = -128,
		}

		[DllImport (CarbonLib)]
		static extern EventStatus InstallEventHandler (IntPtr target, EventDelegate handler, uint count,
													   CarbonEventTypeSpec [] types, IntPtr user_data, out IntPtr handlerRef);

		[DllImport (CarbonLib)]
		static extern EventStatus RemoveEventHandler (IntPtr handlerRef);

		[DllImport (CarbonLib)]
		static extern IntPtr GetApplicationEventTarget ();

		static void CheckReturn (EventStatus status)
		{
			int intStatus = (int)status;
			if (intStatus < 0)
				throw new Exception ($"{status}");
		}

		static void CheckReturn (AEDescStatus status)
		{
			if (status != AEDescStatus.Ok)
				throw new Exception ("Failed with code " + status.ToString ());
		}

		static void CheckReturn (int osErr)
		{
			if (osErr != 0) {
				throw new SystemException ("Unexpected OS error code " + osErr);
			}
		}

		static IntPtr InstallEventHandler (IntPtr target, EventDelegate handler, CarbonEventTypeSpec [] types)
		{
			IntPtr handlerRef;
			CheckReturn (InstallEventHandler (target, handler, (uint)types.Length, types, IntPtr.Zero, out handlerRef));
			return handlerRef;
		}

		static IntPtr InstallEventHandler (IntPtr target, EventDelegate handler, CarbonEventTypeSpec type)
		{
			return InstallEventHandler (target, handler, new CarbonEventTypeSpec [] { type });
		}

		internal static IntPtr InstallApplicationEventHandler (EventDelegate handler, CarbonEventTypeSpec [] types)
		{
			return InstallEventHandler (GetApplicationEventTarget (), handler, types);
		}

		internal static IntPtr InstallApplicationEventHandler (EventDelegate handler, CarbonEventTypeSpec type)
		{
			return InstallEventHandler (GetApplicationEventTarget (), handler, new CarbonEventTypeSpec [] { type });
		}

		#region Event parameter extraction

		[DllImport (CarbonLib)]
		static extern EventStatus GetEventParameter (IntPtr eventRef, CarbonEventParameterName name, CarbonEventParameterType desiredType,
															out CarbonEventParameterType actualType, uint size, ref uint outSize, ref IntPtr outPtr);

		static IntPtr GetEventParameter (IntPtr eventRef, CarbonEventParameterName name, CarbonEventParameterType desiredType)
		{
			CarbonEventParameterType actualType;
			uint outSize = 0;
			IntPtr val = IntPtr.Zero;
			CheckReturn (GetEventParameter (eventRef, name, desiredType, out actualType, (uint)IntPtr.Size, ref outSize, ref val));
			return val;
		}

		[DllImport (CarbonLib)]
		static extern EventStatus GetEventParameter (IntPtr eventRef, CarbonEventParameterName name, CarbonEventParameterType desiredType,
													 out CarbonEventParameterType actualType, uint size, ref uint outSize, IntPtr dataBuffer);

		[DllImport (CarbonLib)]
		static extern EventStatus GetEventParameter (IntPtr eventRef, CarbonEventParameterName name, CarbonEventParameterType desiredType,
													 uint zero, uint size, uint zero2, IntPtr dataBuffer);

		static T GetEventParameter<T> (IntPtr eventRef, CarbonEventParameterName name, CarbonEventParameterType desiredType) where T : struct
		{
			int len = Marshal.SizeOf (typeof (T));
			IntPtr bufferPtr = Marshal.AllocHGlobal (len);
			CheckReturn (GetEventParameter (eventRef, name, desiredType, 0, (uint)len, 0, bufferPtr));
			T val = (T)Marshal.PtrToStructure (bufferPtr, typeof (T));
			Marshal.FreeHGlobal (bufferPtr);
			return val;
		}

		[DllImport (CarbonLib)]
		static extern AEDescStatus AEGetNthPtr (ref AEDesc descList, int index, OSType desiredType, uint keyword,
												uint zero, out IntPtr outPtr, int bufferSize, int zero2);
		[DllImport (CarbonLib)]
		static extern AEDescStatus AEGetNthPtr (ref AEDesc descList, int index, OSType desiredType, uint keyword,
												uint zero, IntPtr buffer, int bufferSize, int zero2);
		#endregion

		[StructLayout (LayoutKind.Sequential, Pack = 2, Size = 80)]
		struct FSRef
		{
			//this is an 80-char opaque byte array
#pragma warning disable 0169
			private byte hidden;
#pragma warning restore 0169
		}

		[StructLayout (LayoutKind.Sequential)]
		struct SelectionRange
		{
			public short unused1; // 0 (not used)
			public short lineNum; // line to select (<0 to specify range)
			public int startRange; // start of selection range (if line < 0)
			public int endRange; // end of selection range (if line < 0)
			public int unused2; // 0 (not used)
			public int theDate; // modification date/time
		}

		[StructLayout (LayoutKind.Sequential, Pack = 2)]
		public struct AEDesc
		{
			public uint descriptorType;
			public IntPtr dataHandle;
		}

		enum CarbonEventParameterName : uint
		{
			DirectObject = 757935405, // '----'
			AEPosition = 1802530675, // 'kpos'
		}

		enum CarbonEventParameterType : uint
		{
			HICommand = 1751346532, // 'hcmd'
			MenuRef = 1835363957, // 'menu'
			WindowRef = 2003398244, // 'wind'
			Char = 1413830740, // 'TEXT'
			UInt32 = 1835100014, // 'magn'
			UTF8Text = 1970562616, // 'utf8'
			UnicodeText = 1970567284, // 'utxt'
			AEList = 1818850164, // 'list'
			WildCard = 707406378, // '****'
			FSRef = 1718841958, // 'fsrf' 
		}

		[DllImport (CarbonLib)]
		static extern AEDescStatus AECountItems (ref AEDesc descList, out int count); //return an OSErr
		[DllImport (CarbonLib)]
		public static extern AEDescStatus AEDisposeDesc (ref AEDesc desc);

		public enum AEDescStatus
		{
			Ok = 0,
			MemoryFull = -108,
			CoercionFail = -1700,
			DescRecordNotFound = -1701,
			WrongDataType = -1703,
			NotAEDesc = -1704,
			ReplyNotArrived = -1718,
		}

		delegate T AEDescValueSelector<TRef, T> (ref TRef desc);
		static int AECountItems (ref AEDesc descList)
		{
			int count;
			CheckReturn (AECountItems (ref descList, out count));
			return count;
		}

		static T AEGetNthPtr<T> (ref AEDesc descList, int index, OSType desiredType) where T : struct
		{
			int len = Marshal.SizeOf (typeof (T));
			IntPtr bufferPtr = Marshal.AllocHGlobal (len);
			try {
				CheckReturn (AEGetNthPtr (ref descList, index, desiredType, 0, 0, bufferPtr, len, 0));
				T val = (T)Marshal.PtrToStructure (bufferPtr, typeof (T));
				return val;
			} finally {
				Marshal.FreeHGlobal (bufferPtr);
			}
		}

		static IntPtr AEGetNthPtr (ref AEDesc descList, int index, OSType desiredType)
		{
			IntPtr ret;
			CheckReturn (AEGetNthPtr (ref descList, index, desiredType, 0, 0, out ret, 4, 0));
			return ret;
		}

		static T [] GetListFromAEDesc<T, TRef> (ref AEDesc list, AEDescValueSelector<TRef, T> sel, OSType type)
			where TRef : struct
		{
			long count = AECountItems (ref list);
			T [] arr = new T [count];
			for (int i = 1; i <= count; i++) {
				TRef r = AEGetNthPtr<TRef> (ref list, i, type);
				arr [i - 1] = sel (ref r);
			}
			return arr;
		}

		[DllImport (CFLib)]
		extern static IntPtr CFURLCreateFromFSRef (IntPtr allocator, ref FSRef fsref);
		[DllImport (CFLib)]
		extern static IntPtr CFURLCopyFileSystemPath (IntPtr urlRef, CFUrlPathStyle pathStyle);

		enum CFUrlPathStyle
		{
			Posix = 0,
			Hfs = 1,
			Windows = 2
		};

		[DllImport (CFLib, EntryPoint = "CFRelease")]
		public static extern void Release (IntPtr cfRef);

		struct CFRange
		{
			public IntPtr Location, Length;
			public CFRange (int l, int len)
			{
				Location = (IntPtr)l;
				Length = (IntPtr)len;
			}
		}

		const string CFLib = "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation";

		[DllImport (CFLib, CharSet = CharSet.Unicode)]
		extern static int CFStringGetLength (IntPtr handle);

		[DllImport (CFLib, CharSet = CharSet.Unicode)]
		extern static IntPtr CFStringGetCharactersPtr (IntPtr handle);

		[DllImport (CFLib, CharSet = CharSet.Unicode)]
		extern static IntPtr CFStringGetCharacters (IntPtr handle, CFRange range, IntPtr buffer);

		public static string FetchString (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;

			string str;

			int l = CFStringGetLength (handle);
			IntPtr u = CFStringGetCharactersPtr (handle);
			IntPtr buffer = IntPtr.Zero;
			if (u == IntPtr.Zero) {
				CFRange r = new CFRange (0, l);
				buffer = Marshal.AllocCoTaskMem (l * 2);
				CFStringGetCharacters (handle, r, buffer);
				u = buffer;
			}
			unsafe {
				str = new string ((char*)u, 0, l);
			}

			if (buffer != IntPtr.Zero)
				Marshal.FreeCoTaskMem (buffer);

			return str;
		}

		static string FSRefToString (ref FSRef fsref)
		{
			IntPtr url = IntPtr.Zero;
			IntPtr str = IntPtr.Zero;
			try {
				url = CFURLCreateFromFSRef (IntPtr.Zero, ref fsref);
				if (url == IntPtr.Zero)
					return null;
				str = CFURLCopyFileSystemPath (url, CFUrlPathStyle.Posix);
				if (str == IntPtr.Zero)
					return null;
				return FetchString (str);
			} finally {
				if (url != IntPtr.Zero)
					Release (url);
				if (str != IntPtr.Zero)
					Release (str);
			}
		}

		public static Dictionary<string, int> GetFileListFromEventRef (IntPtr eventRef)
		{
			AEDesc list = GetEventParameter<AEDesc> (eventRef, CarbonEventParameterName.DirectObject, CarbonEventParameterType.AEList);
			try {
				int line;
				try {
					SelectionRange range = GetEventParameter<SelectionRange> (eventRef, CarbonEventParameterName.AEPosition, CarbonEventParameterType.Char);
					line = range.lineNum + 1;
				} catch (Exception) {
					line = 0;
				}

				var arr = GetListFromAEDesc<string, FSRef> (ref list, FSRefToString,
					(OSType)(int)CarbonEventParameterType.FSRef);
				var files = new Dictionary<string, int> ();
				foreach (var s in arr) {
					if (!string.IsNullOrEmpty (s))
						files [s] = line;
				}
				return files;
			} finally {
				CheckReturn ((int)AEDisposeDesc (ref list));
			}
		}

		static int ConvertCharCode (string fourcc)
		{
			Debug.Assert (fourcc != null);
			Debug.Assert (fourcc.Length == 4);
			return (fourcc [3]) | (fourcc [2] << 8) | (fourcc [1] << 16) | (fourcc [0] << 24);
		}

		internal enum CarbonEventApple
		{
			OpenApplication = 1868656752, // 'oapp'
			ReopenApplication = 1918988400, //'rapp'
			OpenDocuments = 1868853091, // 'odoc'
			PrintDocuments = 188563030, // 'pdoc'
			OpenContents = 1868787566, // 'ocon'
			QuitApplication = 1903520116, // 'quit'
			ShowPreferences = 1886545254, // 'pref'
			ApplicationDied = 1868720500, // 'obit'
			GetUrl = 1196773964, // 'GURL'
		}

		internal enum CarbonEventClass : uint
		{
			Mouse = 1836021107, // 'mous'
			Keyboard = 1801812322, // 'keyb'
			TextInput = 1952807028, // 'text'
			Application = 1634758764, // 'appl'
			RemoteAppleEvent = 1701867619,  //'eppc' //remote apple event?
			Menu = 1835363957, // 'menu'
			Window = 2003398244, // 'wind'
			Control = 1668183148, // 'cntl'
			Command = 1668113523, // 'cmds'
			Tablet = 1952607348, // 'tblt'
			Volume = 1987013664, // 'vol '
			Appearance = 1634758765, // 'appm'
			Service = 1936028278, // 'serv'
			Toolbar = 1952604530, // 'tbar'
			ToolbarItem = 1952606580, // 'tbit'
			Accessibility = 1633903461, // 'acce'
			HIObject = 1751740258, // 'hiob'
			AppleEvent = 1634039412, // 'aevt'
			Internet = 1196773964, // 'GURL'
		}

		[StructLayout (LayoutKind.Sequential, Pack = 2)]
		internal struct CarbonEventTypeSpec
		{
			public CarbonEventClass EventClass;
			public uint EventKind;

			public CarbonEventTypeSpec (CarbonEventClass eventClass, UInt32 eventKind)
			{
				this.EventClass = eventClass;
				this.EventKind = eventKind;
			}

			public CarbonEventTypeSpec (CarbonEventApple kind) : this (CarbonEventClass.AppleEvent, (uint)kind)
			{
			}

			public static implicit operator CarbonEventTypeSpec (CarbonEventApple kind)
			{
				return new CarbonEventTypeSpec (kind);
			}
		}

		struct OSType
		{
			int value;

			public int Value {
				get { return value; }
			}

			public OSType (int value)
			{
				this.value = value;
			}

			public OSType (string fourcc)
			{
				value = ConvertCharCode (fourcc);
			}

			public static explicit operator OSType (string fourcc)
			{
				return new OSType (fourcc);
			}

			public static implicit operator int (OSType o)
			{
				return o.value;
			}

			public static implicit operator OSType (int i)
			{
				return new OSType (i);
			}
		}
	}
}
