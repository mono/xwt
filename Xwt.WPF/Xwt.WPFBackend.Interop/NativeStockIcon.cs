// 
// NativeStockIcon.cs
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

namespace Xwt.WPFBackend.Interop
{
	[Flags]
	internal enum NativeStockIconOptions : uint {
		Large =			0x000000000,
		Small =			0x000000001,
		ShellSize =		0x000000004,
		Handle =		0x000000100,
		SystemIndex =	0x000004000,
		LinkOverlay =	0x000008000,
		Selected =		0x000010000
	}

	internal enum NativeStockIcon : uint {
		DocumentNotAssociated = 0,
		DocumentAssociated = 1,
		Application = 2,
		Folder = 3,
		FolderOpen = 4,
		Drive525 = 5,
		Drive35 = 6,
		DriveRemove = 7,
		DriveFixed = 8,
		DriveNetwork = 9,
		DriveNetworkDisabled = 10,
		DriveCD = 11,
		DriveRAM = 12,
		World = 13,
		Server = 15,
		Printer = 16,
		MyNetwork = 17,
		Find = 22,
		Help = 23,
		Share = 28,
		Link = 29,
		SlowFile = 30,
		Recycler = 31,
		RecyclerFull = 32,
		MediaCDAudio = 40,
		Lock = 47,
		AutoList = 49,
		PrinterNet = 50,
		ServerShare = 51,
		PrinterFax = 52,
		PrinterFaxNet = 53,
		PrinterFile = 54,
		Stack = 55,
		MediaSVCD = 56,
		StuffedFolder = 57,
		DriveUnknown = 58,
		DriveDVD = 59,
		MediaDVD = 60,
		MediaDVDRAM = 61,
		MediaDVDRW = 62,
		MediaDVDR = 63,
		MediaDVDROM = 64,
		MediaCDAudioPlus = 65,
		MediaCDRW = 66,
		MediaCDR = 67,
		MediaCDBurn = 68,
		MediaBlankCD = 69,
		MediaCDROM = 70,
		AudioFiles = 71,
		ImageFiles = 72,
		VideoFiles = 73,
		MixedFiles = 74,
		FolderBack = 75,
		FolderFront = 76,
		Shield = 77,
		Warning = 78,
		Info = 79,
		Error = 80,
		Key = 81,
		Software = 82,
		Rename = 83,
		Delete = 84,
		MediaAudioDVD = 85,
		MediaMovieDVD = 86,
		MediaEnhancedCD = 87,
		MediaEnhancedDVD = 88,
		MediaHDDVD = 89,
		MediaBluRay = 90,
		MediaVCD = 91,
		MediaDVDPlusR = 92,
		MediaDVDPlusRW = 93,
		DesktopPC = 94,
		MobilePC = 95,
		Users = 96,
		MediaSmartMedia = 97,
		MediaCompactFlash = 98,
		DeviceCellPhone = 99,
		DeviceCamera = 100,
		DeviceVideoCamera = 101,
		DeviceAudioPlayer = 102,
		NetworkConnect = 103,
		Internet = 104,
		ZipFile = 105,
		Settings = 106
	}
}