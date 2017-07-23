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

        private static VMADNetLauncher vMADNetLauncher;
        public static VMADNetLauncher VMADNetLauncherStatic
        {
            get
            {
                if (vMADNetLauncher == null)
                    vMADNetLauncher = new VMADNetLauncher();
                return vMADNetLauncher;
            }
        }
        public VMADNetLauncher VMLlauncher
        {
            get { return VMADNetLauncherStatic; }
        }

        private static VMClientView vMLCli;
        public static VMClientView VMLCliStatic
        {
            get
            {
                if (vMLCli == null)
                    vMLCli = new VMClientView();
                return vMLCli;
            }
        }
        public VMClientView VMLCli
        {
            get { return VMLCliStatic; }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}