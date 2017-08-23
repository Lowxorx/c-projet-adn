using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace ADNet.GUI.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
        }
        private static VmadNetLauncher vMadNetLauncher;
        public static VmadNetLauncher VmadNetLauncherStatic => vMadNetLauncher ?? (vMadNetLauncher = new VmadNetLauncher());
        public VmadNetLauncher VmLlauncher => VmadNetLauncherStatic;
        private static VmClientView vMlCli;
        public static VmClientView VmlCliStatic => vMlCli ?? (vMlCli = new VmClientView());
        public VmClientView VmlCli => VmlCliStatic;
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}