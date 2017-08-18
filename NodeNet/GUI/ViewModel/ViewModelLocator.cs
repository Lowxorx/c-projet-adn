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

        private static VMMonitoringUC vMLMonitorUc;
        public static VMMonitoringUC VMLMonitorUcStatic
        {
            get
            {
                if (vMLMonitorUc == null)
                    vMLMonitorUc = new VMMonitoringUC();
                return vMLMonitorUc;
            }
        }
        public VMMonitoringUC VMLMonitorUc
        {
            get { return VMLMonitorUcStatic; }
        }

        private static VMLogBox vMLLogBoxUc;
        public static VMLogBox VMLLogBoxUcStatic
        {
            get
            {
                if (vMLLogBoxUc == null)
                    vMLLogBoxUc = new VMLogBox();
                return vMLLogBoxUc;
            }
        }
        public VMLogBox VMLLogBoxUc
        {
            get { return VMLLogBoxUcStatic; }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}