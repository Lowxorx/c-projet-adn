using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace NodeNet.GUI.ViewModel
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

        private static VmMonitoringUc vMlMonitorUc;
        public static VmMonitoringUc VmlMonitorUcStatic => vMlMonitorUc ?? (vMlMonitorUc = new VmMonitoringUc());
        public VmMonitoringUc VmlMonitorUc => VmlMonitorUcStatic;

        private static VmLogBox vMlLogBoxUc;
        public static VmLogBox VmlLogBoxUcStatic => vMlLogBoxUc ?? (vMlLogBoxUc = new VmLogBox());
        public VmLogBox VmlLogBoxUc => VmlLogBoxUcStatic;

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}