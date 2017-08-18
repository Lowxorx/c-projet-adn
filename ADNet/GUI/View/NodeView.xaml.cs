using ADNet.GUI.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace c_projet_adn.GUI.View
{
    /// <summary>
    /// Logique d'interaction pour Window1.xaml
    /// </summary>
    public partial class NodeView : Window
    {

        public NodeView()
        {
            InitializeComponent();
            VMNodeView vm = (VMNodeView)DataContext;
            if (vm.CloseAction == null)
            {
                vm.CloseAction = new Action(() => Close());
            }
            MouseDown += Window_MouseDown;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
