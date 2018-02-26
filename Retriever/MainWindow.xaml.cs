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
                case Key.None:
                    break;
                case Key.Cancel:
                    break;
                case Key.Back:
                    break;
                case Key.LineFeed:
                    break;
                case Key.Clear:
                    break;
                case Key.Pause:
                    break;
                case Key.Capital:
                    break;
                case Key.KanaMode:
                    break;
                case Key.JunjaMode:
                    break;
                case Key.FinalMode:
                    break;
                case Key.HanjaMode:
                    break;
                case Key.Escape:
                    break;
                case Key.ImeConvert:
                    break;
                case Key.ImeNonConvert:
                    break;
                case Key.ImeAccept:
                    break;
                case Key.ImeModeChange:
                    break;
                case Key.Space:
                    break;
                case Key.Prior:
                    break;
                case Key.Next:
                    break;
                case Key.End:
                    break;
                case Key.Home:
                    break;
                case Key.Left:
                    break;
                case Key.Right:
                    break;
                case Key.Select:
                    break;
                case Key.Print:
                    break;
                case Key.Execute:
                    break;
                case Key.Snapshot:
                    break;
                case Key.Insert:
                    break;
                case Key.Delete:
                    break;
                case Key.Help:
                    break;
                case Key.D0:
                    break;
                case Key.D1:
                    break;
                case Key.D2:
                    break;
                case Key.D3:
                    break;
                case Key.D4:
                    break;
                case Key.D5:
                    break;
                case Key.D6:
                    break;
                case Key.D7:
                    break;
                case Key.D8:
                    break;
                case Key.D9:
                    break;
                case Key.A:
                    break;
                case Key.B:
                    break;
                case Key.C:
                    break;
                case Key.D:
                    break;
                case Key.E:
                    break;
                case Key.F:
                    break;
                case Key.G:
                    break;
                case Key.H:
                    break;
                case Key.I:
                    break;
                case Key.J:
                    break;
                case Key.K:
                    break;
                case Key.L:
                    break;
                case Key.M:
                    break;
                case Key.N:
                    break;
                case Key.O:
                    break;
                case Key.P:
                    break;
                case Key.Q:
                    break;
                case Key.R:
                    break;
                case Key.S:
                    break;
                case Key.T:
                    break;
                case Key.U:
                    break;
                case Key.V:
                    break;
                case Key.W:
                    break;
                case Key.X:
                    break;
                case Key.Y:
                    break;
                case Key.Z:
                    break;
                case Key.LWin:
                    break;
                case Key.RWin:
                    break;
                case Key.Apps:
                    break;
                case Key.Sleep:
                    break;
                case Key.NumPad0:
                    break;
                case Key.NumPad1:
                    break;
                case Key.NumPad2:
                    break;
                case Key.NumPad3:
                    break;
                case Key.NumPad4:
                    break;
                case Key.NumPad5:
                    break;
                case Key.NumPad6:
                    break;
                case Key.NumPad7:
                    break;
                case Key.NumPad8:
                    break;
                case Key.NumPad9:
                    break;
                case Key.Multiply:
                    break;
                case Key.Add:
                    break;
                case Key.Separator:
                    break;
                case Key.Subtract:
                    break;
                case Key.Decimal:
                    break;
                case Key.Divide:
                    break;
                case Key.F1:
                    break;
                case Key.F2:
                    break;
                case Key.F3:
                    break;
                case Key.F4:
                    break;
                case Key.F5:
                    break;
                case Key.F6:
                    break;
                case Key.F7:
                    break;
                case Key.F8:
                    break;
                case Key.F9:
                    break;
                case Key.F10:
                    break;
                case Key.F11:
                    break;
                case Key.F12:
                    break;
                case Key.F13:
                    break;
                case Key.F14:
                    break;
                case Key.F15:
                    break;
                case Key.F16:
                    break;
                case Key.F17:
                    break;
                case Key.F18:
                    break;
                case Key.F19:
                    break;
                case Key.F20:
                    break;
                case Key.F21:
                    break;
                case Key.F22:
                    break;
                case Key.F23:
                    break;
                case Key.F24:
                    break;
                case Key.NumLock:
                    break;
                case Key.Scroll:
                    break;
                case Key.LeftShift:
                    break;
                case Key.RightShift:
                    break;
                case Key.LeftCtrl:
                    break;
                case Key.RightCtrl:
                    break;
                case Key.LeftAlt:
                    break;
                case Key.RightAlt:
                    break;
                case Key.BrowserBack:
                    break;
                case Key.BrowserForward:
                    break;
                case Key.BrowserRefresh:
                    break;
                case Key.BrowserStop:
                    break;
                case Key.BrowserSearch:
                    break;
                case Key.BrowserFavorites:
                    break;
                case Key.BrowserHome:
                    break;
                case Key.VolumeMute:
                    break;
                case Key.VolumeDown:
                    break;
                case Key.VolumeUp:
                    break;
                case Key.MediaNextTrack:
                    break;
                case Key.MediaPreviousTrack:
                    break;
                case Key.MediaStop:
                    break;
                case Key.MediaPlayPause:
                    break;
                case Key.LaunchMail:
                    break;
                case Key.SelectMedia:
                    break;
                case Key.LaunchApplication1:
                    break;
                case Key.LaunchApplication2:
                    break;
                case Key.Oem1:
                    break;
                case Key.OemPlus:
                    break;
                case Key.OemComma:
                    break;
                case Key.OemMinus:
                    break;
                case Key.OemPeriod:
                    break;
                case Key.Oem2:
                    break;
                case Key.Oem3:
                    break;
                case Key.AbntC1:
                    break;
                case Key.AbntC2:
                    break;
                case Key.Oem4:
                    break;
                case Key.Oem5:
                    break;
                case Key.Oem6:
                    break;
                case Key.Oem7:
                    break;
                case Key.Oem8:
                    break;
                case Key.Oem102:
                    break;
                case Key.ImeProcessed:
                    break;
                case Key.System:
                    break;
                case Key.OemAttn:
                    break;
                case Key.OemFinish:
                    break;
                case Key.OemCopy:
                    break;
                case Key.OemAuto:
                    break;
                case Key.OemEnlw:
                    break;
                case Key.OemBackTab:
                    break;
                case Key.Attn:
                    break;
                case Key.CrSel:
                    break;
                case Key.ExSel:
                    break;
                case Key.EraseEof:
                    break;
                case Key.Play:
                    break;
                case Key.Zoom:
                    break;
                case Key.NoName:
                    break;
                case Key.Pa1:
                    break;
                case Key.OemClear:
                    break;
                case Key.DeadCharProcessed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}