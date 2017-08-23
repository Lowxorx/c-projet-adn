using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace NodeNet.Utilities
{

    public class TextboxHelper : Behavior<ScrollViewer>
    {
        private double height;
        private ScrollViewer scrollViewer;

        protected override void OnAttached()
        {
            base.OnAttached();

            scrollViewer = AssociatedObject;
            scrollViewer.LayoutUpdated += ScrollViewer_LayoutUpdated;
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
                scrollViewer.LayoutUpdated -= ScrollViewer_LayoutUpdated;
            }
        }
    }
}
