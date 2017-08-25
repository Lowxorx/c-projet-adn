using System.Windows.Input;
using NodeNet.GUI.ViewModel;

namespace NodeNet.GUI.View
{
    /// <summary>
    /// Logique d'interaction pour AboutAppBox.xaml
    /// </summary>
    public partial class AboutAppBox
    {
        public AboutAppBox(VmAboutBox vm)
        {
            InitializeComponent();
            DataContext = vm;
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
