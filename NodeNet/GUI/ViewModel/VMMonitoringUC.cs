using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NodeNet.Network.Nodes;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NodeNet.GUI.ViewModel
{
    class VMMonitoringUC : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }

        ObservableCollection<Node> nodes;
        public ObservableCollection<Node> Nodes
        {
            get { return nodes; }
            set
            {
                nodes = value;
                RaisePropertyChanged(() => Nodes);
            }
        }


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
