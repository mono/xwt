//
// PanedBackend.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//       Lluis Sanchez <lluis@xamarin.com>
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xwt.Backends;

using Orientation = Xwt.Backends.Orientation;
using SW = System.Windows;
using SWC = System.Windows.Controls;
using System.Collections.Generic;

namespace Xwt.WPFBackend
{
	public class PanedBackend
		: WidgetBackend, IPanedBackend
	{
		double position = -1;
		Orientation direction;
		PanelInfo panel1 = new PanelInfo ();
		PanelInfo panel2 = new PanelInfo ();
		GridSplitter splitter;
		private const int SplitterSize = 4;
		private bool reportPositionChanged;
		double lastSize;

		class PanelInfo
		{
			public UIElement Widget;
			public bool Resize;
			public DefinitionBase Definition;
			public int PanelIndex;
			public bool Shrink;
			public WidgetBackend Backend;

			public bool HasWidget { get { return Widget != null; } }

			public IWidgetSurface WidgetSurface {
				get { return (IWidgetSurface)Backend.Frontend; }
			}

			public GridLength Size
			{
				get
				{
					if (Definition is ColumnDefinition)
						return ((ColumnDefinition)Definition).Width;
					else
						return ((RowDefinition)Definition).Height;
				}
				set
				{
					if (Definition is ColumnDefinition)
						((ColumnDefinition)Definition).Width = value;
					else
						((RowDefinition)Definition).Height = value;
				}
			}
		}

		public void Initialize (Orientation dir)
		{
			this.direction = dir;

			Grid = new PanedGrid () { Backend = this };

			// Create all the row/column definitions and the splitter

			if (direction == Orientation.Horizontal) {
				ColumnDefinition definition = new ColumnDefinition ();
				definition.Width = new GridLength (1, GridUnitType.Star);
				Grid.ColumnDefinitions.Add (definition);
				panel1.Definition = definition;

				splitter = new GridSplitter {
					ResizeDirection = GridResizeDirection.Columns,
					VerticalAlignment = VerticalAlignment.Stretch,
					HorizontalAlignment = HorizontalAlignment.Center,
					Width = SplitterSize
				};
				Grid.ColumnDefinitions.Add (new ColumnDefinition { Width = GridLength.Auto });
				Grid.SetColumn (splitter, 1);
				Grid.Children.Add (splitter);

				definition = new ColumnDefinition ();
				definition.Width = new GridLength (1, GridUnitType.Star);
				Grid.ColumnDefinitions.Add (definition);
				panel2.Definition = definition;
			}
			else {
				RowDefinition definition = new RowDefinition ();
				definition.Height = new GridLength (1, GridUnitType.Star);
				Grid.RowDefinitions.Add (definition);
				panel1.Definition = definition;

				splitter = new GridSplitter {
					ResizeDirection = GridResizeDirection.Rows,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Center,
					Height = SplitterSize
				};
				Grid.RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
				Grid.SetRow (splitter, 1);
				Grid.Children.Add (splitter);

				definition = new RowDefinition ();
				definition.Height = new GridLength (1, GridUnitType.Star);
				Grid.RowDefinitions.Add (definition);
				panel2.Definition = definition;
			}
			panel1.PanelIndex = 0;
			panel2.PanelIndex = 2;
			splitter.Visibility = Visibility.Hidden;

			splitter.DragDelta += delegate
			{
				if (position != panel1.Size.Value) {
					position = panel1.Size.Value;
					NotifyPositionChanged ();
				}
			};
		}

		public double Position
		{
			get {
				return position != -1 ? position : 0;
			}
			set {
				if (position != value) {
					position = value;
					NotifyPositionChanged ();
				}
			}
		}

		internal void NotifyPositionChanged ()
		{
			if (this.reportPositionChanged)
				Context.InvokeUserCode (((IPanedEventSink)EventSink).OnPositionChanged);
		}

		PanelInfo GetPanel (int panel)
		{
			return panel == 1 ? panel1 : panel2;
		}

		bool SplitterVisible
		{
			get { return panel1.HasWidget && panel2.HasWidget; }
		}

		public void SetPanel (int panel, IWidgetBackend widget, bool resize, bool shrink)
		{
			var panelWidget = (UIElement)widget.NativeWidget;

			var pi = GetPanel (panel);
			pi.Widget = panelWidget;
			pi.Backend = (WidgetBackend)widget;
			pi.Resize = resize;
			pi.Shrink = shrink;

			if (direction == Orientation.Horizontal)
				Grid.SetColumn (pi.Widget, pi.PanelIndex);
			else
				Grid.SetRow (pi.Widget, pi.PanelIndex);

			Grid.Children.Add (pi.Widget);

			UpdateSplitterVisibility ();
		}

		public void UpdatePanel (int panel, bool resize, bool shrink)
		{
			var pi = GetPanel (panel);
			pi.Resize = resize;
			pi.Shrink = shrink;
			Grid.InvalidateArrange ();
		}

		public void RemovePanel (int panel)
		{
			var pi = GetPanel (panel);
			Grid.Children.Remove (pi.Widget);
			pi.Widget = null;
			UpdateSplitterVisibility ();
		}

		public override Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			if (panel1.HasWidget && panel2.HasWidget) {
				double w, h;
				if (direction == Orientation.Horizontal) {
					var s1 = panel1.WidgetSurface.GetPreferredSize (SizeConstraint.Unconstrained, heightConstraint);
					var s2 = panel2.WidgetSurface.GetPreferredSize (SizeConstraint.Unconstrained, heightConstraint);
					w = s1.Width + s2.Width + SplitterSize;
					h = Math.Max (s1.Height, s2.Height);
				}
				else {
					var s1 = panel1.WidgetSurface.GetPreferredSize (widthConstraint, SizeConstraint.Unconstrained);
					var s2 = panel2.WidgetSurface.GetPreferredSize (widthConstraint, SizeConstraint.Unconstrained);
					h = s1.Height + s2.Height + SplitterSize;
					w = Math.Max (s1.Width, s2.Width);
				}
				if (direction == Orientation.Horizontal && widthConstraint.IsConstrained && w > widthConstraint.AvailableSize)
					w = widthConstraint.AvailableSize;
				if (direction == Orientation.Vertical && heightConstraint.IsConstrained && h > heightConstraint.AvailableSize)
					h = heightConstraint.AvailableSize;
				return new Size (w, h);
			}
			else if (panel1.HasWidget)
				return panel1.WidgetSurface.GetPreferredSize (widthConstraint, heightConstraint);
			else if (panel2.HasWidget)
				return panel2.WidgetSurface.GetPreferredSize (widthConstraint, heightConstraint);
			else
				return Size.Zero;
		}

		internal void ArrangeChildren (SW.Size size)
		{
			double newSize = direction == Orientation.Horizontal ? size.Width : size.Height;
			double splitterDesiredSize = SplitterSize;
			double availableSize;

			availableSize = newSize - splitterDesiredSize;
			if (availableSize <= 0)
				return;

			if (panel1.Widget != null && panel2.Widget != null) {

				// If the bounds have changed, we have to calculate a new current position
				if (lastSize != newSize || position == -1) {
					double oldAvailableSize = lastSize - SplitterSize;
					if (position == -1)
						position = availableSize / 2;
					else if (IsFixed (panel2)) {
						var oldPanel2Size = oldAvailableSize - position - SplitterSize;
						position = availableSize - oldPanel2Size - SplitterSize;
					}
					else if (!IsFixed (panel1) && lastSize != 0)
						position = availableSize * (position / oldAvailableSize);
				}

				if (!panel1.Shrink) {
					var w = panel1.WidgetSurface;
					var min = direction == Orientation.Horizontal ? w.GetPreferredSize ().Width: w.GetPreferredSize ().Height;
					if (position < min)
						position = min;
				}
				if (!panel2.Shrink) {
					var w = panel2.WidgetSurface;
					var min = direction == Orientation.Horizontal ? w.GetPreferredSize ().Width : w.GetPreferredSize ().Height;
					if (availableSize - position < min) {
						position = availableSize - min;
					}
				}

				if (position < 0)
					position = 0;
				if (position > availableSize)
					position = availableSize;

				panel1.Size = new GridLength (position, GridUnitType.Star);
				panel2.Size = new GridLength (availableSize - position, GridUnitType.Star);
			} else if (panel1.Widget != null) {
				panel1.Size = new GridLength (1, GridUnitType.Star);
				panel2.Size = new GridLength(0);
			} else if (panel2 != null) {
				panel2.Size = new GridLength(1, GridUnitType.Star);
				panel1.Size = new GridLength(0);
			}

			lastSize = newSize;
		}

		bool IsFixed (PanelInfo pi)
		{
			return !pi.Resize && (pi == panel1 ? panel2 : panel1).Resize;
		}

		void UpdateSplitterVisibility ()
		{
			if (panel1.Widget != null && panel2.Widget != null)
				splitter.Visibility = Visibility.Visible;
			else
				splitter.Visibility = Visibility.Hidden;
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);

			if (eventId is PanedEvent) {
				switch ((PanedEvent)eventId) {
				case PanedEvent.PositionChanged:
					this.reportPositionChanged = true;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);

			if (eventId is PanedEvent) {
				switch ((PanedEvent)eventId) {
				case PanedEvent.PositionChanged:
					this.reportPositionChanged = false;
					break;
				}
			}
		}

		private Grid Grid
		{
			get { return (Grid) Widget; }
			set { Widget = value; }
		}
	}

	class PanedGrid: Grid, IWpfWidget
	{
		public WidgetBackend Backend { get; set; }

		protected override SW.Size MeasureOverride (SW.Size constraint)
		{
			// HACK: Fixes invalid size measure
			// This line is hack to fix a measuring issue with Grid. For some reason, the grid 'remembers' the constraint
			// parameter, so if MeasureOverride is called with a constraining size, but ArrangeOverride is later called
			// with a bigger size, the Grid still uses the constrained size when determining the size of the children
			//constraint = new SW.Size (double.PositiveInfinity, double.PositiveInfinity);

			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}

		protected override System.Windows.Size ArrangeOverride (System.Windows.Size arrangeSize)
		{
			PanedBackend b = ((PanedBackend)Backend);
			var oldPos = b.Position;
			b.ArrangeChildren (arrangeSize);
			var s = base.ArrangeOverride (arrangeSize);
			if (oldPos != b.Position)
				b.NotifyPositionChanged ();
			return s;
		}
	}
}
