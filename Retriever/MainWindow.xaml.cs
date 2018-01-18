using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Retriever
{
    
    public partial class MainWindow : Window
    {
        public Reader ReaderInfo { get; private set; }
        public Reader ReaderInfo_WersjaBios { get; private set; }
        public ModelListSource ListSource;
        public Gatherer GathererInfo { get; private set; }
        public Model ThisComputer { get; private set; }
        public RetrieverInfo Retriever2 { get; private set; }
        public Status Statusy { get; private set; }

        public MainWindow()
        {
            //AktwacjaWindows ac = new AktwacjaWindows();
            //try
            {
                InitializeComponent();
                PrzygotujAplikacje();
                //UstawTimer();
            }
            //catch (Exception e)
            //{

            //    MessageBox.Show(string.Format(e.Data + "\n\n" + e.HelpLink + "\n\n" + e.InnerException + "\n\n" + e.Message + "\n\n" + e.Source + "\n\n" + e.StackTrace + "\n\n" + e.TargetSite));
            //}
        }

        void PrzygotujAplikacje()
        {
            //Wczytanie i przypisanie listy modeli do grida
            ListSource = new ModelListSource(new Reader());           
            gridModele.ItemsSource = ListSource.ListaModeli;
      
            //Wczytanie danych rzeczywistych komputera i przypisanie do gridów
            GathererInfo = new Gatherer();

            //Tworzenie dodatkowych kontrolek z informacjami
            CreateSwmDataControls(GathererInfo.Swm);
            CreateWearLevelDataControls(GathererInfo.Komputer.WearLevel);
            CreateDiscDataHeaders(GathererInfo.Dyski);
            CreateDevMgmtDataControls(GathererInfo.MenedzerUrzadzen);
            CreateNetDevDataControls(GathererInfo.UrzadzeniaSieciowe);
            CreateDiscDataControls(GathererInfo.Dyski, ref spDyskiGatherer);

            //Kod wyszukiwarki modeli
            CollectionView widok = (CollectionView)CollectionViewSource.GetDefaultView(gridModele.ItemsSource);
            widok.Filter = FiltrUzytkownika;

            //Próba określenia modelu i automatycznego wczytania danych konkretnrgo modelu do zestawienia
            var q = from item in ListSource.ListaModeli
                    where item.MD.Equals(GathererInfo.Komputer.MD)
                    select item;

            if (q.Count() != 0)
            {
                ReaderInfo = new Reader(q.First() == null ? gridModele.SelectedItem as Model : q.First() as Model);
                ThisComputer = q.First() == null ? gridModele.SelectedItem as Model : q.First() as Model;
            }
               
            else
            {
                ReaderInfo = new Reader(gridModele.SelectedItem as Model);
                ThisComputer = gridModele.SelectedItem as Model;
            }

            ReaderInfo_WersjaBios = new Reader(@"\Medion_NB_Bios.xlsx", ReaderInfo.Komputer);
            spReaderBios.DataContext = ReaderInfo_WersjaBios;
            CreateDiscDataControls(ReaderInfo.Dyski, ref spDyskiReader);            

            //Dodanie kontekstu danych do grida
            Retriever2 = new RetrieverInfo(ReaderInfo, GathererInfo);
            gridZestawienie.DataContext = Retriever2;

            ////Utworzenie instatncji statusów
            //Statusy = new Status();
            //tbWinStatus.DataContext = tbSecureBootStatus.DataContext = Statusy;
            
        }

        #region Wczytywanie nowych danych z pliku excel
        //Metoda wczytująca dane aktualnie zaznaczonego rekordu
        private void gridModele_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReaderInfo = new Reader((gridModele.SelectedItem as Model) == null ? ThisComputer : gridModele.SelectedItem as Model);
            Retriever2 = new RetrieverInfo(ReaderInfo, GathererInfo);
            gridZestawienie.DataContext = Retriever2;
            spReaderBios.DataContext = ReaderInfo_WersjaBios;
            CreateDiscDataControls(ReaderInfo.Dyski, ref spDyskiReader);
        }
        #endregion

        #region Wyszukiwarka
        bool FiltrUzytkownika(object item)
        {
            if (String.IsNullOrEmpty(txtWyszukiwarka.Text) || txtWyszukiwarka.Text.Length > 5 || txtWyszukiwarka.Text.Length < 5) return true;
            else return (((item as Model).MD.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0));
            //Wyszukiwanie również po msn: else return (((item as Model).MD.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0) || ((item as Model).MSN.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        //Metoda wyszukiwania z listy modeli
        void txtWyszukiwarka_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(gridModele.ItemsSource).Refresh();
            if (gridModele.SelectedItem as Model == null)
                gridModele.SelectedIndex = 1;

            ThisComputer = gridModele.SelectedItem as Model;
        }
        #endregion

        #region Ustawienie timera do odświeżania danych
        void UstawTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Statusy.RefreshBatteriesState);
            timer.Interval = new TimeSpan(0, 1, 0);
            timer.Start();
        }
        #endregion

        #region Dynamiczne twozenie kontrolek
        //Dodawanie kontrolek SWM
        public void CreateSwmDataControls(SWM[] Swm)
        {
            spSwm.Children.Clear();
            for(int i = 0; i < Swm.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}: {1}", Swm[i].Dysk[0], Swm[i].Swm);
                text.Style = Resources["PropertyValue"] as Style;
                spSwm.Children.Add(text);
            }  
        }

        //Dodawanie kontrolek Wear Level
        public void CreateWearLevelDataControls(double[] wl)
        {
            spWearLevel.Children.Clear();
            for (int i = 0; i < wl.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}%", wl[i]);
                text.Style = Resources["PropertyValue"] as Style;
                spWearLevel.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z pojemnościami dysków
        public void CreateDiscDataControls(Storage[] Disc, ref StackPanel control)
        {
            control.Children.Clear();
            for (int i = 0; i < Disc.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}GB", Disc[i].Pojemnosc);
                text.Style = Resources["PropertyValue"] as Style;
                control.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami dysków
        public void CreateDiscDataHeaders(Storage[] Disc)
        {
            spDyskiNazwa.Children.Clear();
            for (int i = 0; i < Disc.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Disc[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                DockPanel.SetDock(text, Dock.Top);
                spDyskiNazwa.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami urządzeń z błędami
        public void CreateDevMgmtDataControls(DeviceManager[] Dev)
        {
            spDeviceManagerCaption.Children.Clear();
            spDeviceManagerErrorDescription.Children.Clear();
            for (int i = 0; i < Dev.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                DockPanel.SetDock(text, Dock.Top);
                spDeviceManagerCaption.Children.Add(text);
                text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].TrescBledu);
                text.Style = Resources["PropertyValue"] as Style;
                spDeviceManagerErrorDescription.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami dysków
        public void CreateNetDevDataControls(NetDevice[] Dev)
        {
            spDeviceCaption.Children.Clear();
            spDeviceMACAddress.Children.Clear();
            for (int i = 0; i < Dev.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                text.MaxWidth = 250;
                text.Height = 50;
                text.TextWrapping = TextWrapping.Wrap;
                DockPanel.SetDock(text, Dock.Top);
                spDeviceCaption.Children.Add(text);
                text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].AdresMAC);
                text.Height = 50;
                text.Style = Resources["PropertyValue"] as Style;
                spDeviceMACAddress.Children.Add(text);
            }
        }
        #endregion

       
    }
}