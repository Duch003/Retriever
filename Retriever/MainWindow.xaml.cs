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
                Environment.Exit(0);
            }   
            //EmptyNetworkListException - gdy brak dostępnych połączeń sieciowych
            //DriversNotInstalledException - gdy nagrano zły image
            //Exception podczas próby otwarcia pliku bazy danych
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
                var message = string.Format("Wystąpił błąd podczas otwierania aplikacji:\n{0}" +
                    "\n{1}" +
                    "\n{2}" +
                    "\n{3}" +
                    "\n{4}" +
                    "", e.Message, e.StackTrace, e.Source, e.TargetSite, e.HelpLink);
                MessageBox.Show(message, "Nie można otworzyć aplikacji", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
            
            GridZestawienie.DataContext = Retriever;

            //Przypisanie listy modeli do grida
            if (Retriever.ReaderData.ListaModeli != null)
            {
                GridModele.ItemsSource = Retriever.ReaderData.ListaModeli;
            }

            //Próba odczytania modelu komputera
            if (Retriever.GathererData.Komputer.Md != "" && Retriever.ReaderData.ListaModeli != null)
            {
                var model = from z in Retriever.ReaderData.ListaModeli
                            where z.Md == Retriever.GathererData.Komputer.Md
                            select z;
                if (model.Count() > 0)
                    Retriever.ReaderData.ReadData(model.First() as Model);
            }
            else if (Retriever.ReaderData != null)
                Retriever.ReaderData.ReadData(GridModele.SelectedItem as Model);

            //Tworzenie dynamicznych kontrolek
            DynamicControls();

            //Kod wyszukiwarki modeli
            var widok = (CollectionView)CollectionViewSource.GetDefaultView(GridModele.ItemsSource);
            widok.Filter = FiltrUzytkownika;

        }

        #region Wyszukiwarka
        bool FiltrUzytkownika(object item)
        {
            if (String.IsNullOrEmpty(TxtWyszukiwarka.Text)) return true;
            else return ((item as Model).Md.Contains(TxtWyszukiwarka.Text));

            //Wyszukiwanie również po msn: else return (((item as Model).MD.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0) || ((item as Model).MSN.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        //Metoda wyszukiwania z listy modeli
        void txtWyszukiwarka_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length < 3)
                return;
            else if((sender as TextBox).Text.Length == 0 || (sender as TextBox).Text.Length > 2)
                CollectionViewSource.GetDefaultView(GridModele.ItemsSource).Refresh();
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
            CreateDiscData(         Retriever.GathererData.Dyski, ref SpDyskiGatherer);           
            CreateGraphicCardData(  Retriever.GathererData.KartyGraficzne);
            if(Retriever.ReaderData.Dyski != null) CreateDiscData(Retriever.ReaderData.Dyski, ref SpDyskiReader);
        }

        //Dodawanie kontrolek SWM
        void CreateSwmData(SWM[] Swm)
        {
            SpSwm.Children.Clear();
            for(var i = 0; i < Swm.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}: {1}", Swm[i].Dysk[0], Swm[i].Swm);
                text.Style = Resources["PropertyValue"] as Style;
                SpSwm.Children.Add(text);
            }  
        }

        //Dodawanie kontrolek Wear Level
        void CreateWearLevelData(double[] wl)
        {
            SpWearLevel.Children.Clear();
            for (var i = 0; i < wl.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}%", wl[i]);
                text.Style = Resources["PropertyValue"] as Style;
                SpWearLevel.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z pojemnościami dysków
        void CreateDiscData(Storage[] Disc, ref StackPanel control)
        {
            control.Children.Clear();
            for (var i = 0; i < Disc.Length; i++)
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
            SpDyskiNazwa.Children.Clear();
            for (var i = 0; i < Disc.Length; i++)
            {
                var text = new Label();
                text.Content = string.Format("{0}", Disc[i].Nazwa);
                text.Style = Resources["BlueDescription"] as Style;
                //DockPanel.SetDock(text, Dock.Top);
                SpDyskiNazwa.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami urządzeń z błędami
        void CreateDevMgmtData(DeviceManager[] Dev)
        {
            DpDeviceManagerCaption.Children.Clear();
            SpDeviceManagerErrorDescription.Children.Clear();
            for (var i = 0; i < Dev.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                DockPanel.SetDock(text, Dock.Top);
                DpDeviceManagerCaption.Children.Add(text);
                text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].TrescBledu);
                text.Style = Resources["PropertyValue"] as Style;
                SpDeviceManagerErrorDescription.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami dysków
        void CreateNetDevData(NetDevice[] Dev)
        {
            DpDeviceCaption.Children.Clear();
            DpDeviceMacAddress.Children.Clear();
            for (var i = 0; i < Dev.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                text.MaxWidth = 250;
                text.Height = 50;
                text.TextWrapping = TextWrapping.Wrap;
                DockPanel.SetDock(text, Dock.Top);
                DpDeviceCaption.Children.Add(text);
                text = new TextBlock();
                text.Text = string.Format("{0}", Dev[i].AdresMac);
                text.Height = 50;
                text.Style = Resources["PropertyValue"] as Style;
                DpDeviceMacAddress.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami kart graficznych
        void CreateGraphicCardData(GraphicCard[] Card)
        {
            SpGraphicCardCaption.Children.Clear();
            SpGraphicCardDescription.Children.Clear();
            for (var i = 0; i < Card.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Card[i].Nazwa);
                text.Style = Resources["TextBlockDescription"] as Style;
                text.MaxWidth = 250;
                text.Height = 50;
                text.TextWrapping = TextWrapping.Wrap;
                //DockPanel.SetDock(text, Dock.Top);
                SpGraphicCardCaption.Children.Add(text);
                text = new TextBlock();
                text.Text = string.Format("{0}", Card[i].Opis);
                text.Height = 50;
                text.Style = Resources["PropertyValue"] as Style;
                SpGraphicCardDescription.Children.Add(text);
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
                    else if (TxtWyszukiwarka.IsFocused == false && GridModele.IsFocused == false) TxtWyszukiwarka.Focus();
                    else if (TxtWyszukiwarka.IsFocused == true) GridModele.Focus();
                    else if (GridModele.IsFocused == true) GridModele.SelectedIndex += 1;
                    break;
                case Key.Up:
                    if (TabControl.SelectedIndex == 1) Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset - 100);
                    else if (GridModele.IsFocused == true && GridModele.SelectedIndex == 0) TxtWyszukiwarka.Focus();
                    else if (GridModele.SelectedIndex != 0)
                    {
                        GridModele.SelectedIndex -= 1;
                        GridModele.Focus();
                    }
                    break;
                case Key.Enter:
                    if (GridModele.IsFocused == true)
                    {
                        Retriever.ReaderData.ReadData(GridModele.SelectedItem as Model);
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