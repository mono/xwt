﻿//
// GtkMacEngine.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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
using Xwt.GtkBackend;
using Xwt.Backends;
using AppKit;
using Xwt.Mac;

namespace Xwt.Gtk.Mac
{
	public class MacPlatformBackend: GtkPlatformBackend
	{
		public override void Initialize (ToolkitEngineBackend toolit)
		{
			toolit.RegisterBackend <IWebViewBackend,WebViewBackend> ();
			toolit.RegisterBackend <DesktopBackend,GtkMacDesktopBackend> ();
			toolit.RegisterBackend <FontBackendHandler,GtkMacFontBackendHandler> ();
			toolit.RegisterBackend <IPopoverBackend,GtkMacPopoverBackend> ();
			toolit.RegisterBackend <IOpenFileDialogBackend, GtkMacOpenFileDialogBackend> ();
			toolit.RegisterBackend <ISaveFileDialogBackend, GtkMacSaveFileDialogBackend> ();
			toolit.RegisterBackend <ISelectFolderDialogBackend, GtkMacSelectFolderBackend> ();
		}

		public override Type GetBackendImplementationType (Type backendType)
		{
			if (backendType == typeof (IOpenFileDialogBackend) ||
			    backendType == typeof (ISaveFileDialogBackend) ||
			    backendType == typeof (ISelectFolderDialogBackend) ||
			    backendType == typeof (IWebViewBackend))
				Xwt.Mac.NSApplicationInitializer.Initialize ();
			return base.GetBackendImplementationType (backendType);
		}
	}
}

