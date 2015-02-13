// 
// BoxBackend.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2012 Carlos Alberto Cortez
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
using System.Linq;
using System.Text;
using SW = System.Windows;
using SWC = System.Windows.Controls;

using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.WPFBackend
{
	public class BoxBackend : WidgetBackend, IBoxBackend
	{
		public BoxBackend ()
		{
			Widget = new CustomPanel ();
		}

		new CustomPanel Widget {
			get { return (CustomPanel)base.Widget; }
			set { base.Widget = value; }
		}

		public void Add (IWidgetBackend widget)
		{
			Widget.Children.Add (GetFrameworkElement (widget));
		}

		public void Remove (IWidgetBackend widget)
		{
			Widget.Children.Remove (GetFrameworkElement (widget));
		}

		public void SetAllocation (IWidgetBackend [] widget, Rectangle [] rect)
		{
			Widget.SetAllocation (widget, rect);
		}
	}

	// A Canvas cannot be used, as manually setting Width/Height disables the
	// expected behavior of DesiredSize (used in the GetPreferredSize* methods).
	public class CustomPanel : SWC.Panel, IWpfWidget
	{
		IWidgetBackend [] widgets;
		Rectangle [] rects;

		public WidgetBackend Backend { get; set; }

		public CustomPanel()
		{
			Background = System.Windows.Media.Brushes.Transparent;
		}

		public Action<System.Windows.Media.DrawingContext> RenderAction;

		protected override void OnRender (System.Windows.Media.DrawingContext dc)
		{
			var render = RenderAction;
			if (render != null)
				render (dc);
			base.OnRender (dc);
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}

		public void SetAllocation (IWidgetBackend[] widgets, Rectangle[] rects)
		{
			this.widgets = widgets;
			this.rects = rects;

			ArrangeChildren (true);
		}

		// Whenever a control gets a change affecting its size/appearance,
		// it keep literaly *waiting* for a call to Arrange(), sometimes even if not truly needed.
		// In this cases ArrangeOverride is called, and we 'refresh' the controls that require it
		// (this is done by all the containers inheriting from Panel).
		protected override SW.Size ArrangeOverride (SW.Size finalSize)
		{
			ArrangeChildren (false);
			return base.ArrangeOverride (finalSize);
		}

		void ArrangeChildren (bool force)
		{
			if (widgets == null || rects == null)
				return;

			// Use the 'widgets' field so we can easily map a control position by looking at 'rects'.
			for (int i = 0; i < widgets.Length; i++) {
				var element = WidgetBackend.GetFrameworkElement (widgets [i]);
				if (!element.IsArrangeValid || force) {
					// Measure the widget again using the allocation constraints. This is necessary
					// because WPF widgets my cache some measurement information based on the
					// constraints provided in the last Measure call (which when calculating the
					// preferred size is normally set to infinite.
					var r = rects[i].WithPositiveSize ();
					if (force) {
						// Don't recalculate the size unless a relayout is being forced
						element.InvalidateMeasure ();
						element.Measure (new SW.Size (r.Width, r.Height));
					}
					
					element.Arrange (r.ToWpfRect ());
				//	element.UpdateLayout ();
				}
			}
		}
	}
}
