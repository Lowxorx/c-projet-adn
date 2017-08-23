using ADNet.GUI.ViewModel;
using System.Windows.Input;

namespace ADNet.GUI.View
{
    /// <summary>
    /// Logique d'interaction pour ADNetLauncher.xaml
    /// </summary>
    public partial class AdNetLauncher
    {
        public AdNetLauncher()
        {
            InitializeComponent();
            VmadNetLauncher vm = (VmadNetLauncher)DataContext;
            if ( vm.CloseAction == null)
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
