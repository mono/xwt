// 
// ColorSelector.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt.Drawing;
using Xwt.Backends;
using System.Collections.Generic;


namespace Xwt
{
	[BackendType (typeof(IColorSelectorBackend))]
	public class ColorSelector: Widget
	{
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IColorSelectorEventSink
		{
			protected override IBackend OnCreateBackend ()
			{
				var b = base.OnCreateBackend ();
				if (b == null)
					b = new DefaultColorSelectorBackend ();
				return b;
			}
			
			public void OnColorChanged ()
			{
				((ColorSelector)Parent).OnColorChanged (EventArgs.Empty);
			}
		}
		
		public ColorSelector ()
		{
		}
		
		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IColorSelectorBackend Backend {
			get { return (IColorSelectorBackend) BackendHost.Backend; }
		}
		
		/// <summary>
		/// Gets or sets the selected color
		/// </summary>
		public Color Color {
			get { return Backend.Color; }
			set { Backend.Color = value; }
		}
		
		public bool SupportsAlpha {
			get { return Backend.SupportsAlpha; }
			set { Backend.SupportsAlpha = value; }
		}
		
		protected virtual void OnColorChanged (EventArgs args)
		{
			if (colorChanged != null)
				colorChanged (this, args);
		}
		
		EventHandler colorChanged;
		
		public event EventHandler ColorChanged {
			add {
				BackendHost.OnBeforeEventAdd (ColorSelectorEvent.ColorChanged, colorChanged);
				colorChanged += value;
			}
			remove {
				colorChanged -= value;
				BackendHost.OnAfterEventRemove (ColorSelectorEvent.ColorChanged, colorChanged);
			}
		}
	}
	
	class DefaultColorSelectorBackend: XwtWidgetBackend, IColorSelectorBackend
	{
		HueBox hsBox;
		LightBox lightBox;
		ColorSelectionBox colorBox;
		TextEntry hueEntry;
		TextEntry satEntry;
		TextEntry lightEntry;
		TextEntry redEntry;
		TextEntry greenEntry;
		TextEntry blueEntry;
		TextEntry alphaEntry;
		HSeparator alphaSeparator;
		Color currentColor;
		bool loadingEntries;
		List<Widget> alphaControls = new List<Widget> ();
		
		public DefaultColorSelectorBackend ()
		{
			HBox box = new HBox ();
			Table selBox = new Table ();
			hsBox = new HueBox ();
			hsBox.Light = 0.5;
			lightBox = new LightBox ();
			hsBox.SelectionChanged += delegate {
				lightBox.Hue = hsBox.SelectedColor.Hue;
				lightBox.Saturation = hsBox.SelectedColor.Saturation;
			};
			
			colorBox = new ColorSelectionBox () { MinHeight = 20 };
			
			selBox.Attach (hsBox, 0, 0);
			selBox.Attach (lightBox, 1, 0);
			
			box.PackStart (selBox);
			
			int entryWidth = 40;
			VBox entryBox = new VBox ();
			Table entryTable = new Table ();
			
			entryTable.Attach (new Label ("Color:"), 0, 0);
			entryTable.Attach (colorBox, 1, 5, 0, 1);
			entryTable.Attach (new HSeparator (), 0, 5, 1, 2);
			
			int r = 2;
			entryTable.Attach (new Label ("Hue:"), 0, r);
			entryTable.Attach (hueEntry = new TextEntry () { MinWidth = entryWidth }, 1, r++);
			
			entryTable.Attach (new Label ("Saturation:"), 0, r);
			entryTable.Attach (satEntry = new TextEntry () { MinWidth = entryWidth }, 1, r++);
			
			entryTable.Attach (new Label ("Light:"), 0, r);
			entryTable.Attach (lightEntry = new TextEntry () { MinWidth = entryWidth }, 1, r++);
			
			r = 2;
			entryTable.Attach (new Label ("Red:"), 3, r);
			entryTable.Attach (redEntry = new TextEntry () { MinWidth = entryWidth }, 4, r++);
			
			entryTable.Attach (new Label ("Green:"), 3, r);
			entryTable.Attach (greenEntry = new TextEntry () { MinWidth = entryWidth }, 4, r++);
			
			entryTable.Attach (new Label ("Blue:"), 3, r);
			entryTable.Attach (blueEntry = new TextEntry () { MinWidth = entryWidth }, 4, r++);
			
			Label label;
			entryTable.Attach (alphaSeparator = new HSeparator (), 0, 5, r, ++r);
			entryTable.Attach (label = new Label ("Opacity:"), 0, r);
			entryTable.Attach (alphaEntry = new TextEntry () { MinWidth = entryWidth }, 1, r);
			
			alphaControls.Add (alphaSeparator);
			alphaControls.Add (label);
			alphaControls.Add (alphaEntry);
			
			entryBox.PackStart (entryTable);
			box.PackStart (entryBox);
			Content = box;
			
			hsBox.SelectionChanged += delegate {
				HandleColorBoxSelectionChanged ();
			};
			lightBox.SelectionChanged += delegate {
				HandleColorBoxSelectionChanged ();
			};
			
			hueEntry.Changed += HandleHslChanged;
			satEntry.Changed += HandleHslChanged;
			lightEntry.Changed += HandleHslChanged;
			redEntry.Changed += HandleRgbChanged;
			greenEntry.Changed += HandleRgbChanged;
			blueEntry.Changed += HandleRgbChanged;
			alphaEntry.Changed += HandleAlphaChanged;
			
			Color = Colors.White;
		}

		void HandleAlphaChanged (object sender, EventArgs e)
		{
			if (loadingEntries)
				return;
			
			int a;
			if (!int.TryParse (alphaEntry.Text, out a))
				return;
			
			currentColor = currentColor.WithAlpha ((double)a / 255d);
			LoadColorBoxSelection ();
		}

		void HandleHslChanged (object sender, EventArgs e)
		{
			if (loadingEntries)
				return;
			
			int h, s, l;
			if (!int.TryParse (hueEntry.Text, out h))
				return;
			if (!int.TryParse (satEntry.Text, out s))
				return;
			if (!int.TryParse (lightEntry.Text, out l))
				return;
			
			currentColor = Color.FromHsl ((double)h / 255d, (double)s / 255d, (double)l / 255d, currentColor.Alpha);
			LoadColorBoxSelection ();
			LoadRgbEntries ();
		}

		void HandleRgbChanged (object sender, EventArgs e)
		{
			if (loadingEntries)
				return;
			
			int r, g, b;
			if (!int.TryParse (redEntry.Text, out r))
				return;
			if (!int.TryParse (greenEntry.Text, out g))
				return;
			if (!int.TryParse (blueEntry.Text, out b))
				return;
			
			currentColor = new Color ((double)r / 255d, (double)g / 255d, (double)b / 255d, currentColor.Alpha);
			LoadColorBoxSelection ();
			LoadHslEntries ();
		}

		void HandleColorBoxSelectionChanged ()
		{
			currentColor = Color.FromHsl (
				hsBox.SelectedColor.Hue,
				hsBox.SelectedColor.Saturation,
				lightBox.Light,
				currentColor.Alpha);
			
			colorBox.Color = currentColor;
			LoadHslEntries ();
			LoadRgbEntries ();
		}
		
		void LoadAlphaEntry ()
		{
			alphaEntry.Text = ((int)(currentColor.Alpha * 255)).ToString ();
		}
		
		void LoadHslEntries ()
		{
			loadingEntries = true;
			hueEntry.Text = ((int)(currentColor.Hue * 255)).ToString ();
			satEntry.Text = ((int)(currentColor.Saturation * 255)).ToString ();
			lightEntry.Text = ((int)(currentColor.Light * 255)).ToString ();
			loadingEntries = false;
		}

		void LoadRgbEntries ()
		{
			loadingEntries = true;
			redEntry.Text = ((int)(currentColor.Red * 255)).ToString ();
			greenEntry.Text = ((int)(currentColor.Green * 255)).ToString ();
			blueEntry.Text = ((int)(currentColor.Blue * 255)).ToString ();
			loadingEntries = false;
		}
		
		void LoadColorBoxSelection ()
		{
			hsBox.SelectedColor = currentColor;
			lightBox.Light = currentColor.Light;
			lightBox.Hue = hsBox.SelectedColor.Hue;
			lightBox.Saturation = hsBox.SelectedColor.Saturation;
			colorBox.Color = currentColor;
		}
		
		#region IColorSelectorBackend implementation
		public Color Color {
			get {
				return currentColor;
			}
			set {
				currentColor = value;
				LoadColorBoxSelection ();
				LoadRgbEntries ();
				LoadHslEntries ();
				LoadAlphaEntry ();
			}
		}

		public bool SupportsAlpha {
			get {
				return alphaControls [0].Visible;
			}
			set {
				foreach (var w in alphaControls)
					w.Visible = value;
			}
		}
		#endregion
	}
	
	class HueBox: Canvas
	{
		const int size = 150;
		const int padding = 3;
		bool buttonDown;
		Point selection;
		Image colorBox;
		double light;
		
		public double Light {
			get {
				return light;
			}
			set {
				light = value;
				if (colorBox != null) {
					colorBox.Dispose ();
					colorBox = null;
				}
				QueueDraw ();
			}
		}
		
		public Color SelectedColor {
			get { return GetColor ((int)selection.X, (int)selection.Y); }
			set {
				selection.X = (size - 1) * value.Hue;
				selection.Y = (size - 1) * (1 - value.Saturation);
				QueueDraw ();
			}
		}
		
		public HueBox ()
		{
			MinWidth = size + padding * 2;
			MinHeight = size + padding * 2;
		}
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			if (colorBox == null) {
				ImageBuilder ib = new ImageBuilder (size, size);
				for (int i=0; i<size; i++) {
					for (int j=0; j<size; j++) {
						ib.Context.Rectangle (i, j, 1, 1);
						ib.Context.SetColor (GetColor (i,j));
						ib.Context.Fill ();
					}
				}
				colorBox = ib.ToImage ();
				ib.Dispose ();
			}
			ctx.DrawImage (colorBox, padding, padding);
			ctx.SetLineWidth (1);
			ctx.SetColor (Colors.Black);
			ctx.Rectangle (selection.X + padding - 2 + 0.5, selection.Y + padding - 2 + 0.5, 4, 4);
			ctx.Stroke ();
		}
		
		Color GetColor (int x, int y)
		{
			return Color.FromHsl ((double)x / (double)(size-1), (double)(size - 1 - y) / (double)(size-1), Light);
		}
		
		protected override void OnButtonPressed (ButtonEventArgs args)
		{
			base.OnButtonPressed (args);
			buttonDown = true;
			selection = new Point (args.X - padding, args.Y - padding);
			OnSelectionChanged ();
			QueueDraw ();
		}
		
		protected override void OnButtonReleased (ButtonEventArgs args)
		{
			base.OnButtonReleased (args);
			buttonDown = false;
			QueueDraw ();
		}
		
		protected override void OnMouseMoved (MouseMovedEventArgs args)
		{
			base.OnMouseMoved (args);
			if (buttonDown) {
				QueueDraw ();
				selection = new Point (args.X - padding, args.Y - padding);
				OnSelectionChanged ();
			}
		}
		
		void OnSelectionChanged ()
		{
			if (selection.X < 0)
				selection.X = 0;
			if (selection.Y < 0)
				selection.Y = 0;
			if (selection.X >= size)
				selection.X = size - 1;
			if (selection.Y >= size)
				selection.Y = size - 1;
			if (SelectionChanged != null)
				SelectionChanged (this, EventArgs.Empty);
		}
		
		public event EventHandler SelectionChanged;
	}
	
	class LightBox: Canvas
	{
		const int padding = 3;
		double light;
		double saturation;
		double hue;
		bool buttonPressed;

		public double Hue {
			get {
				return hue;
			}
			set {
				hue = value;
				QueueDraw ();
			}
		}
		
		public double Saturation {
			get {
				return saturation;
			}
			set {
				saturation = value;
				QueueDraw ();
			}
		}
		
		public double Light {
			get {
				return light;
			}
			set {
				light = value;
				QueueDraw ();
			}
		}	
		
		public LightBox ()
		{
			MinWidth = 20;
			MinHeight = 20;
		}
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			double width = Size.Width - padding * 2;
			int range = (int)Size.Height - padding * 2;
			for (int n=0; n < range; n++) {
				ctx.Rectangle (padding, padding + n, width, 1);
				ctx.SetColor (Color.FromHsl (hue, saturation, (double)(range - n - 1) / (double)(range - 1)));
				ctx.Fill ();
			}
			ctx.Rectangle (0.5, padding + (int)(((double)range) * (1-light)) + 0.5 - 2, Size.Width - 1, 4);
			ctx.SetColor (Colors.Black);
			ctx.SetLineWidth (1);
			ctx.Stroke ();
		}
		
		protected override void OnButtonPressed (ButtonEventArgs args)
		{
			base.OnButtonPressed (args);
			buttonPressed = true;
			OnSelectionChanged ((int)args.Y - padding);
			QueueDraw ();
		}
		
		protected override void OnButtonReleased (ButtonEventArgs args)
		{
			base.OnButtonReleased (args);
			buttonPressed = false;
			QueueDraw ();
		}
		
		protected override void OnMouseMoved (MouseMovedEventArgs args)
		{
			base.OnMouseMoved (args);
			if (buttonPressed) {
				OnSelectionChanged ((int)args.Y - padding);
				QueueDraw ();
			}
		}
		
		void OnSelectionChanged (int y)
		{
			int range = (int)Size.Height - padding * 2;
			if (y < 0)
				y = 0;
			if (y >= range)
				y = range - 1;
			light = 1 - ((double) y / (double)(range - 1));
			if (SelectionChanged != null)
				SelectionChanged (this, EventArgs.Empty);
		}
		
		public event EventHandler SelectionChanged;
	}
	
	class ColorSelectionBox: Canvas
	{
		Color color;
		
		public Color Color {
			get {
				return color;
			}
			set {
				color = value;
				QueueDraw ();
			}
		}
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			ctx.Rectangle (Bounds);
			ctx.SetColor (Colors.White);
			ctx.Fill ();
			
			ctx.MoveTo (0, 0);
			ctx.LineTo (Size.Width, 0);
			ctx.LineTo (0, Size.Height);
			ctx.LineTo (0, 0);
			ctx.SetColor (Colors.Black);
			ctx.Fill ();
			
			ctx.Rectangle (Bounds);
			ctx.SetColor (color);
			ctx.Fill ();
		}
	}
}

