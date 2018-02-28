using DataTypes;
using Gatherer;
using Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Retriever
{

    public partial class MainWindow
    {
        private RetrieverInfo Retriever { get; set; }
        private FileSystemManager _settings;
        private DatabaseManager _dbManager;
        public MainWindow()
        {
            InitializeComponent();
            OdczytajKonfiguracjęIHasz();
            PrzygotujAplikacje();
        }

        private void OdczytajKonfiguracjęIHasz()
        {
            try
            {
                _settings = new FileSystemManager();
                _dbManager = new DatabaseManager(_settings);

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

        private void PrzygotujAplikacje()
        {
            //Utworzenie instancji Retriever
            try
            {
                Retriever = new RetrieverInfo(new ReaderInfo(_dbManager), new GathererInfo(), new Status());
            }
            catch(Exception e)
            {
                var message = $"Wystąpił błąd podczas otwierania aplikacji:\n{e.Message}" + $"\n{e.StackTrace}" +
                              $"\n{e.Source}" + $"\n{e.TargetSite}" + $"\n{e.HelpLink}";
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
                var enumerable = model.ToList();
                if (enumerable.Any())
                    Retriever.ReaderData.ReadData(enumerable.First());
            }
            else
            {
                Retriever.ReaderData?.ReadData(GridModele.SelectedItem as Model);
            }

            //Tworzenie dynamicznych kontrolek
            DynamicControls();

            //Kod wyszukiwarki modeli
            var widok = (CollectionView)CollectionViewSource.GetDefaultView(GridModele.ItemsSource);
            widok.Filter = FiltrUzytkownika;

        }

        #region Wyszukiwarka

        private bool FiltrUzytkownika(object item)
        {
            return string.IsNullOrEmpty(TxtWyszukiwarka.Text) || ((Model) item).Md.Contains(TxtWyszukiwarka.Text);

            //Wyszukiwanie również po msn: else return (((item as Model).MD.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0) || ((item as Model).MSN.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        //Metoda wyszukiwania z listy modeli
        private void txtWyszukiwarka_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox) sender).Text.Length < 3)
                return;
            if(((TextBox) sender).Text.Length == 0 || ((TextBox) sender).Text.Length > 2)
                CollectionViewSource.GetDefaultView(GridModele.ItemsSource).Refresh();
        }
        #endregion

        #region Dynamiczne twozenie kontrolek
        //Metoda wywołująca wszystkie poniższe
        private void DynamicControls()
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
        private void CreateSwmData(IEnumerable<SWM> swm)
        {
            SpSwm.Children.Clear();
            foreach (var t in swm)
            {
                var text = new TextBlock
                {
                    Text = $"{t.Dysk[0]}: {t.Swm}",
                    Style = Resources["PropertyValue"] as Style
                };
                SpSwm.Children.Add(text);
            }
        }

        //Dodawanie kontrolek Wear Level
        private void CreateWearLevelData(IEnumerable<double> wl)
        {
            SpWearLevel.Children.Clear();
            foreach (var t in wl)
            {
                var text = new TextBlock
                {
                    Text = $"{t}%",
                    Style = Resources["PropertyValue"] as Style
                };
                SpWearLevel.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z pojemnościami dysków
        private void CreateDiscData(IEnumerable<Storage> disc, ref StackPanel control)
        {
            control.Children.Clear();
            foreach (var t in disc)
            {
                var text = new TextBlock
                {
                    Text = $"{t.Pojemnosc}GB",
                    Style = Resources["PropertyValue"] as Style
                };
                control.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami dysków
        private void CreateDiscDataHeaders(IEnumerable<Storage> disc)
        {
            SpDyskiNazwa.Children.Clear();
            foreach (var t in disc)
            {
                var text = new Label
                {
                    Content = $"{t.Nazwa}",
                    Style = Resources["BlueDescription"] as Style
                };
                //DockPanel.SetDock(text, Dock.Top);
                SpDyskiNazwa.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami urządzeń z błędami
        private void CreateDevMgmtData(IEnumerable<DeviceManager> dev)
        {
            DpDeviceManagerCaption.Children.Clear();
            SpDeviceManagerErrorDescription.Children.Clear();
            foreach (var t in dev)
            {
                var text = new TextBlock
                {
                    Text = $"{t.Nazwa}",
                    Style = Resources["TextBlockDescription"] as Style
                };
                DockPanel.SetDock(text, Dock.Top);
                DpDeviceManagerCaption.Children.Add(text);
                text = new TextBlock
                {
                    Text = $"{t.TrescBledu}",
                    Style = Resources["PropertyValue"] as Style
                };
                SpDeviceManagerErrorDescription.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami dysków
        private void CreateNetDevData(IEnumerable<NetDevice> dev)
        {
            DpDeviceCaption.Children.Clear();
            DpDeviceMacAddress.Children.Clear();
            foreach (var t in dev)
            {
                var text = new TextBlock
                {
                    Text = $"{t.Nazwa}",
                    Style = Resources["TextBlockDescription"] as Style,
                    MaxWidth = 250,
                    Height = 50,
                    TextWrapping = TextWrapping.Wrap
                };
                DockPanel.SetDock(text, Dock.Top);
                DpDeviceCaption.Children.Add(text);
                text = new TextBlock
                {
                    Text = $"{t.AdresMac}",
                    Height = 50,
                    Style = Resources["PropertyValue"] as Style
                };
                DpDeviceMacAddress.Children.Add(text);
            }
        }

        //Dodawanie kontrolek z nazwami kart graficznych
        private void CreateGraphicCardData(IList<GraphicCard> card)
        {
            SpGraphicCardCaption.Children.Clear();
            SpGraphicCardDescription.Children.Clear();
            foreach (var t in card)
            {
                var text = new TextBlock
                {
                    Text = $"{t.Nazwa}",
                    Style = Resources["TextBlockDescription"] as Style,
                    MaxWidth = 250,
                    Height = 50,
                    TextWrapping = TextWrapping.Wrap
                };
                //DockPanel.SetDock(text, Dock.Top);
                SpGraphicCardCaption.Children.Add(text);
                text = new TextBlock
                {
                    Text = $"{t.Opis}",
                    Height = 50,
                    Style = Resources["PropertyValue"] as Style
                };
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
                    TabControl.SelectedIndex = TabControl.SelectedIndex == 0 ? 1 : 0;
                    break;
                case Key.Down:
                    if (TabControl.SelectedIndex == 1) Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset + 100);
                    else switch (TxtWyszukiwarka.IsFocused)
                    {
                        case false when GridModele.IsFocused == false:
                            TxtWyszukiwarka.Focus();
                            break;
                        case true:
                            GridModele.Focus();
                            break;
                        default:
                            if (GridModele.IsFocused) GridModele.SelectedIndex += 1;
                            break;
                    }
                    break;
                case Key.Up:
                    if (TabControl.SelectedIndex == 1) Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset - 100);
                    else if (GridModele.IsFocused && GridModele.SelectedIndex == 0) TxtWyszukiwarka.Focus();
                    else if (GridModele.SelectedIndex != 0)
                    {
                        GridModele.SelectedIndex -= 1;
                        GridModele.Focus();
                    }
                    break;
                case Key.Enter:
                    if (GridModele.IsFocused)
                    {
                        Retriever.ReaderData.ReadData(GridModele.SelectedItem as Model);
                        DynamicControls();
                        TabControl.SelectedIndex = 1;
                    }
                    break;
                
            }
        }

        private void WmiManager_Click(object sender, RoutedEventArgs e)
        {
            var manager = new WmiManager();
            manager.Show();
        }
    }
}