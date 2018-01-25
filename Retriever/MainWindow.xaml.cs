using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Retriever
{
    
    public partial class MainWindow : Window
    {
        //public Reader ReaderInfo { get; private set; }
        //public Gatherer GathererInfo { get; private set; }
        public RetrieverInfo Retriever { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            PrzygotujAplikacje();
                //UstawTimer();     
        }

        void PrzygotujAplikacje()
        {
            //Utworzenie instancji Retriever
            Retriever = new RetrieverInfo(new Reader(), new Gatherer(), new Status());
            gridZestawienie.DataContext = Retriever;

            //Przypisanie listy modeli do grida
            if (Retriever.ReaderInfo != null)
            {
                gridModele.ItemsSource = Retriever.ReaderInfo.listaModeli;
                gridModele.SelectedIndex = 0;
            }

            //Próba odczytania modelu komputera
            if (Retriever.GathererInfo.Komputer.MD != "" && Retriever.ReaderInfo != null)
            {
                var model = from z in Retriever.ReaderInfo.listaModeli
                            where z.MD == Retriever.GathererInfo.Komputer.MD
                            select z;

                if (model.Count() > 0)
                    Retriever.ReaderInfo.ReadData(model.First() as Model);
            }
            else if (Retriever.ReaderInfo != null)
                Retriever.ReaderInfo.ReadData(gridModele.SelectedItem as Model);

            //Tworzenie dynamicznych kontrolek
            DynamicControls();

            //Kod wyszukiwarki modeli
            CollectionView widok = (CollectionView)CollectionViewSource.GetDefaultView(gridModele.ItemsSource);
            widok.Filter = FiltrUzytkownika;

        }

        #region Wczytywanie nowych danych z pliku excel
        //Metoda wczytująca dane aktualnie zaznaczonego rekordu
        private void gridModele_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Retriever.ReaderInfo.ReadData(gridModele.SelectedItem as Model);
            DynamicControls();
        }
        #endregion

        #region Wyszukiwarka
        bool FiltrUzytkownika(object item)
        {
            if (String.IsNullOrEmpty(txtWyszukiwarka.Text) || txtWyszukiwarka.Text.Length > 5 || txtWyszukiwarka.Text.Length < 5) return true;
            else return ((item as Model).MD == Retriever.GathererInfo.Komputer.MD);
            //Wyszukiwanie również po msn: else return (((item as Model).MD.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0) || ((item as Model).MSN.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        //Metoda wyszukiwania z listy modeli
        void txtWyszukiwarka_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(gridModele.ItemsSource).Refresh();
            if (gridModele.SelectedItem as Model == null)
                gridModele.SelectedIndex = 1;
        }
        #endregion

        #region Ustawienie timera do odświeżania danych
        //void UstawTimer()
        //{
        //    DispatcherTimer timer = new DispatcherTimer();
        //    timer.Tick += new EventHandler(Statusy.RefreshBatteriesState);
        //    timer.Interval = new TimeSpan(0, 1, 0);
        //    timer.Start();
        //}
        #endregion

        #region Dynamiczne twozenie kontrolek
        //Metoda wywołująca wszystkie poniższe
        void DynamicControls()
        {
            CreateSwmData(          Retriever.GathererInfo.Swm);
            CreateWearLevelData(    Retriever.GathererInfo.Komputer.WearLevel);
            CreateDiscDataHeaders(  Retriever.GathererInfo.Dyski);
            CreateDevMgmtData(      Retriever.GathererInfo.MenedzerUrzadzen);
            CreateNetDevData(       Retriever.GathererInfo.UrzadzeniaSieciowe);
            CreateDiscData(         Retriever.GathererInfo.Dyski, ref spDyskiGatherer);
            CreateDiscData(         Retriever.ReaderInfo.Dyski, ref spDyskiReader);
            CreateGraphicCardData(  Retriever.GathererInfo.KartyGraficzne);
        }

        //Dodawanie kontrolek SWM
        void CreateSwmData(SWM[] Swm)
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
        void CreateWearLevelData(double[] wl)
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
        void CreateDiscData(Storage[] Disc, ref StackPanel control)
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
        void CreateDiscDataHeaders(Storage[] Disc)
        {
            dpDyskiNazwa.Children.Clear();
            for (int i = 0; i < Disc.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Disc[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                DockPanel.SetDock(text, Dock.Top);
                dpDyskiNazwa.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami urządzeń z błędami
        void CreateDevMgmtData(DeviceManager[] Dev)
        {
            dpDeviceManagerCaption.Children.Clear();
            spDeviceManagerErrorDescription.Children.Clear();
            for (int i = 0; i < Dev.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                DockPanel.SetDock(text, Dock.Top);
                dpDeviceManagerCaption.Children.Add(text);
                text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].TrescBledu);
                text.Style = Resources["PropertyValue"] as Style;
                spDeviceManagerErrorDescription.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami dysków
        void CreateNetDevData(NetDevice[] Dev)
        {
            dpDeviceCaption.Children.Clear();
            dpDeviceMACAddress.Children.Clear();
            for (int i = 0; i < Dev.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                text.MaxWidth = 250;
                text.Height = 50;
                text.TextWrapping = TextWrapping.Wrap;
                DockPanel.SetDock(text, Dock.Top);
                dpDeviceCaption.Children.Add(text);
                text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].AdresMAC);
                text.Height = 50;
                text.Style = Resources["PropertyValue"] as Style;
                dpDeviceMACAddress.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami kart graficznych
        void CreateGraphicCardData(GraphicCard[] Card)
        {
            dpGraphicCardCaption.Children.Clear();
            spGraphicCardDescription.Children.Clear();
            for (int i = 0; i < Card.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Card[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                text.MaxWidth = 250;
                text.Height = 50;
                text.TextWrapping = TextWrapping.Wrap;
                DockPanel.SetDock(text, Dock.Top);
                dpGraphicCardCaption.Children.Add(text);
                text = new TextBlock();
                text.Text = string.Format("{0}", Card[i].Opis);
                text.Height = 50;
                text.Style = Resources["PropertyValue"] as Style;
                spGraphicCardDescription.Children.Add(text);
            }
        }
        #endregion


    }
}