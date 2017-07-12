using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NodeNet.GUI.ViewModel
{
    class VMMonitoringUC
    {
        public ICommand WindowLoaded { get; set; }
        public VMMonitoringUC()
        {
            WindowLoaded = new RelayCommand(OnLoad);
        }

        public void OnLoad()
        {
            // Implement Node's state listening 
        }
    }

}
