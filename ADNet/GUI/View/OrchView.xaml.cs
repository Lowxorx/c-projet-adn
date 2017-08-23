using System.Windows.Input;
using ADNet.GUI.ViewModel;

namespace ADNet.GUI.View
{
    /// <summary>
    /// Logique d'interaction pour Window1.xaml
    /// </summary>
    public partial class OrchView
    {
        public OrchView()
        {
            InitializeComponent();

            VmOrchView vm = (VmOrchView)DataContext;
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
