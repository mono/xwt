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
using Xwt.Engine;
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
				position = panel1.Size.Value;
				if (this.reportPositionChanged)
					Toolkit.Invoke (((IPanedEventSink)EventSink).OnPositionChanged);
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
					Grid.InvalidateArrange ();
					if (this.reportPositionChanged)
						Toolkit.Invoke (((IPanedEventSink)EventSink).OnPositionChanged);
				}
			}
		}

		PanelInfo GetPanel (int panel)
		{
			return panel == 1 ? panel1 : panel2;
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
					var min = direction == Orientation.Horizontal ? panel1.Widget.DesiredSize.Width : panel1.Widget.DesiredSize.Height;
					if (position < min)
						position = min;
				}
				if (!panel2.Shrink) {
					var min = direction == Orientation.Horizontal ? panel2.Widget.DesiredSize.Width : panel1.Widget.DesiredSize.Height;
					if (availableSize - position < min) {
						position = availableSize - min;
					}
				}

				if (position < 0)
					position = 0;
				if (position > availableSize)
					position = availableSize;

				panel1.Size = new GridLength (position, GridUnitType.Pixel);
				panel2.Size = new GridLength (availableSize - position, GridUnitType.Pixel);
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

	class PanedGrid: Grid
	{
		public PanedBackend Backend;

		protected override SW.Size MeasureOverride (SW.Size constraint)
		{
			base.MeasureOverride (constraint);
			return new SW.Size (0, 0);
		}

		protected override System.Windows.Size ArrangeOverride (System.Windows.Size arrangeSize)
		{
			Backend.ArrangeChildren (arrangeSize);
			return base.ArrangeOverride (arrangeSize);
		}
	}
}
