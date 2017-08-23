using ADNet.GUI.ViewModel;
using System.Windows.Input;

namespace ADNet.GUI.View
{
    /// <summary>
    /// Logique d'interaction pour OrchView.xaml
    /// </summary>
    public partial class ClientView
    {
        public ClientView()
        {
            InitializeComponent();
            VmClientView vm = (VmClientView)DataContext;
            if (vm.CloseAction == null)
            {
                vm.CloseAction = Close;
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
