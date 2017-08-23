using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace NodeNet.Utilities
{

    public class TextboxHelper : Behavior<ScrollViewer>
    {
        private double height = 0.0d;
        private ScrollViewer scrollViewer = null;

        protected override void OnAttached()
        {
            base.OnAttached();

            scrollViewer = AssociatedObject;
            scrollViewer.LayoutUpdated += new EventHandler(ScrollViewer_LayoutUpdated);
        }

        private void ScrollViewer_LayoutUpdated(object sender, EventArgs e)
        {
            if (Math.Abs(scrollViewer.ExtentHeight - height) > 1)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
                height = scrollViewer.ExtentHeight;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (scrollViewer != null)
            {
                scrollViewer.LayoutUpdated -= new EventHandler(ScrollViewer_LayoutUpdated);
            }
        }
    }
}
