using ADNet.GUI.ViewModel;
using System;
using System.Windows;

namespace ADNet.GUI.View
{
    /// <summary>
    /// Logique d'interaction pour ADNetLauncher.xaml
    /// </summary>
    public partial class ADNetLauncher : Window
    {
        public ADNetLauncher()
        {
            InitializeComponent();
            VMADNetLauncher vm = (VMADNetLauncher)DataContext;
            if ( vm.CloseAction == null)
                vm.CloseAction = new Action(() => Close());
        }
    }
}
