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
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Reader ReaderInfo { get; private set; }
        public ModelListSource ListSource;
        public Gatherer GathererInfo { get; private set; }
        public Model ThisComputer { get; private set; }
        public RetrieverInfo Retriever2 { get; private set; }
        public SystemDateTime Timer { get; private set; }
        DispatcherTimer TimeThicker; //Timer do odświeżania czasu w kontrolce

        public MainWindow()
        {
            InitializeComponent();
            PrzygotujWiazanie();
            UstawTimer();
        }

        void PrzygotujWiazanie()
        {
            //Wczytanie i przypisanie listy modeli do grida
            ListSource = new ModelListSource(new Reader());           
            gridModele.ItemsSource = ListSource.ListaModeli;
      
            //Wczytanie danych rzeczywistych komputera i przypisanie do gridów
            GathererInfo = new Gatherer();

            //Tworzenie dodatkowych kontrolek z informacjami
            CreateSwmDataControls(GathererInfo.Swm);
            CreateWearLevelDataControls(GathererInfo.Komputer.WearLevel);
            CreateRamDataControls(GathererInfo.Ram);
            CreateDiscDataHeaders(GathererInfo.Dyski, expDyskiNazwa);
            
            CreateDiscDataControls(GathererInfo.Dyski, expDyskiGatherer);

            //Kod wyszukiwarki modeli
            CollectionView widok = (CollectionView)CollectionViewSource.GetDefaultView(gridModele.ItemsSource);
            widok.Filter = FiltrUzytkownika;

            //Próba określenia modelu i automatycznego wczytania danych konkretnrgo modelu do zestawienia
            var q = from item in ListSource.ListaModeli
                    where item.MD.Equals(GathererInfo.Komputer.MD)
                    select item;
            ReaderInfo = new Reader(((q.First() == null) ? (gridModele.SelectedItem as Model)  : (q.First() as Model)));
            
            //Zapisanie aktualnego modelu komputera - albo na podstawie wydobytego, albo na podstawiie pierwszego zaznaczonego
            ThisComputer = q.First() as Model == null ? gridModele.SelectedItem as Model : q.First() as Model;
            CreateDiscDataControls(ReaderInfo.Dyski, expDyskiReader);
            //Dodanie kontekstu danych do grida
            Retriever2 = new RetrieverInfo(ReaderInfo, GathererInfo);
            gridZestawienie.DataContext = Retriever2;
            
        }

        //Metoda wczytująca dane aktualnie zaznaczonego rekordu
        private void gridModele_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReaderInfo = new Reader((gridModele.SelectedItem as Model) == null ? ThisComputer : gridModele.SelectedItem as Model);
            Retriever2 = new RetrieverInfo(ReaderInfo, GathererInfo);
            gridZestawienie.DataContext = Retriever2;
        }

        bool FiltrUzytkownika(object item)
        {
            if (String.IsNullOrEmpty(txtWyszukiwarka.Text)) return true;
            else return (((item as Model).MD.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0));
            //Wyszukiwanie również po msn: else return (((item as Model).MD.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0) || ((item as Model).MSN.IndexOf(txtWyszukiwarka.Text, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        //Metoda wyszukiwania z listy modeli
        void txtWyszukiwarka_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(gridModele.ItemsSource).Refresh();
            ThisComputer = gridModele.SelectedItem as Model;
        }

        #region Dynamiczne twozenie kontrolek
        //Dodawanie kontrolek SWM
        public void CreateSwmDataControls(SWM[] Swm)
        {
            spSwm.Children.Clear();
            for(int i = 0; i < Swm.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}: {1}", Swm[i].Dysk[0], Swm[i].Swm);
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
                text.Text = string.Format("{0}%",wl[i]);
                spWearLevel.Children.Add(text);
            }
        }

        //Dodawanie kontrolek pojenmności RAM
        public void CreateRamDataControls(RAM[] Ram)
        {
            expRam.Content = null;
            var stack = new StackPanel();
            
            for (int i = 0; i < Ram.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}GB", Ram[i].Pojemnosc);
                stack.Children.Add(text);
            }
            expRam.Content = stack;
        }

        //Dodawanie kontrolek z pojemnościami dysków
        public void CreateDiscDataControls(Storage[] Disc, Expander control)
        {
            control.Content = null;
            var stack = new StackPanel();

            for (int i = 0; i < Disc.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}GB", Disc[i].Pojemnosc);
                stack.Children.Add(text);
            }
            control.Content = stack;
        }

        //Dodawanie kontrolek z nazwami dysków
        public void CreateDiscDataHeaders(Storage[] Disc, Expander control)
        {
            control.Content = null;
            var stack = new StackPanel();

            for (int i = 0; i < Disc.Length; i++)
            {
                var text = new TextBlock();
                text.Text = string.Format("{0}", Disc[i].Nazwa);
                text.FontWeight = FontWeights.Normal;
                stack.Children.Add(text);
            }
            control.Content = stack;
        }
        #endregion

        #region Obsługa nagłówka w dyskach twardych
        private void RemoveSpace(object sender, RoutedEventArgs e)
        {
            (sender as Expander).Header = "Dyski twarde";
        }

        private void AddSpace(object sender, RoutedEventArgs e)
        {
            (sender as Expander).Header = "          Dyski twarde";
        }
        #endregion


        private void OpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("::{20d04fe0-3aea-1069-a2d8-08002b30309d}");
        }

        private void WindowsActivationScript_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"../../Skypty/odpal.bat");
        }

        private void DeviceManager_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("devmgmt.msc");
        }

        #region Obsługa daty i czasu w aplikacji
        //Metoda ustawiwjąca nową datę i czas
        private void btnSetDateTime_Click(object sender, RoutedEventArgs e)
        {
            //DateTime control = DateTime.Parse("01.01.0001 00:00:00");
            //SYSTEMTIME myTime = new SYSTEMTIME();
            //DateTime timeToSet = new DateTime();
            //if (String.IsNullOrEmpty(txtSetDateTime.Text))
            //    return;
            //else if (DateTime.TryParse(txtSetDateTime.Text, out timeToSet) && timeToSet != control)
            //{
            //    myTime.wYear = (short)timeToSet.Year;
            //    myTime.wMonth = (short)timeToSet.Month;
            //    myTime.wDay = (short)timeToSet.Day;
            //    myTime.wHour = (short)timeToSet.Hour;
            //    myTime.wMinute = (short)timeToSet.Minute;
            //    myTime.wSecond = (short)timeToSet.Second;
            //    myTime.wMilliseconds = (short)0;
            //    SystemDateTimeChanger.SetSystemTime(ref myTime);
            //}
            Process.Start("timedate.cpl");
        }
        # region PRZETESTOWAC W PRACY
        void UstawTimer()
        {
            //Utworzenie obiektu przechowującego aktualny czas
            Timer = new SystemDateTime();
            tbDateTime.DataContext = Timer;
            TimeThicker = new DispatcherTimer();
            TimeThicker.Interval = new TimeSpan(0, 0, 1); //Czas odświeżania - jedna minuta
            TimeThicker.Tick += new EventHandler(RefreshTime);
            TimeThicker.Start();
        }

        void RefreshTime(object sender, EventArgs e)
        {
            Timer = new SystemDateTime();
            tbDateTime.DataContext = Timer;
        }
        #endregion

        #endregion

        private void AquaKeyTest_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Environment.CurrentDirectory + @"\Skrypty/AquaKeyTest.exe");
        }

        private void aMonitest_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Environment.CurrentDirectory + @"\Skrypty/aMonitest.exe");
        }

        private void touchScreen_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Environment.CurrentDirectory + @"\Skrypty\touchscreen_2.exe");
        }

        private void mmSys_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("mmsys.cpl");
        }

        private void cam_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Environment.CurrentDirectory + @"\Skrypty\cam.exe");
        }

        private void restart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"shutdown","/r /t 0");
        }

        private void restartBios_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"shutdown","/r /fw /t 0");
        }

        private void WindowsActivationTelephone_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"slui 4");
        }
    }
}