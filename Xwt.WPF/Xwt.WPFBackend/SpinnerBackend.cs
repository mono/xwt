﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.Backends;
using System.Windows.Media;
using System.Windows.Media.Animation;

using WpfRectangle = System.Windows.Shapes.Rectangle;
using WpfCanvas = System.Windows.Controls.Canvas;
using System.Windows;

namespace Xwt.WPFBackend
{
	public class SpinnerBackend : WidgetBackend, ISpinnerBackend
	{
		new WpfSpinButton Widget {
			get { return (WpfSpinButton) base.Widget; }
			set { base.Widget = value; }
		}
		public SpinnerBackend ()
		{
			Widget = new WpfSpinButton ();
		}

		public bool IsAnimating {
			get { return Widget.Storyboard.GetIsPaused (); }
		}
		
		public void StartAnimation ()
		{
			Widget.Storyboard.Begin ();
		}

		public void StopAnimation ()
		{
			Widget.Storyboard.Stop ();
		}
	}

	public class WpfSpinButton : System.Windows.Controls.Canvas, IWpfWidget
	{
		const int Duration = 1000;
		static int[] StartTimes = new[] {
			(int)(0.00 * Duration),
			(int)(0.70 * Duration),
			(int)(0.75 * Duration),
			(int)(0.99 * Duration)
		};

		static double[] Values = new[] {
			0.25,
			0.25,
			1,
			0.25
		};

		public Storyboard Storyboard {
			get; private set;
		}
		
		public WidgetBackend Backend {
			get; set;
		}

		public WpfSpinButton ()
		{
			Width = 25;
			Height = 25;
			Background = new SolidColorBrush (Colors.Transparent);
			Storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever, Duration = TimeSpan.FromMilliseconds (Duration) };

			for (int i = 0; i < 360; i += 30) {
				// Create the rectangle and centre it in our widget
				var rect = new WpfRectangle { Width = 2, Height = 8, Fill = new SolidColorBrush (Colors.Black), RadiusX = 1, RadiusY = 1, Opacity = Values[0] };
				WpfCanvas.SetTop (rect, (Height - rect.Height) / 2);
				WpfCanvas.SetLeft (rect, Width / 2);

				// Rotate the element by 'i' degrees, creating a circle out of all the elements
				var group = new TransformGroup ();
				group.Children.Add (new RotateTransform (i, 0.5, -6));
				group.Children.Add (new TranslateTransform (0, 10));
				rect.RenderTransform = group;

				// Set the animation
				var timeline = new DoubleAnimationUsingKeyFrames ();
				Storyboard.SetTarget (timeline, rect);
				Storyboard.SetTargetProperty (timeline, new PropertyPath ("Opacity"));

				var offset = Duration * (i / 360.0);
				for (int j = 0; j < StartTimes.Length; j++) {
					var start = (StartTimes[j] + offset) % Duration;
					timeline.KeyFrames.Add (new EasingDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan (TimeSpan.FromMilliseconds (start)), Value = Values[j] });
				}
				Storyboard.Children.Add (timeline);
				Children.Add (rect);
			}
		}

		protected override System.Windows.Size MeasureOverride (System.Windows.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}
	}
}
