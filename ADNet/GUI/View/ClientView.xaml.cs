using ADNet.GUI.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace ADNet.GUI.View
{
    /// <summary>
    /// Logique d'interaction pour OrchView.xaml
    /// </summary>
    public partial class ClientView : Window
    {
        public ClientView()
        {
            InitializeComponent();
            VMClientView vm = (VMClientView)DataContext;
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
