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
		SpinButton hueEntry;
		SpinButton satEntry;
		SpinButton lightEntry;
		SpinButton redEntry;
		SpinButton greenEntry;
		SpinButton blueEntry;
		SpinButton alphaEntry;
		HSlider alphaSlider;
		HSeparator alphaSeparator;
		Color currentColor;
		bool loadingEntries;
		List<Widget> alphaControls = new List<Widget> ();
		bool enableColorChangedEvent;
		
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
			
			selBox.Add (hsBox, 0, 0);
			selBox.Add (lightBox, 1, 0);
			
			box.PackStart (selBox);
			
			const int entryWidth = 40;
			VBox entryBox = new VBox ();
			Table entryTable = new Table ();
			
			entryTable.Add (new Label ("Color:"), 0, 0);
			entryTable.Add (colorBox, 1, 0, colspan:4);
			entryTable.Add (new HSeparator (), 0, 1, colspan:5);
			
			int r = 2;
			entryTable.Add (new Label ("Hue:"), 0, r);
			entryTable.Add (hueEntry = new SpinButton () { 
				MinWidth = entryWidth, MinimumValue = 0, MaximumValue = 360, Digits = 0, IncrementValue = 1 }, 1, r++);
			
			entryTable.Add (new Label ("Saturation:"), 0, r);
			entryTable.Add (satEntry = new SpinButton () { 
				MinWidth = entryWidth, MinimumValue = 0, MaximumValue = 100, Digits = 0, IncrementValue = 1 }, 1, r++);
			
			entryTable.Add (new Label ("Light:"), 0, r);
			entryTable.Add (lightEntry = new SpinButton () { 
				MinWidth = entryWidth, MinimumValue = 0, MaximumValue = 100, Digits = 0, IncrementValue = 1 }, 1, r++);
			
			r = 2;
			entryTable.Add (new Label ("Red:"), 3, r);
			entryTable.Add (redEntry = new SpinButton () { 
				MinWidth = entryWidth, MinimumValue = 0, MaximumValue = 255, Digits = 0, IncrementValue = 1 }, 4, r++);
			
			entryTable.Add (new Label ("Green:"), 3, r);
			entryTable.Add (greenEntry = new SpinButton () { 
				MinWidth = entryWidth, MinimumValue = 0, MaximumValue = 255, Digits = 0, IncrementValue = 1 }, 4, r++);
			
			entryTable.Add (new Label ("Blue:"), 3, r);
			entryTable.Add (blueEntry = new SpinButton () { 
				MinWidth = entryWidth, MinimumValue = 0, MaximumValue = 255, Digits = 0, IncrementValue = 1 }, 4, r++);
			
			Label label;
			entryTable.Add (alphaSeparator = new HSeparator (), 0, r++, colspan:5);
			entryTable.Add (label = new Label ("Opacity:"), 0, r);
			entryTable.Add (alphaSlider = new HSlider () {
				MinimumValue = 0, MaximumValue = 255,  }, 1, r, colspan: 3);
			entryTable.Add (alphaEntry = new SpinButton () { 
				MinWidth = entryWidth, MinimumValue = 0, MaximumValue = 255, Digits = 0, IncrementValue = 1 }, 4, r);
			
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
			
			hueEntry.ValueChanged += HandleHslChanged;
			satEntry.ValueChanged += HandleHslChanged;
			lightEntry.ValueChanged += HandleHslChanged;
			redEntry.ValueChanged += HandleRgbChanged;
			greenEntry.ValueChanged += HandleRgbChanged;
			blueEntry.ValueChanged += HandleRgbChanged;
			alphaEntry.ValueChanged += HandleAlphaChanged;
			alphaSlider.ValueChanged += HandleAlphaChanged;
			
			Color = Colors.White;
		}

		void HandleAlphaChanged (object sender, EventArgs e)
		{
			if (loadingEntries)
				return;

			if (sender == alphaSlider)
				alphaEntry.Value = alphaSlider.Value;
			if (sender == alphaEntry)
				alphaSlider.Value = alphaEntry.Value;
			
			int a = Convert.ToInt32 (alphaEntry.Value);
			
			currentColor = currentColor.WithAlpha ((double)a / 255d);
			LoadColorBoxSelection ();
			HandleColorChanged ();
		}

		void HandleHslChanged (object sender, EventArgs e)
		{
			if (loadingEntries)
				return;

			int h = Convert.ToInt32 (hueEntry.Value);
			int s = Convert.ToInt32 (satEntry.Value);
			int l = Convert.ToInt32 (lightEntry.Value);
						
			currentColor = Color.FromHsl ((double)h / 360d, (double)s / 100d, (double)l / 100d, currentColor.Alpha);
			LoadColorBoxSelection ();
			LoadRgbEntries ();
			HandleColorChanged ();
		}

		void HandleRgbChanged (object sender, EventArgs e)
		{
			if (loadingEntries)
				return;

			int r = Convert.ToInt32 (redEntry.Value);
			int g = Convert.ToInt32 (greenEntry.Value);
			int b = Convert.ToInt32 (blueEntry.Value);

			currentColor = new Color ((double)r / 255d, (double)g / 255d, (double)b / 255d, currentColor.Alpha);
			LoadColorBoxSelection ();
			LoadHslEntries ();
			HandleColorChanged ();
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
			HandleColorChanged ();
		}
		
		void LoadAlphaEntry ()
		{
			alphaEntry.Value = ((int)(currentColor.Alpha * 255));
			alphaSlider.Value = ((int)(currentColor.Alpha * 255));
		}
		
		void LoadHslEntries ()
		{
			loadingEntries = true;
			hueEntry.Value = ((int)(currentColor.Hue * 360));
			satEntry.Value = ((int)(currentColor.Saturation * 100));
			lightEntry.Value = ((int)(currentColor.Light * 100));
			loadingEntries = false;
		}

		void LoadRgbEntries ()
		{
			loadingEntries = true;
			redEntry.Value = ((int)(currentColor.Red * 255));
			greenEntry.Value = ((int)(currentColor.Green * 255));
			blueEntry.Value = ((int)(currentColor.Blue * 255));
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


		protected new IColorSelectorEventSink EventSink {
			get { return (IColorSelectorEventSink)base.EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ColorSelectorEvent) {
				switch ((ColorSelectorEvent)eventId) {
					case ColorSelectorEvent.ColorChanged: enableColorChangedEvent = true; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ColorSelectorEvent) {
				switch ((ColorSelectorEvent)eventId) {
					case ColorSelectorEvent.ColorChanged: enableColorChangedEvent = false; break;
				}
			}
		}

		void HandleColorChanged ()
		{
			if (enableColorChangedEvent)
			Application.Invoke (delegate {
				EventSink.OnColorChanged ();
			});
		}
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
				using (var ib = new ImageBuilder (size, size)) {
					for (int i=0; i<size; i++) {
						for (int j=0; j<size; j++) {
							ib.Context.Rectangle (i, j, 1, 1);
							ib.Context.SetColor (GetColor (i,j));
							ib.Context.Fill ();
						}
					}

					if (ParentWindow != null)
						colorBox = ib.ToBitmap (this); // take screen scale factor into account
					else
						colorBox = ib.ToBitmap ();
				}
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

