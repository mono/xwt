using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Xwt.Backends;
using Xwt.WPFBackend.Utilities;

namespace Xwt.WPFBackend
{
    class CellViewBackend : ICellViewBackend, ICellDataSource
    {
        FrameworkElement currentElement;
        WidgetEvent enabledEvents;
        ICellRendererTarget rendererTarget;

        public WidgetEvent EnabledEvents {
            get {
                return enabledEvents;
            }
        }

        public FrameworkElement CurrentElement {
            get {
                return currentElement;
            }
        }

        public ICellViewEventSink EventSink { get; private set; }

        public CellViewBackend()
        {
        }

        public void Initialize(CellView cellView, FrameworkElementFactory factory, ICellRendererTarget rendererTarget)
        {
            CellView = cellView;
            this.rendererTarget = rendererTarget;

			factory.AddHandler (UIElement.MouseEnterEvent, new MouseEventHandler (HandleMouseEnter));
			factory.AddHandler (UIElement.MouseLeaveEvent, new MouseEventHandler (HandleMouseLeave));
			factory.AddHandler (UIElement.MouseMoveEvent, new MouseEventHandler (HandleMouseMove));
			factory.AddHandler (UIElement.MouseDownEvent, new MouseButtonEventHandler (HandleMouseDown));
			factory.AddHandler (UIElement.MouseUpEvent, new MouseButtonEventHandler (HandleMouseUp));
			factory.AddHandler (UIElement.PreviewKeyDownEvent, new KeyEventHandler (HandlePreviewKeyDown));
			factory.AddHandler (UIElement.PreviewKeyUpEvent, new KeyEventHandler (HandlePreviewKeyUp));

			OnInitialize (cellView, factory);
        }

		public virtual void OnInitialize(CellView cellView, FrameworkElementFactory factory)
		{
		}

        public void Load (FrameworkElement elem)
        {
            currentElement = elem;
            EventSink = CellFrontend.Load(this);
        }

        public void SetCurrentEventRow ()
        {
            if (currentElement.DataContext == null)
                return;
            rendererTarget?.SetCurrentEventRow (currentElement.DataContext);
        }

        public CellView CellView { get; set; }

        public ApplicationContext ApplicationContext { get; set; }

        public ICellViewFrontend CellFrontend
        {
            get { return CellView; }
        }

        public Rectangle CellBounds
        {
            get {
                return new Rectangle(0, 0, currentElement.ActualWidth, currentElement.ActualHeight);
            }
        }

        public Rectangle BackgroundBounds
        {
            get
            {
                return new Rectangle(0, 0, currentElement.ActualWidth, currentElement.ActualHeight);
            }
        }

        public bool Selected
        {
            get { return false; }
        }

        public bool HasFocus
        {
            get { return false; }
        }

        public void InitializeBackend(object frontend, ApplicationContext context)
        {
            ApplicationContext = context;
        }

        public void EnableEvent(object eventId)
        {
			if (eventId is WidgetEvent)
				enabledEvents |= (WidgetEvent)eventId;
        }

        public void DisableEvent(object eventId)
        {
			if (eventId is WidgetEvent)
				enabledEvents &= ~(WidgetEvent)eventId;
		}

        void HandleMouseEnter(object sender, MouseEventArgs e)
        {
            if (enabledEvents.HasFlag(WidgetEvent.MouseEntered))
            {
                Load(sender as FrameworkElement);
                SetCurrentEventRow ();
                ApplicationContext.InvokeUserCode(EventSink.OnMouseEntered);
            }
        }

        void HandleMouseLeave(object sender, MouseEventArgs e)
        {
            if (enabledEvents.HasFlag(WidgetEvent.MouseExited))
            {
                Load(sender as FrameworkElement);
                SetCurrentEventRow ();
                ApplicationContext.InvokeUserCode(EventSink.OnMouseExited);
            }
        }

        void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (enabledEvents.HasFlag(WidgetEvent.MouseMoved))
            {
                var p = e.GetPosition(sender as FrameworkElement);
                Load(sender as FrameworkElement);
                if (!CellBounds.Contains(p.X, p.Y))
                    return;
                var a = new MouseMovedEventArgs(e.Timestamp, p.X, p.Y);

                SetCurrentEventRow ();
                ApplicationContext.InvokeUserCode(delegate
                    {
                        EventSink.OnMouseMoved(a);
                    });
                if (a.Handled)
                    e.Handled = true;
            }
        }

        void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (enabledEvents.HasFlag(WidgetEvent.ButtonPressed))
            {
                var a = e.ToXwtButtonArgs(sender as FrameworkElement);
                Load(sender as FrameworkElement);
                if (!CellBounds.Contains(a.X, a.Y))
                    return;

                SetCurrentEventRow ();
                ApplicationContext.InvokeUserCode(delegate
                    {
                        EventSink.OnButtonPressed(a);
                    });
                if (a.Handled)
                    e.Handled = true;
            }
        }

        void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (enabledEvents.HasFlag(WidgetEvent.ButtonReleased))
            {
                var a = e.ToXwtButtonArgs(sender as FrameworkElement);
                Load(sender as FrameworkElement);
                if (!CellBounds.Contains(a.X, a.Y))
                    return;

                SetCurrentEventRow ();
                ApplicationContext.InvokeUserCode(delegate
                    {
                        EventSink.OnButtonReleased(a);
                    });
                if (a.Handled)
                    e.Handled = true;
            }
        }

        void HandlePreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (enabledEvents.HasFlag(WidgetEvent.KeyPressed))
            {
                Load(sender as FrameworkElement);
                KeyEventArgs args;
                if (e.MapToXwtKeyArgs(out args))
                {
                    SetCurrentEventRow ();
                    ApplicationContext.InvokeUserCode(delegate
                        {
                            EventSink.OnKeyPressed(args);
                        });
                    if (args.Handled)
                        e.Handled = true;
                }
            }
        }

        void HandlePreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (enabledEvents.HasFlag(WidgetEvent.KeyReleased))
            {
                Load(sender as FrameworkElement);
                KeyEventArgs args;
                if (e.MapToXwtKeyArgs(out args))
                {
                    SetCurrentEventRow ();
                    ApplicationContext.InvokeUserCode(delegate
                        {
                            EventSink.OnKeyReleased(args);
                        });
                    if (args.Handled)
                        e.Handled = true;
                }
            }
        }

        public object GetValue(IDataField field)
        {
            if (currentElement.DataContext == null)
                return null;
            return ((ValuesContainer)currentElement.DataContext)[field.Index];
        }
    }
}
