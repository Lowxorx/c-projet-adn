using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using NodeNet.GUI.View;

namespace NodeNet.GUI.ViewModel
{
    public class VmAboutBox : ViewModelBase
    {
        public ICommand WindowLoaded { get; set; }
        public ICommand CommandBtnClose { get; set; }
        public ICommand LinkCesi { get; set; }
        public ICommand LinkProjet { get; set; }
        public ICommand LinkBastien { get; set; }
        public ICommand LinkAlex { get; set; }
        public ICommand LinkRemi { get; set; }
        public ICommand Egg { get; set; }
        public Action CloseAction { get; set; }
        private BitmapImage imgSourceCesi;
        public BitmapImage ImgSourceCesi
        {
            get { return imgSourceCesi; }
            set
            {
                imgSourceCesi = value;
                RaisePropertyChanged(() => ImgSourceCesi);
            }
        }
        private BitmapImage imgSourceGit;
        public BitmapImage ImgSourceGit
        {
            get { return imgSourceGit; }
            set
            {
                imgSourceGit = value;
                RaisePropertyChanged(() => ImgSourceGit);
            }
        }


        [PreferredConstructor]
        public VmAboutBox()
        {
            CommandBtnClose = new RelayCommand(CloseWindow);
            WindowLoaded = new RelayCommand(OnLoad);
            LinkCesi = new RelayCommand(OpenLinkCesi);
            LinkProjet = new RelayCommand(OpenLinkProjet);
            LinkBastien = new RelayCommand(OpenLinkBastien);
            LinkAlex = new RelayCommand(OpenLinkRemi);
            LinkRemi = new RelayCommand(OpenLinkAlex);
            Egg = new RelayCommand(Lolilol);
            //string pathCesi = "pack://application:,,,/NodeNet;component/GUI/Resources.xaml";
            //string pathGit = "pack://application:,,,/Resources/gitlogo.png";
            //BitmapImage cesiImg = new BitmapImage(new Uri(pathCesi));
            //BitmapImage gitImg = new BitmapImage(new Uri(pathGit));
            //ImgSourceGit = gitImg;
            //ImgSourceCesi = cesiImg;
        }

        private void OnLoad()
        {
            // Loaded
        }
        private void CloseWindow()
        {
            CloseAction.Invoke();
        }
        private static void OpenLinkCesi()
        {
            Process.Start("https://www.cesi-alternance.fr");
        }
        private static void OpenLinkProjet()
        {
            Process.Start("https://github.com/lowxorx/c-projet-adn");
        }
        private static void OpenLinkBastien()
        {
            Process.Start("https://github.com/lowxorx");
        }
        private static void OpenLinkRemi()
        {
            Process.Start("https://github.com/RemiPlantade");
        }
        private static void OpenLinkAlex()
        {
            Process.Start("https://github.com/Alexandre-Schwarze");
        }

        private static void Lolilol()
        {
            AdnCalc egg = new AdnCalc();
            egg.Show();
        }
    }
}
