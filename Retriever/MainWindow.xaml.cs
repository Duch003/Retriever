using DataTypes;
using Gatherer;
using Reader;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Utilities;

namespace Retriever
{

    public partial class MainWindow : Window
    {
        public RetrieverInfo Retriever { get; private set; }
        FileSystemManager settings;
        DatabaseManager dbManager;
        bool sw = false;
        public MainWindow()
        {
            InitializeComponent();
            OdczytajKonfiguracjęIHasz();
            PrzygotujAplikacje();
            UstawTimer();
        }

        void OdczytajKonfiguracjęIHasz()
        {
            try
            {
                settings = new FileSystemManager();
                dbManager = new DatabaseManager(settings);

            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Błąd odczytu pliku konfiguracyjnego lub hasza", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }           
        }

        void PrzygotujAplikacje()
        {
            //Utworzenie instancji Retriever
            try
            {
                Retriever = new RetrieverInfo(new ReaderInfo(dbManager), new GathererInfo(), new Status());
            }
            catch(Exception e)
            {
                var message = string.Format("Wystąpił błąd podczas otwierania aplikacji:\n{0}", e.Message);
                MessageBox.Show(message, "Nie można otworzyć aplikacji", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            
            gridZestawienie.DataContext = Retriever;

            //Przypisanie listy modeli do grida
            if (Retriever.ReaderData.ListaModeli != null)
            {
                gridModele.ItemsSource = Retriever.ReaderData.ListaModeli;
            }

            //Próba odczytania modelu komputera
            if (Retriever.GathererData.Komputer.MD != "" && Retriever.ReaderData.ListaModeli != null)
            {
                var model = from z in Retriever.ReaderData.ListaModeli
                            where z.MD == Retriever.GathererData.Komputer.MD
                            select z;
                if (model.Count() > 0)
                    Retriever.ReaderData.ReadData(model.First() as Model);
            }
            else if (Retriever.ReaderData != null)
                Retriever.ReaderData.ReadData(gridModele.SelectedItem as Model);

            //Tworzenie dynamicznych kontrolek
            DynamicControls();

            //Kod wyszukiwarki modeli
            CollectionView widok = (CollectionView)CollectionViewSource.GetDefaultView(gridModele.ItemsSource);
            widok.Filter = FiltrUzytkownika;

        }

        #region Wczytywanie nowych danych z pliku excel
        //Metoda wczytująca dane aktualnie zaznaczonego rekordu
        //private void gridModele_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if ((gridModele.SelectedItem as Model) != null)
        //    {
        //        Retriever.ReaderInfo.ReadData(gridModele.SelectedItem as Model);
        //        DynamicControls();
        //    }
        //}
        #endregion

        #region Wyszukiwarka
        bool FiltrUzytkownika(object item)
        {
            if (String.IsNullOrEmpty(txtWyszukiwarka.Text)) return true;
            else return ((item as Model).MD.Contains(txtWyszukiwarka.Text));

            //Wyszukiwanie również po msn: else return (((item as Model).MD.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0) || ((item as Model).MSN.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        //Metoda wyszukiwania z listy modeli
        void txtWyszukiwarka_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length < 3)
                return;
            else if((sender as TextBox).Text.Length == 0 || (sender as TextBox).Text.Length > 2)
                CollectionViewSource.GetDefaultView(gridModele.ItemsSource).Refresh();
        }
        #endregion

        #region Ustawienie timera do odświeżania danych
        void UstawTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Retriever.Statusy.RefreshBatteriesState);
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Start();
        }
        #endregion

        #region Dynamiczne twozenie kontrolek
        //Metoda wywołująca wszystkie poniższe
        void DynamicControls()
        {
            CreateSwmData(          Retriever.GathererData.Swm);
            CreateWearLevelData(    Retriever.GathererData.Komputer.WearLevel);
            CreateDiscDataHeaders(  Retriever.GathererData.Dyski);
            CreateDevMgmtData(      Retriever.GathererData.MenedzerUrzadzen);
            CreateNetDevData(       Retriever.GathererData.UrzadzeniaSieciowe);
            CreateDiscData(         Retriever.GathererData.Dyski, ref spDyskiGatherer);           
            CreateGraphicCardData(  Retriever.GathererData.KartyGraficzne);
            if(Retriever.ReaderData.Dyski != null) CreateDiscData(Retriever.ReaderData.Dyski, ref spDyskiReader);
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
            spDyskiNazwa.Children.Clear();
            for (int i = 0; i < Disc.Length; i++)
            {
                var text = new Label();
                text.Content = string.Format("{0}", Disc[i].Nazwa);
                text.Style = Resources["BlueDescription"] as Style;
                //DockPanel.SetDock(text, Dock.Top);
                spDyskiNazwa.Children.Add(text);
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
            spGraphicCardCaption.Children.Clear();
            spGraphicCardDescription.Children.Clear();
            for (int i = 0; i < Card.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Card[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                text.MaxWidth = 250;
                text.Height = 50;
                text.TextWrapping = TextWrapping.Wrap;
                //DockPanel.SetDock(text, Dock.Top);
                spGraphicCardCaption.Children.Add(text);
                text = new TextBlock();
                text.Text = string.Format("{0}", Card[i].Opis);
                text.Height = 50;
                text.Style = Resources["PropertyValue"] as Style;
                spGraphicCardDescription.Children.Add(text);
            }
        }

        #endregion

        //Metoda obsługi programu klawiaturą
        private void KeyUpRecognize(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                    if (TabControl.SelectedIndex == 0) TabControl.SelectedIndex = 1;
                    else TabControl.SelectedIndex = 0;
                    break;
                case Key.Down:
                    if (TabControl.SelectedIndex == 1) Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset + 100);
                    else if (txtWyszukiwarka.IsFocused == false && gridModele.IsFocused == false) txtWyszukiwarka.Focus();
                    else if (txtWyszukiwarka.IsFocused == true) gridModele.Focus();
                    else if (gridModele.IsFocused == true) gridModele.SelectedIndex += 1;
                    break;
                case Key.Up:
                    if (TabControl.SelectedIndex == 1) Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset - 100);
                    else if (gridModele.IsFocused == true && gridModele.SelectedIndex == 0) txtWyszukiwarka.Focus();
                    else if (gridModele.SelectedIndex != 0)
                    {
                        gridModele.SelectedIndex -= 1;
                        gridModele.Focus();
                    }
                    break;
                case Key.Enter:
                    if (gridModele.IsFocused == true)
                    {
                        Retriever.ReaderData.ReadData(gridModele.SelectedItem as Model);
                        DynamicControls();
                        TabControl.SelectedIndex = 1;
                    }
                    break;
            }
        }

        private void DataGrid_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}