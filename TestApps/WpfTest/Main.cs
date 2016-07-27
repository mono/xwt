// Main.cs
//  
// Author:
//       Luís Reis <luiscubal@gmail.com>
//       Vsevolod Kukol <sevoku@microsoft.com>
// 
// Copyright (c) 2012 Luís Reis
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
using Samples;
using Xwt;

namespace WpfTest
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			/*
			 WORKAROUND for Xwt.WebView on Windows using WPF:
			 The WPF WebView Backend is based on System.Windows.Controls.WebView
			 which runs in IE7 emulation mode by default. This behaviour can only be
			 changed by modifying the Windows registry, which is not part of the Xwt
			 API. See https://msdn.microsoft.com/de-de/library/ee330730.aspx#browser_emulation
			 for more information.
			 Uncomment the next line to set different IE Emulation modes
			*/
			//WebViewEmulationMode = IEEmulationMode.IE11;

			App.Run (ToolkitType.Wpf);
		}

		/// <summary>
		/// Gets or sets the System.Windows.Controls.WebView Emulation mode
		/// </summary>
		/// <remarks>This is a simple example on how to change the WebView emulation mode</remarks>
		public static IEEmulationMode WebViewEmulationMode
		{
			get
			{
				var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
				if (regKey == null)
					return IEEmulationMode.Default;

				string myProgramName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
				var currentValue = regKey.GetValue(myProgramName);
				return currentValue != null ? (IEEmulationMode)currentValue : IEEmulationMode.Default;
			}
			set
			{
				var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
				if (regKey == null)
					regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BEHAVIORS", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);

				string executableName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
				var currentValue = regKey.GetValue(executableName);
				if (currentValue == null || (int)currentValue != (int)value)
					regKey.SetValue(executableName, (int)value, Microsoft.Win32.RegistryValueKind.DWord);
			}
		}

		/// <summary>Internet Explorer Emulation Modes</summary>
		/// <remarks>https://msdn.microsoft.com/de-de/library/ee330730.aspx#browser_emulation</remarks>
		public enum IEEmulationMode
		{
			IE7 = 0x00001b58,
			IE8 = 0x00001f40,
			IE8Force = 0x000022b8,
			IE9 = 0x00002328,
			IE9Force = 0x0000270f,
			IE10 = 0x00002710,
			IE10Force = 0x00002711,
			IE11 = 0x00002af8,
			IE11Force = 0x00002af9,
			Default = IE7,
		}
	}
}
