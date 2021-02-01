//
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
using GtkWidget = Gtk.Widget;
using System.Collections.Generic;
using CoreGraphics;

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
			toolit.RegisterBackend <IOpenFileDialogBackend, Xwt.Mac.OpenFileDialogBackend> ();
			toolit.RegisterBackend <ISaveFileDialogBackend, Xwt.Mac.SaveFileDialogBackend> ();
			toolit.RegisterBackend <ISelectFolderDialogBackend, Xwt.Mac.SelectFolderDialogBackend> ();
			toolit.RegisterBackend <IAccessibleBackend, GtkMacAccessibleBackend> ();
		}

		public override Type GetBackendImplementationType (Type backendType)
		{
			if (backendType == typeof (IOpenFileDialogBackend) ||
			    backendType == typeof (ISaveFileDialogBackend) ||
			    backendType == typeof (ISelectFolderDialogBackend) ||
			    backendType == typeof (IWebViewBackend) ||
			    backendType == typeof (IAccessibleBackend))
				Xwt.Mac.NSApplicationInitializer.Initialize ();
			return base.GetBackendImplementationType (backendType);
		}

        public override void EnableNativeTooltip(GtkWidget widget)
        {
            Xwt.Mac.NSApplicationInitializer.Initialize();

            if (nativeTooltips == null)
                nativeTooltips = new Dictionary<GtkWidget, TooltipData>();

            if (nativeTooltips.ContainsKey(widget))
                return;

            widget.AddNotification("tooltip-text", HandleTooltipTextChanged);
            widget.SizeAllocated += UpdateNativeTooltipText;
            widget.Realized += UpdateNativeTooltipText;
            widget.Unrealized += RemoveNativeTooltipOnUnrealized;
            widget.Destroyed += HandleWidgetDestroyed;
            if (!string.IsNullOrEmpty(widget.TooltipText))
                HandleTooltipTextChanged(widget, EventArgs.Empty);
        }

        class TooltipData
        {
            // NSView.AddToolTip needs us to keep the NSString object alive until it gets removed
            public Foundation.NSString Tooltip;
            // keep a weak ref to the NSView, to be able to remove or realign the tooltip 
            public WeakReference<NSView> View;
            // Id returned by NSView.AddToolTip that needs to be passed to NSView.RemoveToolTip
            public nint? TooltipId;
        }

        static Dictionary<GtkWidget, TooltipData> nativeTooltips;

        static void HandleTooltipTextChanged(object sender, EventArgs args)
        {
            if (sender is GtkWidget)
            {
                var widget = (GtkWidget)sender;
                TooltipData tData;
                if (!nativeTooltips.TryGetValue(widget, out tData))
                {
                    if (string.IsNullOrEmpty(widget.TooltipText))
                    {
                        // previous tooltip has been removed or never existed, so no point in queuing another empty tooltip.
                        return;
                    }

                    tData = new TooltipData {
                        Tooltip = new Foundation.NSString(widget.TooltipText),
                        View = new WeakReference<NSView>(null),
                        TooltipId = null
                    };
                    nativeTooltips[widget] = tData;
                }
                else
                {
                    tData.Tooltip = new Foundation.NSString(widget.TooltipText ?? string.Empty);
                }

                // disable the GTK tooltip, this needs to be done every time because Gtk resets it when TooltipText is updated
                widget.HasTooltip = false;
                if (widget.IsRealized)
                {
                    UpdateNativeTooltipText(widget, EventArgs.Empty);
                }
            }
        }

        private static void UpdateNativeTooltipText(object sender, EventArgs e)
        {
            if (sender is GtkWidget)
            {
                var widget = (GtkWidget)sender;
                TooltipData tData;
                if (!widget.IsRealized || !nativeTooltips.TryGetValue(widget, out tData))
                {
                    return;
                }

                var nsv = GtkQuartz.GetView(widget);

                if (tData.TooltipId != null)
                {
                    nsv.RemoveToolTip(tData.TooltipId.Value);
                    tData.TooltipId = null;
                }

                if (string.IsNullOrEmpty (tData.Tooltip))
                {
                    return;
                }
                int x, y;
                widget.TranslateCoordinates(widget.Toplevel, 0, 0, out x, out y);
                var id = nsv.AddToolTip(new CGRect(x, y, widget.Allocation.Width, widget.Allocation.Height), tData.Tooltip, IntPtr.Zero);

                // update the cache
                tData.View.SetTarget(nsv);
                tData.TooltipId = id;
            }
        }

        private static void RemoveNativeTooltipOnUnrealized(object sender, EventArgs e)
        {
            var widget = sender as GtkWidget;
            TooltipData tData;
            NSView nsv;
            if (widget != null && nativeTooltips.TryGetValue(widget, out tData))
            {
                if (tData.TooltipId != null && tData.View.TryGetTarget(out nsv))
                {
                    nsv.RemoveToolTip(tData.TooltipId.Value);
                }          
            }
        }

        private static void HandleWidgetDestroyed(object sender, EventArgs e)
        {
            var widget = sender as GtkWidget;
            TooltipData tData;
            if (widget != null && nativeTooltips.TryGetValue(widget, out tData))
            {
                nativeTooltips.Remove(widget);
                widget.RemoveNotification("tooltip-text", HandleTooltipTextChanged);
            }
        }
	}
}

