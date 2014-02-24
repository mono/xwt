using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Xwt.Backends;

namespace Xwt.WPFBackend
{
    class CellViewBackend : ICellViewBackend, ICellDataSource
    {
        FrameworkElement currentElement;

        public CellViewBackend()
        {
        }

        public void Initialize(CellView cellView, FrameworkElementFactory factory)
        {
            CellView = cellView;
        }

        public void Load (FrameworkElement elem)
        {
            currentElement = elem;
            CellFrontend.Load(this);
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
        }

        public void DisableEvent(object eventId)
        {
        }

        public object GetValue(IDataField field)
        {
            if (currentElement.DataContext == null)
                return null;
            return ((ValuesContainer)currentElement.DataContext)[field.Index];
        }
    }
}
