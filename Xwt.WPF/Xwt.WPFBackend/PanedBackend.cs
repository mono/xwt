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
				return position != -1 ? position * (direction == Orientation.Horizontal ? WidthPixelRatio : HeightPixelRatio) : 0;
			}
			set {
				value /= direction == Orientation.Horizontal ? WidthPixelRatio : HeightPixelRatio;
				if (position != value) {
					position = value;
					NotifyPositionChanged ();
				}
			}
		}

		internal void NotifyPositionChanged ()
		{
			if (this.reportPositionChanged)
				ApplicationContext.InvokeUserCode (((IPanedEventSink)EventSink).OnPositionChanged);
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

		public override WidgetSize GetPreferredWidth ()
		{
			if (panel1.HasWidget && panel2.HasWidget) {
				if (direction == Orientation.Horizontal) {
					var ws = panel1.WidgetSurface.GetPreferredWidth ();
					ws += panel2.WidgetSurface.GetPreferredWidth ();
					ws += SplitterSize;
					return ws;
				}
				else {
					var ws = panel1.WidgetSurface.GetPreferredWidth ();
					return ws.UnionWith (panel2.WidgetSurface.GetPreferredWidth ());
				}
			}
			else if (panel1.HasWidget)
				return panel1.WidgetSurface.GetPreferredWidth ();
			else if (panel2.HasWidget)
				return panel2.WidgetSurface.GetPreferredWidth ();
			else
				return new WidgetSize (0);
		}

		public override WidgetSize GetPreferredHeightForWidth (double width)
		{
			if (panel1.HasWidget && panel2.HasWidget) {
				var tempPos = position;
				var availableWidth = width - SplitterSize;
				if (direction == Orientation.Horizontal) {
					if (!panel1.Shrink) {
						var w1 = panel1.WidgetSurface.GetPreferredWidth ().MinSize;
						if (tempPos < w1)
							tempPos = w1;
					}
					if (!panel2.Shrink) {
						var w2 = panel2.WidgetSurface.GetPreferredWidth ().MinSize;
						if (availableWidth - tempPos < w2)
							tempPos = availableWidth - w2;
					}
					var ws = panel1.WidgetSurface.GetPreferredHeightForWidth (tempPos);
					ws = ws.UnionWith (panel2.WidgetSurface.GetPreferredHeightForWidth (availableWidth - tempPos));
					return ws;
				}
				else {
					var ws = panel1.WidgetSurface.GetPreferredHeightForWidth (width);
					ws = ws.UnionWith (panel2.WidgetSurface.GetPreferredHeightForWidth (width));
					ws += SplitterSize;
					return ws;
				}
			}
			else if (panel1.HasWidget)
				return panel1.WidgetSurface.GetPreferredHeightForWidth (width);
			else if (panel2.HasWidget)
				return panel2.WidgetSurface.GetPreferredHeightForWidth (width);
			else
				return new WidgetSize (0);
		}

		public override WidgetSize GetPreferredHeight ()
		{
			if (panel1.HasWidget && panel2.HasWidget) {
				if (direction == Orientation.Vertical) {
					var ws = panel1.WidgetSurface.GetPreferredHeight ();
					ws += panel2.WidgetSurface.GetPreferredHeight ();
					ws += SplitterSize;
					return ws;
				}
				else {
					var ws = panel1.WidgetSurface.GetPreferredHeight ();
					return ws.UnionWith (panel2.WidgetSurface.GetPreferredHeight ());
				}
			}
			else if (panel1.HasWidget)
				return panel1.WidgetSurface.GetPreferredHeight ();
			else if (panel2.HasWidget)
				return panel2.WidgetSurface.GetPreferredHeight ();
			else
				return new WidgetSize (0);
		}

		public override WidgetSize GetPreferredWidthForHeight (double width)
		{
			if (panel1.HasWidget && panel2.HasWidget) {
				var tempPos = position;
				var availableWidth = width - SplitterSize;
				if (direction == Orientation.Vertical) {
					if (!panel1.Shrink) {
						var w1 = panel1.WidgetSurface.GetPreferredHeight ().MinSize;
						if (tempPos < w1)
							tempPos = w1;
					}
					if (!panel2.Shrink) {
						var w2 = panel2.WidgetSurface.GetPreferredHeight ().MinSize;
						if (availableWidth - tempPos < w2)
							tempPos = availableWidth - w2;
					}
					var ws = panel1.WidgetSurface.GetPreferredWidthForHeight (tempPos);
					ws = ws.UnionWith (panel2.WidgetSurface.GetPreferredWidthForHeight (availableWidth - tempPos));
					return ws;
				}
				else {
					var ws = panel1.WidgetSurface.GetPreferredWidthForHeight (width);
					ws = ws.UnionWith (panel2.WidgetSurface.GetPreferredWidthForHeight (width));
					ws += SplitterSize;
					return ws;
				}
			}
			else if (panel1.HasWidget)
				return panel1.WidgetSurface.GetPreferredWidthForHeight (width);
			else if (panel2.HasWidget)
				return panel2.WidgetSurface.GetPreferredWidthForHeight (width);
			else
				return new WidgetSize (0);
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
					else if (!IsFixed (panel1))
						position = availableSize * (position / oldAvailableSize);
				}

				if (!panel1.Shrink) {
					var w = panel1.WidgetSurface;
					var min = direction == Orientation.Horizontal ? w.GetPreferredWidth ().MinSize: w.GetPreferredHeight ().MinSize;
					if (position < min)
						position = min;
				}
				if (!panel2.Shrink) {
					var w = panel2.WidgetSurface;
					var min = direction == Orientation.Horizontal ? w.GetPreferredWidth ().MinSize : w.GetPreferredHeight ().MinSize;
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
			}
			else if (panel1.Widget != null)
				panel1.Size = new GridLength (1, GridUnitType.Star);
			else if (panel2 != null)
				panel2.Size = new GridLength (1, GridUnitType.Star);

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
			constraint = new SW.Size (double.PositiveInfinity, double.PositiveInfinity);

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
